using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Framework.Networking;
using Xunit;

namespace HermesProxy.Tests.Framework
{
    public class NetworkThreadTests
    {
        /// <summary>
        /// Mock socket for testing NetworkThread behavior
        /// </summary>
        private class MockSocket : ISocket
        {
            public bool IsOpenValue { get; set; } = true;
            public bool UpdateReturnValue { get; set; } = true;
            public int UpdateCallCount { get; private set; }
            public int CloseSocketCallCount { get; private set; }
            public int AcceptCallCount { get; private set; }
            public string Id { get; }

            public MockSocket(string id = null)
            {
                Id = id ?? Guid.NewGuid().ToString("N")[..8];
            }

            public void Accept()
            {
                AcceptCallCount++;
            }

            public bool Update()
            {
                UpdateCallCount++;
                return UpdateReturnValue;
            }

            public bool IsOpen()
            {
                return IsOpenValue;
            }

            public void CloseSocket()
            {
                CloseSocketCallCount++;
                IsOpenValue = false;
            }
        }

        /// <summary>
        /// Testable NetworkThread that exposes internal state
        /// </summary>
        private class TestableNetworkThread : NetworkThread<MockSocket>
        {
            public List<MockSocket> AddedSockets { get; } = new();
            public List<MockSocket> RemovedSockets { get; } = new();

            protected override void SocketAdded(MockSocket sock)
            {
                AddedSockets.Add(sock);
            }

            protected override void SocketRemoved(MockSocket sock)
            {
                RemovedSockets.Add(sock);
            }
        }

        [Fact]
        public void AddSocket_IncrementsConnectionCount()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var socket = new MockSocket();

            // Act
            thread.AddSocket(socket);

            // Assert
            Assert.Equal(1, thread.GetConnectionCount());
            Assert.Contains(socket, thread.AddedSockets);
        }

        [Fact]
        public void AddSocket_MultipleSockets_TracksAll()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var socket1 = new MockSocket("s1");
            var socket2 = new MockSocket("s2");
            var socket3 = new MockSocket("s3");

            // Act
            thread.AddSocket(socket1);
            thread.AddSocket(socket2);
            thread.AddSocket(socket3);

