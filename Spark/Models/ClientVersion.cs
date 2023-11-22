using System;
using System.Collections.Generic;

namespace Spark.Models;

[Serializable]
public sealed class ClientVersion
{
    #region Client Version Comparaer

    private sealed class ClientVersionComparer : IEqualityComparer<ClientVersion>
    {
        public bool Equals(ClientVersion a, ClientVersion b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a == null || b == null)
                return false;

            return a.VersionCode == b.VersionCode && a.Name.Equals(b.Name, StringComparison.Ordinal);
        }

        public int GetHashCode(ClientVersion version)
        {
            return version.Name.GetHashCode() ^ version.VersionCode.GetHashCode();
        }
    }

    public static readonly IEqualityComparer<ClientVersion> VersionComparer = new ClientVersionComparer();
    #endregion

    #region Standard Client Versions

    public static readonly ClientVersion Version913 = new()
    {
        Name = "US Zolian 9.13",
        Hostname = "66.211.203.116",
        Port = 4200,
        VersionCode = 913,
        Hash = "3244dc0e68cd26f4fb1626da3673fda8",
        ServerHostnamePatchAddress = 0x4333C2,
        ServerPortPatchAddress = 0x4333E4,
        IntroVideoPatchAddress = 0x42E61F,
        MultipleInstancePatchAddress = 0x57A7CE,
        HideWallsPatchAddress = 0x5FD874,
        SkipHostnamePatchAddress = 0x433391
    };

    public static readonly ClientVersion Version741 = new()
    {
        Name = "US Dark Ages 7.41",
        Hostname = "da0.kru.com",
        Port = 2610,
        VersionCode = 741,
        Hash = "3244dc0e68cd26f4fb1626da3673fda8",
        ServerHostnamePatchAddress = 0x4333C2,
        ServerPortPatchAddress = 0x4333E4,
        IntroVideoPatchAddress = 0x42E61F,
        MultipleInstancePatchAddress = 0x57A7CE,
        HideWallsPatchAddress = 0x5FD874,
        SkipHostnamePatchAddress = 0x433391
    };

    #endregion

    #region Properties
    public string Name { get; set; }
    public string Hostname { get; set; }
    public int Port { get; set; }
    public int VersionCode { get; set; }
    public string Hash { get; set; }
    public long ServerHostnamePatchAddress { get; set; }
    public long ServerPortPatchAddress { get; set; }
    public long IntroVideoPatchAddress { get; set; }
    public long MultipleInstancePatchAddress { get; set; }
    public long HideWallsPatchAddress { get; set; }
    public long SkipHostnamePatchAddress { get; set; }
    #endregion

    public ClientVersion() { }

    public static IEnumerable<ClientVersion> GetDefaultVersions()
    {
        yield return Version741;
        yield return Version913;
    }
}