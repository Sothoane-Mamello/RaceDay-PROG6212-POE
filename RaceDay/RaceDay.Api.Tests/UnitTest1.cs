
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class ValidationTests
    {
        private static bool IsValid(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(
                model, context, results, true);
        }

        [Fact]
        public void Register_RejectsInvalidEmail()
        {
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "invalid-email",
                Password = "TestPassword123!",
                Role = "Participant"
            };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Register_RejectsShortPassword()
        {
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "123",
                Role = "Participant"
            };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Register_AcceptsValidInput()
        {
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "TestPassword123!",
                Role = "Participant"
            };

            Assert.True(IsValid(dto));
        }

        [Fact]
        public void Event_RejectsNegativeDistance()
        {
            var dto = new EventDto
            {
                Name = "Test Race",
                Description = "A test running event",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Johannesburg",
                DistanceKm = -5,
                EventType = "Run"
            };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Event_RejectsInvalidEventType()
        {
            var dto = new EventDto
            {
                Name = "Test Race",
                Description = "A test running event",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Johannesburg",
                DistanceKm = 10,
                EventType = "Swimming"
            };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Event_AcceptsValidInput()
        {
            var dto = new EventDto
            {
                Name = "Test Race",
                Description = "A test running event",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Johannesburg",
                DistanceKm = 10,
                EventType = "Run"
            };

            Assert.True(IsValid(dto));
        }

        [Fact]
        public void Enrolment_RejectsInvalidEventId()
        {
            var dto = new EnrolmentDto
            {
                EventId = 0,
                CategoryId = 1
            };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Enrolment_RejectsInvalidCategoryId()
        {
            var dto = new EnrolmentDto
            {
                EventId = 2,
                CategoryId = 0
            };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Result_RejectsInvalidFinishTime()
        {
            var dto = new ResultDto
            {
                EnrolmentId = 1,
                FinishTimeSeconds = -100,
                FinishingPosition = 1
            };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Result_AcceptsValidInput()
        {
            var dto = new ResultDto
            {
                EnrolmentId = 1,
                FinishTimeSeconds = 3600,
                FinishingPosition = 1
            };

            Assert.True(IsValid(dto));
        }
    }
}
