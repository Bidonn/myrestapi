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

    public async Task<int> UpdateTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken)
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
                return 0;

            sql = @"SELECT 1 FROM TRIP WHERE IdTrip = @IdTrip;";
            cmd.CommandText = sql;
            
            result = await cmd.ExecuteScalarAsync(cancellationToken);
            //jeśli nie ma takiej wycieczki
            if (result is null)
                return 1;
            
            
            sql = @"SELECT MaxPeople FROM TRIP WHERE IdTrip = @IdTrip;";
            cmd.CommandText = sql;
            int maxPeople = (int)await cmd.ExecuteScalarAsync(cancellationToken);
            
            sql = @"SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip;";
            cmd.CommandText = sql;
            int currentPeople = (int)await cmd.ExecuteScalarAsync(cancellationToken);
            //jeśli max ludzi < obecnie+1
            if (maxPeople < currentPeople + 1)
                return 2;
            
            sql = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@IdClient, @IdTrip, @RegisteredAt);";

            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@RegisteredAt", DateTime.Now.Year*10^4+DateTime.Now.Month*10^2+DateTime.Now.Day);

            int affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
            
            //jeśli zmienilismy 1 wiersz lub błąd
            if (affected == 1)
                return 3;
            else
                return 4;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public async Task<int> DeleteTripAsync(int IdClient, int IdTrip, CancellationToken cancellationToken)
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
                return 0;
                
        
            sql = @"DELETE FROM Client_Trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient;";
            cmd.CommandText = sql;
            int affected = (int)await cmd.ExecuteNonQueryAsync(cancellationToken);
        
            //wszystko g -> wyślij info
            return affected;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

    }
}