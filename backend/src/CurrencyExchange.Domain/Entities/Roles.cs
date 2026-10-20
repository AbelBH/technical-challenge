namespace CurrencyExchange.Domain.Entities;

public static class Roles
{
    /// <summary>Puede crear, actualizar y eliminar tipos de cambio.</summary>
    public const string Admin = "Admin";

    /// <summary>Solo puede consultar tipos de cambio y convertir montos.</summary>
    public const string User = "User";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Admin, User };
}
