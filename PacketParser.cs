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

    public ReadOnlyMemory<byte> ParseEthernet(byte[] record)
    {
        // TODO: need to test and filter non-IP and non-UDP/TCP, best to see the actual DigIF example
        // https://code.amazon.com/packages/Pseudo-sat/blobs/bc6e6559be42c5265e0e8ff24fc18ee46bd13936/--/python/pseudo-sat.py#L266
        var offset = ProtocolConstants.EthernetHeader_Length
        + ProtocolConstants.IpProtocolHeader_Length
        + ProtocolConstants.UdpProtocolHeader_Length;
        return new ReadOnlyMemory<byte>(record, offset, record.Length - offset);
    }

    private static ReadOnlyMemory<byte> ParseEthernetWithVlan(byte[] record)
    {
        var offset = ProtocolConstants.EthernetHeader_Length
        + ProtocolConstants.VlanHeader_Length
        + ProtocolConstants.IpProtocolHeader_Length
        + ProtocolConstants.UdpProtocolHeader_Length;
        return new ReadOnlyMemory<byte>(record, offset, record.Length - offset);
    }

    private ReadOnlyMemory<byte> ParseLoopback(byte[] record)
    {
        var offset = ProtocolConstants.NullLoopbackHeader_Length
        + ProtocolConstants.IpProtocolHeader_Length
        + ProtocolConstants.UdpProtocolHeader_Length;
        return new ReadOnlyMemory<byte>(record, offset, record.Length - offset);
    }
}