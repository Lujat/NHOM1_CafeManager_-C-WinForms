using System.Data;
using CafeManager.Models;
using CafeManager.Repositories;
using CafeManager.Views.Contracts;

namespace CafeManager.Presenters;
// quan ly doanh thu(nho phai phan quyen)

public sealed class RevenuePresenter
{
    readonly IRevenueView _view;
    readonly Role _role;
    public RevenuePresenter(IRevenueView view, Role role)
    {
        if (!RolePermissions.CanViewRevenue(role)) throw new UnauthorizedAccessException("Chỉ quản lý được phép xem doanh thu.");
        _view = view;
        _role = role;
        _view.Loaded += Load;
        _view.SearchRequested += Load;
    }
    void Load()
    {
        if (!RolePermissions.CanViewRevenue(_role)) { _view.ShowError("Chỉ quản lý được phép xem doanh thu."); return; }
        if (_view.FromDate.Date > _view.ToDate.Date) { _view.ShowError("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc."); return; }
        try
        {
            DataTable rows = RevenueRepository.GetDaily(_view.FromDate, _view.ToDate);
            decimal total = rows.AsEnumerable().Sum(row => row.Field<decimal>("DoanhThu"));
            _view.ShowRevenue(rows, total);
        }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
}
