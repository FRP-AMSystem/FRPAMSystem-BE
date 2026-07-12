using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.BusinessTier.AI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AllocationOptimizationsController : ControllerBase
    {
        private readonly IAllocationOptimizationService _allocationOptimizationService;

        public AllocationOptimizationsController(
            IAllocationOptimizationService allocationOptimizationService)
        {
            _allocationOptimizationService = allocationOptimizationService;
        }

        [HttpPost("experiments/{experimentId:int}/suggestions")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> GenerateSuggestions(
            int experimentId,
            [FromBody] OptimizationSettings? settings)
        {
            var result = await _allocationOptimizationService
                .GenerateTopSuggestionsAsync(experimentId, settings);

            return Ok(new
            {
                success = true,
                message = "Generate AI allocation suggestions successfully",
                data = result
            });
        }
    }
}
