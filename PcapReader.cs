using System.Collections;
using System.Runtime.InteropServices;

namespace unpcap;

public class PcapReader : IEnumerable<PcapRecord>
{
    private readonly Stream input;
    private readonly byte[] buffer = new byte[64 * 1024 + 300]; // max UDP ptk size + 300 bytes for headers

    public PcapReader(Stream input)
    {
        this.input = input;
        var header = ReadFileHeader(input, buffer);
    }

    public IEnumerator<PcapRecord> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
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
    private PcapRecordHeader ReadRecordHeader(Stream stream, byte[] buffer)
    {
        var length = ReadFull(input, buffer, Constants.PcapRecordHeader_Length);
        if(length != Constants.PcapRecordHeader_Length) {
            throw new Exception("EOF: too short PCAP record header.");
        }
        return ArrayToStructure<PcapRecordHeader>(buffer);
    }

    private PcapFileHeader ReadFileHeader(Stream stream, byte[] buffer)
    {
        var length = ReadFull(input, buffer, Constants.PcapFileHeader_Length);
        if(length != Constants.PcapFileHeader_Length) {
            throw new Exception("EOF: too short PCAP file header.");
        }
        return ArrayToStructure<PcapFileHeader>(buffer);
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