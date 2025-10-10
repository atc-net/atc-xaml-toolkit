global using System.Collections.Immutable;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Text;
global using System.Text.RegularExpressions;

global using Atc.XamlToolkit.SourceGenerators.Builders;
global using Atc.XamlToolkit.SourceGenerators.Extensions;
global using Atc.XamlToolkit.SourceGenerators.Extensions.Builder;
global using Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;
global using Atc.XamlToolkit.SourceGenerators.Factories;
global using Atc.XamlToolkit.SourceGenerators.Inspectors;
global using Atc.XamlToolkit.SourceGenerators.Inspectors.Attributes;
global using Atc.XamlToolkit.SourceGenerators.Inspectors.Helpers;
global using Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;
global using Atc.XamlToolkit.SourceGenerators.Models.FrameworkElement;
global using Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;
global using Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;
global using Atc.XamlToolkit.SourceGenerators.Models.ViewModel;

global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.Text;