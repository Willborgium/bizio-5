using System.IO;
using System.Text.Json;
using Hyjynx.Bizio.App.Models;
using Hyjynx.Bizio.Engine;

namespace Hyjynx.Bizio.App.Services;

public sealed class GameStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public GameStorageService()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Bizio",
                "Saves"))
    {
    }

    public GameStorageService(string saveDirectory)
    {
        ArgumentNullException.ThrowIfNull(saveDirectory);
        SaveDirectory = saveDirectory;
        DefaultSavePath = Path.Combine(SaveDirectory, "new-game.json");
    }

    public string SaveDirectory { get; }
    public string DefaultSavePath { get; }

    public void SaveGame(GameState state, string path)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Directory.CreateDirectory(SaveDirectory);
        var json = JsonSerializer.Serialize(state, JsonOptions);
        File.WriteAllText(path, json);
    }

    public GameState LoadGame(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        var saveRoot = Path.GetFullPath(SaveDirectory);
        if (!fullPath.StartsWith(saveRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The selected save is outside the configured save directory.");
        }

        var json = File.ReadAllText(fullPath);
        var state = JsonSerializer.Deserialize<GameState>(json);
        if (state is null)
        {
            throw new InvalidDataException("The selected save does not contain a game state.");
        }

        return state;
    }

    public IReadOnlyList<SaveGameSummary> ListSaveGames()
    {
        Directory.CreateDirectory(SaveDirectory);

        return Directory
            .EnumerateFiles(SaveDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => new SaveGameSummary(
                file.Name,
                file.FullName,
                file.LastWriteTimeUtc))
            .ToArray();
    }
}
