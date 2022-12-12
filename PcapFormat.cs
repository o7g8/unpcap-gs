// https://wiki.wireshark.org/Development/LibpcapFileFormat
using System.Runtime.InteropServices;

namespace unpcap;

struct Constants {
    public  const int PcapFileHeader_Length = 24;
    public  const int PcapRecordHeader_Length = 16;
}

[Flags]
public enum MagicNumber : System.UInt32
{
    Identical = 0xa1b2c3d4,
    Swapped = 0xd4c3b2a1
}

[Flags]
public enum LinkLayer : System.UInt32
{
    NullLoopback = 0x00000000,
    Ethernet = 0x00000001
}

// https://wiki.wireshark.org/Development/LibpcapFileFormat
// https://www.netresec.com/?page=Blog&month=2022-10&post=What-is-a-PCAP-file
[StructLayout(LayoutKind.Sequential)]
internal struct PcapFileHeader
{
    public MagicNumber MagicNumber; /* magic number */
    public System.UInt16 VersionMajor; /* major version number */
    public System.UInt16 VersionMinor; /* minor version number */
    public System.UInt32 ThisZone; /* GMT to local correction */
    public System.UInt32 SigFigs; /* accuracy of timestamps */
    public System.UInt32 SnapLen; /* max length of captured packets, in octets */
    public LinkLayer Network; /* data link type */
}

[StructLayout(LayoutKind.Sequential)]
public struct PcapRecordHeader
{
    public System.UInt32 TsSec; /* timestamp seconds */
    public System.UInt32 TsUsec; /* timestamp microseconds */
    public System.UInt32 InclLen; /* number of octets of packet saved in file */
    public System.UInt32 OrigLen; /* actual length of packet */
}
 