using CurrencyExchange.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CurrencyExchange.Infrastructure.Persistence.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceCurrency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.TargetCurrency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.Rate).IsRequired().HasPrecision(18, 6);
        // SQLite guarda las fechas como texto sin zona horaria; al leerlas se marcan como UTC.
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Un solo tipo de cambio por par de monedas.
        builder.HasIndex(x => new { x.SourceCurrency, x.TargetCurrency }).IsUnique();

        // Datos iniciales (seed). Se usan objetos anónimos porque la entidad tiene setters privados.
        var seedDate = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new { Id = 1, SourceCurrency = "USD", TargetCurrency = "PEN", Rate = 3.75m, UpdatedAt = seedDate },
            new { Id = 2, SourceCurrency = "PEN", TargetCurrency = "USD", Rate = 0.2667m, UpdatedAt = seedDate },
            new { Id = 3, SourceCurrency = "EUR", TargetCurrency = "PEN", Rate = 4.05m, UpdatedAt = seedDate },
            new { Id = 4, SourceCurrency = "PEN", TargetCurrency = "EUR", Rate = 0.2469m, UpdatedAt = seedDate },
            new { Id = 5, SourceCurrency = "EUR", TargetCurrency = "USD", Rate = 1.08m, UpdatedAt = seedDate },
            new { Id = 6, SourceCurrency = "USD", TargetCurrency = "EUR", Rate = 0.9259m, UpdatedAt = seedDate });
    }
}
