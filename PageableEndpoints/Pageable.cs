/// <summary>
/// A collection of values that may take multiple service requests to
/// iterate over.
/// </summary>
/// <typeparam name="TItem">The type of the values.</typeparam>
public abstract class Pageable<TItem> : IAsyncEnumerable<TItem> where TItem : notnull
{
    /// <summary>
    /// Gets a <see cref="CancellationToken"/> used for requests made while
    /// enumerating asynchronously.
    /// </summary>
    protected virtual CancellationToken CancellationToken { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pageable{TItem}"/>
    /// class for mocking.
    /// </summary>
    protected Pageable() =>
        CancellationToken = CancellationToken.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pageable{TItem}"/>
    /// class.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> used for requests made while
    /// enumerating asynchronously.
    /// </param>
    protected Pageable(CancellationToken cancellationToken) =>
        CancellationToken = cancellationToken;

    /// <summary>
    /// Enumerate the values a <see cref="Page{TItem}"/> at a time.  This may
    /// make multiple service requests.
    /// </summary>
    /// <returns>
    /// An async sequence of <see cref="Page{TItem}"/>s.
    /// </returns>
    public abstract IAsyncEnumerable<Page<TItem>> AsPagesAsync();

    /// <summary>
    /// Enumerate the values in the collection asynchronously.  This may
    /// make multiple service requests.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> used for requests made while
    /// enumerating asynchronously.
    /// </param>
    /// <returns>An async sequence of values.</returns>
    public virtual async IAsyncEnumerator<TItem> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await foreach (var page in AsPagesAsync().ConfigureAwait(false).WithCancellation(cancellationToken))
        {
            foreach (var value in page.Items)
            {
                yield return value;
            }
        }
    }
}

internal class OffsetPageable<TRequest, TResponse, TItem> : Pageable<TItem> where TItem : notnull
{
    private readonly TRequest _request;
    private readonly Func<TRequest, Task<TResponse>> _sendRequest;
    private readonly Action<TRequest, int> _setOffset;
    private readonly Func<TRequest, int> _getOffset;
    private readonly Func<TResponse, IReadOnlyList<TItem>> _getItems;
    private readonly Func<TRequest, int> _getStep;
    private readonly Func<TResponse, bool> _getHasNextPage;
    private readonly IEnumerable<Page<TItem>> _pages;

    public OffsetPageable(
        TRequest request,
        Func<TRequest, Task<TResponse>> sendRequest,
        Action<TRequest, int> setOffset, 
        Func<TRequest, int> getOffset, 
        Func<TResponse, IReadOnlyList<TItem>> getItems, 
        Func<TRequest, int> getStep, 
        Func<TResponse, bool> getHasNextPage
        )
    {
        _request = request;
        _sendRequest = sendRequest;
        _setOffset = setOffset;
        _getOffset = getOffset;
        _getItems = getItems;
        _getStep = getStep;
        _getHasNextPage = getHasNextPage;
    }

    public override async IAsyncEnumerable<Page<TItem>> AsPagesAsync()
    {
        var offset = _getOffset(_request);
        var step = _getStep(_request);
        bool hasNextPage;
        do
        {
            var response = await _sendRequest(_request).ConfigureAwait(false);
            var items = _getItems(response);
            hasNextPage = _getHasNextPage(response);
            yield return Page<TItem>.FromItems(items, response);
            offset += step;
            _setOffset(_request, offset);
        } while (hasNextPage);
    }
}

internal class CursorPageable<TRequest, TResponse, TItem> : Pageable<TItem> where TItem : notnull
{
    private readonly TRequest _request;
    private readonly Func<TRequest, Task<TResponse>> _sendRequest;
    private readonly Action<TRequest, string> _setCursor;
    private readonly Func<TResponse, string?> _getNextCursor;
    private readonly Func<TResponse, IReadOnlyList<TItem>> _getItems;

    public CursorPageable(
        TRequest request,
        Func<TRequest, Task<TResponse>> sendRequest,
        Action<TRequest, string> setCursor, 
        Func<TResponse, string?> getNextCursor, 
        Func<TResponse, IReadOnlyList<TItem>> getItems
    )
    {
        _request = request;
        _sendRequest = sendRequest;
        _setCursor = setCursor;
        _getNextCursor = getNextCursor;
        _getItems = getItems;
    }

    public override async IAsyncEnumerable<Page<TItem>> AsPagesAsync()
    {
        do
        {
            var response = await _sendRequest(_request).ConfigureAwait(false);
            var items = _getItems(response);
            var nextCursor = _getNextCursor(response);
            yield return Page<TItem>.FromItems(items, response);
            if (nextCursor == null)
            {
                break;
            }
            _setCursor(_request, nextCursor);
        } while (true);
    }
}