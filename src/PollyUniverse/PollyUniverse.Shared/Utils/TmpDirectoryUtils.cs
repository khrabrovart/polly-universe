namespace PollyUniverse.Shared.Utils;

static internal class TmpDirectoryUtils
{
    public static string GetTmpDirectory()
    {
        return Directory.Exists("./tmp") ? "./tmp" : "/tmp";
    }
}
