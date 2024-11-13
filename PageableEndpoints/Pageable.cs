/// <summary>
/// A collection of values that may take multiple service requests to
/// iterate over.
/// </summary>
/// <typeparam name="TItem">The type of the values.</typeparam>
/// <typeparam name="TResponse">The original HTTP response body.</typeparam>
public abstract class Pageable<TItem, TResponse> : IAsyncEnumerable<TItem> where TItem : notnull where TResponse : notnull
{
    /// <summary>
    /// Gets a <see cref="CancellationToken"/> used for requests made while
    /// enumerating asynchronously.
    /// </summary>
    protected virtual CancellationToken CancellationToken { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pageable{TItem, TResponse}"/>
    /// class for mocking.
    /// </summary>
    protected Pageable() =>
        CancellationToken = CancellationToken.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pageable{TItem, TResponse}"/>
    /// class.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> used for requests made while
    /// enumerating asynchronously.
    /// </param>
    protected Pageable(CancellationToken cancellationToken) =>
        CancellationToken = cancellationToken;

    /// <summary>
    /// Enumerate the values a <see cref="Page{TItem, TResponse}"/> at a time.  This may
    /// make multiple service requests.
    /// </summary>
    /// <returns>
    /// An async sequence of <see cref="Page{TItem, TResponse}"/>s.
    /// </returns>
    public abstract IAsyncEnumerable<Page<TItem, TResponse>> AsPagesAsync();

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

internal abstract class OffsetPageable<TRequest, TResponse, TItem> : Pageable<TItem, TResponse> where TItem : notnull where TResponse : notnull
{
    private readonly TRequest _request;
    private readonly Func<TRequest, Task<TResponse>> _getNextPage;
    protected abstract void SetOffset(TRequest request, int offset);
    protected abstract int GetOffset(TRequest request);
    protected abstract IReadOnlyList<TItem>? GetItems(TResponse response);
    protected abstract int? GetStep(TRequest request);
    protected abstract bool? HasNextPage(TResponse response);

    public OffsetPageable(
        TRequest request,
        Func<TRequest, Task<TResponse>> getNextPage
        )
    {
        _request = request;
        _getNextPage = getNextPage;
    }

    public override async IAsyncEnumerable<Page<TItem, TResponse>> AsPagesAsync()
    {
        var hasStep = GetStep(_request) is not null;
        var offset = GetOffset(_request);
        bool hasNextPage;
        do
        {
            var response = await _getNextPage(_request).ConfigureAwait(false);
            var items = GetItems(response);
            var itemCount = items?.Count ?? 0;
            hasNextPage = HasNextPage(response) ?? itemCount > 0;
            if (items != null)
            {
                yield return new Page<TItem, TResponse>(items, response);
            }

            // If there is a step, we need to increment the offset by the number of items
            if (hasStep)
            {
                offset += items?.Count ?? 1;
            }
            else
            {
                offset += 1;
            }
            SetOffset(_request, offset);
        } while (hasNextPage);
    }
}

internal abstract class CursorPageable<TRequest, TResponse, TItem> : Pageable<TItem, TResponse> where TItem : notnull where TResponse : notnull
{
    private readonly TRequest _request;
    private readonly Func<TRequest, Task<TResponse>> _sendRequest;
    protected abstract void SetCursor(TRequest request, string cursor);
    protected abstract string? GetNextCursor(TResponse response);
    protected abstract IReadOnlyList<TItem>? GetItems(TResponse response);

    public CursorPageable(
        TRequest request,
        Func<TRequest, Task<TResponse>> sendRequest
    )
    {
        _request = request;
        _sendRequest = sendRequest;
    }

    public override async IAsyncEnumerable<Page<TItem, TResponse>> AsPagesAsync()
    {
        do
        {
            var response = await _sendRequest(_request).ConfigureAwait(false);
            var items = GetItems(response);
            var nextCursor = GetNextCursor(response);
            if (items != null)
            {
                yield return new Page<TItem, TResponse>(items, response);
            }

            if (nextCursor == null)
            {
                break;
            }
            SetCursor(_request, nextCursor);
        } while (true);
    }
}