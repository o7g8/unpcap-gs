using System.Buffers;
using CommandLine;
using unpcap;

var hasHeader = false;
var workers = 2;

// NB! the parsing takes ~1 sec!!
Parser.Default.ParseArguments<CommandLineOptions>(args)
      .WithParsed<CommandLineOptions>(o => {
           hasHeader = o.HasPcapFileHeader;
           workers = o.Workers;
      });

using var stdin = new BufferedStream(Console.OpenStandardInput());
using var stdout = new BufferedStream(Console.OpenStandardOutput());

var reader = new PcapReader(stdin, hasHeader);
var vitaRecords = BuildPcapQuery(reader, workers)
    .Select(record => ExtractVita(record));

// write results in a separate task
var writer = Task.Run(async () => await WriteResult(stdout, vitaRecords, reader.ArrayPool));
writer.Wait();

IEnumerable<PcapRecord> BuildPcapQuery(PcapReader reader, int parallelism) {
    if(parallelism == 1) {
        return reader;
    }
    var query = reader
    .AsParallel()
    .AsOrdered();

    Console.Error.WriteLine($"using parallelism {parallelism}");
    return parallelism == 0
    ? query
    : query.WithDegreeOfParallelism(parallelism);
}

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
    [Option('p', "with-pcap-file-header", Required = false, Default = false, HelpText = "The input stream has PCAP file header")]
    public bool HasPcapFileHeader { get; set; }

    [Option('w', "workers", Required = false, Default = 2, HelpText = "Amount of workers.")]
    public int Workers { get; set; }
}