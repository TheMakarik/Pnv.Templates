using System.Reactive;
using ReactiveUI;

namespace Pnv.Desktop.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public Interaction<string, Unit> ShowMessage { get; } = new();
}
