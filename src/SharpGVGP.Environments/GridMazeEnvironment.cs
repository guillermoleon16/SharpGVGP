using SharpGVGP.Core;

namespace SharpGVGP.Environments;

public enum MazeAction { Up, Down, Left, Right }

public class GridMazeEnvironment : IEnvironment<GridCoordinates, MazeAction>
{
    private readonly int[,] _grid;
    private GridCoordinates _agent;
    private GridCoordinates _start;

    public GridMazeEnvironment(int[,] grid, GridCoordinates start)
    {
        _grid = grid;
        _agent = start;
    }

    public GridCoordinates Reset()
    {
        _agent = _start;
        return _agent;
    }

    public (GridCoordinates NextState, double Reward, bool Done) Step(MazeAction action)
    {
        var currentCoordinates = _agent;
        var next = action switch
        {
            MazeAction.Up => new GridCoordinates { X = currentCoordinates.X, Y = currentCoordinates.Y - 1 },
            MazeAction.Down => new GridCoordinates { X = currentCoordinates.X, Y = currentCoordinates.Y + 1 },
            MazeAction.Left => new GridCoordinates { X = currentCoordinates.X - 1, Y = currentCoordinates.Y },
            MazeAction.Right => new GridCoordinates { X = currentCoordinates.X + 1, Y = currentCoordinates.Y },
            _ => _agent
        };

        bool valid =
            next.X >= 0 && next.Y >= 0 &&
            next.X < _grid.GetLength(0) && next.Y < _grid.GetLength(1) &&
            _grid[next.X, next.Y] != 1;

        if (!valid) next = _agent;

        _agent = next;
        double reward = _grid[next.X, next.Y] == 2 ? 1.0 : 0.0;
        bool done = reward > 0;
        return (next, reward, done);
    }

    public IEnumerable<MazeAction> Actions(GridCoordinates state)
    {
        yield return MazeAction.Up;
        yield return MazeAction.Down;
        yield return MazeAction.Left;
        yield return MazeAction.Right;
    }
}
