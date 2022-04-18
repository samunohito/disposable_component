using System;
using System.Collections.Generic;
using System.Threading;
using DisposableComponent.Internal;

namespace DisposableComponent
{
    /// <summary>
    /// Reference implementation class for <see cref="IDisposableComponent"/>. See Interface for detailed behavior.
    /// </summary>
    /// <seealso cref="IDisposableComponent"/>
    public abstract class DisposableComponent : IDisposableComponent
    {
        /// <inheritdoc cref="IDisposableComponent.Disposing"/>
        public event EventHandler<DisposableComponentEventArgs> Disposing;

        /// <inheritdoc cref="IDisposableComponent.Disposed"/>
        public event EventHandler<DisposableComponentEventArgs> Disposed;

        private readonly ReaderWriterLockSlim _lock;
        private readonly ConcurrentDisposableCollection<IDisposable> _disposables;

        /// <summary>
        /// ctor.
        /// </summary>
        protected DisposableComponent()
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _disposables = new ConcurrentDisposableCollection<IDisposable>();
            IsDisposed = false;
        }

        ~DisposableComponent()
        {
            _lock.Dispose();
        }

        /// <summary>
        /// Obtains an ICollection that holds IDisposable and allows the IDisposable associated with this object to be destroyed at once.
        /// IDisposable registered with an ICollection that can be retrieved from this property will be destroyed as soon as
        /// this object's <see cref="Dispose"/> method is called.
        /// </summary>
        protected ICollection<IDisposable> Disposable => _disposables;

        /// <inheritdoc cref="IDisposableComponent.IsDisposed"/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// This method is called when all of the following conditions are met
        /// - When <see cref="Dispose"/> is called
        /// - Before <see cref="IsDisposed"/> becomes true
        /// - Before <see cref="Disposing"/> event is fired
        /// - Before an element of the <see cref="Disposable"/> property is destroyed
        ///
        /// By extending this method, arbitrary processing can be performed at the above timing.
        /// </summary>
        protected virtual void OnDisposing()
        {
        }

        /// <summary>
        /// This method is called when all of the following conditions are met
        /// - When <see cref="Dispose"/> is called
        /// - After <see cref="IsDisposed"/> is true
        /// - Before the <see cref="Disposed"/> event occurs
        /// - After the element of the <see cref="Disposable"/> property is destroyed
        ///
        /// By extending this method, arbitrary processing can be performed at the above timing.
        /// </summary>
        protected virtual void OnDisposed()
        {
        }

        /// <inheritdoc cref="IDisposableComponent.Dispose"/>
        public void Dispose()
        {
            try
            {
                _lock.EnterWriteLock();

                if (IsDisposed)
                {
                    return;
                }

                OnDisposing();
                Disposing?.Invoke(this, new DisposableComponentEventArgs(this));

                IsDisposed = true;

                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }

                _disposables.Dispose();

                OnDisposed();
                Disposed?.Invoke(this, new DisposableComponentEventArgs(this));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc cref="IDisposableComponent.Dispose"/>
        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}