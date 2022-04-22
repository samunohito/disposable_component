using System;
using System.Runtime.CompilerServices;

namespace DisposableComponents
{
    /// <summary>
    /// Extends <see cref="DisposableComponent"/> to provide a mechanism whereby read/write functions can be executed only when the object is not destroyed.
    /// </summary>
    /// <seealso cref="DisposableComponent"/>
    public abstract class SafetyAccessComponent : DisposableComponent
    {
        /// <summary>
        /// Assists in safely reading values from the reader.
        /// If this object is destroyed, the value of <see cref="defaultValue"/> is returned.
        /// Use the <see cref="DisposableComponent.IsDisposed"/> property to determine if it has been destroyed.
        /// </summary>
        /// <param name="reader">Function to read the value</param>
        /// <param name="defaultValue">Default value for when the object was destroyed</param>
        /// <typeparam name="T">Type of value to be read</typeparam>
        /// <returns>Value read from reader or default value</returns>
        protected virtual T SafeRead<T>(Func<T> reader, T defaultValue)
        {
            return IsDisposed
                ? defaultValue
                : reader();
        }

        /// <summary>
        /// Enclose <see cref="SafeRead{T}"/> in try - catch to make it safer to use.
        /// If an exception occurs, send to <see cref="HandleError"/>.
        /// </summary>
        /// <param name="reader">Function to read the value</param>
        /// <param name="defaultValue">Default value for when the object was destroyed</param>
        /// <param name="result">Value read from reader or default value</param>
        /// <typeparam name="T">Type of value to be read</typeparam>
        /// <returns><see cref="SafeRead{T}"/> execution success or failure</returns>
        protected bool TrySafeRead<T>(Func<T> reader, T defaultValue, out T result)
        {
            try
            {
                result = SafeRead(reader, defaultValue);
                return true;
            }
            catch (Exception ex)
            {
                HandleError(ex);
                result = defaultValue;
                return false;
            }
        }

        /// <summary>
        /// Assists in safely writing the value of param.
        /// If this object is destroyed, no call to writer is made.
        /// Use the <see cref="DisposableComponent.IsDisposed"/> property to determine if it has been destroyed.
        /// </summary>
        /// <param name="writer">Function for writing values</param>
        /// <param name="param">Value to be written</param>
        /// <typeparam name="T">Type of value to be written</typeparam>
        protected virtual void SafeWrite<T>(Action<T> writer, T param)
        {
            if (!IsDisposed)
            {
                writer(param);
            }
        }

        /// <summary>
        /// Enclose <see cref="SafeWrite{T}"/> in try - catch to make it safer to use.
        /// If an exception occurs, send to <see cref="HandleError"/>.
        /// </summary>
        /// <param name="writer">Function for writing values</param>
        /// <param name="param">Value to be written</param>
        /// <typeparam name="T">Type of value to be written</typeparam>
        /// <returns><see cref="SafeWrite{T}"/> execution success or failure</returns>
        protected bool TrySafeWrite<T>(Action<T> writer, T param)
        {
            try
            {
                SafeWrite(writer, param);
                return true;
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return false;
            }
        }

        /// <summary>
        /// Called when an exception occurs in <see cref="TrySafeRead{T}"/> or <see cref="TrySafeWrite{T}"/>.
        /// </summary>
        /// <param name="ex">Exceptions that have occurred</param>
        /// <param name="caller">Occurred method</param>
        protected virtual void HandleError(Exception ex, [CallerMemberName] string caller = null)
        {
        }
    }
}