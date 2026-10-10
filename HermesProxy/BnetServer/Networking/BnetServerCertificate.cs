using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Framework.Logging;

namespace BNetServer;

public static class BnetServerCertificate
{
    private const string BNET_SERVER_CERT_RESOURCE = "HermesProxy.BNetServer.pfx";

    private static X509Certificate2? _certificate;

    public static X509Certificate2 Certificate => _certificate ??= LoadEmbedded();

    /// <summary>
    /// True for TrinityCore's development wildcard certificate (common name <c>*.*</c>), which the
    /// embedded one is. No system trust store accepts it.
    /// </summary>
    public static bool IsDevWildcard => Certificate.GetNameInfo(X509NameType.SimpleName, false) == "*.*";

    /// <summary>
    /// Loads the certificate served on the BNet TLS endpoints. With a null/empty path the
    /// embedded TrinityCore-compatible certificate is used — the chain most patched 3.4.3
    /// clients are pinned to. A custom pfx is only needed for setups validating against the
    /// system trust store (e.g. wowpatch).
    /// </summary>
    public static void Initialize(string? pfxPath, string? pfxPassword)
    {
        if (string.IsNullOrEmpty(pfxPath))
        {
            _certificate = LoadEmbedded();
        }
        else
        {
            _certificate = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, pfxPassword);
            Log.Print(LogType.Server, $"Using custom BNet TLS certificate from '{pfxPath}'");
        }

        Log.Print(LogType.Server, $"BNet TLS certificate: '{_certificate.Subject}' issued by '{_certificate.Issuer}', expires {_certificate.NotAfter:yyyy-MM-dd}");
    }

    private static X509Certificate2 LoadEmbedded()
    {
        Assembly currentAsm = Assembly.GetExecutingAssembly();
        using (var stream = currentAsm.GetManifestResourceStream(BNET_SERVER_CERT_RESOURCE))
        {
            if (stream == null)
                throw new Exception($"Resource not found: '{BNET_SERVER_CERT_RESOURCE}'");
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();
            return X509CertificateLoader.LoadPkcs12(bytes, null);
        }
    }
}
