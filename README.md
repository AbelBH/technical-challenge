# API de Tipo de Cambio

Solución full stack para aplicar tipos de cambio a montos: **API REST en .NET 10** con Entity Framework Core, JWT y Clean Architecture, y un **front en Angular 22** que la consume. Todo se levanta con Docker en un solo comando.

| | |
|---|---|
| **Backend** | .NET 10 (LTS) · ASP.NET Core Web API · EF Core 10 · SQLite en memoria · FluentValidation · JWT |
| **Frontend** | Angular 22 · standalone + zoneless · signals · Reactive Forms · interceptors · lazy loading |
| **Infraestructura** | Docker (contenedores Linux) · docker compose · nginx como proxy inverso |
| **Pruebas** | Colección Postman con 31 aserciones · archivo `.http` para VS Code · tests unitarios Vitest |

---

## Inicio rápido (Docker)

Requisito: [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
docker compose up --build -d
```

| Servicio | URL |
|---|---|
| Front (Angular + nginx) | http://localhost:4200 |
| API · Swagger UI | http://localhost:8080/swagger |
| API · Health check | http://localhost:8080/health |

**Usuarios de demostración**

| Usuario | Contraseña | Rol | Permisos |
|---|---|---|---|
| `admin` | `Admin123*` | Admin | Consultar, convertir y administrar (crear / editar / eliminar) tipos de cambio |
| `user` | `User123*` | User | Consultar y convertir |

Para detener: `docker compose down`.

> La clave de firma JWT se toma de la variable `JWT_KEY` (archivo `.env`, ver [`.env.example`](.env.example)). Si no existe, se usa un valor de demostración para que el proyecto funcione sin configuración.

---

## Ejecución local (sin Docker)

Requisitos: .NET SDK 10 y Node.js 24 LTS.

```bash
# API → http://localhost:5062/swagger
cd backend
dotnet run --project src/CurrencyExchange.Api

# Front → http://localhost:4200 (en otra terminal)
cd frontend
npm install
npm start
```

---

## Funcionalidades

- **Conversión**: recibe `amount`, `sourceCurrency`, `targetCurrency` y devuelve `amount`, `convertedAmount` (monto con tipo de cambio), `sourceCurrency`, `targetCurrency` y `exchangeRate`.
- **CRUD de tipos de cambio**, con opción de crear también el tipo de cambio inverso en la **misma transacción**.
- **Seguridad JWT** con roles: lectura para cualquier usuario autenticado, escritura solo para Admin.
- **Middleware de excepciones** que responde todos los errores en formato estándar ProblemDetails (RFC 9457).
- **Front Angular** con login, interceptor que envía el JWT, guards, pantalla de conversión y CRUD según el rol.

### Endpoints

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | Público | Devuelve el JWT |
| GET | `/api/exchange-rates` | Autenticado | Lista los tipos de cambio |
| GET | `/api/exchange-rates/{id}` | Autenticado | Obtiene un tipo de cambio |
| POST | `/api/exchange-rates` | Admin | Crea (opcional `createInverse: true`) |
| PUT | `/api/exchange-rates/{id}` | Admin | Actualiza la tasa |
| DELETE | `/api/exchange-rates/{id}` | Admin | Elimina |
| POST | `/api/conversions` | Autenticado | Aplica el tipo de cambio a un monto |

**Ejemplo de conversión**

```http
POST /api/conversions
Authorization: Bearer <token>
Content-Type: application/json

{ "amount": 100, "sourceCurrency": "USD", "targetCurrency": "PEN" }
```

```json
{ "amount": 100, "convertedAmount": 375.00, "sourceCurrency": "USD", "targetCurrency": "PEN", "exchangeRate": 3.75 }
```

---

## Uso desde Postman o VS Code

**Postman**: importe los archivos de [`backend/postman/`](backend/postman/), seleccione el environment *Local* (`:5062`) o *Docker* (`:8080`) y ejecute **Run collection**. El login guarda el token automáticamente y cada request incluye tests: 18 requests y 31 aserciones que cubren el flujo feliz, la seguridad (401/403) y los errores (400/404/409).

Por consola, con la API en ejecución:

```bash
cd backend
npx newman run postman/CurrencyExchange.postman_collection.json -e postman/CurrencyExchange.Docker.postman_environment.json
```

**VS Code**: con la extensión *REST Client*, abra [`CurrencyExchange.Api.http`](backend/src/CurrencyExchange.Api/CurrencyExchange.Api.http) y ejecute primero *Login Admin*.

---

## Arquitectura

### Backend: Clean Architecture

```
backend/src/
├── CurrencyExchange.Domain/          Entidades y reglas de negocio. Sin dependencias.
├── CurrencyExchange.Application/     Casos de uso (servicios), DTOs, validadores e interfaces.
├── CurrencyExchange.Infrastructure/  EF Core, repositorios, Unit of Work, JWT y hash de contraseñas.
└── CurrencyExchange.Api/             Controllers, middleware, configuración y composición (DI).
```

Las dependencias apuntan hacia el dominio: `Api → Application → Domain` e `Infrastructure → Application`. Application define las interfaces (`IUnitOfWork`, `IJwtTokenGenerator`, `IPasswordHasher`) e Infrastructure las implementa, de modo que la lógica de negocio no depende de EF Core ni de librerías externas.

**Patrones utilizados**

| Patrón | Dónde | Para qué |
|---|---|---|
| Repository + Unit of Work | `Infrastructure/Persistence` | Encapsular el acceso a datos y controlar transacciones |
| Modelo de dominio rico | `Domain/Entities` | Las entidades validan sus invariantes (setters privados) |
| DTO + mapeo | `Application/DTOs`, `Mappings` | No exponer entidades en el contrato HTTP |
| Middleware | `Api/Middleware/ExceptionHandlingMiddleware` | Manejo centralizado de errores |
| Options pattern | `JwtSettings` | Configuración tipada y validada al arrancar |
| Inyección de dependencias | `AddApplication()`, `AddInfrastructure()` | Cada capa registra sus servicios |

**Manejo de errores**: los servicios lanzan excepciones de negocio y el middleware las traduce al código HTTP correspondiente, siempre en formato ProblemDetails con `traceId`:

| Excepción | HTTP |
|---|---|
| `ValidationException` (FluentValidation) | 400, con los errores por campo |
| `DomainException` | 400 |
| `UnauthorizedException` | 401 |
| `NotFoundException` | 404 |
| `ConflictException` (incluye violación del índice único) | 409 |
| Cualquier otra | 500, sin detalles internos fuera de Development |

### Frontend

```
frontend/src/app/
├── core/       auth (servicio, interceptor JWT, guards), interceptor de errores, servicios HTTP, modelos
├── layout/     estructura común (barra de navegación)
├── shared/     componentes reutilizables (notificaciones)
└── features/   login · converter · exchange-rates (página + formulario presentacional)
```

- `authInterceptor` agrega `Authorization: Bearer <token>` solo a las peticiones dirigidas a la API.
- `errorInterceptor` cierra la sesión ante un 401 (token expirado o inválido).
- La sesión se restaura al recargar la página y se cierra automáticamente cuando el token expira.
- La UI oculta las acciones de administración según el rol; la autorización real la aplica la API.

### Docker

```
Navegador ──► :4200  web (nginx) ──/api/*──► api:8080  (.NET)
          └─► :8080  api  (Swagger / Postman)
```

- Imágenes **multi-stage**: se compila con el SDK y solo el resultado pasa a la imagen de runtime.
- La API se ejecuta con un usuario sin privilegios (no root).
- nginx sirve el front y reenvía `/api` al contenedor de la API por la red interna de Docker. Al ser el mismo origen, no se necesita CORS.

---

## Decisiones técnicas

| Decisión | Justificación |
|---|---|
| **SQLite en memoria** en lugar del proveedor EF InMemory | El proveedor InMemory no soporta transacciones ni restricciones relacionales. SQLite en memoria es una base relacional real (transacciones, índices únicos) y sigue sin requerir instalación. Cambiar a SQL Server solo implica cambiar el proveedor y la cadena de conexión. |
| **Transacciones** solo al crear con inverso | Es la operación que hace varias escrituras que deben ser atómicas (se guardan ambas o ninguna). Update y Delete modifican un único registro, y un `SaveChanges` ya es atómico. |
| **Datos iniciales** al arrancar | Al ser una base en memoria, los datos se recrean en cada inicio: 6 tipos de cambio (USD/PEN/EUR) mediante `HasData`, y los usuarios por código, porque el hash de la contraseña usa un salt aleatorio. |
| **Contenedores Linux** (imagen oficial Debian) | El enunciado admite Windows o CentOS. CentOS Linux llegó a su fin de vida y Microsoft no publica imágenes .NET para esa distribución; las imágenes Linux oficiales son el estándar, más livianas que las de Windows y portables a cualquier host. |
| **Excepciones + middleware** para errores de negocio | Es lo que pide el enunciado y centraliza el manejo de errores. En un sistema de alto tráfico se podría usar el *Result pattern* para los errores esperados. |
| **Contraseñas** con el hasher de ASP.NET Core Identity | PBKDF2 con salt aleatorio, sin implementar criptografía propia. El login devuelve el mismo mensaje para usuario inexistente y contraseña incorrecta, para evitar la enumeración de usuarios. |
| **Configuración validada al arrancar** | Si la clave JWT falta o tiene menos de 32 caracteres, la API no inicia. |
| **Conversión de misma moneda** | Devuelve tipo de cambio 1 sin consultar la base. |
| **Códigos de moneda** | Se normalizan a mayúsculas (`usd` → `USD`) y deben tener 3 letras (ISO 4217). |
| **Límites** | Monto máximo de 1 000 000 000 000 y tasa máxima de 1 000 000 (6 decimales). Evitan el desborde de `decimal` en monto × tasa y garantizan que el tipo de cambio inverso sea representable. |

---

## Pruebas

```bash
# Front: tests unitarios (interceptor JWT, manejo de errores)
cd frontend && npx ng test --watch=false

# API: colección Postman completa (con la API en ejecución)
cd backend && npx newman run postman/CurrencyExchange.postman_collection.json -e postman/CurrencyExchange.Local.postman_environment.json
```
