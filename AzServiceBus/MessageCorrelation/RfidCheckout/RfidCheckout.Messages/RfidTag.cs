namespace RfidCheckout.Messages;

public record RfidTag(string Product, double Price)
{
    public string TagId { get; init; } = Guid.NewGuid().ToString();
}