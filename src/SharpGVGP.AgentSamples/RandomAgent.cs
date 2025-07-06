using SharpGVGP.Core;

namespace SharpGVGP.AgentSamples;

public class RandomAgent<TState, TAction> : IAgent<TState, TAction>
{
    private readonly Func<TState, TAction[]> _actionProvider;
    private readonly Random _rng = new();

    public RandomAgent(Func<TState, TAction[]> actionProvider)
    {
        _actionProvider = actionProvider;
    }

    public TAction ChooseAction(TState state)
    {
        var actions = _actionProvider(state);
        return actions[_rng.Next(actions.Length)];
    }

    public void Observe(TState state, TAction action, double reward, TState nextState, bool done) { }

    public void Reset() { }
}
