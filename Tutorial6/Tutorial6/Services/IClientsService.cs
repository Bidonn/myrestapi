using Microsoft.AspNetCore.Mvc;
using Tutorial6.Models;

namespace Tutorial6.Services;

public interface IClientsService
{
    Task<IList<Object>> GetTripsForClient(int id, CancellationToken cancellationToken);
    Task<decimal> AddClientAsync(Client client, CancellationToken cancellationToken);
    Task<int> UpdateTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken);
    Task<int> DeleteTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken);


}