using CafeManager.Models;
namespace CafeManager.Views.Contracts;
public interface IFoodEditorView
{
    event Action SaveRequested;
    event Action ChooseImageRequested;
    event Action ClearImageRequested;
    string FoodName { get; }
    int? CategoryId { get; }
    decimal Price { get; }
    string SelectedImageSource { get; }
    bool ClearImage { get; }
    void ShowCategories(IReadOnlyList<Category> categories, int? selectedCategoryId);
    void ShowFood(Food food);
    void SetImageSource(string imagePath);
    void SetSavedFood(Food food);
    void ShowValidationMessage(string message);
    void ShowError(string message);
}
