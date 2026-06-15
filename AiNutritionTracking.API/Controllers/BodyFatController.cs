using AiNutritionTracking.API.DTOs.AI;
using AiNutritionTracking.API.Services;
using AiNutritionTracking.API.Validators.AI;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AiNutritionTracking.API.Controllers;

/// <summary>
/// AI Body Fat Analysis — estimate body fat percentage from photos or body measurements.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
[EnableRateLimiting("ai-policy")]
[Produces("application/json")]
public class BodyFatController : ControllerBase
{
    private readonly IBodyFatAnalysisService _bodyFatService;
    private readonly IValidator<BodyFatImageRequestDto> _imageValidator;
    private readonly IValidator<BodyFatMeasurementRequestDto> _measurementValidator;
    private readonly ILogger<BodyFatController> _logger;

    public BodyFatController(
        IBodyFatAnalysisService bodyFatService,
        IValidator<BodyFatImageRequestDto> imageValidator,
        IValidator<BodyFatMeasurementRequestDto> measurementValidator,
        ILogger<BodyFatController> logger)
    {
        _bodyFatService       = bodyFatService;
        _imageValidator       = imageValidator;
        _measurementValidator = measurementValidator;
        _logger               = logger;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst("id")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    private IActionResult ValidationFailed(FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return BadRequest(new { success = false, errors });
    }

    // ─── Photo-based analysis ────────────────────────────────────────────────

    /// <summary>Analyze body fat percentage from 1–3 body photos.</summary>
    /// <remarks>
    /// Upload 1 to 3 body photos together with basic measurements.
    /// Each image is analyzed separately by the AI vision model; results are averaged for accuracy.
    ///
    ///     POST /api/ai/analyze-body-fat
    ///     Content-Type: multipart/form-data
    ///     images: &lt;file1&gt;, &lt;file2&gt;, ...
    ///     gender: Male
    ///     age: 22
    ///     height: 175
    ///     weight: 75
    ///
    /// </remarks>
    /// <response code="200">Body fat analysis result.</response>
    /// <response code="400">Validation error or missing images.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("analyze-body-fat", Name = "AnalyzeBodyFatFromImages")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BodyFatAnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AnalyzeBodyFatFromImages([FromForm] BodyFatImageRequestDto request)
    {
        var validation = await _imageValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationFailed(validation);

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };

        foreach (var img in request.Images)
        {
            if (!allowedTypes.Contains(img.ContentType?.ToLower()))
                return BadRequest(new { success = false, message = $"{img.FileName}: only JPEG, PNG, or WEBP images are accepted." });

            if (img.Length > 10 * 1024 * 1024)
                return BadRequest(new { success = false, message = $"{img.FileName}: image must not exceed 10 MB." });
        }

        var userId = GetUserId();
        _logger.LogInformation("Body fat photo analysis for userId={UserId}", userId);

        try
        {
            var result = await _bodyFatService.AnalyzeFromImagesAsync(request, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(502, new { success = false, message = ex.Message });
        }
    }

    // ─── Measurement-based analysis ──────────────────────────────────────────

    /// <summary>Estimate body fat percentage using the US Navy formula from body measurements.</summary>
    /// <remarks>
    /// Provide waist, neck (and hip for female) measurements.
    /// Body fat is calculated using the US Navy formula — no photos required.
    ///
    ///     POST /api/ai/analyze-body-fat
    ///     Content-Type: application/json
    ///     {
    ///       "gender": "Male",
    ///       "age": 22,
    ///       "height": 175,
    ///       "weight": 75,
    ///       "waist": 84,
    ///       "neck": 38
    ///     }
    ///
    /// For female, include "hip" measurement (required by the US Navy formula).
    ///
    /// Body fat categories — Male: Essential Fat (2-5%), Athlete (6-13%), Fitness (14-17%), Average (18-24%), Obese (25%+)
    /// Body fat categories — Female: Essential Fat (10-13%), Athlete (14-20%), Fitness (21-24%), Average (25-31%), Obese (32%+)
    /// </remarks>
    /// <response code="200">Body fat analysis result.</response>
    /// <response code="400">Validation error or invalid measurements.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("analyze-body-fat", Name = "AnalyzeBodyFatFromMeasurements")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BodyFatAnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AnalyzeBodyFatFromMeasurements([FromBody] BodyFatMeasurementRequestDto request)
    {
        var validation = await _measurementValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationFailed(validation);

        var userId = GetUserId();
        _logger.LogInformation("Body fat measurement analysis for userId={UserId}", userId);

        var result = await _bodyFatService.AnalyzeFromMeasurementsAsync(request, userId);
        return Ok(result);
    }
}
