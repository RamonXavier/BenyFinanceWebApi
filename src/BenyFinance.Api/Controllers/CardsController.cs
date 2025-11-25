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
[Route("cards")]
public class CardsController : ControllerBase
{
    private readonly ICreditCardService _service;

    public CardsController(ICreditCardService service)
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
        var cards = await _service.GetAllAsync(GetUserId());
        return Ok(cards);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreditCardDto createDto)
    {
        var card = await _service.CreateAsync(GetUserId(), createDto);
        return Created("", card);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }
}
