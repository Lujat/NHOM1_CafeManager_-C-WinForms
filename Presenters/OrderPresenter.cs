using CafeManager.Helpers;
using CafeManager.Models;
using CafeManager.Repositories;
using CafeManager.Views.Contracts;

namespace CafeManager.Presenters;
// phan quyen phucvu 

public sealed class OrderPresenter
{
    readonly IOrderView _view;
    readonly IOrderNavigator _navigator;
    readonly Account _user;
    List<TableFood> _tables = new();
    List<BillItem> _items = new();
    TableFood _currentTable;
    int _billId;

    public OrderPresenter(IOrderView view, IOrderNavigator navigator)
    {
        _view = view;
        _navigator = navigator;
        _user = view.User;

        _view.Loaded += Initialize;
        _view.TableSelected += SelectTable;
        _view.CategorySelected += LoadFoods;
        _view.AddFoodRequested += AddFood;
        _view.QuantityChangeRequested += ChangeSelectedQuantity;
        _view.DiscountChanged += UpdateTotal;
        _view.MoveRequested += MoveOrMerge;
        _view.CheckoutRequested += Checkout;
        _view.KitchenRequested += OpenKitchen;
        _view.MenuManagementRequested += ManageMenu;
        _view.RevenueRequested += OpenRevenue;
        _view.LogoutRequested += _view.CloseForLogout;
    }

