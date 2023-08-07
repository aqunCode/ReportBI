namespace Bi.Entities.Entity;

public class Authority
{
    public string? Id { get; set; }
    public int Category { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? ParentId { get; set; }
}
