namespace Synaptix.Analyzers.Samples.Contracts;

public sealed class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class OrderModel
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = "";
    public decimal Total { get; set; }
}

public sealed class AddressViewModel
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
}

public sealed class JsonHelperImpl
{
    public string Serialize(object value) => System.Text.Json.JsonSerializer.Serialize(value);
}

public sealed class LoggingUtil
{
    public static string FormatTimestamp(DateTimeOffset timestamp)
        => timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");
}

public sealed class StringUtility
{
    public static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";
}
