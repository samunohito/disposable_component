using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DisposableComponents
{
    /// <summary>
    /// This collection can only register objects with the IDisposable interface.
    /// The collection itself implements the <see cref="IDisposable"/> interface, and by calling <see cref="Dispose()"/>,
    /// registered objects can be destroyed en masse.
    /// </summary>
    /// <typeparam name="T">The type must implement or inherit from the <see cref="IDisposable"/> interface</typeparam>
    public class DisposableCollection<T> :
        ICollection<T>,
        IReadOnlyCollection<T>,
        IDisposable
        where T : IDisposable
    {
        private readonly object _gate;
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
            _gate = new object();
            _disposed = false;
        }

        /// <summary>
        /// destructor
        /// </summary>
        ~DisposableCollection()
        {
            Dispose(false);
        }

        /// <summary>
        /// indexer
        /// </summary>
        /// <param name="i"></param>
        public T this[int i] => _list[i];

        /// <inheritdoc cref="ICollection{T}.GetEnumerator"/>
        public IEnumerator<T> GetEnumerator()
        {
            lock (_gate)
            {
                return _list.ToList().GetEnumerator();
            }
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc cref="ICollection{T}.Add"/>
        public void Add(T item)
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    item?.Dispose();
                    return;
                }

                _list.Add(item);
            }
        }

        /// <inheritdoc cref="ICollection{T}.Clear"/>
        public void Clear()
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _list.Clear();
            }
        }

        /// <inheritdoc cref="ICollection{T}.Contains"/>
        public bool Contains(T item)
        {
            lock (_gate)
            {
                return _disposed ? false : _list.Contains(item);
            }
        }

        /// <inheritdoc cref="ICollection{T}.CopyTo"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _list.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc cref="ICollection{T}.Remove"/>
        public bool Remove(T item)
        {
            lock (_gate)
            {
                return _disposed ? false : _list.Remove(item);
            }
        }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count
        {
            get
            {
                lock (_gate)
                {
                    return _disposed ? 0 : _list.Count;
                }
            }
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
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            // Copy for destruction of elements to be implemented later.
            // Destruction of elements does not need to be locked, so we want to keep them out of WriteLockScope.
            var list = _list.ToList();

            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _list.Clear();

                _disposed = true;
            }

            foreach (var items in list)
            {
                items.Dispose();
            }
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
        public DisposableCollection()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="capacity">Specify the initial size of the collection</param>
        public DisposableCollection(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="items">Used to initialize the collection</param>
        public DisposableCollection(IEnumerable<IDisposable> items) : base(items)
        {
        }
    }
}