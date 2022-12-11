namespace unpcap;

public class PcapRecord
{
    public PcapRecordHeader Header {get; private set; }
    public byte[] Record {get; private set; }

    public PcapRecord(PcapRecordHeader header, byte[] record)
    {
        Header = header;
        Record = record;
    }
}