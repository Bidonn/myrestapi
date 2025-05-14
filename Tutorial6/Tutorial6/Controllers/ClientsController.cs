using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Tutorial6.Models;
using Tutorial6.Services;

namespace Tutorial6.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clients;

    public ClientsController(IClientsService clients)
    {
        _clients = clients;
    }



 


    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTripsForClient(int id, CancellationToken cancellationToken)
    {
        try
        {
            var clients =  await _clients.GetTripsForClient(id, cancellationToken);
            if (clients.IsNullOrEmpty())
            {
                return NotFound();
            }
            return Ok(clients);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        decimal result;
        try
        {
            result = await _clients.AddClientAsync(client, cancellationToken);
            return Ok(result+" Client added");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{IdClient}/trips/{IdTrip}")]
    public async Task<IActionResult> UpdateTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken)
    {
        try
        {
            await using var con = new SqlConnection(connectionString);
            await using var cmd = new SqlCommand();
            cmd.Connection = con;
            await con.OpenAsync(cancellationToken);
            
            string sql = @"SELECT 1 FROM Client WHERE IdClient = @IdClient;";
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@IdClient", IdClient);
            cmd.Parameters.AddWithValue("@IdTrip", IdTrip);
            cmd.CommandText = sql;
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            //jeśli nie ma takiego klienta
            if(result is null)
                return NotFound("Client not found.");

            sql = @"SELECT 1 FROM TRIP WHERE IdTrip = @IdTrip;";
            cmd.CommandText = sql;
            
            result = await cmd.ExecuteScalarAsync(cancellationToken);
            //jeśli nie ma takiej wycieczki
            if (result is null)
                return NotFound("Trip not found.");
            
            
            sql = @"SELECT MaxPeople FROM TRIP WHERE IdTrip = @IdTrip;";
            cmd.CommandText = sql;
            int maxPeople = (int)await cmd.ExecuteScalarAsync(cancellationToken);
            
            sql = @"SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip;";
            cmd.CommandText = sql;
            int currentPeople = (int)await cmd.ExecuteScalarAsync(cancellationToken);
            //jeśli max ludzi < obecnie+1
            if (maxPeople < currentPeople + 1)
                return Conflict("Maximum number of people on trip.");
            
            sql = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@IdClient, @IdTrip, @RegisteredAt);";

            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@RegisteredAt", DateTime.Now.Year*10^4+DateTime.Now.Month*10^2+DateTime.Now.Day);

            int affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
            
            //jeśli zmienilismy 1 wiersz lub błąd
            if (affected == 1)
                return Ok("All good");
            else
                return BadRequest("No good.");

        }
        catch (Exception e)
        {
            //jesli o czymś zapomniałem
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{IdClient}/trips/{IdTrip}")]
    public async Task<IActionResult> DeleteTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken)
    {
        await using var con = new SqlConnection(connectionString);
        await using var cmd = new SqlCommand();

        try
        {
            string sql = @"SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip;";
            cmd.CommandText = sql;
            cmd.Connection = con;
        
            cmd.Parameters.AddWithValue("@IdClient", IdClient);
            cmd.Parameters.AddWithValue("@IdTrip", IdTrip);
        
            await con.OpenAsync(cancellationToken);
        
            int? res = (int?)await cmd.ExecuteScalarAsync(cancellationToken);
            //nie znaleziono klienta lub wycieczki
            if (res is null)
                return NotFound("No client on such trip was found.");
        
            sql = @"DELETE FROM Client_Trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient;";
            cmd.CommandText = sql;
            int affected = (int)await cmd.ExecuteNonQueryAsync(cancellationToken);
        
            //wszystko g -> wyślij info
            return Ok($"Affected {affected} trip(s).");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
       
    }
    
}