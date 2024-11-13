using System.ComponentModel;

    /// <summary>
    /// A single <see cref="Page{TItem}"/> of items from a request that may return
    /// zero or more <see cref="Page{TItem}"/>s of items.
    /// </summary>
    /// <typeparam name="TItem">The type of items.</typeparam>
    public abstract class Page<TItem>
    {
        /// <summary>
        /// Gets the items in this <see cref="Page{TItem}"/>.
        /// </summary>
        public abstract IReadOnlyList<TItem> Items { get; }

        /// <summary>
        /// Gets the response that provided this
        /// <see cref="Page{TItem}"/>.
        /// </summary>
        public abstract object GetOriginalResponse();

        /// <summary>
        /// Gets the <see cref="TResponse"/> that provided this
        /// <see cref="Page{TItem}"/>.
        /// </summary>
        public abstract TResponse GetOriginalResponse<TResponse>();

        /// <summary>
        /// Creates a new <see cref="Page{TItem}"/>.
        /// </summary>
        /// <param name="items">
        /// The items in this <see cref="Page{TItem}"/>.
        /// </param>
        /// <param name="continuationToken">
        /// The continuation token used to request the next <see cref="Page{TItem}"/>.
        /// </param>
        /// <param name="response">
        /// The <see cref="TResponse"/> that provided this <see cref="Page{TItem}"/>.
        /// </param>
        public static Page<TItem> FromItems<TResponse>(IReadOnlyList<TItem> items, TResponse response)
        {
            return new PageCore(items, response);
        }

        private class PageCore : Page<TItem>
        {
            private readonly object _response;

            public PageCore(IReadOnlyList<TItem> items, object response)
            {
                _response = response;
                Items = items;
            }

            public override IReadOnlyList<TItem> Items { get; }
            public override object GetOriginalResponse() => _response;
            public override TResponse GetOriginalResponse<TResponse>() => (TResponse)_response;
        }
    }
