using TopicsAndSubscriptions;

internal static class TestOrders
{
    public static List<Order> CreateTestOrders()
    {
        return
        [
            new()
            {
                Name = "Loyal Customer",
                Value = 19.99,
                Region = "USA",
                Items = 1,
                HasLoyaltyCard = true,
                OrderDate = DateTime.Now,
                Priority = "High"
            },

            new()
            {
                Name = "Large Order",
                Value = 49.99,
                Region = "USA",
                Items = 50,
                HasLoyaltyCard = false,
                OrderDate = DateTime.Now,
                Priority = "Medium"
            },

            new()
            {
                Name = "High Value",
                Value = 749.45,
                Region = "USA",
                Items = 45,
                HasLoyaltyCard = false
            },

            new()
            {
                Name = "Loyal Europe",
                Value = 49.45,
                Region = "EU",
                Items = 3,
                HasLoyaltyCard = true
            },

            new()
            {
                Name = "UK Order",
                Value = 49.45,
                Region = "UK",
                Items = 3,
                HasLoyaltyCard = false
            }
        ];
    }

}