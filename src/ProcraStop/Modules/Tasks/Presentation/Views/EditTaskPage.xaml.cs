
using ProcraStop.Modules.Tasks.Presentation.ViewModels;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class EditTaskPage : ContentPage
{
	public EditTaskPage(EditTaskViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}