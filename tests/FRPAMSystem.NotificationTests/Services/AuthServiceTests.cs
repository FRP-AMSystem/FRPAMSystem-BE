using FRPAMSystem.BusinessTier.Payload.Auth;
using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();

        private readonly Mock<IGenericRepository<User>> _userRepoMock = new();
        private readonly Mock<IGenericRepository<HumanResourceProfile>> _hrRepoMock = new();

        public AuthServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>()).Returns(_hrRepoMock.Object);

            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("FRPAMSystem");
            _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("SuperSecretKeyForJWTTokenGeneration1234567890!");
        }

        // UT148-TC45
        // Normal
        [Fact]
        public async Task LoginAsync_WithValidUsernameAndPassword_ShouldReturnTokenAndUserPayload()
        {
            // Arrange
            string plainPassword = "Password123!";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            var user = new User
            {
                UserId = 1,
                Username = "john_doe",
                Email = "john@example.com",
                FullName = "John Doe",
                PasswordHash = hashedPassword,
                RoleId = 2,
                Role = new Role { RoleId = 2, RoleName = "Researcher" }
            };

            _userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new HumanResourceProfile { HumanResourceId = 10, UserId = 1 });

            var request = new LoginRequest
            {
                UsernameOrEmail = "john_doe",
                Password = plainPassword
            };

            var service = new AuthService(_unitOfWorkMock.Object, _configurationMock.Object);

            // Act
            var result = await service.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal("john_doe", result.Username);
            Assert.Equal("Researcher", result.RoleName);
            Assert.Equal(10, result.HumanResourceId);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        }

        // UT148-TC46
        // Abnormal
        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ShouldThrowException()
        {
            // Arrange
            _userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((User?)null);

            var request = new LoginRequest
            {
                UsernameOrEmail = "nonexistent_user",
                Password = "anypassword"
            };

            var service = new AuthService(_unitOfWorkMock.Object, _configurationMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
            Assert.Equal("Invalid username/email or password.", ex.Message);
        }

        // UT148-TC47
        // Abnormal
        [Fact]
        public async Task LoginAsync_WhenPasswordIncorrect_ShouldThrowException()
        {
            // Arrange
            string correctPassword = "CorrectPassword123!";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

            var user = new User
            {
                UserId = 1,
                Username = "john_doe",
                PasswordHash = hashedPassword,
                Role = new Role { RoleId = 2, RoleName = "Researcher" }
            };

            _userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            var request = new LoginRequest
            {
                UsernameOrEmail = "john_doe",
                Password = "WrongPassword"
            };

            var service = new AuthService(_unitOfWorkMock.Object, _configurationMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
            Assert.Equal("Invalid username/email or password.", ex.Message);
        }
    }
}
