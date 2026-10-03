using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem_BE.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FRPAMSystem.NotificationTests
{
    public class AuditLogActionFilterTests
    {
        private ActionExecutingContext CreateActionExecutingContext(
            string httpMethod,
            string controllerName,
            string actionName,
            RouteValueDictionary? routeValues = null,
            IDictionary<string, object?>? actionArguments = null,
            ClaimsPrincipal? user = null,
            int statusCode = 200)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = httpMethod;
            httpContext.Request.Path = $"/api/{controllerName.ToLower()}";
            httpContext.Response.StatusCode = statusCode;

            if (user != null)
            {
                httpContext.User = user;
            }

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(routeValues ?? new RouteValueDictionary()),
                ActionDescriptor = new ActionDescriptor()
            };
            actionContext.RouteData.Values["controller"] = controllerName;
            actionContext.RouteData.Values["action"] = actionName;

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments ?? new Dictionary<string, object?>(),
                new object()
            );
        }

        private ActionExecutionDelegate CreateNextDelegate(ActionExecutedContext executedContext)
        {
            return () => Task.FromResult(executedContext);
        }

        [Fact]
        public async Task OnActionExecutionAsync_SuccessfulDelete_CreatesAuditLogWithReferenceTypeAndId()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();

            CreateAuditLogRequest? capturedRequest = null;
            mockAuditService.Setup(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()))
                .Callback<CreateAuditLogRequest>(req => capturedRequest = req)
                .ReturnsAsync(new AuditLogResponse());

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42")
            }));

            var routeValues = new RouteValueDictionary { { "id", "15" } };
            var context = CreateActionExecutingContext("DELETE", "Experiments", "DeleteExperiment", routeValues, null, user, 200);
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            mockAuditService.Verify(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()), Times.Once);
            Assert.NotNull(capturedRequest);
            Assert.Equal(42, capturedRequest.ActorUserId);
            Assert.Equal("Experiments", capturedRequest.Module);
            Assert.Equal("DeleteExperiment", capturedRequest.Action);
            Assert.Equal("Experiment", capturedRequest.ReferenceType);
            Assert.Equal(15, capturedRequest.ReferenceId);
            Assert.Equal("INFO", capturedRequest.Severity);
        }

        [Fact]
        public async Task OnActionExecutionAsync_Delete4xxStatus_SetsWarningSeverity()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();
            CreateAuditLogRequest? capturedRequest = null;
            mockAuditService.Setup(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()))
                .Callback<CreateAuditLogRequest>(req => capturedRequest = req)
                .ReturnsAsync(new AuditLogResponse());

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var routeValues = new RouteValueDictionary { { "id", "99" } };
            var context = CreateActionExecutingContext("DELETE", "Users", "DeleteUser", routeValues, null, null, 404);
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("User", capturedRequest.ReferenceType);
            Assert.Equal(99, capturedRequest.ReferenceId);
            Assert.Equal("WARNING", capturedRequest.Severity);
        }

        [Fact]
        public async Task OnActionExecutionAsync_Delete5xxStatusOrException_SetsErrorSeverity()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();
            CreateAuditLogRequest? capturedRequest = null;
            mockAuditService.Setup(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()))
                .Callback<CreateAuditLogRequest>(req => capturedRequest = req)
                .ReturnsAsync(new AuditLogResponse());

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var routeValues = new RouteValueDictionary { { "id", "77" } };
            var context = CreateActionExecutingContext("DELETE", "EquipmentCategories", "DeleteCategory", routeValues, null, null, 500);
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object())
            {
                Exception = new InvalidOperationException("Internal server error")
            };

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("EquipmentCategory", capturedRequest.ReferenceType);
            Assert.Equal(77, capturedRequest.ReferenceId);
            Assert.Equal("ERROR", capturedRequest.Severity);
        }

        [Fact]
        public async Task OnActionExecutionAsync_InvalidOrMissingDeleteId_DoesNotCrashFilter()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();
            CreateAuditLogRequest? capturedRequest = null;
            mockAuditService.Setup(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()))
                .Callback<CreateAuditLogRequest>(req => capturedRequest = req)
                .ReturnsAsync(new AuditLogResponse());

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var context = CreateActionExecutingContext("DELETE", "Schedules", "DeleteSchedule", new RouteValueDictionary(), null, null, 200);
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("Schedule", capturedRequest.ReferenceType);
            Assert.Null(capturedRequest.ReferenceId);
        }

        [Fact]
        public async Task OnActionExecutionAsync_AuditLogsController_IsExcludedFromAuditing()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var context = CreateActionExecutingContext("DELETE", "AuditLogs", "DeleteAuditLog", new RouteValueDictionary { { "id", "1" } });
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            mockAuditService.Verify(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()), Times.Never);
        }

        [Fact]
        public async Task OnActionExecutionAsync_AuditServiceException_DoesNotCrashRequest()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();
            mockAuditService.Setup(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()))
                .ThrowsAsync(new Exception("Database offline"));

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var context = CreateActionExecutingContext("DELETE", "Experiments", "DeleteExperiment", new RouteValueDictionary { { "id", "10" } });
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act & Assert (Should not throw exception)
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to write audit log")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenDomainEventAuditRecorded_SkipsDuplicateHttpAuditLog()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var context = CreateActionExecutingContext("POST", "Experiments", "CreateExperiment");
            // Simulate Domain Event handler having already recorded a rich business audit log
            context.HttpContext.Items["AuditLogRecordedByDomainEvent"] = true;

            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert: AuditLogService.RecordLogAsync must NOT be called by the filter (avoiding duplicate log)
            mockAuditService.Verify(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()), Times.Never);
        }
    }
}
