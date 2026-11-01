using System;

namespace HermesProxy.Configuration;

/// <summary>
/// A class bound to the configuration section of the same name. Every property with a public
/// setter is a setting and joins <see cref="ConfigReference"/>; <c>HPSG010</c> fails the build for
/// one without <see cref="SettingAttribute"/>. A value the proxy derives gets an internal setter.
/// </summary>
/// <param name="order">Where the section goes in <c>--help</c> and docs/configuration.md.</param>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class ConfigSectionAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}

/// <summary>What a setting does, in the words <c>--help</c> and docs/configuration.md show.</summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class SettingAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
