# OrderFlow

A small .NET 10 project for practicing asynchronous order processing, RabbitMQ messaging, and safe message redelivery.

## How it works

1. `POST /api/orders` saves an order with the `Pending` status and publishes `OrderCreated` through the transactional outbox.
2. The inventory consumer reserves stock and publishes either `InventoryReserved` or `InventoryReservationFailed`.
3. A successful reservation starts a simulated payment. `PaymentCompleted` changes the order status to `Confirmed`.
4. A failed reservation changes the order status to `Cancelled`.

Order reads use Redis with the cache-aside pattern. Inventory reservations are idempotent per `OrderId`.

## Stack

- .NET 10 / ASP.NET Core
- EF Core / PostgreSQL
- MassTransit / RabbitMQ
- Redis
- xUnit / NSubstitute
- Docker Compose

## Running with Docker

Requirements: Docker.

```bash
docker compose up --build
```

This starts PostgreSQL, Redis, RabbitMQ and the API. The API waits for the
dependencies to become healthy and applies EF Core migrations on startup.

- Swagger: [http://localhost:5173/swagger](http://localhost:5173/swagger)
- Liveness: [http://localhost:5173/health/live](http://localhost:5173/health/live)
- Readiness: [http://localhost:5173/health/ready](http://localhost:5173/health/ready)
- RabbitMQ Management: [http://localhost:15672](http://localhost:15672) (`guest` / `guest`)

Database data lives in the named `postgres_data` volume and survives
`docker compose down`. Use `docker compose down -v` to remove that volume and
drop the database.

## Running from the SDK

Requirements: .NET 10 SDK and Docker.

```bash
docker compose up -d postgres redis rabbitmq
dotnet run --project src/OrderFlow.Api --launch-profile http
```

Example request:

```http
POST http://localhost:5173/api/orders
Content-Type: application/json

{
  "items": [
    {
      "productId": "11111111-1111-1111-1111-111111111111",
      "quantity": 1
    }
  ]
}
```

## Tests

```bash
dotnet test OrderFlow.sln
```
