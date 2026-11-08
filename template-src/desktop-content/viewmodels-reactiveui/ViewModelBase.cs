using System.Reactive;
using ReactiveUI;

namespace PnvApp.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public Interaction<string, Unit> ShowMessage { get; } = new();
}
