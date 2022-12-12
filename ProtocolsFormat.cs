using System.Runtime.InteropServices;

namespace unpcap;


public class ProtocolConstants {
    public const int NullLoopbackHeader_Length = 4;
    public const int EthernetHeader_Length = 14;
    public const int VlanHeader_Length = 4;

    public const int IpProtocolHeader_Length = 20;

    public const int UdpProtocolHeader_Length = 8;
}

// see https://wiki.wireshark.org/NullLoopback.md
[Flags]
public enum LoopbackProtocolType : System.UInt32
{
    IPv4 = 2,
    //AF_INET6
    IPv6_1 = 10,
    IPv6_2 = 24,
    IPv6_3 = 28,
    IPv6_4 = 30,
}

// https://wiki.wireshark.org/NullLoopback.md
[StructLayout(LayoutKind.Sequential)]
internal struct NullLoopbackHeader
{
    public LoopbackProtocolType Protocol;
}

[StructLayout(LayoutKind.Sequential)]
internal struct EthernetHeader
{
    public System.Byte DST_MAC_1;
    public System.Byte DST_MAC_2;
    public System.Byte DST_MAC_3;
    public System.Byte DST_MAC_4;
    public System.Byte DST_MAC_5;
    public System.Byte DST_MAC_6;

    public System.Byte SRC_MAC_1;
    public System.Byte SRC_MAC_2;
    public System.Byte SRC_MAC_3;
    public System.Byte SRC_MAC_4;
    public System.Byte SRC_MAC_5;
    public System.Byte SRC_MAC_6;

    public EthernetProtocolType EthernetProtocolType;
}

[StructLayout(LayoutKind.Sequential)]
internal struct VlanHeader
{
    public System.UInt16 VlanId;
    public System.UInt16 VlanEtype;
}

[Flags]
internal enum EthernetProtocolType : System.UInt16
{
    IP = 0x0800,
    VLAN = 0x8100
}

// The first 20 bytes from https://en.wikipedia.org/wiki/Internet_Protocol_version_4
[StructLayout(LayoutKind.Sequential)]
internal struct IpHeader
{
    public System.Byte Version_Ihl;
    public System.Byte Tos;
    public System.UInt16 TotalLength;
    public System.UInt16 Identification;
    public System.UInt16 Flags_FragmentOffset;
    public System.Byte TTL;
    public System.Byte Protocol;
    public System.UInt16 HeaderChecksum;
    public System.UInt32 SourceAddress;
    public System.UInt32 DestinationAddress;

}