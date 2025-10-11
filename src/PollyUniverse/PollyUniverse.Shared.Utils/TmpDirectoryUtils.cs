namespace PollyUniverse.Shared.Utils;

public static class TmpDirectoryUtils
{
    public static string GetTmpDirectory()
    {
        return Directory.Exists("./tmp") ? "./tmp" : "/tmp";
    }
}
