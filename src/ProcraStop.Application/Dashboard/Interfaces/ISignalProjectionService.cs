using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Application.Dashboard.Interfaces
{
    public interface ISignalProjectionService
    {
        List<SignalReadModel> Build(DecisionContext context, DecisionResult decision);
    }
}
