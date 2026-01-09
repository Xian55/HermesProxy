using System;
using Framework.Util;
using Xunit;

namespace HermesProxy.Tests.Framework
{
    public class ExtensionsCombineTests
    {
        [Fact]
        public void Combine_WithSingleArray_ReturnsCombined()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02 };
            var toAdd = new byte[] { 0x03, 0x04 };

            // Act
            var result = data.Combine(toAdd);

            // Assert
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, result);
        }

        [Fact]
        public void Combine_WithMultipleArrays_ReturnsCombined()
        {
            // Arrange
            var data = new byte[] { 0x01 };
            var arr1 = new byte[] { 0x02, 0x03 };
            var arr2 = new byte[] { 0x04, 0x05, 0x06 };
            var arr3 = new byte[] { 0x07 };

            // Act
            var result = data.Combine(arr1, arr2, arr3);

            // Assert
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }, result);
        }

        [Fact]
        public void Combine_WithEmptyInitialArray_ReturnsCombined()
        {
            // Arrange
            var data = Array.Empty<byte>();
            var toAdd = new byte[] { 0x01, 0x02, 0x03 };

            // Act
            var result = data.Combine(toAdd);

            // Assert
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, result);
        }

        [Fact]
        public void Combine_WithEmptyArrayToAdd_ReturnsOriginal()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02, 0x03 };
            var toAdd = Array.Empty<byte>();

            // Act
            var result = data.Combine(toAdd);

            // Assert
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, result);
        }

        [Fact]
        public void Combine_WithNoArraysToAdd_ReturnsOriginal()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02, 0x03 };

            // Act
            var result = data.Combine();

            // Assert
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, result);
        }

        [Fact]
        public void Combine_WithLargeArrays_ReturnsCombined()
        {
            // Arrange
            var data = new byte[1000];
            var arr1 = new byte[2000];
            var arr2 = new byte[3000];

            for (int i = 0; i < data.Length; i++) data[i] = 0x01;
            for (int i = 0; i < arr1.Length; i++) arr1[i] = 0x02;
            for (int i = 0; i < arr2.Length; i++) arr2[i] = 0x03;

            // Act
            var result = data.Combine(arr1, arr2);

            // Assert
            Assert.Equal(6000, result.Length);
            Assert.Equal(0x01, result[0]);
            Assert.Equal(0x01, result[999]);
            Assert.Equal(0x02, result[1000]);
            Assert.Equal(0x02, result[2999]);
            Assert.Equal(0x03, result[3000]);
            Assert.Equal(0x03, result[5999]);
        }

        [Fact]
        public void Combine_PreservesOriginalArrayUnchanged()
        {
            // Arrange
            var original = new byte[] { 0x01, 0x02 };
            var originalCopy = new byte[] { 0x01, 0x02 };
            var toAdd = new byte[] { 0x03, 0x04 };

            // Act
            var result = original.Combine(toAdd);

            // Assert - result is new array, original unchanged in content
            Assert.Equal(4, result.Length);
            Assert.Equal(originalCopy, original);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 10)]
        [InlineData(100, 100)]
        [InlineData(1000, 1000)]
        public void Combine_VariousSizes_ReturnsCorrectLength(int size1, int size2)
        {
            // Arrange
            var data = new byte[size1];
            var toAdd = new byte[size2];
            new Random(42).NextBytes(data);
            new Random(43).NextBytes(toAdd);

            // Act
            var result = data.Combine(toAdd);

            // Assert
            Assert.Equal(size1 + size2, result.Length);

            // Verify contents
            for (int i = 0; i < size1; i++)
                Assert.Equal(data[i], result[i]);
            for (int i = 0; i < size2; i++)
                Assert.Equal(toAdd[i], result[size1 + i]);
        }

        [Fact]
        public void Combine_TypicalCryptoUsage_WorksCorrectly()
        {
            // Simulate typical usage: BitConverter.GetBytes(counter).Combine(BitConverter.GetBytes(magic))
            var counter = BitConverter.GetBytes(12345678UL);
            var magic = BitConverter.GetBytes(0x52565253);

            // Act
            var result = counter.Combine(magic);

            // Assert
            Assert.Equal(12, result.Length); // 8 + 4
            Assert.Equal(counter, result[..8]);
            Assert.Equal(magic, result[8..]);
        }
    }
}
