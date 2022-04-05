using System.Buffers.Binary;

namespace SequentialGuids;

public class SequentialGuid_Optimized
{
    /// <summary>
    /// Equivalent to 'SELECT CAST(0 AS DATETIME)'. Specify UTC since we're using UTC in GenerateComb, and 
    /// since all the servers we run on are set to UTC.
    /// </summary>
    private static readonly long _epochTicks = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

    private static readonly int[] _sqlOrderMap = new int[16] { 3, 2, 1, 0, 5, 4, 7, 6, 9, 8, 15, 14, 13, 12, 11, 10 };

    public static Guid GenerateComb()
    {
        //
        // NOTE: the last 8 bytes of a UNIQUEIDENTIFIER in SQL Server are stored as Big Endian. Since we're
        //   overwriting the last 6 bytes, we need to convert those numbers to Big Endian.
        //
        //  Bits    Bytes   Name    Endianness
        //  32      4       Data1   Native
        //  16      2       Data2   Native
        //  16      2       Data3   Native
        //  64      8       Data4   Big
        //
        // Native on Windows means Little Endian.
        //
        // See: https://dba.stackexchange.com/a/121878
        //

        // Generate a new Guid. We'll alter its last 6 bytes to make it less random and more sequential.
        Span<byte> guidBytes = stackalloc byte[16];
        Guid.NewGuid().TryWriteBytes(guidBytes);

        var now = DateTime.UtcNow;

        // Get the days elapsed since the SQL Server DATETIME epoch.
        var timeSinceEpoch = new TimeSpan(now.Ticks - _epochTicks);
        Span<byte> daysBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(daysBytes, timeSinceEpoch.Days);

        // Get the current time of day.
        // NOTE: SQL Server is accurate to 1/300th of a millisecond, so we divide by 3.333333 
        TimeSpan timeOfDay = now.TimeOfDay;
        Span<byte> msecsBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(msecsBytes, (long)(timeOfDay.TotalMilliseconds / 3.333333));

        // Copy the days and milliseconds bytes into the end of the Guid, starting at offset 10.
        daysBytes.Slice(start: daysBytes.Length - 2, length: 2).CopyTo(guidBytes.Slice(start: guidBytes.Length - 6));
        msecsBytes.Slice(start: msecsBytes.Length - 4, length: 4).CopyTo(guidBytes.Slice(start: guidBytes.Length - 4));

        return new Guid(guidBytes);
    }

    public Guid CurrentGuid { get; private set; }

    public SequentialGuid_Optimized()
    {
        CurrentGuid = GenerateComb();
    }

    public SequentialGuid_Optimized(Guid previousGuid)
    {
        CurrentGuid = previousGuid;
    }

    public static SequentialGuid_Optimized operator ++(SequentialGuid_Optimized sequentialGuid)
    {
        Span<byte> currentGuidBytes = stackalloc byte[16];
        sequentialGuid.CurrentGuid.TryWriteBytes(currentGuidBytes);

        for (int mapIndex = 0; mapIndex < 16; mapIndex++)
        {
            int bytesIndex = _sqlOrderMap[mapIndex];
            currentGuidBytes[bytesIndex]++;
            if (currentGuidBytes[bytesIndex] != 0)
            {
                break; // No need to increment more significant bytes
            }
        }

        sequentialGuid.CurrentGuid = new Guid(currentGuidBytes);
        return sequentialGuid;
    }
}
