// See https://aka.ms/new-console-template for more information
// If we want to use a span-like class in asynchronous programming we could take advantage of Memory<> and ReadOnlyMemory<>
using System.Buffers;
using CommandLine;
using unpcap;

var hasHeader = true;
var workers = 2;

Parser.Default.ParseArguments<CommandLineOptions>(args)
      .WithParsed<CommandLineOptions>(o => {
           hasHeader = o.HasFileHeader;
           workers = o.Workers;
      })
      .WithNotParsed<CommandLineOptions>(o => {
           System.Environment.Exit(0);
      });

using var stdin = new BufferedStream(Console.OpenStandardInput());
using var stdout = new BufferedStream(Console.OpenStandardOutput());

var reader = new PcapReader(stdin);

var vitaRecords = reader
    .AsParallel()
    .AsOrdered()
    .WithDegreeOfParallelism(2)
    .Select(record => ExtractVita(record));

// write to stream in separate task
var writer = Task.Run(async () => await WriteResult(stdout, vitaRecords, reader.ArrayPool));
writer.Wait();

VitaRecord ExtractVita(PcapRecord record)
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
    [Option('w', "workers", Required = false, Default = 2, HelpText = "Amount of workers.")]
    public int Workers { get; set; }

    [Option('h', "with-pcap-file-header", Required = true, Default = true, HelpText = "Input stream has a PCAP file header")]
    public bool HasFileHeader { get; set; }
}