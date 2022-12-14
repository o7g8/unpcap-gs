// See https://aka.ms/new-console-template for more information
// If we want to use a span-like class in asynchronous programming we could take advantage of Memory<> and ReadOnlyMemory<>
using System.Buffers;
using CommandLine;
using unpcap;

using var stdin = new BufferedStream(Console.OpenStandardInput());
using var stdout = new BufferedStream(Console.OpenStandardOutput());

var reader = new PcapReader(stdin);

var query = reader
    .AsParallel()
    .AsOrdered()
    .WithDegreeOfParallelism(2)
    .Select(record => ParseEthernet(record));

// write to stream in another task/thread 
var writer = Task.Run(async () => await WriteResult(stdout, query, reader.ArrayPool));
writer.Wait();

VitaRecord ParseEthernet(PcapRecord record)
{
    var offset = ProtocolConstants.EthernetHeader_Length
    + ProtocolConstants.IpProtocolHeader_Length
    + ProtocolConstants.UdpProtocolHeader_Length;
    return new VitaRecord(record.Record, offset, record.RecordLength - offset);
}


async Task WriteResult(BufferedStream stdout, IEnumerable<VitaRecord> query, ArrayPool<byte> arrayPool)
{
    foreach (var item in query)
    {
        await stdout.WriteAsync(item.Record, item.Offset, item.RecordLength);
        await stdout.FlushAsync();
        arrayPool.Return(item.Record);
    }
}

public class CommandLineOptions
{
    [Option('e', "endianness", Required = true, HelpText = "Identical (0), swapped (1)")]
    public int Endianness { get; set; }

    [Option('h', "has-file-header", Required = true, Default = true, HelpText = "Indicate if the stream has a PCAP file header")]
    public bool HasFileHeader { get; set; }

    [Option('p', "payload-protocol", Required = true, Default = "none", HelpText = "Payload protocol: none | udp | tcp")]
    public string? PayloadProtocol { get; set; }
}