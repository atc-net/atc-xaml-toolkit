// ReSharper disable UnusedMember.Global
namespace Atc.XamlToolkit.Controls.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AttachedPropertyAttribute<T>(string propertyName) : DependencyPropertyAttribute<T>(propertyName);