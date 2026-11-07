using CafeManager.Models;

namespace CafeManager.Views.Contracts;

public interface IMenuManagementView
{
    event Action Loaded;
    event Action AddFoodRequested;
    event Action EditFoodRequested;
    event Action DeleteFoodRequested;
    event Action RefreshRequested;
    int? SelectedFoodId { get; }
    void ShowFoods(IReadOnlyList<Food> foods);
    bool Confirm(string message);
    void ShowMessage(string message);
    void ShowError(string message);
    Food ShowFoodEditor(Food existingFood);
}
