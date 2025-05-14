using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial6.Models;

namespace Tutorial6.Services;

public class ClientsService : IClientsService
{

    private readonly string? connectionString;
    public ClientsService(IConfiguration configuration)
    {
        connectionString = configuration["ConnectionString"];
    }


    public async Task<IList<object>> GetTripsForClient(int idClient, CancellationToken cancellationToken)
    {
        string query = @"SELECT * FROM Trip, Client_Trip 
                           Where Client_Trip.IdClient = @idClient and Trip.IdTrip = Client_Trip.IdTrip";

        IList<Object> trips = new List<Object>();
        await using var con = new SqlConnection(connectionString);
        await using var cmd = new SqlCommand(query);
        cmd.Connection = con;
        try
        {
            cmd.Parameters.AddWithValue("@idClient", idClient);
            await con.OpenAsync(cancellationToken);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var t = new
                {
                    ClientId = (int)reader["IdClient"],
                    IdTrip = (int)reader["IdTrip"],
                    Name = (string)reader["Name"],
                    Description = (string)reader["Description"],
                    DateFrom = (DateTime)reader["DateFrom"],
                    DateTo = (DateTime)reader["DateTo"],
                    MaxPeople = (int)reader["MaxPeople"],
                    RegisteredAt = (int)reader["RegisteredAt"],
                    //zabezpiezenie, przed wartością NULL
                    PaymentDate = reader["PaymentDate"] != DBNull.Value ? (int?)reader["PaymentDate"] : null
                };
                trips.Add(t);
            }


            return trips;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public async Task<decimal> AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        
        await using var con = new SqlConnection(connectionString);
        await using var cmd = new SqlCommand();

        try
        {
            string sql = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                        values (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                        Select SCOPE_IDENTITY();";
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);
            cmd.Connection = con;


            await con.OpenAsync(cancellationToken);

            decimal result = (decimal)await cmd.ExecuteScalarAsync(cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public Task<IActionResult> UpdateTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> DeleteTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}