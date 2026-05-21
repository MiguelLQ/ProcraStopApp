using ProcraStop.Core.Decisions.Memory;

namespace ProcraStop.Core.Decisions.Memory
{
    public class InMemoryDecisionMemoryStore : IDecisionMemoryStore
    {
        private DecisionMemory _memory = new DecisionMemory();

        public DecisionMemory Get() => _memory;

        public void Save(DecisionMemory memory) => _memory = memory;
    }
}
