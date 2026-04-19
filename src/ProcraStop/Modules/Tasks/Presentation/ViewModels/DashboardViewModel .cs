using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.Queries.GetDashboardGuidance;
using ProcraStop.Modules.Tasks.Application.Queries.GetFocusTasks;
using ProcraStop.Modules.Tasks.Application.Queries.GetTodayRecommendations;
using ProcraStop.Modules.Tasks.Domain.Enums;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly GetTodayRecommendationsHandler _handler;
        private readonly GetDashboardGuidanceHandler _guidanceHandler;
        private readonly GetFocusTasksHandler _focushandler;

        public DashboardViewModel(GetTodayRecommendationsHandler handler,
            GetDashboardGuidanceHandler guidanceHandler,
            GetFocusTasksHandler focusHandler)

        {
            _handler = handler;
            _guidanceHandler = guidanceHandler;
            _focushandler = focusHandler;
        }

        [ObservableProperty]
        private ObservableCollection<RecommendedTaskDto> todayTasks = new();
        // guia inteligente 
        [ObservableProperty]
        private string guidanceMessage = string.Empty;
        // focus inteligente

        [ObservableProperty]
        private ObservableCollection<FocusTaskDto> focusTasks = new();
        [ObservableProperty]
        private GuidanceType guidanceType;

        public async Task LoadAsync()
        {
            await LoadGuidance();
            await LoadToday();
            await LoadFocus();
        }
        private async Task LoadGuidance()
        {
            var guidance = await _guidanceHandler.Handle(new GetDashboardGuidanceQuery());
            GuidanceMessage = guidance.Message;
            GuidanceType = guidance.Type;
        }

        private async Task LoadToday()
        {
            var result = await _handler.Handle(new GetTodayRecommendationsQuery());
            TodayTasks = new ObservableCollection<RecommendedTaskDto>(result);
        }

        private async Task LoadFocus()
        {
            var result = await _focushandler.Handle(new GetFocusTasksQuery());
            FocusTasks = new ObservableCollection<FocusTaskDto>(result);
        }
       
    }
}
