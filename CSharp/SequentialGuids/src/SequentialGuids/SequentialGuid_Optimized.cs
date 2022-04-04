using System.Buffers.Binary;

namespace SequentialGuids;

public class SequentialGuid_Optimized
{
    private static readonly long _epochTicks = new DateTime(1900, 1, 1).Ticks;
    private static readonly int[] _sqlOrderMap = new int[16] { 3, 2, 1, 0, 5, 4, 7, 6, 9, 8, 15, 14, 13, 12, 11, 10 };

    public static Guid GenerateComb()
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan timeSinceEpoch = new TimeSpan(now.Ticks - _epochTicks);
        TimeSpan timeOfDay = now.TimeOfDay;

        // Generate a new Guid. We'll alter several of its bytes to suit our purposes.
        Span<byte> guidBytes = stackalloc byte[16];
        Guid.NewGuid().TryWriteBytes(guidBytes);

        // Get the days and milliseconds which will be used to build the byte string.
        // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
        //Span<byte> daysBytes = BitConverter.GetBytes(timeSinceEpoch.Days);
        //Span<byte> msecsBytes = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));

        //// Reverse the bytes to match SQL Servers ordering 
        //daysBytes.Reverse();
        //msecsBytes.Reverse();
        Span<byte> daysBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(daysBytes, timeSinceEpoch.Days);

        Span<byte> msecsBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(msecsBytes, (long)(timeOfDay.TotalMilliseconds / 3.333333));

        // Copy the bytes into the guid 
        //Array.Copy(sourceArray: daysBytes, sourceIndex: daysBytes.Length - 2, destinationArray: guidBytes, destinationIndex: guidBytes.Length - 6, length: 2);
        //Array.Copy(sourceArray: msecsBytes, sourceIndex: msecsBytes.Length - 4, destinationArray: guidBytes, destinationIndex: guidBytes.Length - 4, length: 4);
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
        byte[] bytes = sequentialGuid.CurrentGuid.ToByteArray();
        for (int mapIndex = 0; mapIndex < 16; mapIndex++)
        {
            int bytesIndex = _sqlOrderMap[mapIndex];
            bytes[bytesIndex]++;
            if (bytes[bytesIndex] != 0)
            {
                break; // No need to increment more significant bytes
            }
        }
        sequentialGuid.CurrentGuid = new Guid(bytes);
        return sequentialGuid;
    }
}
