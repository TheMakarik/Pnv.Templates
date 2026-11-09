using Avalonia.Controls;
using Pnv.Combo.Desktop.ViewModels;

namespace Pnv.Combo.Desktop.View;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is MainViewModel viewModel)
        {
            _ = viewModel.InitializeAsync();
        }
    }
}
