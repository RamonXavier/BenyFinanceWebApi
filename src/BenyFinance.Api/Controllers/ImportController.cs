using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BenyFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("import")]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return id != null ? Guid.Parse(id) : Guid.Empty;
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(IFormFile file, [FromQuery] string source = "nubank")
    {
        try
        {
            var preview = await _importService.PreviewAsync(GetUserId(), file, source);
            return Ok(preview);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ImportConfirmDto confirmDto)
    {
        try
        {
            var count = await _importService.ConfirmAsync(GetUserId(), confirmDto);
            return Ok(new { imported = count });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
