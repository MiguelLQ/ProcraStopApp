

using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Modules.Dashboard.Application.Models;

namespace ProcraStop.Modules.Dashboard.Application.Services
{
    public interface ISignalProjectionService
    {
        List<SignalReadModel> Build(DecisionContext context, DecisionResult decision);
    }
}
