namespace OrderFlow.Api.Caching;

public static class OrderCacheKeys
{
    public static string ById(Guid id) => $"orders:{id:N}";
}