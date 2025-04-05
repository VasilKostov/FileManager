namespace FileManager.Helpers;

public class Helper
{
    public const double BytesInKb = 1024;
    public const double BytesInMb = 1024 * 1024;
    public const double BytesInGb = 1024 * 1024 * 1024;

    public static string GetMb(long size)
    {
        if (size < BytesInMb)
        {
            double sizeInKb = size / BytesInKb;

            return $"{sizeInKb:F2} KB";
        }
        else
        {
            double sizeInMb = size / BytesInMb;

            return $"{sizeInMb:F2} MB";
        }
    }
}
