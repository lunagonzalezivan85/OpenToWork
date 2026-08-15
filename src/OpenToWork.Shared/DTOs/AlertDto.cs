namespace OpenToWork.Shared.DTOs;

public enum AlertType
{
    Info = 0,
    Warning = 1,
    Success = 2,
    Danger = 3
}

public class AlertDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public string? Url { get; set; }
    public AlertType AlertType { get; set; }
}
