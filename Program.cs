// See https://aka.ms/new-console-template for more information
// If we want to use a span-like class in asynchronous programming we could take advantage of Memory<> and ReadOnlyMemory<>
using CommandLine;
using unpcap;

// TODO: test BufferedStream <https://learn.microsoft.com/en-us/dotnet/api/system.io.bufferedstream?view=net-7.0>
using var stdin = new BufferedStream(Console.OpenStandardInput());
using var stdout = new BufferedStream(Console.OpenStandardOutput());

var reader = new PcapReader(stdin);
var parser = new PacketParser(reader.LinkLayer, reader.ByteOrder);

/*
    The concurrency should look like:
    - reader runs on own (main) thread
    - parsers run on their own threads (managed by PLINQ)
    - writer to stdout runs on its own thread.
*/

// parallel TODO: play with concurrency level
var query = reader
    .AsParallel()
    .AsOrdered()
    .Select(record => parser.ParseEthernet(record.Record));

// TODO: write to stream in another task/thread 
var writer = Task.Run(async () => await WriteResult(stdout, query));
writer.Wait();

async Task WriteToStream(Stream stream, ReadOnlyMemory<byte> buffer)
{
    await stream.WriteAsync(buffer);
    await stream.FlushAsync();
}

async Task WriteResult(BufferedStream stdout, ParallelQuery<ReadOnlyMemory<byte>> query)
{
    foreach (var item in query)
    {
        await WriteToStream(stdout, item);
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