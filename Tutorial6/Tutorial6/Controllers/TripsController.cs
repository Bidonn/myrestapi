using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial6.Models;
using Tutorial6.Services;

namespace Tutorial6.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ITripsService _tripsService;

    public TripsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetTrips(CancellationToken cancellationToken)
    {
        try
        {
            var trips = await _tripsService.GetTrips(cancellationToken);
            return Ok(trips);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        
    }
    
    
}