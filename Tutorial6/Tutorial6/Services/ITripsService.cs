using Microsoft.AspNetCore.Mvc;
using Tutorial6.Models;

namespace Tutorial6.Services;

public interface ITripsService
{
    Task<IList<Trip>>  GetTrips(CancellationToken cancellationToken);
}