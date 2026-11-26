namespace Hyjynx.Bizio.App.Models;

public sealed record SaveGameSummary(
    string Name,
    string FullPath,
    DateTime LastWriteUtc);
