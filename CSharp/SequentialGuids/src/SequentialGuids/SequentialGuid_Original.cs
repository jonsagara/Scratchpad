namespace SequentialGuids;

public class SequentialGuid_Original
{
    static readonly DateTime epoch = new DateTime(1900, 1, 1);
    static readonly int[] sqlOrderMap = new int[16] { 3, 2, 1, 0, 5, 4, 7, 6, 9, 8, 15, 14, 13, 12, 11, 10 };

    public static Guid GenerateComb()
    {
        DateTime now = DateTime.Now;
        TimeSpan span = new TimeSpan(now.Ticks - epoch.Ticks);
        TimeSpan timeOfDay = now.TimeOfDay;

        // Generate a new Guid. We'll alter several of its bytes to suit our purposes.
        byte[] destinationArray = Guid.NewGuid().ToByteArray();

        // Get the days and milliseconds which will be used to build the byte string.
        // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
        byte[] bytes = BitConverter.GetBytes(span.Days);
        byte[] array = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));

        // Reverse the bytes to match SQL Servers ordering 
        Array.Reverse(bytes);
        Array.Reverse(array);

        // Copy the bytes into the guid 
        Array.Copy(bytes, bytes.Length - 2, destinationArray, destinationArray.Length - 6, 2);
        Array.Copy(array, array.Length - 4, destinationArray, destinationArray.Length - 4, 4);

        return new Guid(destinationArray);
    }

    public Guid CurrentGuid { get; private set; }

    public SequentialGuid_Original()
    {
        CurrentGuid = GenerateComb();
    }

    public SequentialGuid_Original(Guid previousGuid)
    {
        CurrentGuid = previousGuid;
    }

    public static SequentialGuid_Original operator ++(SequentialGuid_Original sequentialGuid)
    {
        byte[] bytes = sequentialGuid.CurrentGuid.ToByteArray();
        for (int mapIndex = 0; mapIndex < 16; mapIndex++)
        {
            int bytesIndex = sqlOrderMap[mapIndex];
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
