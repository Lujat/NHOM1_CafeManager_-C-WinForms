using System.Data;
using CafeManager.Models;
using CafeManager.Views.Contracts;

namespace CafeManager.Forms;

public sealed class KitchenView : Form, IKitchenView
{
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false
    };
    readonly System.Windows.Forms.Timer refreshTimer = new() { Interval = 5000 };
    readonly Label lblState = new() { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft };

    public event Action Loaded;
    public event Action RefreshRequested;
    public event Action MarkDoneRequested;
    public int? SelectedItemId => grid.CurrentRow?.Cells["Id"].Value is int id ? id : null;

    public KitchenView(Role role)
    {
        if (!RolePermissions.CanViewKitchen(role)) throw new UnauthorizedAccessException("Vai trò của bạn không được phép mở màn hình bếp.");
        Text = "Màn hình bếp";
        ClientSize = new Size(760, 500);
        StartPosition = FormStartPosition.CenterParent;

        var actions = new Panel { Dock = DockStyle.Bottom, Height = 42 };
        var done = new Button { Text = "Đánh dấu hoàn thành", Dock = DockStyle.Right, Width = 190, Height = 38 };
        var refresh = new Button { Text = "Làm mới", Dock = DockStyle.Left, Width = 100, Height = 38 };
        done.Click += (s, e) => MarkDoneRequested?.Invoke();
        refresh.Click += (s, e) => RefreshRequested?.Invoke();
        actions.Controls.Add(done);
        actions.Controls.Add(refresh);
        Controls.Add(grid);
        Controls.Add(lblState);
        Controls.Add(actions);

        grid.DataBindingComplete += (s, e) => ConfigureColumns();
        refreshTimer.Tick += (s, e) => RefreshRequested?.Invoke();
        Load += (s, e) => { Loaded?.Invoke(); refreshTimer.Start(); };
        FormClosed += (s, e) => refreshTimer.Stop();
    }

    public void ShowPendingItems(DataTable items)
    {
        int? keepId = SelectedItemId;
        grid.DataSource = items;
        ConfigureColumns();
        if (keepId.HasValue)
            foreach (DataGridViewRow row in grid.Rows)
                if (row.Cells["Id"].Value is int id && id == keepId.Value) { row.Selected = true; break; }
        lblState.Text = $"{grid.Rows.Count} món đang chờ · Tự làm mới mỗi 5 giây";
    }

    void ConfigureColumns()
    {
        if (grid.Columns["Id"] != null) grid.Columns["Id"].Visible = false;
        if (grid.Columns["Ban"] != null) grid.Columns["Ban"].HeaderText = "Bàn";
        if (grid.Columns["Mon"] != null) grid.Columns["Mon"].HeaderText = "Món";
        if (grid.Columns["SoLuong"] != null) grid.Columns["SoLuong"].HeaderText = "SL";
        if (grid.Columns["GioVao"] != null) { grid.Columns["GioVao"].HeaderText = "Giờ vào"; grid.Columns["GioVao"].DefaultCellStyle.Format = "HH:mm"; }
    }

    public void ShowMessage(string message)
        => MessageBox.Show(this, message, "Màn hình bếp", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void ShowError(string message)
        => MessageBox.Show(this, message, "Lỗi màn hình bếp", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
