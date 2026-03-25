using System.Diagnostics;
using System.Text.Json.Serialization;

/// <summary>
/// Represents page parameters
/// </summary>
[DebuggerDisplay("PageNumber: {" + nameof(PageNumber) + "}, PageSize: {" + nameof(PageSize) + "}")]
public class PageParameters
{
    /// <summary>
    /// Use for page number that is passed from frontend.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Use for page size that is passed from frontend.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 20;
}