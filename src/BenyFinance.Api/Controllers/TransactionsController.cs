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
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    private readonly IRecurringTemplateService _recurringService;

    public TransactionsController(ITransactionService service, IRecurringTemplateService recurringService)
    {
        _service = service;
        _recurringService = recurringService;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return id != null ? Guid.Parse(id) : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? type)
    {
        var transactions = await _service.GetAllAsync(GetUserId(), month, year, type);
        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transaction = await _service.GetByIdAsync(id, GetUserId());
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto createDto)
    {
        try
        {
            var transaction = await _service.CreateAsync(GetUserId(), createDto);
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionDto updateDto)
    {
        var transaction = await _service.UpdateAsync(id, GetUserId(), updateDto);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }
    
    [HttpPost("generate-monthly")]
    public async Task<IActionResult> GenerateMonthly([FromBody] GenerateMonthlyDto dto)
    {
        var transactions = await _recurringService.GenerateMonthlyTransactionsAsync(GetUserId(), dto.Month, dto.Year);
        return Created("", transactions);
    }
}

public record GenerateMonthlyDto(int Month, int Year);
