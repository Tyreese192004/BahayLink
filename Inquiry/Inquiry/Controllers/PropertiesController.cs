using Microsoft.AspNetCore.Mvc;
using Inquiry.DTOs;
using Inquiry.Services;

namespace Inquiry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(IPropertyService propertyService, ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    /// <summary>
    /// Kunin ang lahat ng available properties
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertyResponseDto>>> GetAll([FromQuery] bool includeDeleted = false)
    {
        var properties = await _propertyService.GetAllPropertiesAsync(includeDeleted);
        return Ok(properties);
    }

    /// <summary>
    /// Kunin ang isang property gamit ang ID
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PropertyResponseDto>> GetById(long id)
    {
        var property = await _propertyService.GetPropertyByIdAsync(id);
        if (property == null)
        {
            return NotFound(new { message = $"Property with ID {id} not found." });
        }
        return Ok(property);
    }

    /// <summary>
    /// Kunin ang lahat ng properties ng isang partikular na Owner
    /// </summary>
    [HttpGet("owner/{ownerId:long}")]
    public async Task<ActionResult<IEnumerable<PropertyResponseDto>>> GetByOwner(long ownerId)
    {
        var properties = await _propertyService.GetPropertiesByOwnerAsync(ownerId);
        return Ok(properties);
    }

    /// <summary>
    /// Lumikha at i-save ang bagong property sa database
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PropertyResponseDto>> Create([FromBody] CreatePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _propertyService.CreatePropertyAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property");
            return StatusCode(500, new { 
                message = "An error occurred while creating the property.", 
                error = ex.Message,
                inner = ex.InnerException?.Message 
            });
        }
    }

    /// <summary>
    /// I-update ang property
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PropertyResponseDto>> Update(long id, [FromBody] CreatePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _propertyService.UpdatePropertyAsync(id, dto);
        if (updated == null)
        {
            return NotFound(new { message = $"Property with ID {id} not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Soft delete property (temporary delete)
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> SoftDelete(long id)
    {
        var deleted = await _propertyService.SoftDeletePropertyAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Property with ID {id} not found." });
        }

        return Ok(new { message = "Property moved to trash." });
    }

    /// <summary>
    /// Restore property
    /// </summary>
    [HttpPost("{id:long}/restore")]
    public async Task<IActionResult> Restore(long id)
    {
        var restored = await _propertyService.RestorePropertyAsync(id);
        if (!restored)
        {
            return NotFound(new { message = $"Property with ID {id} not found." });
        }

        return Ok(new { message = "Property successfully restored." });
    }

    /// <summary>
    /// Permanent delete property
    /// </summary>
    [HttpDelete("{id:long}/permanent")]
    public async Task<IActionResult> PermanentDelete(long id)
    {
        var deleted = await _propertyService.PermanentDeletePropertyAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Property with ID {id} not found." });
        }

        return Ok(new { message = "Property permanently deleted." });
    }
}

