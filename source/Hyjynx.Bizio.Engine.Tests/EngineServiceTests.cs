namespace Hyjynx.Bizio.Engine.Tests;

public sealed class EngineServiceTests
{
    [Fact]
    public void CreateNewGameStartsAtTurnOne()
    {
        var state = EngineService.CreateNewGame();

        Assert.Equal(1, state.Turn);
    }

    [Fact]
    public void RunTurnAdvancesStateAndPreservesResultData()
    {
        var state = EngineService.CreateNewGame();
        string[] inputs = ["start"];

        var result = EngineService.RunTurn(state, inputs);

        Assert.Equal(2, result.EndState.Turn);
        Assert.Equal(inputs, result.Inputs);
        Assert.NotEmpty(result.Logs);
    }
}
