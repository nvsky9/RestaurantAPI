using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class MinimumAgeRequirmentHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        private readonly ILogger<MinimumAgeRequirement> _logger;

        public MinimumAgeRequirmentHandler(ILogger<MinimumAgeRequirement> logger)
        {
            _logger = logger;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
           var dateOfBirth = DateTime.Parse(context.User.FindFirst( c => c.Type == "DateOfBirth").Value);

            var userEmail = context.User.FindFirst(c => c.Type == ClaimTypes.Name).Value;

            _logger.LogInformation($"User: {userEmail} with date of birth [{dateOfBirth}]");

            if (dateOfBirth.AddYears(requirement.MinimumAge) < DateTime.Now)
            {
                _logger.LogInformation("Authentication succedded");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation("Authentication failed");

            }

            return Task.CompletedTask;
        }
    }
}
