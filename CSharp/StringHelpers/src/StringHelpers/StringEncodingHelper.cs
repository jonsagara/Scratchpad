using Microsoft.AspNetCore.WebUtilities;

namespace StringHelpers;

public static class StringEncodingHelper
{
    public static string ToUrlEncodedString(this Guid id)
    {
        // Copying the Guid bytes into a stack-allocated array allocates far fewer bytes
        //   than calling ToByteArray().
        Span<byte> idBytes = stackalloc byte[16];

        if (!id.TryWriteBytes(idBytes))
        {
            throw new InvalidOperationException($"Unable to copy Guid '{id}' bytes to a stackalloc byte[16].");
        }

        return WebEncoders.Base64UrlEncode(idBytes);
    }
}
