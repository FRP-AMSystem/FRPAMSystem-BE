using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FRPAMSystem.AuditLogTests
{
    public class AuditLogIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuditLogIntegrationTests(WebApplicationFactory<Program> factory)
        {
            var dbName = "IntegrationTestDb_" + Guid.NewGuid().ToString();
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ForestryResourcePlanningDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ForestryResourcePlanningDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });
                });
            });
        }

        private HttpClient CreateAuthenticatedClient(int userId = 1, string role = "Admin")
        {
            var client = _factory.CreateClient();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("FRPAMSystemSuperSecretKey123456789");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "FRPAMSystem",
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, "testuser"),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            return client;
        }

        [Fact]
        public async Task RealHttpPipeline_CreateExperiment_ProducesExactlyOneAuditLogWithActualId()
        {
            // Arrange
            var client = CreateAuthenticatedClient(1, "Admin");
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ForestryResourcePlanningDbContext>();
                db.Users.Add(new User { UserId = 1, FullName = "Researcher User", Username = "researcher", Email = "researcher@frpam.edu.vn", RoleId = 1, PasswordHash = "hash" });
                await db.SaveChangesAsync();
            }

            var request = new
            {
                experimentName = "Integration Test Experiment",
                description = "Integration Test Description",
                researcherId = 1,
                expectStartDate = DateTime.UtcNow.AddDays(1),
                expectEndDate = DateTime.UtcNow.AddDays(10),
                deadline = DateTime.UtcNow.AddDays(12),
                priority = 1,
                status = "Draft"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Experiments", request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Request failed with status {response.StatusCode}");

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ForestryResourcePlanningDbContext>();
                var auditLogs = await db.AuditLogs.ToListAsync();

                // Exactly ONE AuditLog created for the real HTTP request
                Assert.Single(auditLogs);
                var log = auditLogs.First();
                Assert.Equal("Experiment", log.Module);
                Assert.Equal("CreateExperiment", log.Action);
                Assert.Equal("Experiment", log.ReferenceType);
                Assert.NotNull(log.ReferenceId); // Actual created ExperimentId
            }
        }

        [Fact]
        public async Task RealHttpPipeline_DeleteExperiment_ProducesExactlyOneAuditLogWithReferenceId()
        {
            // Arrange
            var client = CreateAuthenticatedClient(1, "Admin");
            int experimentId = 50;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ForestryResourcePlanningDbContext>();
                if (!await db.Users.AnyAsync(u => u.UserId == 1))
                {
                    db.Users.Add(new User { UserId = 1, FullName = "Admin User", Username = "admin", Email = "admin@frpam.edu.vn", RoleId = 1, PasswordHash = "hash" });
                }
                db.Experiments.Add(new Experiment
                {
                    ExperimentId = experimentId,
                    ExperimentName = "To Delete",
                    ResearcherId = 1,
                    ExpectStartDate = DateTime.UtcNow,
                    ExpectEndDate = DateTime.UtcNow.AddDays(5),
                    Deadline = DateTime.UtcNow.AddDays(7),
                    Priority = 1,
                    Status = "Draft"
                });
                await db.SaveChangesAsync();
            }

            // Act: Perform real HTTP DELETE request
            var response = await client.DeleteAsync($"/api/Experiments/{experimentId}");

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Delete failed with status {response.StatusCode}");

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ForestryResourcePlanningDbContext>();
                var deleteLogs = await db.AuditLogs.Where(l => l.Action == "DeleteExperiment").ToListAsync();

                Assert.Single(deleteLogs);
                var log = deleteLogs.First();
                Assert.Equal("Experiments", log.Module);
                Assert.Equal("Experiment", log.ReferenceType);
                Assert.Equal(experimentId, log.ReferenceId);
                Assert.Equal("Information", log.Severity);
            }
        }

        [Fact]
        public async Task RealHttpPipeline_GenericCrud_ProducesExactlyOneAuditLog()
        {
            // Arrange
            var client = CreateAuthenticatedClient(1, "Admin");
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ForestryResourcePlanningDbContext>();
                if (!await db.Users.AnyAsync(u => u.UserId == 1))
                {
                    db.Users.Add(new User { UserId = 1, FullName = "Admin User", Username = "admin", Email = "admin@frpam.edu.vn", RoleId = 1, PasswordHash = "hash" });
                    await db.SaveChangesAsync();
                }
            }

            var areaRequest = new
            {
                areaName = "New Test Area",
                description = "Area Description"
            };

            // Act: Real HTTP POST to AreasController (Generic CRUD without domain event handler)
            var response = await client.PostAsJsonAsync("/api/Areas", areaRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"POST /api/Areas failed with status {response.StatusCode}");

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ForestryResourcePlanningDbContext>();
                var areaLogs = await db.AuditLogs.Where(l => l.Module == "Areas").ToListAsync();

                Assert.Single(areaLogs);
                var log = areaLogs.First();
                Assert.Equal("Areas", log.Module);
                Assert.Equal("CreateArea", log.Action);
                Assert.Equal("Area", log.ReferenceType);
            }
        }

        [Fact]
        public async Task RealHttpPipeline_AuditLogsController_ExcludesSelfAuditing()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act: Real HTTP GET request to AuditLogsController
            var response = await client.GetAsync("/api/AuditLogs");

            // Assert: Query returns response, but filter creates 0 AuditLogs for AuditLogsController
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ForestryResourcePlanningDbContext>();
                var auditLogCount = await db.AuditLogs.CountAsync(l => l.Module == "AuditLogs");
                Assert.Equal(0, auditLogCount);
            }
        }
    }
}
