using CafeManager.Models;
using CafeManager.Presenters;
using CafeManager.Helpers;
using CafeManager.Views.Contracts;

namespace CafeManager.Forms;

public sealed class OrderView : Form, IOrderView, IOrderNavigator
{
    readonly FlowLayoutPanel pnlTables = new() { Dock = DockStyle.Fill, AutoScroll = true };
    readonly ListView billList = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
    readonly ComboBox cbCategory = new() { Left = 10, Top = 48, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox cbFood = new() { Left = 160, Top = 48, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox cbTargetTable = new() { Left = 235, Top = 11, Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly NumericUpDown nudQuantity = new() { Left = 400, Top = 48, Width = 50, Minimum = 1, Maximum = 99, Value = 1 };
    readonly NumericUpDown nudDiscount = new() { Left = 95, Top = 54, Width = 55, Minimum = 0, Maximum = 100 };
    readonly Label lblTable = new() { Left = 10, Top = 8, Width = 600, Height = 28, Font = new Font("Segoe UI", 14, FontStyle.Bold), Text = "Chọn một bàn" };
    readonly Label lblTotal = new() { Left = 170, Top = 50, Width = 400, Height = 30, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.Firebrick };
    readonly PictureBox picFood = new() { Left = 710, Top = 12, Width = 78, Height = 78, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, Visible = false };
    readonly Button btnAdd = new() { Left = 460, Top = 46, Width = 100, Height = 28, Text = "Thêm món" };
    readonly Button btnManageFood = new() { Left = 570, Top = 46, Width = 125, Height = 28, Text = "Quản lý món" };
    readonly Button btnIncrease = new() { Left = 10, Top = 10, Width = 40, Height = 28, Text = "+" };
    readonly Button btnDecrease = new() { Left = 55, Top = 10, Width = 40, Height = 28, Text = "−" };
    readonly Button btnMove = new() { Left = 435, Top = 9, Width = 125, Height = 28, Text = "Chuyển / Gộp bàn" };
    readonly Button btnCheckout = new() { Left = 10, Top = 88, Width = 220, Height = 34, Text = "Thanh toán && In hóa đơn" };
    readonly ToolStripMenuItem miKitchen = new("Màn hình bếp");
    readonly ToolStripMenuItem miRevenue = new("Doanh thu");
    readonly ToolStripMenuItem miMenuManagement = new("Quản lý thực đơn");
    readonly ToolStripMenuItem miLogout = new("Đăng xuất");
    bool _bindingCategories;

    public Account User { get; }
    public bool LoggedOut { get; private set; }
    public int? SelectedTableId { get; private set; }
    public int? SelectedCategoryId => (cbCategory.SelectedItem as Category)?.Id;
    public int? SelectedFoodId => (cbFood.SelectedItem as Food)?.Id;
    public int? SelectedTargetTableId => (cbTargetTable.SelectedItem as TableFood)?.Id;
    public int? SelectedBillFoodId => billList.SelectedItems.Count == 0 ? null : (billList.SelectedItems[0].Tag as BillItem)?.FoodId;
    public int Quantity => (int)nudQuantity.Value;
    public int DiscountPercent => (int)nudDiscount.Value;

    public event Action Loaded;
    public event Action<int> TableSelected;
    public event Action<int> CategorySelected;
    public event Action AddFoodRequested;
    public event Action<int> QuantityChangeRequested;
    public event Action DiscountChanged;
    public event Action MoveRequested;
    public event Action CheckoutRequested;
    public event Action KitchenRequested;
    public event Action MenuManagementRequested;
    public event Action RevenueRequested;
    public event Action LogoutRequested;

    public OrderView(Account user)
    {
        User = user;
        Text = $"Quản lý quán cà phê - {user.FullName} ({user.Role})";
        ClientSize = new Size(1250, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BuildLayout();
        WireEvents();
        Load += (s, e) => Loaded?.Invoke();
    }

    void BuildLayout()
    {
        var menu = new MenuStrip();
        var system = new ToolStripMenuItem("Hệ thống");
        system.DropDownItems.AddRange(new ToolStripItem[] { miKitchen, miMenuManagement, miRevenue, new ToolStripSeparator(), miLogout });
        menu.Items.Add(system);
        MainMenuStrip = menu;

        var tableGroup = new GroupBox { Text = "Danh sách bàn", Dock = DockStyle.Left, Width = 420, Padding = new Padding(8) };
        tableGroup.Controls.Add(pnlTables);
        tableGroup.Controls.Add(new Label { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft, Text = "  ■ Xanh: Trống      ■ Cam: Có khách" });

        var top = new Panel { Dock = DockStyle.Top, Height = 100 };
        top.Controls.AddRange(new Control[] { lblTable, cbCategory, cbFood, nudQuantity, btnAdd, btnManageFood, picFood });

        billList.Columns.Add("Món", 230);
        billList.Columns.Add("Đơn giá", 100, HorizontalAlignment.Right);
        billList.Columns.Add("SL", 50, HorizontalAlignment.Right);
        billList.Columns.Add("Thành tiền", 110, HorizontalAlignment.Right);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 130 };
        bottom.Controls.AddRange(new Control[]
        {
            btnIncrease, btnDecrease,
            new Label { Left = 120, Top = 15, AutoSize = true, Text = "Chuyển/gộp sang:" }, cbTargetTable, btnMove,
            new Label { Left = 10, Top = 58, AutoSize = true, Text = "Giảm giá (%):" }, nudDiscount,
            lblTotal, btnCheckout
        });

        var right = new Panel { Dock = DockStyle.Fill };
        right.Controls.Add(billList);
        right.Controls.Add(bottom);
        right.Controls.Add(top);
        Controls.Add(right);
        Controls.Add(tableGroup);
        Controls.Add(menu);
    }

    void WireEvents()
    {
        btnAdd.Click += (s, e) => AddFoodRequested?.Invoke();
        btnIncrease.Click += (s, e) => QuantityChangeRequested?.Invoke(+1);
        btnDecrease.Click += (s, e) => QuantityChangeRequested?.Invoke(-1);
        btnMove.Click += (s, e) => MoveRequested?.Invoke();
        btnCheckout.Click += (s, e) => CheckoutRequested?.Invoke();
        btnManageFood.Click += (s, e) => MenuManagementRequested?.Invoke();
        nudDiscount.ValueChanged += (s, e) => DiscountChanged?.Invoke();
        cbCategory.SelectedIndexChanged += (s, e) =>
        {
            ShowSelectedFoodImage();
            if (!_bindingCategories && SelectedCategoryId is int categoryId) CategorySelected?.Invoke(categoryId);
        };
        cbFood.SelectedIndexChanged += (s, e) => ShowSelectedFoodImage();
        miKitchen.Click += (s, e) => KitchenRequested?.Invoke();
        miMenuManagement.Click += (s, e) => MenuManagementRequested?.Invoke();
        miRevenue.Click += (s, e) => RevenueRequested?.Invoke();
        miLogout.Click += (s, e) => LogoutRequested?.Invoke();
    }

    public void SetPermissions(bool canManageOrders, bool canCheckout, bool canManageMenu, bool canViewKitchen, bool canViewRevenue)
    {
        cbCategory.Enabled = cbFood.Enabled = nudQuantity.Enabled = btnAdd.Enabled = canManageOrders;
        btnIncrease.Enabled = btnDecrease.Enabled = btnMove.Enabled = canManageOrders;
        nudDiscount.Enabled = btnCheckout.Enabled = canCheckout;
        btnManageFood.Visible = canManageMenu;
        miMenuManagement.Enabled = canManageMenu;
        miKitchen.Enabled = canViewKitchen;
        miRevenue.Enabled = canViewRevenue;
    }

    public void ShowTables(IReadOnlyList<TableFood> tables, int? selectedTableId)
    {
        SelectedTableId = selectedTableId;
        pnlTables.SuspendLayout();
        pnlTables.Controls.Clear();
        foreach (var table in tables)
        {
            var button = new Button
            {
                Width = 100, Height = 80, Margin = new Padding(6), Text = $"{table.Name}\n{table.StatusText}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat,
                BackColor = table.Status == 0 ? Color.LightGreen : Color.Orange
            };
            bool selected = selectedTableId == table.Id;
            button.FlatAppearance.BorderSize = selected ? 4 : 1;
            button.FlatAppearance.BorderColor = selected ? Color.DodgerBlue : Color.DimGray;
            button.Click += (s, e) => TableSelected?.Invoke(table.Id);
            pnlTables.Controls.Add(button);
        }
        pnlTables.ResumeLayout();
    }

    public void ShowCategories(IReadOnlyList<Category> categories)
    {
        int? keepId = SelectedCategoryId;
        _bindingCategories = true;
        cbCategory.DataSource = categories.ToList();
        if (keepId.HasValue) cbCategory.SelectedItem = categories.FirstOrDefault(item => item.Id == keepId.Value);
        if (cbCategory.SelectedIndex < 0 && cbCategory.Items.Count > 0) cbCategory.SelectedIndex = 0;
        _bindingCategories = false;
        ShowSelectedFoodImage();
    }

    public void ShowFoods(IReadOnlyList<Food> foods)
    {
        cbFood.DataSource = foods.ToList();
        ShowSelectedFoodImage();
    }

    public void ShowBill(TableFood table, IReadOnlyList<BillItem> items, IReadOnlyList<TableFood> targetTables, decimal total)
    {
        lblTable.Text = table == null ? "Chọn một bàn" : $"{table.Name} - {table.StatusText}";
        billList.Items.Clear();
        foreach (var item in items)
        {
            var row = new ListViewItem(item.FoodName) { Tag = item };
            row.SubItems.Add(item.Price.ToString("N0"));
            row.SubItems.Add(item.Quantity.ToString());
            row.SubItems.Add(item.Total.ToString("N0"));
            billList.Items.Add(row);
        }
        cbTargetTable.DataSource = targetTables.ToList();
        lblTotal.Text = $"Tổng: {total:N0} đ";
    }

    public void SetDiscountPercent(int discountPercent)
        => nudDiscount.Value = Math.Clamp(discountPercent, (int)nudDiscount.Minimum, (int)nudDiscount.Maximum);

    public bool Confirm(string message, string title)
        => MessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public void ShowMessage(string message)
        => MessageBox.Show(this, message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void ShowError(string message)
        => MessageBox.Show(this, message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

    public void CloseForLogout()
    {
        LoggedOut = true;
        Close();
    }

    void ShowSelectedFoodImage()
    {
        var old = picFood.Image;
        picFood.Image = cbFood.SelectedItem is Food food ? FoodImageStorage.LoadImage(food.ImagePath) : null;
        old?.Dispose();
        picFood.Visible = picFood.Image != null;
    }

    public void OpenKitchen(Role role)
    {
        using var view = new KitchenView(role);
        _ = new KitchenPresenter(view, role);
        view.ShowDialog(this);
    }

    public void OpenMenuManagement(Role role)
    {
        using var view = new MenuManagementView(role);
        _ = new MenuManagementPresenter(view, role);
        view.ShowDialog(this);
    }

    public void OpenRevenue(Role role)
    {
        using var view = new RevenueView(role);
        _ = new RevenuePresenter(view, role);
        view.ShowDialog(this);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        picFood.Image?.Dispose();
        base.OnFormClosed(e);
    }
}
