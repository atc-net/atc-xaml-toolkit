namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

/// <summary>
/// Attached properties covering the <see cref="FrameworkPropertyMetadata"/> argument combinations
/// that need the full constructor.
/// </summary>
[AttachedProperty<int>(
    "FlagsAndIsAnimationProhibited",
    Flags = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
    IsAnimationProhibited = true)]

[AttachedProperty<int>(
    "IsAnimationProhibitedAndUpdateSourceTrigger",
    IsAnimationProhibited = true,
    DefaultUpdateSourceTrigger = UpdateSourceTrigger.LostFocus)]
public static partial class FrameworkPropertyMetadataTestAttach;