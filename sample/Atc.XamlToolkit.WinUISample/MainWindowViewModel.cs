// ReSharper disable PartialTypeWithSinglePart
namespace Atc.XamlToolkit.WinUISample;

public partial class MainWindowViewModel : MainWindowViewModelBase
{
    [ObservableProperty]
    private List<SampleViewItem> sampleViews = [];

    private SampleViewItem? selectedSampleView;

    [ObservableProperty]
    private object? currentView;

    public SampleViewItem? SelectedSampleView
    {
        get => selectedSampleView;
        set
        {
            if (Set(ref selectedSampleView, value))
            {
                UpdateCurrentView();
            }
        }
    }

    public MainWindowViewModel()
        => InitializeSampleViews();

    private void InitializeSampleViews()
    {
        SampleViews =
        [
            new SampleViewItem("Mvvm", "SampleControls/Behaviors")
            {
                Children =
                {
                    new SampleViewItem("EventToCommandBehaviorView", viewType: typeof(SampleControls.Behaviors.EventToCommandBehaviorView)),
                },
            },
            new SampleViewItem("Mvvm", "SampleControls/Mvvm")
            {
                Children =
                {
                    new SampleViewItem("Dto To ViewModel", "SampleControls/Mvvm/DtoToViewModel")
                    {
                        Children =
                        {
                            new SampleViewItem("PersonView", typeof(SampleControls.Mvvm.DtoToViewModel.PersonView)),
                        },
                    },
                    new SampleViewItem("General Attributes", "SampleControls/Mvvm/GeneralAttributes")
                    {
                        Children =
                        {
                            new SampleViewItem("Person", "SampleControls/Mvvm/GeneralAttributes/Person")
                            {
                                Children =
                                {
                                    new SampleViewItem("PersonView", typeof(SampleControls.Mvvm.GeneralAttributes.Person.PersonView)),
                                },
                            },
                            new SampleViewItem("Azure", "SampleControls/Mvvm/GeneralAttributes/Azure")
                            {
                                Children =
                                {
                                    new SampleViewItem("AzureTenantManagerView", typeof(SampleControls.Mvvm.GeneralAttributes.Azure.AzureTenantManagerView)),
                                },
                            },
                        },
                    },
                },
            },
        ];
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
    private void UpdateCurrentView()
    {
        if (SelectedSampleView is null || SelectedSampleView.ViewType is null)
        {
            CurrentView = null;
            return;
        }

        try
        {
            CurrentView = Activator.CreateInstance(SelectedSampleView.ViewType);
        }
        catch (Exception ex)
        {
            CurrentView = $"Error loading view: {ex.Message}";
        }
    }
}