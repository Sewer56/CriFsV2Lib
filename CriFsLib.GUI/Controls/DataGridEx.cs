using System.Collections;
using System.Windows.Controls;
using System.Windows;

namespace CriFsLib.GUI.Controls;

public class DataGridEx : DataGrid
{
    public DataGridEx() => this.SelectionChanged += CustomDataGrid_SelectionChanged;

    void CustomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) => this.SelectedItemsList = this.SelectedItems;

    #region SelectedItemsList
    public IList SelectedItemsList
    {
        get { return (IList)GetValue(SelectedItemsListProperty); }
        set { SetValue(SelectedItemsListProperty, value); }
    }

    public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(DataGridEx), new PropertyMetadata(null));
    #endregion
}