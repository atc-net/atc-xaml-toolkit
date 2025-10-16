// ReSharper disable PartialTypeWithSinglePart
namespace Atc.XamlToolkit.WpfSample;

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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
    private void UpdateCurrentView()
    {
        if (SelectedSampleView?.ViewType is null)
        {
            CurrentView = null;
            return;
        }

        try
        {
            var newView = Activator.CreateInstance(SelectedSampleView.ViewType);

            // Force clear first to ensure change notification
            CurrentView = null;
            CurrentView = newView;
        }
        catch (Exception)
        {
            CurrentView = null;
        }
    }

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
                            new SampleViewItem("PersonView", viewType: typeof(SampleControls.Mvvm.DtoToViewModel.PersonView)),
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
                                    new SampleViewItem("PersonView", viewType: typeof(SampleControls.Mvvm.GeneralAttributes.Person.PersonView)),
                                },
                            },
                            new SampleViewItem("Azure", "SampleControls/Mvvm/GeneralAttributes/Azure")
                            {
                                Children =
                                {
                                    new SampleViewItem("AzureTenantManagerView", viewType: typeof(SampleControls.Mvvm.GeneralAttributes.Azure.AzureTenantManagerView)),
                                },
                            },
                        },
                    },
                },
            },
        ];
    }
}