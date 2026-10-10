using System.Runtime.Serialization;

namespace Framework.Web;

/// <summary>
/// The answer to <c>POST /bnetserver/login/srp/</c>: what the client needs to compute its half of
/// an SRP login. Numbers are uppercase big-endian hex.
/// </summary>
[DataContract]
public sealed class SrpLoginChallenge
{
    [DataMember(Name = "version")]
    public uint Version { get; set; }

    [DataMember(Name = "iterations")]
    public uint Iterations { get; set; }

    [DataMember(Name = "modulus")]
    public string? Modulus { get; set; }

    [DataMember(Name = "generator")]
    public string? Generator { get; set; }

    [DataMember(Name = "hash_function")]
    public string? HashFunction { get; set; }

    [DataMember(Name = "username")]
    public string? Username { get; set; }

    [DataMember(Name = "salt")]
    public string? Salt { get; set; }

    [DataMember(Name = "public_B")]
    public string? PublicB { get; set; }
}
