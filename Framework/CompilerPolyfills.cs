// Polyfills for C# 15 union types — required while targeting .NET 11 Preview 3.
// Remove once these types ship in the runtime (.NET 11 GA).
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute;

public interface IUnion
{
    object? Value { get; }
}
