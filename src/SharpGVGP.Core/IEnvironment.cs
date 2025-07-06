namespace SharpGVGP.Core;
public interface IEnvironment<TState, TAction>
{
    TState Reset();
    (TState NextState, double Reward, bool Done) Step(TAction action);
    IEnumerable<TAction> Actions(TState state);
}

