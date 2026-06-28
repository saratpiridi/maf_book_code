using System.ComponentModel;
using System.Globalization;

namespace MasteringAgentFramework.Ch04.RetailReturns;

internal static class ReturnsTools
{
    [Description("Looks up an order by order id and returns a readable summary of its items. Read-only.")]
    public static string LookUpOrder(
        [Description("The order id to look up, such as ORD-1001.")] string orderId)
    {
        Order? order = ReturnsData.FindOrder(orderId);

        if (order is null)
        {
            return $"Order '{orderId}' was not found.";
        }

        string[] itemLines = order.Items
            .Select(item => $"- {item.ItemId}: {item.Name}, category: {item.Category}, purchase date: {item.PurchaseDate:yyyy-MM-dd}, price: {item.Price.ToString("C", CultureInfo.InvariantCulture)}{(item.IsFinalSale ? ", final sale" : string.Empty)}")
            .ToArray();

        return $"Order {order.OrderId} contains:{Environment.NewLine}{string.Join(Environment.NewLine, itemLines)}";
    }

    [Description("Checks whether an item from an order is eligible for return. Read-only.")]
    public static string CheckReturnEligibility(
        [Description("The order id containing the item, such as ORD-1001.")] string orderId,
        [Description("The item id to check, such as ITM-02.")] string itemId)
    {
        EligibilityResult result = ReturnsData.CheckEligibility(orderId, itemId);
        return result.Message;
    }

    [Description("Creates a return request for an eligible item and returns a confirmation id. State-changing.")]
    public static string CreateReturnRequest(
        [Description("The order id containing the item, such as ORD-1001.")] string orderId,
        [Description("The item id to return, such as ITM-02.")] string itemId,
        [Description("The customer-provided reason for the return.")] string reason)
    {
        EligibilityResult eligibility = ReturnsData.CheckEligibility(orderId, itemId);

        if (!eligibility.IsEligible)
        {
            return $"Return request not created. {eligibility.Message}";
        }

        ReturnRequest request = ReturnsData.CreateReturn(orderId, itemId, reason);
        return $"Return request {request.ReturnId} created for order {orderId}, item {itemId}. Reason: {reason}";
    }
}

internal static class ReturnsData
{
    private static readonly List<ReturnRequest> ReturnRequests = [];
    private static int _nextReturnNumber = 5001;

    private static readonly IReadOnlyList<Order> Orders =
    [
        new(
            "ORD-1001",
            [
                new("ITM-01", "wireless headphones", "electronics", DateOnly.FromDateTime(DateTime.Today).AddDays(-12), 79.99m, false),
                new("ITM-02", "cotton pullover", "apparel", DateOnly.FromDateTime(DateTime.Today).AddDays(-10), 34.50m, false),
                new("ITM-03", "ceramic mug set", "home", DateOnly.FromDateTime(DateTime.Today).AddDays(-10), 22.00m, false)
            ]),
        new(
            "ORD-1002",
            [
                new("ITM-04", "desk lamp", "home", DateOnly.FromDateTime(DateTime.Today).AddDays(-42), 48.00m, false),
                new("ITM-05", "notebook pack", "office", DateOnly.FromDateTime(DateTime.Today).AddDays(-42), 12.99m, false)
            ]),
        new(
            "ORD-1003",
            [
                new("ITM-06", "travel pouch", "accessories", DateOnly.FromDateTime(DateTime.Today).AddDays(-5), 18.75m, false),
                new("ITM-07", "clearance scarf", "final-sale", DateOnly.FromDateTime(DateTime.Today).AddDays(-5), 15.00m, true)
            ]),
        new(
            "ORD-1004",
            [
                new("ITM-08", "water bottle", "outdoor", DateOnly.FromDateTime(DateTime.Today).AddDays(-28), 24.25m, false),
                new("ITM-09", "storage basket", "home", DateOnly.FromDateTime(DateTime.Today).AddDays(-31), 19.99m, false)
            ])
    ];

    public static Order? FindOrder(string orderId)
    {
        return Orders.FirstOrDefault(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
    }

    public static EligibilityResult CheckEligibility(string orderId, string itemId)
    {
        Order? order = FindOrder(orderId);

        if (order is null)
        {
            return new(false, $"Order '{orderId}' was not found.");
        }

        OrderItem? item = order.Items.FirstOrDefault(candidate => string.Equals(candidate.ItemId, itemId, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            return new(false, $"Item '{itemId}' was not found in order {order.OrderId}.");
        }

        if (item.IsFinalSale)
        {
            return new(false, $"Item {item.ItemId} ({item.Name}) is final sale and is not returnable.");
        }

        int daysSincePurchase = DateOnly.FromDateTime(DateTime.Today).DayNumber - item.PurchaseDate.DayNumber;

        if (daysSincePurchase > 30)
        {
            return new(false, $"Item {item.ItemId} ({item.Name}) is outside the 30-day return window. It was purchased {daysSincePurchase} days ago.");
        }

        return new(true, $"Item {item.ItemId} ({item.Name}) is eligible for return because it is within the 30-day return window.");
    }

    public static ReturnRequest CreateReturn(string orderId, string itemId, string reason)
    {
        string returnId = $"RET-{_nextReturnNumber++}";
        ReturnRequest request = new(returnId, orderId, itemId, reason, DateTimeOffset.UtcNow);
        ReturnRequests.Add(request);
        return request;
    }
}

internal sealed record Order(string OrderId, IReadOnlyList<OrderItem> Items);

internal sealed record OrderItem(
    string ItemId,
    string Name,
    string Category,
    DateOnly PurchaseDate,
    decimal Price,
    bool IsFinalSale);

internal sealed record EligibilityResult(bool IsEligible, string Message);

internal sealed record ReturnRequest(
    string ReturnId,
    string OrderId,
    string ItemId,
    string Reason,
    DateTimeOffset CreatedAt);
