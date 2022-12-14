// See https://aka.ms/new-console-template for more information
// If we want to use a span-like class in asynchronous programming we could take advantage of Memory<> and ReadOnlyMemory<>
using System.Buffers;
using CommandLine;
using unpcap;

// TODO: test BufferedStream <https://learn.microsoft.com/en-us/dotnet/api/system.io.bufferedstream?view=net-7.0>
using var stdin = new BufferedStream(Console.OpenStandardInput());
using var stdout = new BufferedStream(Console.OpenStandardOutput());

var reader = new PcapReader(stdin);
var parser = new PacketParser(reader.LinkLayer, reader.ByteOrder);

var query = reader
    .AsParallel()
    .AsOrdered()
    .WithDegreeOfParallelism(2)
    //.Select(record => parser.ParseEthernet(record.Record));
    .Select(record => ParseEthernet(record));

// TODO: write to stream in another task/thread 
var writer = Task.Run(async () => await WriteResult(stdout, query, reader.ArrayPool));
writer.Wait();

(byte[], int, int) ParseEthernet(PcapRecord record)
{
    // TODO: need to test and filter non-IP and non-UDP/TCP, best to see the actual DigIF example
    // https://code.amazon.com/packages/Pseudo-sat/blobs/bc6e6559be42c5265e0e8ff24fc18ee46bd13936/--/python/pseudo-sat.py#L266
    var offset = ProtocolConstants.EthernetHeader_Length
    + ProtocolConstants.IpProtocolHeader_Length
    + ProtocolConstants.UdpProtocolHeader_Length;
    //return new ReadOnlyMemory<byte>(record, offset, record.Length - offset);
    return (record.Record, offset, record.RecordLength - offset);
}

async Task WriteToStream(Stream stream, ReadOnlyMemory<byte> buffer)
{
    await stream.WriteAsync(buffer);
    await stream.FlushAsync();
}

async Task WriteResult(BufferedStream stdout, IEnumerable<(byte[], int, int)> query, ArrayPool<byte> arrayPool)
{
    foreach (var item in query)
    {
        await stdout.WriteAsync(item.Item1, item.Item2, item.Item3);
        await stdout.FlushAsync();
        arrayPool.Return(item.Item1);
    }
}

/*
async Task WriteResult(BufferedStream stdout, IEnumerable<ReadOnlyMemory<byte>> query)
{
    foreach (var item in query)
    {
        await WriteToStream(stdout, item);
    }
}
*/
public class CommandLineOptions
{
    [Option('e', "endianness", Required = true, HelpText = "Identical (0), swapped (1)")]
    public int Endianness { get; set; }

    [Option('h', "has-file-header", Required = true, Default = true, HelpText = "Indicate if the stream has a PCAP file header")]
    public bool HasFileHeader { get; set; }

    [Option('p', "payload-protocol", Required = true, Default = "none", HelpText = "Payload protocol: none | udp | tcp")]
    public string? PayloadProtocol { get; set; }
}