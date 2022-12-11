using System.Collections;
using System.Runtime.InteropServices;

namespace unpcap;

public class PcapReader : IEnumerable<PcapRecord>
{
    private MagicNumber byteOrder;
    private System.UInt32 capturedBytes;
    private readonly Stream input;
    private bool noData = false;

    public PcapReader(Stream input)
    {
        this.input = input;
        var (eof, header) = ReadFileHeader(input);
        if(eof) {
            noData = true;
        } else {
            byteOrder = header.MagicNumber;
            capturedBytes = header.SnapLen;
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
            var buffer = new byte[packetLength];

            var bytesRead = ReadFull(input, buffer, packetLength);
            var record = bytesRead < packetLength ?
                buffer[0..bytesRead]
                : buffer;
            yield return new PcapRecord(recordHeader, record);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    // https://www.codeproject.com/Articles/5296628/Fast-Conversions-between-tightly-packed-Structures
    private S ArrayToStructure<S>(byte[] abSource) where S : struct
    {
        var iHandle = GCHandle.Alloc(abSource, GCHandleType.Pinned);
        S rTarget;
        try
        {
            rTarget = Marshal.PtrToStructure<S>(iHandle.AddrOfPinnedObject());
        }
        finally
        {
            iHandle.Free();
        }
        return rTarget;
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
        return (false, ArrayToStructure<PcapRecordHeader>(buffer));
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
        return (false, ArrayToStructure<PcapFileHeader>(buffer));
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

    # region Async
    async Task<PcapRecordHeader> ReadRecordHeaderAsync(Stream stream, byte[] buffer)
    {
        await ReadFullAsync(input, buffer, Constants.PcapRecordHeader_Length);
        return ArrayToStructure<PcapRecordHeader>(buffer);
    }

    async Task<PcapFileHeader> ReadFileHeaderAsync(Stream stream, byte[] buffer)
    {
        await ReadFullAsync(input, buffer, Constants.PcapFileHeader_Length);
        return ArrayToStructure<PcapFileHeader>(buffer);
    }

    async Task<int> ReadFullAsync(Stream stream, byte[] buffer, int length)
    {
        var bytesToRead = length;
        var offset = 0;
        while (bytesToRead > 0)
        {
            var bytesRead = await stream.ReadAsync(buffer, offset, bytesToRead);
            bytesToRead = bytesToRead - bytesRead;
            offset = offset + bytesRead;
        }
        return length;
    }

    # endregion
}