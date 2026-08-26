# OrderFlow

Mały projekt w .NET 10 do przećwiczenia asynchronicznej obsługi zamówień, komunikacji przez RabbitMQ i odporności na ponowne dostarczenie wiadomości.

## Jak działa

1. `POST /api/orders` zapisuje zamówienie ze statusem `Pending` i publikuje `OrderCreated` przez transactional outbox.
2. Konsument rezerwuje produkty i publikuje `InventoryReserved` albo `InventoryReservationFailed`.
3. Udana rezerwacja uruchamia symulację płatności. `PaymentCompleted` zmienia status zamówienia na `Confirmed`.
4. Nieudana rezerwacja zmienia status na `Cancelled`.

Odczyt zamówienia korzysta z Redis w wariancie cache-aside. Rezerwacja magazynowa jest idempotentna względem `OrderId`.

## Stack

- .NET 10 / ASP.NET Core
- EF Core / PostgreSQL
- MassTransit / RabbitMQ
- Redis
- xUnit / NSubstitute
- Docker Compose

## Uruchomienie

Wymagane: .NET 10 SDK i Docker.

```bash
docker compose up -d
dotnet ef database update --project src/OrderFlow.Api
dotnet run --project src/OrderFlow.Api --launch-profile http
```

Swagger: [http://localhost:5173/swagger](http://localhost:5173/swagger)  
RabbitMQ Management: [http://localhost:15672](http://localhost:15672) (`guest` / `guest`)

Przykładowe zamówienie:

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

## Testy

```bash
dotnet test OrderFlow.sln
```
