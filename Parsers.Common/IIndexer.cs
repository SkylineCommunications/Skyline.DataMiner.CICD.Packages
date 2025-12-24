namespace Skyline.DataMiner.CICD.Parsers.Common
{
    /// <summary>
    /// Generic indexer interface.
    /// </summary>
    /// <typeparam name="TKey">The key of the indexer.</typeparam>
    /// <typeparam name="TResult">The result type of hte indexer.</typeparam>
    public interface IIndexer<in TKey, out TResult>
    {
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        TResult this[TKey key] { get; }
    }
}
