namespace SharpGVGP.Core;

public interface IRewardStrategy<TState, TAction>
{
    double ComputeReward(TState state, TAction action, TState nextState);
}
