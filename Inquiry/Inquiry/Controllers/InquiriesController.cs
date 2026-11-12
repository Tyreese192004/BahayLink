using Microsoft.AspNetCore.Mvc;
using Inquiry.DTOs;
using Inquiry.Services;

namespace Inquiry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InquiriesController : ControllerBase
{
    private readonly IInquiryService _inquiryService;
    private readonly ILogger<InquiriesController> _logger;

    public InquiriesController(IInquiryService inquiryService, ILogger<InquiriesController> logger)
    {
        _inquiryService = inquiryService;
        _logger = logger;
    }

    /// <summary>
    /// Kunin ang lahat ng inquiries/conversations mula sa database.
    /// Pwedeng i-filter via ?ownerId=1 o ?renterId=2.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InquiryResponseDto>>> GetAll([FromQuery] long? ownerId, [FromQuery] long? renterId)
    {
        var list = await _inquiryService.GetInquiriesAsync(ownerId, renterId);
        return Ok(list);
    }

    /// <summary>
    /// Kunin ang isang inquiry gamit ang ID
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<InquiryResponseDto>> GetById(long id)
    {
        var item = await _inquiryService.GetInquiryByIdAsync(id);
        if (item == null)
        {
            return NotFound(new { message = $"Inquiry #{id} not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Lumikha ng bagong inquiry at initial message sa database
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InquiryResponseDto>> Create([FromBody] CreateInquiryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _inquiryService.CreateInquiryAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.InquiryId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inquiry");
            return StatusCode(500, new { message = "An error occurred while creating inquiry.", error = ex.Message });
        }
    }

    /// <summary>
    /// Magpadala ng mensahe sa loob ng isang inquiry conversation
    /// </summary>
    [HttpPost("{id:long}/messages")]
    public async Task<ActionResult<MessageResponseDto>> SendMessage(long id, [FromBody] SendMessageDto dto)
    {
        dto.InquiryId = id;
        try
        {
            var msg = await _inquiryService.SendMessageAsync(dto);
            return Ok(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// I-update ang status ng inquiry (Pending, Ongoing, Resolved)
    /// </summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateInquiryStatusDto dto)
    {
        var ok = await _inquiryService.UpdateStatusAsync(id, dto.Status);
        if (!ok) return NotFound(new { message = $"Inquiry #{id} not found." });
        return Ok(new { message = "Status updated successfully.", status = dto.Status });
    }
}

