using CriFsLib.GUI.Model;
using CriFsLib.GUI.ViewModel;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;
using System;
using System.Windows;
using System.Windows.Data;

namespace CriFsLib.GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ReloadedWindow
{
    public new MainPageViewModel ViewModel { get; set; }

    private CollectionViewSource _viewSource;

    public MainWindow()
    {
        ViewModel = new MainPageViewModel();
        InitializeComponent();

        var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
        _viewSource = manipulator.Get<CollectionViewSource>("FilteredItems");
        _viewSource.Filter += FilterItems;
    }

    private void FilterItems(object sender, FilterEventArgs e)
    {
        if (ItemsFilter.Text.Length <= 0)
        {
            e.Accepted = true;
            return;
        }

        var model = (CpkFileModel)e.Item;
        e.Accepted = model.FullPath.Contains(ItemsFilter.Text, StringComparison.OrdinalIgnoreCase);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;
        
        // Note that you can have more than one file.
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        ViewModel.OpenCpk(files[0]);
    }

    private void ItemsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => _viewSource.View.Refresh();

    private async void Extract_Click(object sender, RoutedEventArgs e) => await ViewModel.Extract();

    private async void ExtractAll_Click(object sender, RoutedEventArgs e) => await ViewModel.ExtractAllAsync();
}
