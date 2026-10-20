using Microsoft.Data.Sqlite;

namespace CurrencyExchange.Infrastructure.Persistence;

/// <summary>
/// Una base SQLite en memoria se destruye cuando se cierra su última conexión.
/// Esta clase (registrada como singleton) mantiene una conexión abierta durante
/// toda la vida de la API para que los datos no se pierdan entre peticiones.
/// </summary>
public sealed class InMemoryDatabaseKeeper : IDisposable
{
    private readonly SqliteConnection _connection;

    public InMemoryDatabaseKeeper(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();
}
