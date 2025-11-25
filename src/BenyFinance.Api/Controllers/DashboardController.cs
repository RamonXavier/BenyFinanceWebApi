using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BenyFinance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BenyFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return id != null ? Guid.Parse(id) : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData([FromQuery] int month, [FromQuery] int year)
    {
        var data = await _service.GetDashboardDataAsync(GetUserId(), month, year);
        return Ok(data);
    }
}
