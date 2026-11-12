using Microsoft.AspNetCore.Mvc;
using Inquiry.DTOs;
using Inquiry.Services;

namespace Inquiry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IInquiryService _inquiryService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IInquiryService inquiryService, ILogger<MessagesController> logger)
    {
        _inquiryService = inquiryService;
        _logger = logger;
    }

    /// <summary>
    /// Mag-save ng bagong mensahe sa database
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MessageResponseDto>> Send([FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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
}

