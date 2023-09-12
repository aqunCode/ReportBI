
namespace Bi.Entities.Response;

public class DataItemTree
{
    public string? Id { get; set; }

    public string? ParentId { get; set; }

    public string? Title { get; set; }

    public string? Code { get; set; }

    public int SortCode { get; set; }

    public bool Expand { get; set; } = true;

    public bool contextmenu { get; set; } = true;


    public List<DataItemTree> Children { get; set; }
}
