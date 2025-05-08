using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial6.Models;

namespace Tutorial6.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly string? connectionString;

    public TripsController(IConfiguration configuration)
    {
        connectionString = configuration["ConnectionString"];
    }

    
    [HttpGet]
    public async Task<IActionResult> GetTrips(CancellationToken cancellationToken)
    {
        
        using var con = new SqlConnection(connectionString);
        
        try
        {
            using var cmd = new SqlCommand("select * from trip");
            cmd.Connection = con;

            await con.OpenAsync(cancellationToken);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            IList<Trip> trips = new List<Trip>();
            while (await reader.ReadAsync())
            {
                Trip t = new Trip()
                {
                    IdTrip = (int)reader["IdTrip"],
                    Name = (string)reader["Name"],
                    Description = (string)reader["Description"],
                    DateFrom = (DateTime)reader["DateFrom"],
                    DateTo = (DateTime)reader["DateTo"],
                    MaxPeople = (int)reader["MaxPeople"]
                };
                trips.Add(t);

            }

            return Ok(trips);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    
}