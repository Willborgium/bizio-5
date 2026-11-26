namespace Hyjynx.Bizio.Engine;

public sealed record GameState(int Turn);

public sealed record SimulationResult(
    GameState EndState,
    IReadOnlyList<string> Logs,
    IReadOnlyList<string> Inputs);

public sealed class EngineService
{
    public static GameState CreateNewGame()
    {
        return new GameState(Turn: 1);
    }

    public static SimulationResult RunTurn(GameState state, IReadOnlyList<string> inputs)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(inputs);

        var nextState = state with { Turn = state.Turn + 1 };
        var logs = new List<string>
        {
            $"Simulation started from turn {state.Turn}.",
            $"Simulation completed turn {nextState.Turn}.",
        };

        return new SimulationResult(
            nextState,
            logs,
            inputs.ToArray());
    }
}
