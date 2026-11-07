using System.Data;

namespace CafeManager.Views.Contracts;

public interface IKitchenView
{
    event Action Loaded;
    event Action RefreshRequested;
    event Action MarkDoneRequested;
    int? SelectedItemId { get; }
    void ShowPendingItems(DataTable items);
    void ShowMessage(string message);
    void ShowError(string message);
}
