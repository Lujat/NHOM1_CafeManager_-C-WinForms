using CafeManager.Helpers;
using CafeManager.Models;
using CafeManager.Repositories;
using CafeManager.Views.Contracts;
namespace CafeManager.Presenters;

public sealed class FoodEditorPresenter
{
    readonly IFoodEditorView _view;
    readonly Food _existing;
    public FoodEditorPresenter(IFoodEditorView view, Food existing)
    {
        _view = view;
        _existing = existing;
        _view.SaveRequested += Save;
        _view.ChooseImageRequested += ChooseImage;
        _view.ClearImageRequested += ClearImage;
        Load();
    }
    void Load()
    {
        try
        {
            var categories = FoodRepository.GetCategories();
            _view.ShowCategories(categories, _existing?.CategoryId ?? categories.FirstOrDefault()?.Id);
            if (_existing != null) _view.ShowFood(_existing);
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
    void ChooseImage()
    {
        var source = _view.SelectedImageSource;
        if (!string.IsNullOrWhiteSpace(source)) _view.SetImageSource(source);
    }
    void ClearImage()
    {
        _view.SetImageSource(null);
    }
    void Save()
    {
        if (string.IsNullOrWhiteSpace(_view.FoodName)) { _view.ShowValidationMessage("Hãy nhập tên món."); return; }
        if (_view.CategoryId is not int categoryId) { _view.ShowValidationMessage("Hãy chọn nhóm món."); return; }
        try
        {
            string imagePath = _existing?.ImagePath;
            if (_view.ClearImage) imagePath = null;  
            else if (!string.IsNullOrWhiteSpace(_view.SelectedImageSource))
                imagePath = FoodImageStorage.CopyToLibrary(_view.SelectedImageSource, _view.FoodName);

            var category = FoodRepository.GetCategories().FirstOrDefault(item => item.Id == categoryId);
            _view.SetSavedFood(new Food
            {
                Id = _existing?.Id ?? 0,
                Name = _view.FoodName.Trim(),
                CategoryId = categoryId,
                CategoryName = category?.Name,
                Price = _view.Price,
                ImagePath = imagePath
            });
        }
        catch (Exception ex) { _view.ShowError("Không lưu được hình món:\n" + ex.Message); }
    }
}
