using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DisposableComponent.Internal;

namespace DisposableComponent
{
    /// <summary>
    /// This collection can only register objects with the IDisposable interface.
    /// The collection itself implements the <see cref="IDisposable"/> interface, and by calling <see cref="Dispose()"/>,
    /// registered objects can be destroyed en masse.
    /// </summary>
    /// <remarks>
    /// The contents of the following sites were of great help in implementing this class.
    /// https://stackoverflow.com/a/23446622
    /// </remarks>
    /// <typeparam name="T">The type must implement or inherit from the <see cref="IDisposable"/> interface</typeparam>
    public class DisposableCollection<T> :
        ICollection<T>,
        IReadOnlyCollection<T>,
        IDisposable
        where T : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        private readonly List<T> _list;
        private bool _disposed;

        /// <summary>
        /// ctor
        /// </summary>
        public DisposableCollection() : this(new List<T>())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="capacity">Specify the initial size of the collection</param>
        public DisposableCollection(int capacity) : this(new List<T>(capacity))
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="items">Used to initialize the collection</param>
        public DisposableCollection(IEnumerable<T> items)
        {
            _list = new List<T>(items);
            _lock = new ReaderWriterLockSlim();
            _disposed = false;
        }

        /// <summary>
        /// destructor
        /// </summary>
        ~DisposableCollection()
        {
            Dispose(false);
            _lock.Dispose();
        }
        
        /// <summary>
        /// indexer
        /// </summary>
        /// <param name="i"></param>
        public T this[int i] => _list[i];

        /// <inheritdoc cref="ICollection{T}.GetEnumerator"/>
        public IEnumerator<T> GetEnumerator()
        {
            return new ConcurrentEnumerator<T>(_list, _lock);
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc cref="ICollection{T}.Add"/>
        public void Add(T item)
        {
            _lock.WriteLockScope(item, it =>
            {
                if (_disposed)
                {
                    it?.Dispose();
                    return;
                }

                _list.Add(it);
            });
        }

        /// <inheritdoc cref="ICollection{T}.Clear"/>
        public void Clear()
        {
            _lock.WriteLockScope(() =>
            {
                if (_disposed)
                {
                    return;
                }

                _list.Clear();
            });
        }

        /// <inheritdoc cref="ICollection{T}.Contains"/>
        public bool Contains(T item)
        {
            return _lock.ReadLockScope(() => _disposed ? false : _list.Contains(item));
        }

        /// <inheritdoc cref="ICollection{T}.CopyTo"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.ReadLockScope(() =>
            {
                if (_disposed)
                {
                    return;
                }

                _list.CopyTo(array, arrayIndex);
            });
        }

        /// <inheritdoc cref="ICollection{T}.Remove"/>
        public bool Remove(T item)
        {
            return _lock.WriteLockScope(item, it => _disposed ? false : _list.Remove(it));
        }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count
        {
            get { return _lock.ReadLockScope(() => _disposed ? 0 : _list.Count); }
        }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public bool IsReadOnly => false;

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// All objects registered in the collection are destroyed at once, leaving the collection empty.
        /// After calling <see cref="Dispose()"/>, the objects themselves are treated as destroyed and are no longer available.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            _lock.WriteLockScope(() =>
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }

                foreach (var items in _list)
                {
                    items.Dispose();
                }

                _list.Clear();

                _disposed = true;
            });
        }

        private class ConcurrentEnumerator<T1> : IEnumerator<T1>
        {
            private readonly IEnumerator<T1> _inner;
            private readonly ReaderWriterLockSlim _lock;

            public ConcurrentEnumerator(IEnumerable<T1> inner, ReaderWriterLockSlim @lock)
            {
                _lock = @lock;
                _inner = inner.GetEnumerator();
            }

            public bool MoveNext() => _inner.MoveNext();

            public void Reset() => _inner.Reset();

            public T1 Current => _inner.Current;

            object IEnumerator.Current => _inner.Current;

            public void Dispose() => _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Alias for <see cref="DisposableCollection{IDisposable}"/>.
    /// </summary>
    public class DisposableCollection : DisposableCollection<IDisposable>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DisposableCollection() : base(new List<IDisposable>())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="capacity">Specify the initial size of the collection</param>
        public DisposableCollection(int capacity) : base(capacity)
        {
        }
    }
}