/// <summary>
/// A single <see cref="Page{TItem, TResponse}"/> of items from a request that may return
/// zero or more <see cref="Page{TItem, TResponse}"/>s of items.
/// </summary>
/// <typeparam name="TItem">The type of items.</typeparam>
/// <typeparam name="TResponse">The original HTTP response body.</typeparam>
public class Page<TItem, TResponse> where TItem : notnull where TResponse : notnull
{
    public Page(IReadOnlyList<TItem> items, TResponse response)
    {
        Items = items;
        OriginalResponse = response;
    }
    
    /// <summary>
    /// Gets the items in this <see cref="Page{TItem, TResponse}"/>.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }
    
    /// <summary>
    /// Gets the items in this <see cref="Page{TItem, TResponse}"/>.
    /// </summary>
    public TResponse OriginalResponse { get; }
}