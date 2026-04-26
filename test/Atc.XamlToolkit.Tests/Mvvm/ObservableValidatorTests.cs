namespace Atc.XamlToolkit.Tests.Mvvm;

public sealed class ObservableValidatorTests
{
    [Fact]
    public void HasErrors_IsFalse_BeforeAnyValidation()
    {
        var sut = new TestPersonValidator();
        sut.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void ValidateAllProperties_FlagsRequiredField_WhenInitialised()
    {
        var sut = new TestPersonValidator();
        sut.RunValidateAllProperties()
            .Should().BeFalse("an empty FirstName must fail [Required] validation");

        sut.HasErrors.Should().BeTrue();
        sut.GetErrors(nameof(TestPersonValidator.FirstName)).Cast<string>()
            .Should().Contain("First name is required");
    }

    [Fact]
    public void PropertyChange_TriggersValidation_WhenAutoValidateIsOn()
    {
        var sut = new TestPersonValidator(validateOnPropertyChanged: true);

        // Setting an invalid value should produce an error.
        sut.FirstName = "A"; // < 2 chars
        sut.GetErrors(nameof(TestPersonValidator.FirstName)).Cast<string>()
            .Should().Contain(e => e.Contains("at least 2 characters", StringComparison.Ordinal));

        // Setting a valid value should clear it.
        sut.FirstName = "Alice";
        sut.GetErrors(nameof(TestPersonValidator.FirstName)).Cast<string>()
            .Should().BeEmpty();
    }

    [Fact]
    public void ErrorsChanged_FiresWith_PropertyName_OnAddAndClear()
    {
        var sut = new TestPersonValidator(validateOnPropertyChanged: true);
        var notifications = new List<string?>();
        sut.ErrorsChanged += (_, e) => notifications.Add(e.PropertyName);

        sut.FirstName = "A";    // invalid → ErrorsChanged(FirstName)
        sut.FirstName = "Alice"; // valid → ErrorsChanged(FirstName) again to clear

        notifications.Should().Contain(nameof(TestPersonValidator.FirstName));
        notifications.Where(n => n == nameof(TestPersonValidator.FirstName))
            .Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void HasErrors_RaisesPropertyChanged_WhenErrorStateFlips()
    {
        var sut = new TestPersonValidator(validateOnPropertyChanged: true);
        var hasErrorsRaises = 0;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(sut.HasErrors))
            {
                hasErrorsRaises++;
            }
        };

        sut.FirstName = "A";    // gains errors
        sut.FirstName = "Alice"; // clears errors

        hasErrorsRaises.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ObservableValidator_DoesNotRequire_ViewModelBase_OrMessenger()
    {
        // Smoke test — proves the class can stand alone without ViewModelBase's UI-state baggage.
        var sut = new TestPersonValidator();
        sut.Should().BeAssignableTo<ObservableValidator>();
        sut.Should().NotBeAssignableTo<ViewModelBase>();
    }

    [Fact]
    public void ValidateProperty_BuildsValidationCacheLazily_WithoutInitializeValidationCall()
    {
        // Pin the lazy-init contract: callers using [NotifyDataErrorInfo]
        // — which emits inline ValidateProperty(...) calls in the setter —
        // must not need to invoke InitializeValidation() to opt in.
        // The cache builds itself on first ValidateProperty invocation.
        var sut = new TestLazyValidator();

        sut.SetFirstNameDirect("A"); // invalid (< 2 chars)

        sut.HasErrors.Should().BeTrue();
        sut.GetErrors(nameof(TestLazyValidator.FirstName)).Cast<string>()
            .Should().Contain(e => e.Contains("at least 2 characters", StringComparison.Ordinal));
    }

    private sealed class TestLazyValidator : ObservableValidator
    {
        private string firstName = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
        public string FirstName => firstName;

        public void SetFirstNameDirect(string value)
        {
            firstName = value;
            ValidateProperty(value, nameof(FirstName));
        }
    }

    private sealed class TestPersonValidator : ObservableValidator
    {
        private string firstName = string.Empty;

        public TestPersonValidator(bool validateOnPropertyChanged = false)
        {
            InitializeValidation(validateOnPropertyChanged);
        }

        [Required(ErrorMessage = "First name is required")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
        public string FirstName
        {
            get => firstName;
            set
            {
                if (firstName == value)
                {
                    return;
                }

                firstName = value;
                RaisePropertyChanged();
            }
        }

        public bool RunValidateAllProperties()
            => ValidateAllProperties();
    }
}