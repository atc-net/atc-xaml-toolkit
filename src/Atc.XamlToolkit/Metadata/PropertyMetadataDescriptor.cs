namespace Atc.XamlToolkit.Metadata;

/// <summary>
/// Provides reflection-based extraction of property metadata from types decorated
/// with <see cref="PropertyDisplayAttribute"/> and related attributes.
/// </summary>
public static class PropertyMetadataDescriptor
{
    /// <summary>
    /// Gets the property metadata descriptors for the specified type.
    /// Only properties decorated with <see cref="PropertyDisplayAttribute"/> are included (opt-in).
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>A sorted list of property metadata.</returns>
    public static IReadOnlyList<PropertyMetadataInfo> GetDescriptors(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var result = new List<PropertyMetadataInfo>();

        foreach (var property in properties)
        {
            if (property.GetMethod is null)
            {
                continue;
            }

            if (IsBrowsableExcluded(property))
            {
                continue;
            }

            var displayAttr = property.GetCustomAttribute<PropertyDisplayAttribute>();
            if (displayAttr is null)
            {
                continue;
            }

            var info = CreateMetadataInfo(property, displayAttr);
            result.Add(info);
        }

        result.Sort(CompareMetadataInfo);
        return result;
    }

    /// <summary>
    /// Gets the property metadata descriptors for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to inspect.</typeparam>
    /// <returns>A sorted list of property metadata.</returns>
    public static IReadOnlyList<PropertyMetadataInfo> GetDescriptors<T>()
        => GetDescriptors(typeof(T));

    /// <summary>
    /// Gets the property metadata descriptors grouped by <see cref="PropertyMetadataInfo.GroupName"/>.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>A list of groups, each containing the group name and its properties.</returns>
    public static IReadOnlyList<(string GroupName, IReadOnlyList<PropertyMetadataInfo> Properties)>
        GetGroupedDescriptors(Type type)
    {
        var descriptors = GetDescriptors(type);
        var groups = new List<(string GroupName, IReadOnlyList<PropertyMetadataInfo> Properties)>();

        string? currentGroup = null;
        List<PropertyMetadataInfo>? currentList = null;

        foreach (var descriptor in descriptors)
        {
            if (!string.Equals(descriptor.GroupName, currentGroup, StringComparison.Ordinal))
            {
                if (currentList is not null)
                {
                    groups.Add((currentGroup!, currentList));
                }

                currentGroup = descriptor.GroupName;
                currentList = [];
            }

            currentList!.Add(descriptor);
        }

        if (currentList is { Count: > 0 })
        {
            groups.Add((currentGroup!, currentList));
        }

        return groups;
    }

    private static bool IsBrowsableExcluded(PropertyInfo property)
    {
        var browsableAttr = property.GetCustomAttribute<PropertyBrowsableAttribute>();
        if (browsableAttr is not null)
        {
            return !browsableAttr.Browsable;
        }

        var bclBrowsable = property.GetCustomAttribute<BrowsableAttribute>();
        return bclBrowsable is { Browsable: false };
    }

    private static PropertyMetadataInfo CreateMetadataInfo(
        PropertyInfo property,
        PropertyDisplayAttribute displayAttr)
    {
        var displayName = ResolveDisplayName(property, displayAttr);
        var groupName = ResolveGroupName(property, displayAttr);
        var description = ResolveDescription(property, displayAttr);

        var info = new PropertyMetadataInfo(
            property.Name,
            displayName,
            property.PropertyType)
        {
            GroupName = groupName,
            Description = description,
            Order = displayAttr.Order,
            IsReadOnly = ResolveIsReadOnly(property),
        };

        ApplyRangeMetadata(property, info);
        ApplyEditorHint(property, info);

        return info;
    }

    private static string ResolveDisplayName(
        PropertyInfo property,
        PropertyDisplayAttribute displayAttr)
    {
        if (displayAttr.DisplayName is not null)
        {
            return displayAttr.DisplayName;
        }

        var bclDisplayName = property.GetCustomAttribute<DisplayNameAttribute>();
        if (bclDisplayName is not null && !string.IsNullOrEmpty(bclDisplayName.DisplayName))
        {
            return bclDisplayName.DisplayName;
        }

        return property.Name;
    }

    private static string ResolveGroupName(
        PropertyInfo property,
        PropertyDisplayAttribute displayAttr)
    {
        if (displayAttr.GroupName is not null)
        {
            return displayAttr.GroupName;
        }

        var category = property.GetCustomAttribute<CategoryAttribute>();
        if (category is not null && !string.IsNullOrEmpty(category.Category))
        {
            return category.Category;
        }

        return "General";
    }

    private static string ResolveDescription(
        PropertyInfo property,
        PropertyDisplayAttribute displayAttr)
    {
        if (displayAttr.Description is not null)
        {
            return displayAttr.Description;
        }

        var bclDescription = property.GetCustomAttribute<DescriptionAttribute>();
        return bclDescription is not null && !string.IsNullOrEmpty(bclDescription.Description)
            ? bclDescription.Description
            : string.Empty;
    }

    private static bool ResolveIsReadOnly(PropertyInfo property)
    {
        if (property.SetMethod is null || !property.SetMethod.IsPublic)
        {
            return true;
        }

        var readOnlyAttr = property.GetCustomAttribute<ReadOnlyAttribute>();
        return readOnlyAttr is { IsReadOnly: true };
    }

    private static void ApplyRangeMetadata(
        PropertyInfo property,
        PropertyMetadataInfo info)
    {
        var rangeAttr = property.GetCustomAttribute<PropertyRangeAttribute>();
        if (rangeAttr is not null)
        {
            info.RangeMinimum = rangeAttr.Minimum;
            info.RangeMaximum = rangeAttr.Maximum;
            info.RangeStep = rangeAttr.Step;
            return;
        }

        var bclRange = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RangeAttribute>();
        if (bclRange is not null)
        {
            if (bclRange.Minimum is double minD)
            {
                info.RangeMinimum = minD;
            }
            else if (bclRange.Minimum is int minI)
            {
                info.RangeMinimum = minI;
            }

            if (bclRange.Maximum is double maxD)
            {
                info.RangeMaximum = maxD;
            }
            else if (bclRange.Maximum is int maxI)
            {
                info.RangeMaximum = maxI;
            }
        }
    }

    private static void ApplyEditorHint(
        PropertyInfo property,
        PropertyMetadataInfo info)
    {
        var hintAttr = property.GetCustomAttribute<PropertyEditorHintAttribute>();
        if (hintAttr is not null)
        {
            info.EditorHint = hintAttr.Hint;
        }
    }

    private static int CompareMetadataInfo(
        PropertyMetadataInfo x,
        PropertyMetadataInfo y)
    {
        var groupCompare = string.Compare(x.GroupName, y.GroupName, StringComparison.Ordinal);
        if (groupCompare != 0)
        {
            return groupCompare;
        }

        var orderCompare = x.Order.CompareTo(y.Order);
        if (orderCompare != 0)
        {
            return orderCompare;
        }

        return string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
    }
}