using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DisposableComponent.Internal
{
    internal class ConcurrentDisposableCollection<T> : ICollection<T>, IDisposable where T : IDisposable
    {
        private readonly List<T> _list;
        private readonly ReaderWriterLockSlim _lock;
        private bool _disposed;

        public ConcurrentDisposableCollection() : this(
            new List<T>(),
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion))
        {
        }

        private ConcurrentDisposableCollection(IEnumerable<T> items, ReaderWriterLockSlim @lock)
        {
            _list = new List<T>(items);
            _lock = @lock;
            _disposed = false;
        }

        ~ConcurrentDisposableCollection()
        {
            Dispose(false);
        }

        /// <inheritdoc cref="ICollection{T}.GetEnumerator"/>
        public IEnumerator<T> GetEnumerator()
        {
            return new ConcurrentEnumerator<T>(_list, _lock);
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ConcurrentEnumerator<T>(_list, _lock);
        }

        /// <inheritdoc cref="ICollection{T}.Add"/>
        public void Add(T item)
        {
            if (_disposed)
            {
                item?.Dispose();
                return;
            }

            _lock.WriteLockScope(it => _list.Add(it), item);
        }

        /// <inheritdoc cref="ICollection{T}.Clear"/>
        public void Clear()
        {
            if (_disposed)
            {
                return;
            }
            
            _lock.WriteLockScope(() => _list.Clear());
        }

        /// <inheritdoc cref="ICollection{T}.Contains"/>
        public bool Contains(T item)
        {
            if (_disposed)
            {
                return false;
            }
            
            return _lock.ReadLockScope(() => _list.Contains(item));
        }

        /// <inheritdoc cref="ICollection{T}.CopyTo"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_disposed)
            {
                return;
            }
            
            _lock.ReadLockScope(() => _list.CopyTo(array, arrayIndex));
        }

        /// <inheritdoc cref="ICollection{T}.Remove"/>
        public bool Remove(T item)
        {
            if (_disposed)
            {
                return false;
            }
            
            return _lock.WriteLockScope(it => _list.Remove(it), item);
        }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count
        {
            get
            {
                if (_disposed)
                {
                    return 0;
                }
                
                return _lock.ReadLockScope(() => _list.Count);
            }
        }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public bool IsReadOnly => false;

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }

                _list.Clear();
                _lock.Dispose();
                _disposed = true;
            }
        }

        private class ConcurrentEnumerator<T1> : IEnumerator<T1>
        {
            private readonly IEnumerator<T1> _inner;
            private readonly ReaderWriterLockSlim _lock;

            public ConcurrentEnumerator(IEnumerable<T1> inner, ReaderWriterLockSlim @lock)
            {
                _lock = @lock;
                _lock.EnterReadLock();

                _inner = inner.GetEnumerator();
            }

            public bool MoveNext()
            {
                return _inner.MoveNext();
            }

            public void Reset()
            {
                _inner.Reset();
            }

            public T1 Current => _inner.Current;

            object IEnumerator.Current => _inner.Current;

            public void Dispose()
            {
                _lock.ExitReadLock();
            }
        }
    }
}