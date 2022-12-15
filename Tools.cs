using System.Runtime.InteropServices;

namespace  unpcap;

public class Tools
{
    // https://www.codeproject.com/Articles/5296628/Fast-Conversions-between-tightly-packed-Structures
    public static S ArrayToStructure<S>(byte[] abSource) where S : struct
    {
        var iHandle = GCHandle.Alloc(abSource, GCHandleType.Pinned);
        S rTarget;
        try
        {
            rTarget = Marshal.PtrToStructure<S>(iHandle.AddrOfPinnedObject());
        }
        finally
        {
            iHandle.Free();
        }
        return rTarget;
    }
}