    void Initialize()
    {
        try
        {
            _view.SetPermissions(
                RolePermissions.CanManageOrders(_user.Role), RolePermissions.CanCheckout(_user.Role),
                RolePermissions.CanManageMenu(_user.Role), RolePermissions.CanViewKitchen(_user.Role),
                RolePermissions.CanViewRevenue(_user.Role));
            int? selectedTableId = _view.SelectedTableId;
            _tables = TableRepository.GetAll();
            _currentTable = selectedTableId.HasValue ? _tables.FirstOrDefault(t => t.Id == selectedTableId.Value) : null;
            _view.ShowTables(_tables, _currentTable?.Id);
            _view.ShowCategories(FoodRepository.GetCategories());
            if (_view.SelectedCategoryId is int categoryId) LoadFoods(categoryId);
            ShowCurrentBill();
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }

    void SelectTable(int tableId)
    {
        try
        {
            _tables = TableRepository.GetAll();
            _currentTable = _tables.FirstOrDefault(t => t.Id == tableId);
            _view.ShowTables(_tables, _currentTable?.Id);
            ShowCurrentBill();
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }

    void LoadFoods(int categoryId)
    {
        try { _view.ShowFoods(categoryId > 0 ? FoodRepository.GetByCategory(categoryId) : new List<Food>()); }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
    void AddFood()
    {
        if (!RolePermissions.CanManageOrders(_user.Role)) { _view.ShowMessage("Vai trò của bạn không có quyền gọi món."); return; }
        if (_currentTable == null) { _view.ShowMessage("Hãy chọn bàn trước."); return; }
        if (_view.SelectedFoodId is not int foodId) return;
        ExecuteQuantityChange(foodId, _view.Quantity);
    }
    void ChangeSelectedQuantity(int delta)
    {
        if (!RolePermissions.CanManageOrders(_user.Role)) { _view.ShowMessage("Vai trò của bạn không có quyền sửa số lượng món."); return; }
        if (_currentTable == null || _view.SelectedBillFoodId is not int foodId) return;
        ExecuteQuantityChange(foodId, delta);}
    void ExecuteQuantityChange(int foodId, int delta)
    {
        try
        {
            BillRepository.ChangeQuantity(_currentTable.Id, foodId, delta);
            RefreshTables();
            ShowCurrentBill();
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
    void MoveOrMerge()
    {
        if (!RolePermissions.CanManageOrders(_user.Role)) { _view.ShowMessage("Vai trò của bạn không có quyền chuyển hoặc gộp bàn."); return; }
        if (_currentTable == null || _billId == 0) { _view.ShowMessage("Bàn hiện tại chưa có hóa đơn."); return; }
        if (_view.SelectedTargetTableId is not int targetId) return;
        var target = _tables.FirstOrDefault(t => t.Id == targetId);
        if (target == null) return;
        string action = target.Status == 0 ? "chuyển sang" : "gộp hóa đơn vào";
        if (!_view.Confirm($"Xác nhận {action} {target.Name}?", "Xác nhận")) return;
        try
        {
            BillRepository.MoveOrMerge(_currentTable.Id, target.Id);
            RefreshTables();
            SelectTable(target.Id);
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
    void Checkout()
    {
        if (!RolePermissions.CanCheckout(_user.Role)) { _view.ShowMessage("Vai trò của bạn không có quyền thanh toán."); return; }
        if (_currentTable == null || _billId == 0 || _items.Count == 0) { _view.ShowMessage("Bàn này chưa có món để thanh toán."); return; }
        decimal subtotal = _items.Sum(item => item.Total);
        int discount = Math.Clamp(_view.DiscountPercent, 0, 100);
        decimal total = Math.Round(subtotal * (100 - discount) / 100m, 0);
        if (!_view.Confirm($"Thanh toán {_currentTable.Name}: {total:N0} đ?", "Thanh toán")) return;
        try
        {
            var itemsToPrint = _items.ToList();
            string tableName = _currentTable.Name;
            total = BillRepository.Checkout(_billId, _currentTable.Id, discount);
            _view.SetDiscountPercent(0);
            RefreshTables();
            ShowCurrentBill();
            PrintHelper.PrintBill(tableName, itemsToPrint, subtotal, discount, total, _user.FullName);
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
    void UpdateTotal()
    {
        if (_currentTable != null) ShowCurrentBill();
    }
    void OpenKitchen()
    {
        if (!RolePermissions.CanViewKitchen(_user.Role)) { _view.ShowMessage("Vai trò của bạn không có quyền xem màn hình bếp."); return; }
        _navigator.OpenKitchen(_user.Role);
    }
    void OpenRevenue()
    {
        if (!RolePermissions.CanViewRevenue(_user.Role)) { _view.ShowMessage("Chỉ quản lý được xem doanh thu."); return; }
        _navigator.OpenRevenue(_user.Role);
    }
    void ManageMenu()
    {
        if (!RolePermissions.CanManageMenu(_user.Role)) { _view.ShowMessage("Chỉ quản lý được phép sửa thực đơn."); return; }
        try
        {
            _navigator.OpenMenuManagement(_user.Role);
            _tables = TableRepository.GetAll();
            if (_currentTable != null) _currentTable = _tables.FirstOrDefault(t => t.Id == _currentTable.Id);
            _view.ShowTables(_tables, _currentTable?.Id);
            _view.ShowCategories(FoodRepository.GetCategories());
            if (_view.SelectedCategoryId is int categoryId) LoadFoods(categoryId);
            ShowCurrentBill();
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
    void RefreshTables()
    {
        _tables = TableRepository.GetAll();
        if (_currentTable != null) _currentTable = _tables.FirstOrDefault(t => t.Id == _currentTable.Id);
        _view.ShowTables(_tables, _currentTable?.Id);
    }
    void ShowCurrentBill()
    {
        _items = new List<BillItem>();
        _billId = 0;
        if (_currentTable != null)
        {
            _billId = BillRepository.GetUnpaidBillId(_currentTable.Id);
            if (_billId != 0) _items = BillRepository.GetItems(_billId);
        }
        var subtotal = _items.Sum(item => item.Total);
        var discount = Math.Clamp(_view.DiscountPercent, 0, 100);
        var total = Math.Round(subtotal * (100 - discount) / 100m, 0);
        _view.ShowBill(_currentTable, _items, _tables.Where(t => t.Id != _currentTable?.Id).ToList(), total);
    }
}
