namespace Outseek.AvaloniaClient.ViewModels
{
    public class WorkingAreaToolsViewModel : ViewModelBase
    {
        public WorkingAreaViewModel WorkingAreaViewModel { get; }

        public WorkingAreaToolsViewModel(WorkingAreaViewModel workingAreaViewModel)
        {
            WorkingAreaViewModel = workingAreaViewModel;
        }

        public WorkingAreaToolsViewModel() : this(new WorkingAreaViewModel())
        {
            // the default constructor is only used by the designer
        }
    }
}
