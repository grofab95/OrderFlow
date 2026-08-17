namespace OrderFlow.Api.Features.Products;

public class ProductNotFoundException(Guid[] missingProductIds)
    : Exception($"Products not found: {string.Join(", ", missingProductIds)}.")
{
    public Guid[] MissingProductIds { get; } = missingProductIds;
}
