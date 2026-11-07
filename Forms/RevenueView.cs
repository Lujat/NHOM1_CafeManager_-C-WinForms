using System.Data;
using CafeManager.Models;
using CafeManager.Views.Contracts;

namespace CafeManager.Forms;

public sealed class RevenueView : Form, IRevenueView
{
    readonly DateTimePicker dtFrom = new() { Left = 60, Top = 12, Width = 110, Format = DateTimePickerFormat.Short };
    readonly DateTimePicker dtTo = new() { Left = 230, Top = 12, Width = 110, Format = DateTimePickerFormat.Short };
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    readonly Label lblSum = new() { Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
    readonly ChartPanel chart = new() { Dock = DockStyle.Fill };

    public DateTime FromDate => dtFrom.Value.Date;
    public DateTime ToDate => dtTo.Value.Date;
    public event Action Loaded;
    public event Action SearchRequested;

    public RevenueView(Role role)
    {
        if (!RolePermissions.CanViewRevenue(role)) throw new UnauthorizedAccessException("Chỉ quản lý được phép xem doanh thu.");
        Text = "Doanh thu";
        ClientSize = new Size(820, 600);
        StartPosition = FormStartPosition.CenterParent;
        dtFrom.Value = DateTime.Today.AddDays(-6);
        dtTo.Value = DateTime.Today;

        var filters = new Panel { Dock = DockStyle.Top, Height = 46 };
        var search = new Button { Left = 360, Top = 10, Width = 90, Height = 28, Text = "Xem" };
        search.Click += (s, e) => SearchRequested?.Invoke();
        filters.Controls.AddRange(new Control[]
        {
            new Label { Left = 10, Top = 16, AutoSize = true, Text = "Từ:" }, dtFrom,
            new Label { Left = 185, Top = 16, AutoSize = true, Text = "Đến:" }, dtTo, search
        });

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 200 };
        split.Panel1.Controls.Add(grid);
        split.Panel1.Controls.Add(lblSum);
        split.Panel2.Controls.Add(chart);
        Controls.Add(split);
        Controls.Add(filters);
        Load += (s, e) => Loaded?.Invoke();
    }

    public void ShowRevenue(DataTable dailyRevenue, decimal totalRevenue)
    {
        grid.DataSource = dailyRevenue;
        if (grid.Columns["Ngay"] != null) { grid.Columns["Ngay"].HeaderText = "Ngày"; grid.Columns["Ngay"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
        if (grid.Columns["SoHoaDon"] != null) grid.Columns["SoHoaDon"].HeaderText = "Số hóa đơn";
        if (grid.Columns["DoanhThu"] != null) { grid.Columns["DoanhThu"].HeaderText = "Doanh thu (đ)"; grid.Columns["DoanhThu"].DefaultCellStyle.Format = "N0"; }
        lblSum.Text = $"Tổng doanh thu: {totalRevenue:N0} đ   ";
        chart.SetData(dailyRevenue.AsEnumerable().Select(row =>
            (row.Field<DateTime>("Ngay").ToString("dd/MM"), row.Field<decimal>("DoanhThu"))).ToList());
    }

    public void ShowError(string message)
        => MessageBox.Show(this, message, "Doanh thu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
