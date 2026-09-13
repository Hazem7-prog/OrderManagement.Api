namespace OrderManagement.Application.Orders;

public class OrderDetailsDto
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public string StatusLabel { get; set; } = string.Empty;

    public bool CanConfirm { get; set; }
}