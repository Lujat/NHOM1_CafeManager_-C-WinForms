using System.Data;

namespace CafeManager.Views.Contracts;

public interface IRevenueView
{
    event Action Loaded;
    event Action SearchRequested;
    DateTime FromDate { get; }
    DateTime ToDate { get; }
    void ShowRevenue(DataTable dailyRevenue, decimal totalRevenue);
    void ShowError(string message);
}
