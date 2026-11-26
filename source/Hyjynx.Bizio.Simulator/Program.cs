using System.Text.Json;
using Hyjynx.Bizio.Engine;

var state = EngineService.CreateNewGame();
var inputs = args.Length > 0
    ? args
    : new[] { "start" };

var result = EngineService.RunTurn(state, inputs);

Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
{
    WriteIndented = true,
}));
