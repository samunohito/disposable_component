using System;

namespace DisposableComponents
{
    /// <summary>
    /// Arguments for events related to <see cref="IDisposableComponent"/>.
    /// </summary>
    public class DisposableComponentEventArgs : EventArgs
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="component">Event Originator</param>
        public DisposableComponentEventArgs(IDisposableComponent component)
        {
            Source = component;
        }

        /// <summary>
        /// Event Originator
        /// </summary>
        public IDisposableComponent Source { get; }
    }
}