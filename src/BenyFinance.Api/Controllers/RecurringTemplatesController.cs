using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BenyFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("recurring-templates")]
public class RecurringTemplatesController : ControllerBase
{
    private readonly IRecurringTemplateService _service;

    public RecurringTemplatesController(IRecurringTemplateService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return id != null ? Guid.Parse(id) : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _service.GetAllAsync(GetUserId());
        return Ok(templates);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringTemplateDto createDto)
    {
        try
        {
            var template = await _service.CreateAsync(GetUserId(), createDto);
            return Created("", template);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }
}
