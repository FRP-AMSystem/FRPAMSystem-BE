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

namespace FRPAMSystem.NotificationTests
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
        public async Task RecordLogAsync_ShouldInsertAuditLog_AndReturnResponse()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            IUnitOfWork uow = new UnitOfWork(dbContext);
            var service = new AuditLogService(uow);

            var request = new CreateAuditLogRequest
            {
                ActorUserId = 1,
                Module = "Experiments",
                Action = "DeleteExperiment",
                ReferenceType = "Experiment",
                ReferenceId = 15,
                Severity = "INFO",
                Description = "DELETE /api/experiments/15 - Status 200",
                Metadata = "{\"Path\":\"/api/experiments/15\"}"
            };

            // Act
            var result = await service.RecordLogAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Experiments", result.Module);
            Assert.Equal("DeleteExperiment", result.Action);
            Assert.Equal("Experiment", result.ReferenceType);
            Assert.Equal(15, result.ReferenceId);
            Assert.Equal("INFO", result.Severity);

            var logInDb = await dbContext.AuditLogs.FirstOrDefaultAsync();
            Assert.NotNull(logInDb);
            Assert.Equal("Experiment", logInDb.ReferenceType);
            Assert.Equal(15, logInDb.ReferenceId);
        }

        [Fact]
        public async Task RecordLogAsync_ShouldTrimExpiredLogs_WhenExceedingMaxRecords()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            IUnitOfWork uow = new UnitOfWork(dbContext);

            var options = Options.Create(new AuditLogOptions { MaxRecords = 5 });
            var service = new AuditLogService(uow, options);

            var baseTime = DateTime.UtcNow;
            // Seed 5 existing records
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

            // Act - Add 6th record (exceeding MaxRecords 5)
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

            // The oldest record (AuditLogId = 1) should have been deleted
            Assert.DoesNotContain(remainingLogs, x => x.AuditLogId == 1);
            Assert.Contains(remainingLogs, x => x.Description == "Log 6");
        }

        [Fact]
        public async Task RecordLogAsync_ShouldRespectCustomMaxRecordsConfiguration()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            IUnitOfWork uow = new UnitOfWork(dbContext);

            var options = Options.Create(new AuditLogOptions { MaxRecords = 3 });
            var service = new AuditLogService(uow, options);

            for (int i = 1; i <= 5; i++)
            {
                await service.RecordLogAsync(new CreateAuditLogRequest
                {
                    Module = "Module",
                    Action = "Action",
                    Severity = "INFO",
                    Description = $"Log {i}"
                });
            }

            // Assert
            var remainingLogs = await dbContext.AuditLogs.ToListAsync();
            Assert.Equal(3, remainingLogs.Count);
        }

        [Fact]
        public async Task RecordLogAsync_ShouldHandleEmptyTable_WithoutErrors()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            IUnitOfWork uow = new UnitOfWork(dbContext);
            var options = Options.Create(new AuditLogOptions { MaxRecords = 100 });
            var service = new AuditLogService(uow, options);

            // Act
            var result = await service.RecordLogAsync(new CreateAuditLogRequest
            {
                Module = "TestModule",
                Action = "TestAction",
                Severity = "INFO",
                Description = "First log"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, await dbContext.AuditLogs.CountAsync());
        }

        [Fact]
        public async Task RecordLogAsync_ShouldNotCrash_WhenTrimmingFails()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<AuditLog>>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<AuditLogService>>();

            var auditLog = new AuditLog { AuditLogId = 1, Module = "Test", Action = "Test", Severity = "INFO", Description = "Test" };
            
            mockRepo.Setup(r => r.InsertAsync(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(a => a.AuditLogId = 1)
                .Returns(Task.CompletedTask);
            mockRepo.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<AuditLog, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<AuditLog>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<AuditLog, object>>>(),
                true))
                .ReturnsAsync(auditLog);

            // Simulate Trim error in GetQueryable or ExecuteDeleteAsync
            mockRepo.Setup(r => r.GetQueryable()).Throws(new Exception("Database connection error during trim"));

            mockUow.Setup(u => u.GetRepository<AuditLog>()).Returns(mockRepo.Object);
            mockUow.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            var options = Options.Create(new AuditLogOptions { MaxRecords = 5 });
            var service = new AuditLogService(mockUow.Object, options, mockLogger.Object);

            // Act & Assert (Should not throw exception)
            var response = await service.RecordLogAsync(new CreateAuditLogRequest
            {
                Module = "Test",
                Action = "Test",
                Description = "Test"
            });

            Assert.NotNull(response);
            // Verify error was logged
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to trim")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Trimming_DoesNotCreateRecursiveAuditLogs()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            IUnitOfWork uow = new UnitOfWork(dbContext);
            var options = Options.Create(new AuditLogOptions { MaxRecords = 2 });
            var service = new AuditLogService(uow, options);

            await service.RecordLogAsync(new CreateAuditLogRequest { Module = "M", Action = "A", Description = "L1" });
            await service.RecordLogAsync(new CreateAuditLogRequest { Module = "M", Action = "A", Description = "L2" });
            await service.RecordLogAsync(new CreateAuditLogRequest { Module = "M", Action = "A", Description = "L3" });

            // Assert
            var logs = await dbContext.AuditLogs.ToListAsync();
            Assert.Equal(2, logs.Count);
            // Ensure no extra audit log entries were generated by the trim mechanism itself
            Assert.All(logs, l => Assert.Equal("M", l.Module));
        }
    }
}
