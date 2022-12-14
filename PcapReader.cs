using System.Collections;

namespace unpcap;

// TODO: look into IEnumerableAsync<T>
public class PcapReader : IEnumerable<PcapRecord>
{
    private System.UInt32 capturedBytes;
    private readonly Stream input;
    private bool noData = false;

    public MagicNumber ByteOrder { get; private set; }
    public LinkLayer LinkLayer { get; private set; }

    public PcapReader(Stream input)
    {
        this.input = input;
        var (eof, header) = ReadFileHeader(input);
        if(eof) {
            noData = true;
        } else {
            ByteOrder = header.MagicNumber;
            LinkLayer = header.Network;
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

    # region Async
    async Task<PcapRecordHeader> ReadRecordHeaderAsync(Stream stream, byte[] buffer)
    {
        await ReadFullAsync(input, buffer, Constants.PcapRecordHeader_Length);
        return Tools.ArrayToStructure<PcapRecordHeader>(buffer);
    }

    async Task<PcapFileHeader> ReadFileHeaderAsync(Stream stream, byte[] buffer)
    {
        await ReadFullAsync(input, buffer, Constants.PcapFileHeader_Length);
        return Tools.ArrayToStructure<PcapFileHeader>(buffer);
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