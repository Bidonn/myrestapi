using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial6.Models;

namespace Tutorial6.Services;

public class TripsService : ITripsService
{


    private readonly string? connectionString;

    public TripsService(IConfiguration configuration)
    {
        connectionString = configuration["ConnectionString"];
    }

    public async Task<IList<Trip>> GetTrips(CancellationToken cancellationToken)
    {
        
        IList<Trip> trips = new List<Trip>();

        try
        {
            await using var con = new SqlConnection(connectionString);
            await using var cmd = new SqlCommand("select * from trip");
            cmd.Connection = con;

            await con.OpenAsync(cancellationToken);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
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
            return trips;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
           
            
    }

}