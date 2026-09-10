namespace HermesProxy;

internal static class Trimming
{
    // Justification for [UnconditionalSuppressMessage] on reflection the trim analyzer cannot
    // follow. It holds only while HermesProxy.csproj keeps both assemblies as TrimmerRootAssembly.
    public const string RootedAssembly =
        "HermesProxy and Framework are TrimmerRootAssembly in HermesProxy.csproj, so every type and member reflected over here survives trimming.";
}
