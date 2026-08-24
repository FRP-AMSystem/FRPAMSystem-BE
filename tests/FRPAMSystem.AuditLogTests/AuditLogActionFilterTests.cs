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

namespace FRPAMSystem.AuditLogTests
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
                new Claim(ClaimTypes.NameIdentifier, "1")
            }));

            var routeValues = new RouteValueDictionary { { "id", "15" } };
            var context = CreateActionExecutingContext("DELETE", "Experiments", "DeleteExperiment", routeValues, null, user, 200);
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            mockAuditService.Verify(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()), Times.Once);
            Assert.NotNull(capturedRequest);
            Assert.Equal(1, capturedRequest.ActorUserId);
            Assert.Equal("Experiments", capturedRequest.Module);
            Assert.Equal("DeleteExperiment", capturedRequest.Action);
            Assert.Equal("Experiment", capturedRequest.ReferenceType);
            Assert.Equal(15, capturedRequest.ReferenceId);
            Assert.Equal("Information", capturedRequest.Severity);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenDomainEventAuditSucceeded_SkipsDuplicateHttpAuditLog()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var context = CreateActionExecutingContext("POST", "Experiments", "CreateExperiment");
            // Simulate Domain Event handler having ALREADY successfully committed an audit log
            context.HttpContext.Items["AuditLogRecordedByDomainEvent"] = true;

            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            mockAuditService.Verify(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()), Times.Never);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenDomainEventAuditFailed_ExecutesTechnicalFallbackHttpAuditLog()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();

            CreateAuditLogRequest? capturedRequest = null;
            mockAuditService.Setup(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()))
                .Callback<CreateAuditLogRequest>(req => capturedRequest = req)
                .ReturnsAsync(new AuditLogResponse());

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var context = CreateActionExecutingContext("POST", "Experiments", "CreateExperiment");
            // Flag is absent because Domain Event audit failed before commit
            Assert.False(context.HttpContext.Items.ContainsKey("AuditLogRecordedByDomainEvent"));

            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert: Fallback HTTP action filter log IS executed
            mockAuditService.Verify(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()), Times.Once);
            Assert.NotNull(capturedRequest);
            Assert.Equal("Experiments", capturedRequest.Module);
        }

        [Fact]
        public async Task OnActionExecutionAsync_AuditLogsController_IsExcludedFromAuditing()
        {
            // Arrange
            var mockAuditService = new Mock<IAuditLogService>();
            var mockLogger = new Mock<ILogger<AuditLogActionFilter>>();

            var filter = new AuditLogActionFilter(mockAuditService.Object, mockLogger.Object);

            var context = CreateActionExecutingContext("GET", "AuditLogs", "GetAuditLogs");
            var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object());

            // Act
            await filter.OnActionExecutionAsync(context, CreateNextDelegate(executedContext));

            // Assert
            mockAuditService.Verify(s => s.RecordLogAsync(It.IsAny<CreateAuditLogRequest>()), Times.Never);
        }
    }
}
