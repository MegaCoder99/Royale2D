using Shared;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SpriteEditor;

public partial class ChangeRectDialog : Window, INotifyPropertyChanged
{
    private int x1, y1, x2, y2;

    public int X1 { get => x1; set { x1 = value; OnPropertyChanged(); } }
    public int Y1 { get => y1; set { y1 = value; OnPropertyChanged(); } }
    public int X2 { get => x2; set { x2 = value; OnPropertyChanged(); } }
    public int Y2 { get => y2; set { y2 = value; OnPropertyChanged(); } }

    public ChangeRectDialog(MyRect initialRect)
    {
        InitializeComponent();
        X1 = initialRect.x1;
        Y1 = initialRect.y1;
        X2 = initialRect.x2;
        Y2 = initialRect.y2;
        DataContext = this;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (X2 <= X1 || Y2 <= Y1)
        {
            MessageBox.Show(this, "Invalid rectangle: X2 must be greater than X1 and Y2 must be greater than Y1.",
                            "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
