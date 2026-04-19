using System.Runtime.CompilerServices;
using Framework;
using HermesProxy.Enums;

namespace HermesProxy.Tests;

internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }
}
