namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class AddTaskPage : ContentPage
{
	public AddTaskPage()
	{
		InitializeComponent();
        // Asegurarte de que la flecha de retroceso esté habilitada
        NavigationPage.SetHasBackButton(this, true);
    }
}