namespace SharpGVGP.Core;
public interface IAgent<TState, TAction>
{
    TAction ChooseAction(TState state);
    void Observe(TState state, TAction action, double reward, TState nextState, bool done);
    void Reset();
}
