
using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Domain.Entities;
using ProcraStop.Modules.Stress.Domain.Enums;

namespace ProcraStop.Modules.Stress.Application.Commands.RegisterStress
{
    public class RegisterStressHandler
    {
        private readonly IStressRepository _repo;

        public RegisterStressHandler(IStressRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(RegisterStressCommand cmd)
        {
            var record = new StressRecord(cmd.Level, StressSource.Manual);
            await _repo.AddAsync(record);
        }
    }
}
