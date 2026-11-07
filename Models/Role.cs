namespace CafeManager.Models;

public enum Role { Waiter = 0, Cashier = 1, Manager = 2 }

public static class RolePermissions
{
    public static bool CanManageOrders(Role role) => role is Role.Waiter or Role.Manager;
    public static bool CanCheckout(Role role) => role is Role.Cashier or Role.Manager;
    public static bool CanViewKitchen(Role role) => role is Role.Waiter or Role.Manager;
    public static bool CanManageMenu(Role role) => role == Role.Manager;
    public static bool CanViewRevenue(Role role) => role == Role.Manager;
}
