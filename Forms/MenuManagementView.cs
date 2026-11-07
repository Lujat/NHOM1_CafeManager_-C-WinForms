using CafeManager.Models;
using CafeManager.Presenters;
using CafeManager.Views.Contracts;

namespace CafeManager.Forms;

public sealed class MenuManagementView : Form, IMenuManagementView
{
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false
    };

    public int? SelectedFoodId => (grid.CurrentRow?.DataBoundItem as Food)?.Id;
    public event Action Loaded;
    public event Action AddFoodRequested;
    public event Action EditFoodRequested;
    public event Action DeleteFoodRequested;
    public event Action RefreshRequested;

    public MenuManagementView(Role role)
    {
        if (!RolePermissions.CanManageMenu(role)) throw new UnauthorizedAccessException("Chỉ quản lý được phép sửa thực đơn.");
        Text = "Quản lý thực đơn";
        ClientSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterParent;

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6) };
        var add = new Button { Text = "Thêm món", AutoSize = true };
        var edit = new Button { Text = "Sửa món đang chọn", AutoSize = true };
        var delete = new Button { Text = "Xóa món", AutoSize = true };
        var refresh = new Button { Text = "Làm mới", AutoSize = true };
        add.Click += (s, e) => AddFoodRequested?.Invoke();
        edit.Click += (s, e) => EditFoodRequested?.Invoke();
        delete.Click += (s, e) => DeleteFoodRequested?.Invoke();
        refresh.Click += (s, e) => RefreshRequested?.Invoke();
        toolbar.Controls.AddRange(new Control[] { add, edit, delete, refresh });
        grid.DataBindingComplete += (s, e) => ConfigureColumns();
        Controls.Add(grid);
        Controls.Add(toolbar);
        Load += (s, e) => Loaded?.Invoke();
    }

    public void ShowFoods(IReadOnlyList<Food> foods)
    {
        grid.DataSource = foods.ToList();
        ConfigureColumns();
    }

    void ConfigureColumns()
    {
        if (grid.Columns.Count == 0) return;
        grid.Columns[nameof(Food.Id)].Visible = false;
        grid.Columns[nameof(Food.CategoryId)].Visible = false;
        grid.Columns[nameof(Food.ImagePath)].Visible = false;
        grid.Columns[nameof(Food.Name)].HeaderText = "Tên món";
        grid.Columns[nameof(Food.CategoryName)].HeaderText = "Nhóm món";
        grid.Columns[nameof(Food.Price)].HeaderText = "Giá (đ)";
        grid.Columns[nameof(Food.Price)].DefaultCellStyle.Format = "N0";
    }

    public Food ShowFoodEditor(Food existingFood)
    {
        using var editor = new FoodEditorView(existingFood);
        _ = new FoodEditorPresenter(editor, existingFood);
        return editor.ShowDialog(this) == DialogResult.OK ? editor.Result : null;
    }

    public bool Confirm(string message)
        => MessageBox.Show(this, message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

    public void ShowMessage(string message)
        => MessageBox.Show(this, message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void ShowError(string message)
        => MessageBox.Show(this, message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
