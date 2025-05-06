using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Editor;

/// <summary>
/// Interaction logic for ListBoxWithFilter.xaml
/// </summary>
public partial class ListBoxWithFilter : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ListBoxWithFilter()
    {
        InitializeComponent();
    }

    // Dependency Property: Title
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(ListBoxWithFilter),
        new PropertyMetadata("Filter items:")
    );

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    // Bindable property for ItemsSource
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ListBoxWithFilter),
            new PropertyMetadata(null, OnFilterOrItemsSourceChanged));

    // Bindable property for Filter string
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(nameof(Filter), typeof(string), typeof(ListBoxWithFilter),
            new PropertyMetadata(string.Empty, OnFilterOrItemsSourceChanged));

    // Internal property for filtered items
    public IEnumerable FilteredItems
    {
        get => (IEnumerable)GetValue(FilteredItemsProperty);
        private set => SetValue(FilteredItemsProperty, value);
    }
    public static readonly DependencyProperty FilteredItemsProperty =
        DependencyProperty.Register(nameof(FilteredItems), typeof(IEnumerable), typeof(ListBoxWithFilter), new PropertyMetadata(null));

    // Bindable property for SelectedItem
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(ListBoxWithFilter),
            new PropertyMetadata(null, OnSelectedItemChanged));

    // Bindable property for ItemTemplate
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(ListBoxWithFilter), new PropertyMetadata(null));

    // Update FilteredItems when ItemsSource or Filter changes
    private static void OnFilterOrItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListBoxWithFilter wrapper)
        {
            wrapper.UpdateFilteredItems();
        }
    }

    // Apply filter logic
    private void UpdateFilteredItems()
    {
        if (ItemsSource == null)
        {
            FilteredItems = null;
            return;
        }

        var items = ItemsSource.Cast<object>();

        if (!string.IsNullOrEmpty(Filter))
        {
            // item == SelectedItem ensures that the selected item is never filtered out, which could lead to unexpected null selection and unexpected UI behavior and/or null refs
            items = items.Where(item => item.ToString()?.Contains(Filter, StringComparison.OrdinalIgnoreCase) == true || item == SelectedItem);
        }

        var itemsList = items.ToList();

        // Don't change if the items already are equal, otherwise due to OnSelectedItemChanged, it would cause focus to bounce to focus text box every time, messing up/down focus behavior
        if (FilteredItems == null || !itemsList.SequenceEqual(FilteredItems.Cast<object>()))
        {
            FilteredItems = itemsList;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // When the selected item changes, update the filter logic. This may seem strange, but the reason we're doing this is because that item == SelectedItem check mentioned above
        // If we change the selected item and it should have been filtered out in the first place, we'd want to filter it out then, otherwise it could linger
        if (d is ListBoxWithFilter wrapper)
        {
            wrapper.UpdateFilteredItems();
        }
    }
}
