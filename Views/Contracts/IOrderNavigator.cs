using CafeManager.Models;

namespace CafeManager.Views.Contracts;

public interface IOrderNavigator
{
    void OpenKitchen(Role role);
    void OpenMenuManagement(Role role);
    void OpenRevenue(Role role);
}
