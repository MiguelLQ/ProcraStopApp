using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Application.Commands.CompleteTask
{
    public sealed class CompleteTaskCommand
    {
        public Guid TaskId { get; init; }
    }
}
