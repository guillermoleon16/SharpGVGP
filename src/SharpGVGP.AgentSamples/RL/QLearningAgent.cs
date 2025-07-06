using SharpGVGP.Core;

namespace SharpGVGP.AgentSamples
{
    public class QLearningAgent<TAction> : IAgent<GridCoordinates, TAction>
    {
        private readonly IEnvironment<GridCoordinates, TAction> _env;
        private readonly double _alpha, _gamma;
        private double _epsilon;
        private readonly Random _rng = new();

        private readonly Dictionary<(GridCoordinates, TAction), double> _qTable
            = new();

        public QLearningAgent(
            IEnvironment<GridCoordinates, TAction> env,
            double alpha = 0.1,
            double gamma = 0.99,
            double epsilon = 0.2)
        {
            _env = env;
            _alpha = alpha;
            _gamma = gamma;
            _epsilon = epsilon;
        }

        public TAction ChooseAction(GridCoordinates state)
        {
            var actions = _env.Actions(state).ToArray();

            // ε-greedy
            if (_rng.NextDouble() < _epsilon)
                return actions[_rng.Next(actions.Length)];

            // pick best Q-value
            return actions
                .OrderByDescending(a => GetQValue(state, a))
                .First();
        }

        public void Observe(
            GridCoordinates state,
            TAction action,
            double reward,
            GridCoordinates nextState,
            bool done)
        {
            // Current Q
            var key = (state, action);
            double oldQ = GetQValue(state, action);

            // Estimate optimal future value
            double maxNextQ = _env
                .Actions(nextState)
                .Max(a => GetQValue(nextState, a));

            // Q-learning update
            double updatedQ = oldQ
                + _alpha * (reward + _gamma * maxNextQ - oldQ);

            _qTable[key] = updatedQ;
        }

        public void Reset()
        {
            // Optionally decay epsilon per episode
            _epsilon *= 0.99;
        }

        // Helper: safely get Q, defaulting to 0
        private double GetQValue(GridCoordinates s, TAction a) =>
            _qTable.TryGetValue((s, a), out var q) ? q : 0.0;
    }
}
