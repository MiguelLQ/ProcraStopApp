
    using ProcraStop.Modules.Stress.Domain.Entities;
    using ProcraStop.Modules.Stress.Domain.Enums;

    namespace ProcraStop.Modules.Stress.Application.Abstractions
    {
        public interface IStressRepository
        {
            Task AddAsync(StressRecord record);
            Task<List<StressRecord>> GetRecentAsync(int days);
            Task<StressRecord?> GetLatestAsync();
            Task<bool> ExistsTodayAsync(StressSource source);
            Task UpdateTodayAsync(double level, StressSource source);
        // mostrar checkin cada 12 horas
            Task<bool> ExistsInLastHoursAsync(StressSource source, int hours);

         }
    }
