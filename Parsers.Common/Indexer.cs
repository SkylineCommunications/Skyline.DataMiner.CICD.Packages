namespace Skyline.DataMiner.CICD.Parsers.Common
{
    using System;

    internal class Indexer<TSource, TKey, TResult> : IIndexer<TKey, TResult>
    {
        private readonly TSource _source;
        private readonly Func<TSource, TKey, TResult> _func;

        public Indexer(TSource source, Func<TSource, TKey, TResult> func)
        {
            if (Object.Equals(source, default(TSource)))
            {
                throw new ArgumentNullException(nameof(source));
            }

            _source = source;
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TResult this[TKey key] => _func(_source, key);
    }
}