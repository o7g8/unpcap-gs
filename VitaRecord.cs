namespace unpcap;

public struct VitaRecord {
    public byte[] Record {get; private set; }

    public int Offset {get; private set; }
    public int RecordLength {get; private set; }

    public VitaRecord(byte[] record, int offset, int recordLength)
    {
        Record = record;
        Offset = offset;
        RecordLength = recordLength;
    }
}