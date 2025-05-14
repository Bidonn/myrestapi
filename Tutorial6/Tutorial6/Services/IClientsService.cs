using Microsoft.AspNetCore.Mvc;
using Tutorial6.Models;

namespace Tutorial6.Services;

public interface IClientsService
{
    Task<IActionResult> GetTripsForClient(string id, CancellationToken cancellationToken);
    Task<IActionResult> AddClientAsync(Client client, CancellationToken cancellationToken);
    Task<IActionResult> UpdateTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken);
    Task<IActionResult> DeleteTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken);


}