using System.Text;
using Framework.IO;
using Xunit;

namespace HermesProxy.Tests.Framework
{
    public class ByteBufferReadCStringTests
    {
        [Fact]
        public void ReadCString_WithEmptyString_ReturnsEmpty()
        {
            // Arrange - just a null terminator
            var data = new byte[] { 0x00 };
            using var buffer = new ByteBuffer(data);

            // Act
            var result = buffer.ReadCString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ReadCString_WithSimpleAscii_ReturnsCorrectString()
        {
            // Arrange - "Hello" + null terminator
            var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00 };
            using var buffer = new ByteBuffer(data);

            // Act
            var result = buffer.ReadCString();

            // Assert
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void ReadCString_WithUtf8_ReturnsCorrectString()
        {
            // Arrange - UTF-8 encoded string with multi-byte characters
            var testString = "Héllo Wörld";
            var stringBytes = Encoding.UTF8.GetBytes(testString);
            var data = new byte[stringBytes.Length + 1];
            stringBytes.CopyTo(data, 0);
            data[^1] = 0x00; // null terminator

            using var buffer = new ByteBuffer(data);

            // Act
            var result = buffer.ReadCString();

            // Assert
            Assert.Equal(testString, result);
        }

        [Fact]
        public void ReadCString_WithMultipleStrings_ReadsSequentially()
        {
            // Arrange - "One" + null + "Two" + null
            var data = new byte[] { 0x4F, 0x6E, 0x65, 0x00, 0x54, 0x77, 0x6F, 0x00 };
            using var buffer = new ByteBuffer(data);

            // Act
            var result1 = buffer.ReadCString();
            var result2 = buffer.ReadCString();

            // Assert
            Assert.Equal("One", result1);
            Assert.Equal("Two", result2);
        }

        [Fact]
        public void ReadCString_WithLongString_ReturnsCorrectString()
        {
            // Arrange - string longer than 256 bytes (stackalloc threshold)
            var testString = new string('A', 300);
            var stringBytes = Encoding.UTF8.GetBytes(testString);
            var data = new byte[stringBytes.Length + 1];
            stringBytes.CopyTo(data, 0);
            data[^1] = 0x00;

            using var buffer = new ByteBuffer(data);

            // Act
            var result = buffer.ReadCString();

            // Assert
            Assert.Equal(testString, result);
            Assert.Equal(300, result.Length);
        }

        [Fact]
        public void ReadCString_With256ByteString_UsesStackalloc()
        {
            // Arrange - exactly 256 bytes (boundary case for stackalloc)
            var testString = new string('B', 256);
            var stringBytes = Encoding.UTF8.GetBytes(testString);
            var data = new byte[stringBytes.Length + 1];
            stringBytes.CopyTo(data, 0);
            data[^1] = 0x00;

            using var buffer = new ByteBuffer(data);

            // Act
            var result = buffer.ReadCString();

            // Assert
            Assert.Equal(testString, result);
            Assert.Equal(256, result.Length);
        }

        [Fact]
        public void ReadCString_WithSpecialCharacters_ReturnsCorrectString()
        {
            // Arrange - string with special UTF-8 characters (emoji, CJK)
            var testString = "Test 🎮 游戏";
            var stringBytes = Encoding.UTF8.GetBytes(testString);
            var data = new byte[stringBytes.Length + 1];
            stringBytes.CopyTo(data, 0);
            data[^1] = 0x00;

            using var buffer = new ByteBuffer(data);

            // Act
            var result = buffer.ReadCString();

            // Assert
            Assert.Equal(testString, result);
        }

        [Fact]
        public void ReadCString_PositionAdvancesCorrectly()
        {
            // Arrange - "ABC" + null + extra bytes
            var data = new byte[] { 0x41, 0x42, 0x43, 0x00, 0xFF, 0xFF };
            using var buffer = new ByteBuffer(data);

            // Act
            var result = buffer.ReadCString();
            var nextByte = buffer.ReadUInt8();

            // Assert
            Assert.Equal("ABC", result);
            Assert.Equal(0xFF, nextByte);
        }
    }
}
