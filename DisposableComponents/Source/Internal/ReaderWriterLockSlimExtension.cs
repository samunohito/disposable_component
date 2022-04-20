using System;
using System.Threading;

namespace DisposableComponents.Internal
{
    internal static class ReaderWriterLockSlimExtension
    {
        public static void ReadLockScope(this ReaderWriterLockSlim @lock, Action reader)
        {
            try
            {
                @lock.EnterReadLock();
                reader();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        public static T1 ReadLockScope<T1>(this ReaderWriterLockSlim @lock, Func<T1> reader)
        {
            try
            {
                @lock.EnterReadLock();
                return reader();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }
        
        public static void WriteLockScope(this ReaderWriterLockSlim @lock, Action writer)
        {
            try
            {
                @lock.EnterWriteLock();
                writer();
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        public static void WriteLockScope<T1>(this ReaderWriterLockSlim @lock, T1 value, Action<T1> writer)
        {
            try
            {
                @lock.EnterWriteLock();
                writer(value);
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        public static T2 WriteLockScope<T1, T2>(this ReaderWriterLockSlim @lock, T1 value, Func<T1, T2> writer)
        {
            try
            {
                @lock.EnterWriteLock();
                return writer(value);
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }
    }
}