            // Assert
            Assert.Equal(3, thread.GetConnectionCount());
            Assert.Equal(3, thread.AddedSockets.Count);
        }

        [Fact]
        public void StartAndStop_ThreadLifecycle()
        {
            // Arrange
            var thread = new TestableNetworkThread();

            // Act
            bool started = thread.Start();
            Thread.Sleep(50); // Let thread run a bit
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.True(started);
        }

        [Fact]
        public void Start_CalledTwice_ReturnsFalse()
        {
            // Arrange
            var thread = new TestableNetworkThread();

            // Act
            bool first = thread.Start();
            bool second = thread.Start();
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.True(first);
            Assert.False(second);
        }

        [Fact]
        public void Run_UpdatesActiveSockets()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var socket = new MockSocket();
            thread.AddSocket(socket);

            // Act
            thread.Start();
            Thread.Sleep(50); // Let thread run a few cycles
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.True(socket.UpdateCallCount > 0, "Socket should have been updated");
        }

        [Fact]
        public void Run_SocketUpdateReturnsFalse_RemovesSocket()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var socket = new MockSocket();
            socket.UpdateReturnValue = false; // Will cause removal
            thread.AddSocket(socket);

            // Act
            thread.Start();
            Thread.Sleep(100); // Let thread process
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.Contains(socket, thread.RemovedSockets);
            Assert.Equal(0, thread.GetConnectionCount());
        }

        [Fact]
        public void Run_SocketNotOpen_NotAddedToActiveList()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var socket = new MockSocket();
            socket.IsOpenValue = false; // Already closed when added
            thread.AddSocket(socket);

            // Act
            thread.Start();
            Thread.Sleep(50);
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.Contains(socket, thread.RemovedSockets);
            Assert.Equal(0, thread.GetConnectionCount());
        }

        [Fact]
        public void Run_MultipleSocketsClose_AllRemoved()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var sockets = new List<MockSocket>();
            for (int i = 0; i < 5; i++)
            {
                var socket = new MockSocket($"s{i}");
                socket.UpdateReturnValue = false; // All will be removed
                sockets.Add(socket);
                thread.AddSocket(socket);
            }

            // Act
            thread.Start();
            Thread.Sleep(100);
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.Equal(0, thread.GetConnectionCount());
            Assert.Equal(5, thread.RemovedSockets.Count);
            foreach (var socket in sockets)
            {
                Assert.Contains(socket, thread.RemovedSockets);
            }
        }

        [Fact]
        public void Run_AlternatingSocketStates_CorrectlyHandled()
        {
            // Arrange - some sockets stay open, some close
            var thread = new TestableNetworkThread();
            var stayOpen1 = new MockSocket("open1") { UpdateReturnValue = true };
            var willClose = new MockSocket("close") { UpdateReturnValue = false };
            var stayOpen2 = new MockSocket("open2") { UpdateReturnValue = true };

            thread.AddSocket(stayOpen1);
            thread.AddSocket(willClose);
            thread.AddSocket(stayOpen2);

            // Act
            thread.Start();
            Thread.Sleep(100);
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.Equal(2, thread.GetConnectionCount()); // Two should remain
            Assert.Contains(willClose, thread.RemovedSockets);
            Assert.DoesNotContain(stayOpen1, thread.RemovedSockets);
            Assert.DoesNotContain(stayOpen2, thread.RemovedSockets);
        }

        [Fact]
        public void Run_SocketClosedDuringUpdate_ClosesSocket()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var socket = new MockSocket();
            socket.UpdateReturnValue = false;
            socket.IsOpenValue = true; // Still open when Update returns false
            thread.AddSocket(socket);

            // Act
            thread.Start();
            Thread.Sleep(100);
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.True(socket.CloseSocketCallCount > 0, "CloseSocket should have been called");
        }

        [Fact]
        public void Run_ConsecutiveSocketRemovals_NoSkipping()
        {
            // This test verifies that when multiple consecutive sockets need removal,
            // none are skipped (tests the iteration bug)

            // Arrange
            var thread = new TestableNetworkThread();
            var sockets = new List<MockSocket>();

            // Create 10 sockets, all will return false on Update
            for (int i = 0; i < 10; i++)
            {
                var socket = new MockSocket($"socket{i}");
                socket.UpdateReturnValue = false;
                sockets.Add(socket);
                thread.AddSocket(socket);
            }

            // Act
            thread.Start();
            Thread.Sleep(150); // Give enough time for processing
            thread.Stop();
            thread.Wait();

            // Assert - ALL sockets should be removed, none skipped
            Assert.Equal(0, thread.GetConnectionCount());
            Assert.Equal(10, thread.RemovedSockets.Count);

            // Verify each socket was updated at least once
            foreach (var socket in sockets)
            {
                Assert.True(socket.UpdateCallCount >= 1,
                    $"Socket {socket.Id} should have been updated at least once, but was updated {socket.UpdateCallCount} times");
                Assert.Contains(socket, thread.RemovedSockets);
            }
        }

        [Fact]
        public void Run_MixedRemovalPattern_AllProcessed()
        {
            // Arrange - alternating pattern: remove, keep, remove, keep, etc.
            var thread = new TestableNetworkThread();
            var allSockets = new List<MockSocket>();
            var shouldRemove = new List<MockSocket>();
            var shouldKeep = new List<MockSocket>();

            for (int i = 0; i < 10; i++)
            {
                var socket = new MockSocket($"socket{i}");
                socket.UpdateReturnValue = (i % 2 == 0); // Even: keep, Odd: remove
                allSockets.Add(socket);

                if (i % 2 == 0)
                    shouldKeep.Add(socket);
                else
                    shouldRemove.Add(socket);

                thread.AddSocket(socket);
            }

            // Act
            thread.Start();
            Thread.Sleep(150);
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.Equal(5, thread.GetConnectionCount()); // 5 should remain (even indices)
            Assert.Equal(5, thread.RemovedSockets.Count); // 5 should be removed (odd indices)

            foreach (var socket in shouldRemove)
            {
                Assert.Contains(socket, thread.RemovedSockets);
            }

            foreach (var socket in shouldKeep)
            {
                Assert.DoesNotContain(socket, thread.RemovedSockets);
                Assert.True(socket.UpdateCallCount >= 1);
            }
        }

        [Fact]
        public void Run_HighVolumeConnections_HandlesCorrectly()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var sockets = new List<MockSocket>();

            for (int i = 0; i < 100; i++)
            {
                var socket = new MockSocket($"s{i}");
                // 30% will close
                socket.UpdateReturnValue = (i % 10) < 7;
                sockets.Add(socket);
                thread.AddSocket(socket);
            }

            // Act
            thread.Start();
            Thread.Sleep(200);
            thread.Stop();
            thread.Wait();

            // Assert
            int expectedRemoved = sockets.Count(s => !s.UpdateReturnValue);
            int expectedRemaining = sockets.Count(s => s.UpdateReturnValue);

            Assert.Equal(expectedRemaining, thread.GetConnectionCount());
            Assert.Equal(expectedRemoved, thread.RemovedSockets.Count);
        }

        [Fact]
        public void AddSocket_DuringRun_ProcessedInNextCycle()
        {
            // Arrange
            var thread = new TestableNetworkThread();
            var socket1 = new MockSocket("initial");
            thread.AddSocket(socket1);

            // Act
            thread.Start();
            Thread.Sleep(30);

            // Add socket while running
            var socket2 = new MockSocket("added_during_run");
            thread.AddSocket(socket2);

            Thread.Sleep(50);
            thread.Stop();
            thread.Wait();

            // Assert
            Assert.Equal(2, thread.AddedSockets.Count);
            Assert.True(socket2.UpdateCallCount > 0, "Dynamically added socket should be updated");
        }
    }
}
