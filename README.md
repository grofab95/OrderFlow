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

## Running locally

Requirements: .NET 10 SDK and Docker.

```bash
docker compose up -d
dotnet ef database update --project src/OrderFlow.Api
dotnet run --project src/OrderFlow.Api --launch-profile http
```

Swagger: [http://localhost:5173/swagger](http://localhost:5173/swagger)  
RabbitMQ Management: [http://localhost:15672](http://localhost:15672) (`guest` / `guest`)

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
