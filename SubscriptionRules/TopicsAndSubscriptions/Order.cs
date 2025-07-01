namespace TopicsAndSubscriptions;

internal record Order
{
    public string Name { get; init; }

    public DateTime OrderDate { get; set; }

    public int Items { get; init; }

    public double Value { get; init; }

    public string Priority { get; init; }

    public string Region { get; init; }

    public bool HasLoyaltyCard { get; init; }
}