using System;
using System.Collections.Generic;

namespace Spark.Models
{
    [Serializable]
    public sealed class ClientVersion
    {
        #region Client Version Comparaer
        sealed class ClientVersionComparer : IEqualityComparer<ClientVersion>
        {
            public bool Equals(ClientVersion a, ClientVersion b)
            {
                if (Object.ReferenceEquals(a, b))
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

        public static readonly ClientVersion Version718 = new()
        {
            Name = "CA Legends 7.18",
            VersionCode = 718,
            Hash = "36f4689b09a4a91c74555b3c3603b196",
            ServerHostnamePatchAddress = 0x4341FA,
            ServerPortPatchAddress = 0x434224,
            IntroVideoPatchAddress = 0x42F48F,
            MultipleInstancePatchAddress = 0x5911AE,
            HideWallsPatchAddress = 0x624BC4
        };

        public static readonly ClientVersion Version913 = new()
        {
            Name = "US Zolian 9.13",
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
            yield return Version718;
            yield return Version741;
            yield return Version913;
        }
    }
}
