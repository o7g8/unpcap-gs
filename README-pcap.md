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