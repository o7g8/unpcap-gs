// See https://aka.ms/new-console-template for more information
// f we want to use a span-like class in asynchronous programming we could take advantage of Memory<> and ReadOnlyMemory<>

using unpcap;

internal class PacketParser
{
    private LinkLayer linkLayer;
    private MagicNumber byteOrder;

    public PacketParser(LinkLayer linkLayer, MagicNumber byteOrder)
    {
        this.linkLayer = linkLayer;
        this.byteOrder = byteOrder;
    }

    public ReadOnlyMemory<byte> Parse(byte[] record)
    {
        if(linkLayer == LinkLayer.NullLoopback) {
            return ParseLoopback(record);
        } else if (linkLayer == LinkLayer.Ethernet) {
            return ParseEthernet(record);
        } else {
            throw new Exception($"Unknown link layer: {linkLayer}");
        }
    }

    private ReadOnlyMemory<byte> ParseEthernet(byte[] record)
    {
        throw new NotImplementedException();
    }

    private ReadOnlyMemory<byte> ParseLoopback(byte[] record)
    {
        var offset = ProtocolConstants.NullLoopbackHeader_Length
        + ProtocolConstants.IpProtocolHeader_Length
        + ProtocolConstants.UdpProtocolHeader_Length;
        return new ReadOnlyMemory<byte>(record, offset, record.Length - offset);
    }
}