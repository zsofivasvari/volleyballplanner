using VolleyPlanner.API.DTOs.Generation;

namespace VolleyPlanner.API.Interfaces;

public interface IGenerationService
{
    Task<GeneratedTrainingPlanResponseDto> GenerateAsync(GenerateTrainingPlanRequestDto request);
}