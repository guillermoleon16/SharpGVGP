using SharpGVGP.AgentSamples;
using SharpGVGP.Core;
using SharpGVGP.Environments;
using SharpGVGP.Visualization;

internal class Program
{
    static void Main()
    {
        int[,] grid = {
            {0,0,1,2},
            {0,1,0,1},
            {0,0,0,0},
            {2,1,0,0}
        };
        var env = new GridMazeEnvironment(grid, new GridCoordinates { X = 0, Y = 0 });
        var agent = new RandomAgent<GridCoordinates, MazeAction>(s => env.Actions(s).ToArray());
        //var agent = new QLearningAgent<MazeAction>(env, alpha: 0.1, gamma: 0.99, epsilon: 0.2);
        var viz = new ConsoleVisualizer();

        for (int ep = 1; ep <= 5; ep++)
        {
            var state = env.Reset();
            double total = 0;
            bool done = false;
            while (!done)
            {
                var action = agent.ChooseAction(state);
                var (next, reward, isDone) = env.Step(action);
                agent.Observe(state, action, reward, next, isDone);
                viz.Render(next, action);
                state = next;
                total += reward;
                done = isDone;
                Thread.Sleep(200);
            }
            viz.DisplayMetrics(ep, total);
            agent.Reset();
            Thread.Sleep(500);
        }
    }
}
