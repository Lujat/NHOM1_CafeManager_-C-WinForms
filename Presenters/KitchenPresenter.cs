using CafeManager.Models;
using CafeManager.Repositories;
using CafeManager.Views.Contracts;

namespace CafeManager.Presenters;

// phan quyen cho bep
public sealed class KitchenPresenter
{
    readonly IKitchenView _view;
    readonly Role _role;
    public KitchenPresenter(IKitchenView view, Role role)
    {
        if (!RolePermissions.CanViewKitchen(role)) throw new UnauthorizedAccessException("Vai trò của bạn không được phép mở màn hình bếp.");
        _view = view;
        _role = role;
        _view.Loaded += Reload;
        _view.RefreshRequested += Reload;
        _view.MarkDoneRequested += MarkDone;
    }
    void Reload()
    {
        try { _view.ShowPendingItems(KitchenRepository.GetPending()); }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
    void MarkDone()
    {
        if (!RolePermissions.CanViewKitchen(_role)) { _view.ShowMessage("Vai trò của bạn không có quyền cập nhật món bếp."); return; }
        if (_view.SelectedItemId is not int itemId) { _view.ShowMessage("Hãy chọn món cần cập nhật."); return; }
        try
        {
            if (!KitchenRepository.MarkDone(itemId))
                _view.ShowMessage("Món đã được cập nhật hoặc hóa đơn đã đóng.");
            Reload();
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
}
