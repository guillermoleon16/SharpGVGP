namespace SharpGVGP.Core;

public interface IVisualizer<TState, TAction>
{
    void Render(TState state, TAction lastAction);
    void DisplayMetrics(int episode, double cumulativeReward);
}
