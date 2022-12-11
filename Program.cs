// See https://aka.ms/new-console-template for more information
// f we want to use a span-like class in asynchronous programming we could take advantage of Memory<> and ReadOnlyMemory<>

using System.Runtime.InteropServices;
using CommandLine;
using unpcap;


using var stdin = Console.OpenStandardInput();
using var stdout = Console.OpenStandardOutput();

var reader = new PcapReader(stdin);
foreach (var record in reader)
{
    Console.WriteLine($"{record.Header.InclLen} {record.Header.OrigLen}");
}

async Task<int> WriteToStream(Stream stream, byte[] buffer, int length)
{
    await stream.WriteAsync(buffer, 0, length);
    await stream.FlushAsync();
    return length;
}

public class CommandLineOptions
{
    [Option('e', "endianness", Required = true, HelpText = "Identical (0), swapped (1)")]
    public int Endpoint { get; set; }

    [Option('h', "has-file-header", Required = true, Default = true, HelpText = "Indicate if the stream has a PCAP file header")]
    public bool HasFileHeader { get; set; }

    [Option('p', "payload-protocol", Required = true, Default = "none", HelpText = "Payload protocol: none | udp | tcp")]
    public string? PayloadProtocol { get; set; }
}