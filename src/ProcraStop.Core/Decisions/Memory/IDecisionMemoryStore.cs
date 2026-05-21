
namespace ProcraStop.Core.Decisions.Memory
{
    public interface IDecisionMemoryStore
    {
        DecisionMemory Get();
        void Save(DecisionMemory memory);
    }
}
