using System;
using Xunit;
using Afip.Dotnet.Abstractions.Models;

namespace Afip.Dotnet.UnitTests.Models
{
    public class AfipAuthTicketTests
    {
        [Fact]
        public void AfipAuthTicket_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var ticket = new AfipAuthTicket();

            // Assert
            Assert.Equal(string.Empty, ticket.Token);
            Assert.Equal(string.Empty, ticket.Sign);
            Assert.Equal(default(DateTime), ticket.GeneratedAt);
            Assert.Equal(default(DateTime), ticket.ExpiresAt);
            Assert.Equal(string.Empty, ticket.Service);
        }

        [Fact]
        public void IsValid_WhenTicketNotExpired_ShouldReturnTrue()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            // Act & Assert
            Assert.True(ticket.IsValid);
        }

        [Fact]
        public void IsValid_WhenTicketExpired_ShouldReturnFalse()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };

            // Act & Assert
            Assert.False(ticket.IsValid);
        }

        [Fact]
        public void IsValid_WhenTicketExpiresNow_ShouldReturnFalse()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.False(ticket.IsValid);
        }

        [Theory]
        [InlineData(5, true)]  // Expires in 5 minutes, check for 10 minutes
        [InlineData(15, false)] // Expires in 15 minutes, check for 10 minutes
        public void WillExpireSoon_WithDefaultMinutes_ShouldReturnExpectedResult(int expiresInMinutes, bool expectedResult)
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes)
            };

            // Act
            var result = ticket.WillExpireSoon();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(3, 5, true)]   // Expires in 3 minutes, check for 5 minutes
        [InlineData(7, 5, false)]  // Expires in 7 minutes, check for 5 minutes
        [InlineData(5, 5, true)]   // Expires in exactly 5 minutes, check for 5 minutes
        public void WillExpireSoon_WithCustomMinutes_ShouldReturnExpectedResult(int expiresInMinutes, int checkMinutes, bool expectedResult)
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes)
            };

            // Act
            var result = ticket.WillExpireSoon(checkMinutes);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void WillExpireSoon_WhenAlreadyExpired_ShouldReturnTrue()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };

            // Act
            var result = ticket.WillExpireSoon();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AfipAuthTicket_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var token = "sample_token_value";
            var sign = "sample_signature_value";
            var generatedAt = DateTime.UtcNow.AddMinutes(-5);
            var expiresAt = DateTime.UtcNow.AddHours(12);
            var service = "wsfe";

            // Act
            var ticket = new AfipAuthTicket
            {
                Token = token,
                Sign = sign,
                GeneratedAt = generatedAt,
                ExpiresAt = expiresAt,
                Service = service
            };

            // Assert
            Assert.Equal(token, ticket.Token);
            Assert.Equal(sign, ticket.Sign);
            Assert.Equal(generatedAt, ticket.GeneratedAt);
            Assert.Equal(expiresAt, ticket.ExpiresAt);
            Assert.Equal(service, ticket.Service);
        }

        [Fact]
        public void IsValid_EdgeCase_ExpiresInOneSecond_ShouldReturnTrue()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow.AddSeconds(1)
            };

            // Act & Assert
            Assert.True(ticket.IsValid);
        }

        [Fact]
        public void WillExpireSoon_EdgeCase_ZeroMinutes_ShouldCheckCurrentTime()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                ExpiresAt = DateTime.UtcNow.AddSeconds(30)
            };

            // Act
            var result = ticket.WillExpireSoon(0);

            // Assert
            Assert.False(result); // Should not expire "soon" if we're checking for 0 minutes
        }
    }
}