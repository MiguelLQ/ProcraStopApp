using System.Collections.ObjectModel;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public sealed class TechniqueLibraryViewModel
{
    public TechniqueLibraryViewModel(StudyTechniqueCatalogService catalog)
    {
        foreach (var technique in catalog.GetAll())
            Techniques.Add(new StudyTechniqueOption(technique));
    }

    public ObservableCollection<StudyTechniqueOption> Techniques { get; } = new();
}
