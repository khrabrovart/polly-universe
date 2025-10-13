using System.Text.Json;
using PollyUniverse.Shared.Services.Files;

namespace PollyUniverse.Shared.Services;

public interface IDictionaryService
{
    Task PreloadDictionary();

    string GetString(string key);
}

public class DictionaryService : IDictionaryService
{
    private readonly IDictionaryFileService _dictionaryFileService;

    private Dictionary<string, string> _dictionary;

    public DictionaryService(IDictionaryFileService dictionaryFileService)
    {
        _dictionaryFileService = dictionaryFileService;
    }

    public async Task PreloadDictionary()
    {
        var filePath = await _dictionaryFileService.DownloadDictionaryFile();

        if (filePath == null)
        {
            throw new Exception("Dictionary file not found");
        }

        var json = await File.ReadAllTextAsync(filePath);

        _dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    public string GetString(string key)
    {
        return _dictionary[key];
    }
}
