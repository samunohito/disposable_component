using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DisposableComponents.Test
{
    public class DisposableCollectionTest
    {
        private DisposableCollection _collection = null;

        [SetUp]
        public void Setup()
        {
            _collection = new DisposableCollection(0);
        }

        /// <summary>
        /// Checks if registered objects are destroyed
        /// </summary>
        [Test]
        public void Checks_if_registered_objects_are_destroyed()
        {
            var disposable = new DummyDisposable();

            _collection.Add(disposable);
            Assert.AreEqual(1, _collection.Count);
            Assert.False(disposable.IsDisposed);

            _collection.Dispose();
            Assert.AreEqual(0, _collection.Count);
            Assert.True(disposable.IsDisposed);
        }

        /// <summary>
        /// Check for collection compliance registration and deletion
        /// </summary>
        [Test]
        public void Check_for_collection_compliance_registration_and_deletion()
        {
            var disposable = new DummyDisposable();

            _collection.Add(disposable);
            Assert.AreEqual(1, _collection.Count);
            Assert.False(disposable.IsDisposed);

            _collection.Remove(disposable);
            Assert.AreEqual(0, _collection.Count);
            Assert.False(disposable.IsDisposed);
        }

        /// <summary>
        /// Check for collection compliant clear.
        /// </summary>
        [Test]
        public void Check_for_collection_compliant_clear()
        {
            _collection.Add(new DummyDisposable());
            _collection.Add(new DummyDisposable());
            _collection.Add(new DummyDisposable());
            Assert.AreEqual(3, _collection.Count);

            _collection.Clear();
            Assert.AreEqual(0, _collection.Count);
        }

        /// <summary>
        /// Check Contains for compliance with the collection.
        /// </summary>
        [Test]
        public void Check_Contains_for_compliance_with_the_collection()
        {
            var disposable = new DummyDisposable();

            _collection.Add(disposable);
            Assert.AreEqual(1, _collection.Count);
            Assert.True(_collection.Contains(disposable));
            
            _collection.Dispose();
            Assert.False(_collection.Contains(disposable));
        }

        /// <summary>
        /// Check to see if you can still add after calling Dispose().
        /// </summary>
        [Test]
        public void Check_to_see_if_you_can_still_add_after_calling_Dispose()
        {
            _collection.Dispose();

            var disposable = new DummyDisposable();
            _collection.Add(disposable);

            Assert.AreEqual(0, _collection.Count);
            Assert.True(disposable.IsDisposed);
        }

        /// <summary>
        /// Synchronization check of Add
        /// </summary>
        [Test]
        public void Synchronization_check_of_Add()
        {
            var taskCount = 4096;

            Parallel.For(0, taskCount, _ => _collection.Add(new DummyDisposable()));

            Assert.AreEqual(taskCount, _collection.Count);
        }
        
        /// <summary>
        /// Synchronization check of Remove
        /// </summary>
        [Test]
        public void Synchronization_check_of_Remove()
        {
            var taskCount = 4096;

            for (var i = 0; i < taskCount; i++)
            {
                _collection.Add(new DummyDisposable());
            }

            Parallel.ForEach(_collection.ToList(), it => _collection.Remove(it));

            Assert.AreEqual(0, _collection.Count);
        }
        
        /// <summary>
        /// Synchronization check of Clear
        /// </summary>
        [Test]
        public void Synchronization_check_of_Clear()
        {
            var taskCount = 4096;

            Parallel.For(0, taskCount, _ =>
            {
                _collection.Add(new DummyDisposable());
                _collection.Clear();
            });

            Assert.AreEqual(0, _collection.Count);
        }

        public class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
                IsDisposed = true;
            }

            public bool IsDisposed { get; private set; }
        }

        public class NumberDisposable : IDisposable
        {
            public int Number { get; }

            public NumberDisposable(int num)
            {
                Number = num;
            }

            public void Dispose()
            {
                Console.WriteLine(Number);
                IsDisposed = true;
            }

            public bool IsDisposed { get; private set; }
        }
    }
}