namespace AirwaysMergeSafeServer.ViewModels;

public class DatabaseViewerViewModel
{
    public string SelectedTable { get; set; } = "Highways";
    public int    Page          { get; set; } = 1;
    public int    PageSize      { get; set; } = 50;
    public int    TotalRows     { get; set; }
    public int    TotalPages    => TotalRows == 0 ? 1 : (int)Math.Ceiling(TotalRows / (double)PageSize);

    public List<string>                       Columns      { get; set; } = new();
    public List<Dictionary<string, string>>   Rows         { get; set; } = new();
    public List<(string Name, int Count)>     TableSummary { get; set; } = new();
}
