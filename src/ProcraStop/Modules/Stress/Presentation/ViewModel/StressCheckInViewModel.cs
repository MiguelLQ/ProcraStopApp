
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Application.Stress.Commands.RegisterStress;
using ProcraStop.Domain.Stress.Enums;
using ProcraStop.Modules.Stress.Presentation.Messages;
using ProcraStop.Modules.Stress.Presentation.Models;

namespace ProcraStop.Modules.Stress.Presentation.ViewModel
{
    public partial class StressCheckInViewModel : ObservableObject
    {
        private readonly RegisterStressHandler _handler;
        private readonly IStressRepository _repo;

        public StressCheckInViewModel(
            RegisterStressHandler handler,
            IStressRepository repo)
        {
            _handler = handler;
            _repo = repo;
        }

        [ObservableProperty]
        private bool isVisible;

        [ObservableProperty]
        private StressLevelInput? selectedLevel;

        public bool CanSubmit => SelectedLevel != null;

        public async Task InitializeAsync()
        {
            var exists = await _repo.ExistsInLastHoursAsync(StressSource.Manual, 12);

            IsVisible = !exists;
        }

        public List<StressOption> Options { get; } = new()
    {
        new("😌", "Calmado", StressLevelInput.VeryLow),
        new("🙂", "Bien", StressLevelInput.Low),
        new("😐", "Neutro", StressLevelInput.Medium),
        new("😓", "Estresado", StressLevelInput.High),
        new("😵", "Saturado", StressLevelInput.VeryHigh),
    };
        private StressLevelInput MapFromValue(double value)
        {
            if (value <= 0.2) return StressLevelInput.VeryLow;
            if (value <= 0.4) return StressLevelInput.Low;
            if (value <= 0.6) return StressLevelInput.Medium;
            if (value <= 0.8) return StressLevelInput.High;

            return StressLevelInput.VeryHigh;
        }
        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            if (SelectedLevel is null)
                return;

            var command = new RegisterStressCommand(SelectedLevel.Value);

            await _handler.Handle(command);

            IsVisible = false;
            SelectedLevel = null;
            WeakReferenceMessenger.Default.Send(StressCheckInSubmittedMessage.Instance);
        }

        [RelayCommand]
        private void Select(StressLevelInput value)
        {
            SelectedLevel = value;

            foreach (var option in Options)
            {
                option.IsSelected = option.Value == value;
            }
        }

        partial void OnSelectedLevelChanged(StressLevelInput? value)
        {
            SubmitCommand.NotifyCanExecuteChanged();
        }

    }
}
