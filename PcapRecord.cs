namespace unpcap;

public struct PcapRecord
{
    public byte[] Record {get; private set; }

    public int RecordLength {get; private set; }

    public PcapRecord(byte[] record, int recordLength)
    {
        Record = record;
        RecordLength = recordLength;
    }
}
