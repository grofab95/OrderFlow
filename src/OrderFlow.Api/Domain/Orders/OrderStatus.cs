namespace OrderFlow.Api.Domain.Orders;

public enum OrderStatus
{
    Unknown,
    Pending,
    Processing,
    Confirmed,
    Cancelled
}