# unpcap Read payload from PCAP files

Usage:

```bash
cat <pcap-file> | unpcap -u > result
```

### Record test capture

```bash
sudo tcpdump -w capture.pcap -i lo0 udp port 6666
```

Listen to the data:

```bash
ncat -l -u localhost 6666 > zz
```

Send test data:

```bash
cat README-Blink-AMI.md | ncat -u localhost 6666
```

### Performance

```bash
dotnet run - <GS_1G.pcap >GS_1G.vita
# compare md5 of the `GS_1G.vita` with `GS_1G.vita.expected`
md5 GS_1G.vita*
time dotnet run - <GS_1G.pcap >/dev/null
```

Possible variable values:
Alloc = Array | ArrayPool. Affects GC.
Return = Memory | byte[] | Span
PLINQ = Default | None | custom parallelism
Streams = unbuffered | buffered*
Writer = MainThread | NewTask
InputStorage = SDD | RAM*

Preferences:

* Streams = buffered

* InputStorage = RAM

* Writer = NewTask. minimally better

* PLINQ = 2

### Test runs

PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=default, Streams=unbuffered, Writer=MainThread, Input=1GB: 5.58s
PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=default, Streams=buffered, Writer=MainThread, Input=1GB: 3.38 (4.76s)
PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=default, Streams=buffered, Writer=NewTask, Input=1GB: 3.28
PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=10, Streams=buffered, Writer=NewTask, Input=1GB: 4.65
PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=30, Streams=buffered, Writer=NewTask, Input=1GB: 7.98s
PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=3, Streams=buffered, Writer=NewTask, Input=1GB: 2.365
PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=None, Streams=buffered, Writer=NewTask, Input=1GB: 2.7
PC=Mac, InputStorage=SDD, Alloc=Array, Return=Memory, PLINQ=2, Streams=buffered, Writer=NewTask, Input=1GB: 2.17
PC=Mac, InputStorage=SDD, Alloc=Pool, Return=Array, AsyncEnumerable, Input=1GB: 2.95 (also got wrong results)
PC=g4dn.8xlarge, InputStorage=RAM, OutputStorage=/dev/null Alloc=Array, Return=Memory, PLINQ=2, Streams=buffered, Writer=NewTask, Input=1GB, Build=Native: 0.875s
PC=g4dn.8xlarge, InputStorage=RAM, OutputStorage=RAM Alloc=Array, Return=Memory, PLINQ=2, Streams=buffered, Writer=NewTask, Input=1GB, Build=Native: 1.58s
PC=g4dn.8xlarge, InputStorage=RAM, OutputStorage=/dev/null Alloc=Pool, Return=Struct, PLINQ=2, Streams=buffered, Writer=NewTask, Input=1GB, Build=Native: 0.731s
PC=g4dn.8xlarge, InputStorage=RAM, OutputStorage=RAM Alloc=Pool, Return=Struct, PLINQ=2, Streams=buffered, Writer=NewTask, Input=1GB, Build=Native: 1.422s

TODO:

* [DONE] big machine testing: RAM input, Alloc=Pool, Use structs to pass around (check the Gen2 GC).

* confirm correctness -> make a test on a smaller pcap (watch the last packet!!!)

### End-to-end test

```bash
cat s3objects.txt | ~/bin/reS3m -s 24 -w 60 -c 16777216 | ~/bin/unpcap  
```

### .NET Install on AL2

```bash
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install dotnet-sdk-7.0
```

### Build

Build self-contained executable for Linux/x64:

```bash
dotnet publish -r linux-x64 -p:PublishSingleFile=true -c Release --self-contained true
```

for Linux/ARM64:

```bash
dotnet publish -r linux-arm64 -p:PublishSingleFile=true -c Release --self-contained true
```

for Windows:

```bash
dotnet publish -r win-x64 -p:PublishSingleFile=true -c Release --self-contained true
```

for Mac OS X/x64:

```bash
dotnet publish -r osx.12-x64 -p:PublishSingleFile=true -c Release --self-contained true
```

See more about Runtime Identifiers (RIDs) in <https://learn.microsoft.com/en-us/dotnet/core/rid-catalog>

### Tests

Create a ramdisk:

```bash
sudo mkdir /mnt/ramdisk
sudo mount -t tmpfs -o size=32g tmpfs /mnt/ramdisk
```

Copy the pcap file from S3:

```bash
aws s3 cp s3://<gs-delivery-s3-bucket>/<file>.pcap .
```

Create a 1Gb chunk and copy it into RAM-based filesystem:

```bash
head -c 1G <file>.pcap > /mnt/ramdisk/GS_1G.pcap
```

Run the test (debug build):

```bash
time dotnet run - < /mnt/ramdisk/GS_1G.pcap >/dev/null
```

Run the test (native build):

```bash
time ./bin/Release/net7.0/linux-x64/publish/unpcap < /mnt/ramdisk/GS_1G.pcap > /dev/null 
```

### VITA Compression

lz4 - doesn't compress

gzip - compression factor 0.89

### pcap examples

* <https://www.netresec.com/?page=PcapFiles>

### pcap reading

* Binary format reading:
  * <https://stackoverflow.com/questions/59825404/efficient-reading-structured-binary-data-from-a-file> !!!
  * <https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.layoutkind?view=net-7.0>
  * <https://jonskeet.uk/csharp/readbinary.html>
  * 

* Network Endiannes conversion:
  * <https://learn.microsoft.com/en-us/dotnet/api/system.net.ipaddress.networktohostorder?view=net-7.0>
  * BinaryReader not very useful provided we have LayoutKind above <https://www.bestprog.net/en/2021/02/15/c-the-binaryreader-class-working-with-files/>

* 


* Read streams https://jonskeet.uk/csharp/readbinary.html

* PCAP format description <https://github.com/pcapng/pcapng/>

* https://github.com/wfurt/PcapStream

* https://github.com/awalsh128/PcapngFile

https://www.nuget.org/packages?q=pcap

https://nmap.org/ncat/ 

https://github.com/dreadl0ck/gopcap

Work on zero-copy from the streams (look into spans).

Look if it makes sense to separate stdin read and stdout write in different threads.

https://stackoverflow.com/questions/50078640/spant-and-streams - span over stream

https://nishanc.medium.com/an-introduction-to-writing-high-performance-c-using-span-t-struct-b859862a84e4

https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay