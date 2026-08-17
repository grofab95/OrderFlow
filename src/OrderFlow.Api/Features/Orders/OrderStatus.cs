namespace OrderFlow.Api.Features.Orders;

public enum OrderStatus
{
    Unknown,
    Pending,
    Processing,
    Confirmed,
    Cancelled
}