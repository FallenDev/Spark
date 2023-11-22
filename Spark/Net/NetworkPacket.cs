using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spark.Common;

namespace Spark.Net;

public sealed class NetworkPacket : INetworkPacket
{
    public static readonly int HeaderSize = 4;
    public static readonly byte NexonSignature = 0xAA;

    #region Properties
    public byte Signature { get; protected set; }
    public short Size { get; protected set; }
    public byte Command { get; protected set; }
    public IReadOnlyList<byte> Data { get; protected set; }
    #endregion

    #region Constructors
    public NetworkPacket(byte signature, short size, byte command, IEnumerable<byte> data = null)
    {
        Signature = signature;
        Size = size;
        Command = command;

        if (data != null)
            Data = data.ToList();
    }

    public NetworkPacket(byte signature, short size, byte command, byte[] data, int offset, int count)
        : this(signature, size, command)
    {
        if (data != null)
            Data = new ArraySegment<byte>(data, offset, count).ToList();
    }

    public NetworkPacket(params byte[] packet)
        : this(packet, 0, packet.Length) { }

    public NetworkPacket(byte[] packet, int offset, int count)
        : this(packet[offset], IntegerExtender.MakeWord(packet[offset + 2], packet[offset + 1]), packet[offset + 3], packet, HeaderSize, count - HeaderSize) { }
    #endregion

    public override string ToString()
    {
        // Returns a hex string of the packet ("AA 00 03 FF ...")
        return string.Join(" ", from byteValue in this
            select byteValue.ToString("X2"));
    }

    #region IEnumerable<byte> Methods
    public IEnumerator<byte> GetEnumerator()
    {
        yield return Signature;
        yield return Size.HiByte();
        yield return Size.LoByte();
        yield return Command;

        if (Data != null)
        {
            foreach (var dataByte in Data)
                yield return dataByte;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion
}