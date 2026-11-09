namespace Pnv.Combo.Web.Domain;

public sealed class GreetingRecord
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
