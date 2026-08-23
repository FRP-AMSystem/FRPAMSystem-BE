using FRPAMSystem.BusinessTier.Configuration;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Implement;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FRPAMSystem.AuditLogTests
{
    public class AuditLogServiceTests
    {
        private ForestryResourcePlanningDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ForestryResourcePlanningDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ForestryResourcePlanningDbContext(options);
        }

        [Fact]
        public async Task RecordLogAsync_ValidUser_InsertsAuditLogWithActorUserId()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            dbContext.Users.Add(new User { UserId = 10, FullName = "Test User", Username = "test", Email = "test@frpam.edu.vn", RoleId = 1, PasswordHash = "hash" });
            await dbContext.SaveChangesAsync();

            IUnitOfWork uow = new UnitOfWork(dbContext);
            var service = new AuditLogService(uow);

            var request = new CreateAuditLogRequest
            {
                ActorUserId = 10,
                Module = "Experiments",
                Action = "DeleteExperiment",
                ReferenceType = "Experiment",
                ReferenceId = 15,
                Severity = "INFO",
                Description = "DELETE /api/experiments/15 - Status 200"
            };

            // Act
            var result = await service.RecordLogAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.ActorUserId);
            Assert.Equal("Experiment", result.ReferenceType);
            Assert.Equal(15, result.ReferenceId);
        }

        [Fact]
        public async Task RecordLogAsync_InvalidUser_StoresNullActorUserIdAndLogsWarning()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            IUnitOfWork uow = new UnitOfWork(dbContext);
            var mockLogger = new Mock<ILogger<AuditLogService>>();

            var service = new AuditLogService(uow, logger: mockLogger.Object);

            var request = new CreateAuditLogRequest
            {
                ActorUserId = 9999, // User 9999 does not exist in DB
                Module = "Experiments",
                Action = "CreateExperiment",
                Severity = "INFO",
                Description = "Test invalid user"
            };

            // Act
            var result = await service.RecordLogAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ActorUserId); // Stored NULL to avoid FK error

            // Verify warning log
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("invalid ActorUserId 9999")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RecordLogAsync_TrimsExpiredLogs_MaintainingMaxRecords()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            IUnitOfWork uow = new UnitOfWork(dbContext);

            var options = Options.Create(new AuditLogOptions { MaxRecords = 5 });
            var service = new AuditLogService(uow, options);

            var baseTime = DateTime.UtcNow;
            for (int i = 1; i <= 5; i++)
            {
                dbContext.AuditLogs.Add(new AuditLog
                {
                    AuditLogId = i,
                    Module = "Experiments",
                    Action = "Test",
                    Severity = "INFO",
                    Description = $"Log {i}",
                    CreatedAt = baseTime.AddMinutes(-10 + i)
                });
            }
            await dbContext.SaveChangesAsync();

            // Act - Add 6th record
            await service.RecordLogAsync(new CreateAuditLogRequest
            {
                Module = "Experiments",
                Action = "TestNew",
                Severity = "INFO",
                Description = "Log 6"
            });

            // Assert
            var remainingLogs = await dbContext.AuditLogs.OrderBy(x => x.CreatedAt).ThenBy(x => x.AuditLogId).ToListAsync();
            Assert.Equal(5, remainingLogs.Count);
            Assert.DoesNotContain(remainingLogs, x => x.AuditLogId == 1);
            Assert.Contains(remainingLogs, x => x.Description == "Log 6");
        }
    }
}
