// See https://aka.ms/new-console-template for more information
// f we want to use a span-like class in asynchronous programming we could take advantage of Memory<> and ReadOnlyMemory<>

using System.Runtime.InteropServices;
using CommandLine;
using unpcap;


using var stdin = Console.OpenStandardInput();
using var stdout = Console.OpenStandardOutput();

var reader = new PcapReader(stdin);
var parser = new PacketParser(reader.LinkLayer, reader.ByteOrder);

/*
foreach (var record in reader)
{
    Console.WriteLine($"{record.Header.InclLen} {record.Header.OrigLen}");
    var result = parser.Parse(record.Record);
}
*/

// parallel
var query = reader
    .AsParallel()
    .AsOrdered()
    .Select(record => parser.Parse(record.Record));
foreach (var item in query)
{
    await WriteToStream(stdout, item);
}

async Task WriteToStream(Stream stream, ReadOnlyMemory<byte> buffer)
{
    await stream.WriteAsync(buffer);
    await stream.FlushAsync();
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