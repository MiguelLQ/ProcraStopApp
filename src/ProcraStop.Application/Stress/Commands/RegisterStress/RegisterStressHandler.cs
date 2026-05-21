
using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Domain.Stress.Enums;

namespace ProcraStop.Application.Stress.Commands.RegisterStress
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
            var level = MapToValue(cmd.Level);

            await _repo.UpdateTodayAsync(level, StressSource.Manual);
        }

        private double MapToValue(StressLevelInput input)
        {
            return input switch
            {
                StressLevelInput.VeryLow => 0.1,
                StressLevelInput.Low => 0.3,
                StressLevelInput.Medium => 0.5,
                StressLevelInput.High => 0.7,
                StressLevelInput.VeryHigh => 0.9,
                _ => 0.5
            };
        }
    }
}
