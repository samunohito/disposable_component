using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace DisposableComponents.Test
{
    public class DisposableComponentTest
    {
        private TestDisposableComponent _component;

        [SetUp]
        public void Setup()
        {
            _component = new TestDisposableComponent();
        }

        /// <summary>
        /// Ensure that IsDisposed is changed to true by a call to Dispose()
        /// </summary>
        /// <seealso cref="IDisposableComponent.IsDisposed"/>
        [Test]
        public void Checks_IsDisposed_is_changed_to_true()
        {
            Assert.False(_component.IsDisposed);

            _component.Dispose();

            Assert.True(_component.IsDisposed);
        }

        /// <summary>
        /// Checks for destruction of related objects registered in Disposable.
        /// </summary>
        /// <seealso cref="DisposableComponent.Disposable"/>
        [Test]
        public void Checks_destruction_objects_in_Disposable()
        {
            var dummyList = new[]
            {
                new DummyDisposable(),
                new DummyDisposable(),
                new DummyDisposable(),
            };

            foreach (var dummyDisposable in dummyList)
            {
                _component.AppendDisposable(dummyDisposable);
            }

            Assert.True(dummyList.All(x => !x.IsDisposed));
            Assert.False(_component.IsEmptyDisposable());

            _component.Dispose();

            Assert.True(dummyList.All(x => x.IsDisposed));
            Assert.True(_component.IsEmptyDisposable());
        }

        /// <summary>
        /// Check the relationship between the event triggered by Dispose() and the IsDisposed before and after the event.
        /// </summary>
        /// <seealso cref="IDisposableComponent.Disposing"/>
        /// <seealso cref="IDisposableComponent.Disposed"/>
        [Test]
        public void Check_event_triggered_by_Dispose_and_IsDisposed()
        {
            int disposingCount = 0, disposedCount = 0;
            _component.Disposing += (s, e) =>
            {
                disposingCount++;
                Assert.False(_component.IsDisposed);
            };

            _component.Disposed += (s, e) =>
            {
                disposedCount++;
                Assert.True(_component.IsDisposed);
            };

            Assert.AreEqual(0, disposingCount);
            Assert.AreEqual(0, disposedCount);

            _component.Dispose();

            Assert.AreEqual(1, disposingCount);
            Assert.AreEqual(1, disposedCount);
        }
    }

    public class TestDisposableComponent : DisposableComponent
    {
        public void AppendDisposable(IDisposable disposable)
        {
            Disposable.Add(disposable);
        }

        public bool IsEmptyDisposable() => Disposable.Count <= 0;
    }

    public class DummyDisposable : IDisposable
    {
        public void Dispose()
        {
            IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }
    }
}