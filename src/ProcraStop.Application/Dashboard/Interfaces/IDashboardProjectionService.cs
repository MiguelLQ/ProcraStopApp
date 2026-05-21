using ProcraStop.Application.Dashboard.Models;

namespace ProcraStop.Application.Dashboard.Interfaces
{
    public interface IDashboardProjectionService
    {
        // esto es un servicio de proyección, no debe tener lógica de negocio, solo debe mapear los datos a un modelo de lectura
        Task<DashboardReadModel> BuildAsync(CancellationToken ct);

    }
}
