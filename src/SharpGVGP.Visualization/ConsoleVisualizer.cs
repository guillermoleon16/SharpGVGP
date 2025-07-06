using SharpGVGP.Core;

namespace SharpGVGP.Visualization;

public class ConsoleVisualizer : IVisualizer<GridCoordinates, Enum>
{
    public void Render(GridCoordinates state, Enum lastAction)
    {
        Console.Clear();
        Console.WriteLine($"Agent at: ({state.X},{state.Y}), took: {lastAction}");
    }

    public void DisplayMetrics(int episode, double cumulativeReward)
    {
        Console.WriteLine($"Episode {episode}: Reward = {cumulativeReward:F2}");
    }
}
