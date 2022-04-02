using System.Runtime.InteropServices;
using Microsoft.AspNetCore.WebUtilities;

namespace StringHelpers;

public static class StringEncodingHelper
{
    public static string ToUrlEncodedString(this Guid id)
    {
        // Copying the Guid bytes into a stack-allocated array allocates far fewer bytes
        //   than calling ToByteArray().
        Span<byte> idBytes = stackalloc byte[16];
        MemoryMarshal.TryWrite(idBytes, ref id);

        return WebEncoders.Base64UrlEncode(idBytes);
    }
}
