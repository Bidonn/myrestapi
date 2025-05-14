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
        int result;
        try
        {
            result = await _clients.UpdateTripAsync(IdClient, IdTrip, cancellationToken);
            if(result == 0)
                return NotFound("Client not found");
            if (result == 1)
                return NotFound("Trip not found");
            if (result == 2)
                return BadRequest("Max people limit reached");
            if (result == 3)
                return Ok("Trip updated");
            
            return BadRequest("Something went wrong");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{IdClient}/trips/{IdTrip}")]
    public async Task<IActionResult> DeleteTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken)
    {
       int result;
       try
       {
           result = await _clients.DeleteTripAsync(IdClient, IdTrip, cancellationToken);
           if(result == 0)
               return NotFound("No such client on trip was found.");
           else if (result == 1)
               return Ok("Trip deleted");
           else
           {
               return BadRequest();
           }
            
       }
       catch (Exception e)
       {
           return BadRequest(e.Message);
       }
    }
    
}