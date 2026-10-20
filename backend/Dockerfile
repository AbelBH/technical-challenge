# ---------- Etapa 1: build (imagen con el SDK completo, ~800 MB) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: si no cambian, Docker reutiliza la capa del restore (builds más rápidos).
COPY src/CurrencyExchange.Domain/CurrencyExchange.Domain.csproj src/CurrencyExchange.Domain/
COPY src/CurrencyExchange.Application/CurrencyExchange.Application.csproj src/CurrencyExchange.Application/
COPY src/CurrencyExchange.Infrastructure/CurrencyExchange.Infrastructure.csproj src/CurrencyExchange.Infrastructure/
COPY src/CurrencyExchange.Api/CurrencyExchange.Api.csproj src/CurrencyExchange.Api/
RUN dotnet restore src/CurrencyExchange.Api/CurrencyExchange.Api.csproj

# Luego el código fuente y la publicación en Release.
COPY src/ src/
RUN dotnet publish src/CurrencyExchange.Api/CurrencyExchange.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: runtime (solo ASP.NET Core, imagen mucho más liviana) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

# Se ejecuta con el usuario sin privilegios que trae la imagen oficial (no como root).
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "CurrencyExchange.Api.dll"]
