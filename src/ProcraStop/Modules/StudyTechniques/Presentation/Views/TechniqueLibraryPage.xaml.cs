using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Views;

public partial class TechniqueLibraryPage : ContentPage
{
    public TechniqueLibraryPage(TechniqueLibraryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
