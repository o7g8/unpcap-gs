using System.Collections;
using System.Buffers;

namespace unpcap;

public class PcapReader : IEnumerable<PcapRecord>
{
    private System.UInt32 capturedBytes;
    private readonly Stream input;

    public MagicNumber ByteOrder { get; private set; }
    public LinkLayer LinkLayer { get; private set; }

    public readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;

    public PcapReader(Stream input) : this(input, true)
    {
    }

    public PcapReader(Stream input, bool hasHeader)
    {
        this.input = input;
        if(hasHeader) {
            var (eof, header) = ReadFileHeader(input);
            if(!eof) {
                ByteOrder = header.MagicNumber;
                LinkLayer = header.Network;
                capturedBytes = header.SnapLen;
            }
        }
    }

    public IEnumerator<PcapRecord> GetEnumerator()
    {
        while (true)
        {
            var (eof, recordHeader) = ReadRecordHeader(input);
            if(eof) {
                yield break;
            }

            var packetLength = (int)(recordHeader.InclLen);
            var buffer = ArrayPool.Rent(packetLength);

            var bytesRead = ReadFull(input, buffer, packetLength);
            yield return new PcapRecord(buffer, bytesRead);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    # region Blocking
    // returns (eof, data)
    private (bool, PcapRecordHeader) ReadRecordHeader(Stream stream)
    {
        var buffer = new byte[Constants.PcapRecordHeader_Length];
        var length = ReadFull(input, buffer, Constants.PcapRecordHeader_Length);
        if(length == 0) {
            return (true, new PcapRecordHeader());
        }
        if(length != Constants.PcapRecordHeader_Length) {
            throw new Exception("EOF: too short PCAP record header.");
        }
        return (false, Tools.ArrayToStructure<PcapRecordHeader>(buffer));
    }

    private (bool, PcapFileHeader) ReadFileHeader(Stream stream)
    {
        var buffer = new byte[Constants.PcapFileHeader_Length];
        var length = ReadFull(input, buffer, Constants.PcapFileHeader_Length);
        if(length == 0) {
            return (true, new PcapFileHeader());
        }
        if(length != Constants.PcapFileHeader_Length) {
            throw new Exception("EOF: too short PCAP file header.");
        }
        return (false, Tools.ArrayToStructure<PcapFileHeader>(buffer));
    }

    private int ReadFull(Stream stream, byte[] buffer, int length)
    {
        var bytesToRead = length;
        var offset = 0;
        while (bytesToRead > 0)
        {
            var bytesRead = stream.Read(buffer, offset, bytesToRead);
            if(bytesRead == 0) {
                // EOF
                return offset;
            }
            bytesToRead = bytesToRead - bytesRead;
            offset = offset + bytesRead;
        }
        return length;
    }
    # endregion
}