using CafeManager.Models;

namespace CafeManager.Views.Contracts;

public interface IOrderView
{
    event Action Loaded;
    event Action<int> TableSelected;
    event Action<int> CategorySelected;
    event Action AddFoodRequested;
    event Action<int> QuantityChangeRequested;
    event Action DiscountChanged;
    event Action MoveRequested;
    event Action CheckoutRequested;
    event Action KitchenRequested;
    event Action MenuManagementRequested;
    event Action RevenueRequested;
    event Action LogoutRequested;

    Account User { get; }
    int? SelectedTableId { get; }
    int? SelectedFoodId { get; }
    int? SelectedCategoryId { get; }
    int? SelectedTargetTableId { get; }
    int? SelectedBillFoodId { get; }
    int Quantity { get; }
    int DiscountPercent { get; }

    void SetPermissions(bool canManageOrders, bool canCheckout, bool canManageMenu, bool canViewKitchen, bool canViewRevenue);
    void ShowTables(IReadOnlyList<TableFood> tables, int? selectedTableId);
    void ShowCategories(IReadOnlyList<Category> categories);
    void ShowFoods(IReadOnlyList<Food> foods);
    void ShowBill(TableFood table, IReadOnlyList<BillItem> items, IReadOnlyList<TableFood> targetTables, decimal total);
    void SetDiscountPercent(int discountPercent);
    bool Confirm(string message, string title);
    void ShowMessage(string message);
    void ShowError(string message);
    void CloseForLogout();
}
