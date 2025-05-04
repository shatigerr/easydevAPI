using easydev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace easydev.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HomeController : ControllerBase
{
    private readonly PostgresContext _context;
    public HomeController(PostgresContext context) 
    {
        _context = context;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetHomeDashboard(long id)
    {
        var homeDashboard = new HomeDashboard();
        homeDashboard.project = _context.Projects.First(x => x.Id == id);
        homeDashboard.endpointsCount = _context.Endpoints.Count(x => x.IdProject == id);
        homeDashboard.database = _context.Databases.Where(x => x.Id == homeDashboard.project.Iddatabase).First();
        var logs = _context.Logs
            .Where(x => x.IdProject == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        homeDashboard.logs = logs;

        var lastRequestDate = logs.Any() ? logs.First().CreatedAt : (DateTime?)null;

        homeDashboard.apiStatus = true;
        homeDashboard.dbStatus = true;
        homeDashboard.appStatus = true;

        // ---------------------
        // Generar últimos 7 días
        // ---------------------

        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .Reverse()
            .ToList();

        homeDashboard.requestsPerDay = last7Days.Select(day => new RequestsPerDay
        {
            Day = day.ToString("dddd"),
            Requests = logs.Count(log => log.CreatedAt.Date == day)
        }).ToList();

        // ----------------------
        // Calcular tiempo transcurrido
        // ----------------------

        homeDashboard.lastRequest = lastRequestDate == null
            ? "Sin ejecuciones"
            : GetTimeAgo(lastRequestDate.Value);

        return Ok(homeDashboard);
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var diff = DateTime.UtcNow - dateTime;

        if (diff.TotalSeconds < 60)
            return "Hace unos segundos";

        if (diff.TotalMinutes < 60)
            return $"Hace {Math.Floor(diff.TotalMinutes)} minuto(s)";

        if (diff.TotalHours < 24)
            return $"Hace {Math.Floor(diff.TotalHours)} hora(s)";

        if (diff.TotalDays < 7)
            return $"Hace {Math.Floor(diff.TotalDays)} día(s)";

        return "Hace mucho tiempo";
    }
}