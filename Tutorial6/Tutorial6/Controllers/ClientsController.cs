using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial6.Models;

namespace Tutorial6.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly string? connectionString;

    public ClientsController(IConfiguration configuration)
    {
        connectionString = configuration["ConnectionString"];
    }



    /*[HttpGet]
    public async Task<IActionResult> GetClientsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var con = new SqlConnection(connectionString); // połączenie z bazą
            await using var cmd = new SqlCommand("select * from client"); // komenda sql

            cmd.Connection = con;

            await con.OpenAsync(cancellationToken);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            IList<Client> clients = new List<Client>();
            while (await reader.ReadAsync())
            {
                Client client = new Client()
                {
                    IdClient = (int)reader["IdClient"],
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    Email = (string)reader["Email"],
                    Telephone = (string)reader["Telephone"],
                    Pesel = (string)reader["Pesel"],
                };
                clients.Add(client);
            }

            return Ok(clients);
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
    }*/


    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTripsForClient(string id, CancellationToken cancellationToken)
    {
        if (!int.TryParse(id, out int idClient))
        {
            //jeśli nie id to nie int, błąd
            return BadRequest("400 Invalid syntax");
        }

        string query = @"SELECT * FROM Trip, Client_Trip 
                           Where Client_Trip.IdClient = @idClient and Trip.IdTrip = Client_Trip.IdTrip";

        await using var con = new SqlConnection(connectionString);
        await using var cmd = new SqlCommand(query);
        cmd.Connection = con;
        try
        {
            cmd.Parameters.AddWithValue("@idClient", idClient);

        

            await con.OpenAsync(cancellationToken);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            IList<Object> trips = new List<Object>();
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

            if (trips.Count == 0)
            {
                //jeśli nie ma wycieczek albo takiego użytkownika, notfound
                return NotFound("No trips for user or no such user exists.");
            }
            // wszystko git
            return Ok(trips);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

       
    }

    [HttpPost]
    public async Task<IActionResult> AddClientAsync(Client client, CancellationToken cancellationToken)
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

            //wszystko git, w przeciwnym razie asp sam wyśle badrequest, nie wiem jak to naprawić, znaczy wiem (chyba), ale za dużo roboty
            return Created("Client", result);
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