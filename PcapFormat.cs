// https://wiki.wireshark.org/Development/LibpcapFileFormat
using System.Runtime.InteropServices;

namespace unpcap;

[Flags]
internal enum MagicNumbers : System.UInt32
{
    Identical = 0xa1b2c3d4,
    Swapped = 0xd4c3b2a1
}

struct Constants {
    public  const int PcapFileHeader_Length = 192;
    public  const int PcapRecordHeader_Length = 128;
}


// Length 192 bytes
[StructLayout(LayoutKind.Sequential)]
internal struct PcapFileHeader
{
    //System.UInt32 MagicNumber; // 0xa1b2c3d4 (identical) or 0xd4c3b2a1 (swapped)
    MagicNumbers MagicNumber; /* magic number */
    System.UInt16 VersionMajor; /* major version number */
    System.UInt16 VersionMinor; /* minor version number */
    System.UInt32 ThisZone; /* GMT to local correction */
    System.UInt32 SigFigs; /* accuracy of timestamps */
    System.UInt32 SnapLen; /* max length of captured packets, in octets */
    System.UInt32 Network; /* data link type */
}

// Length 128 bytes
[StructLayout(LayoutKind.Sequential)]
public struct PcapRecordHeader
{
    System.UInt32 TsSec; /* timestamp seconds */
    System.UInt32 TsUsec; /* timestamp microseconds */
    System.UInt32 InclLen; /* number of octets of packet saved in file */
    System.UInt32 OrigLen; /* actual length of packet */
}
 