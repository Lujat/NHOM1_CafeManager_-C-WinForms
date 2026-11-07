using CafeManager.Models;
using CafeManager.Repositories;
using CafeManager.Views.Contracts;

namespace CafeManager.Presenters;

// phan quyen admin menu
public sealed class MenuManagementPresenter
{
    readonly IMenuManagementView _view;
    readonly Role _role;

    public MenuManagementPresenter(IMenuManagementView view, Role role)
    {
        if (!RolePermissions.CanManageMenu(role)) throw new UnauthorizedAccessException("Chỉ quản lý được phép sửa thực đơn.");
        _view = view;
        _role = role;
        _view.Loaded += Reload;
        _view.RefreshRequested += Reload;
        _view.AddFoodRequested += () => EditFood(null);
        _view.EditFoodRequested += EditSelectedFood;
        _view.DeleteFoodRequested += DeleteSelectedFood;
    }

    void Reload()
    {
        if (!RolePermissions.CanManageMenu(_role)) { _view.ShowError("Chỉ quản lý được phép sửa thực đơn."); return; }
        try { _view.ShowFoods(FoodRepository.GetAll()); }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }

    void EditSelectedFood()
    {
        if (_view.SelectedFoodId is not int foodId) { _view.ShowMessage("Hãy chọn món cần sửa."); return; }
        var food = FoodRepository.GetAll().FirstOrDefault(item => item.Id == foodId);
        if (food == null) { _view.ShowMessage("Không tìm thấy món đã chọn."); Reload(); return; }
        EditFood(food);
    }

    void EditFood(Food existing)
    {
        if (!RolePermissions.CanManageMenu(_role)) { _view.ShowError("Chỉ quản lý được phép sửa thực đơn."); return; }
        var edited = _view.ShowFoodEditor(existing);
        if (edited == null) return;
        try { FoodRepository.Save(edited); Reload(); }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }

    void DeleteSelectedFood()
    {
        if (!RolePermissions.CanManageMenu(_role)) { _view.ShowError("Chỉ quản lý được phép xóa món."); return; }
        if (_view.SelectedFoodId is not int foodId) { _view.ShowMessage("Hãy chọn món cần xóa."); return; }
        var food = FoodRepository.GetAll().FirstOrDefault(item => item.Id == foodId);
        if (food == null) { _view.ShowMessage("Không tìm thấy món đã chọn."); Reload(); return; }
        if (!_view.Confirm($"Xóa món “{food.Name}”?")) return;
        try
        {
            if (!FoodRepository.Delete(food.Id))
                _view.ShowMessage("Món đã xuất hiện trong hóa đơn nên không thể xóa để giữ lịch sử bán hàng.");
            Reload();
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
}
