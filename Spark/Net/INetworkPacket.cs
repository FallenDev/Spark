using System.Collections.Generic;

namespace Spark.Net;

public interface INetworkPacket : IEnumerable<byte>
{
    byte Signature { get; }
    short Size { get; }
    byte Command { get; }
    IReadOnlyList<byte> Data { get; }
}