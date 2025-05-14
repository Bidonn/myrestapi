using Microsoft.AspNetCore.Mvc;
using Tutorial6.Models;

namespace Tutorial6.Services;

public interface IClientsService
{
    Task<IList<Object>> GetTripsForClient(int id, CancellationToken cancellationToken);
    Task<decimal> AddClientAsync(Client client, CancellationToken cancellationToken);
    Task<IActionResult> UpdateTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken);
    Task<IActionResult> DeleteTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken);


}