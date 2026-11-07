using CafeManager.Helpers;
using CafeManager.Models;
using CafeManager.Views.Contracts;

namespace CafeManager.Forms;

public sealed class FoodEditorView : Form, IFoodEditorView
{
    readonly TextBox txtName = new() { Left = 105, Top = 16, Width = 260 };
    readonly ComboBox cbCategory = new() { Left = 105, Top = 52, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly NumericUpDown nudPrice = new() { Left = 105, Top = 88, Width = 150, Maximum = 1000000000, ThousandsSeparator = true };
    readonly PictureBox preview = new() { Left = 105, Top = 128, Width = 160, Height = 130, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
    readonly Label lblImage = new() { Left = 105, Top = 262, Width = 260, Height = 32, AutoEllipsis = true };

    public string FoodName => txtName.Text;
    public int? CategoryId => (cbCategory.SelectedItem as Category)?.Id;
    public decimal Price => nudPrice.Value;
    public string SelectedImageSource { get; private set; }
    public bool ClearImage { get; private set; }
    public Food Result { get; private set; }
    public event Action SaveRequested;
    public event Action ChooseImageRequested;
    public event Action ClearImageRequested;

    public FoodEditorView(Food existing)
    {
        Text = existing == null ? "Thêm món" : "Sửa món";
        ClientSize = new Size(395, 365);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.AddRange(new Control[]
        {
            new Label { Text = "Tên món:", Left = 18, Top = 20, AutoSize = true }, txtName,
            new Label { Text = "Nhóm món:", Left = 18, Top = 56, AutoSize = true }, cbCategory,
            new Label { Text = "Giá (đ):", Left = 18, Top = 92, AutoSize = true }, nudPrice,
            new Label { Text = "Hình món:", Left = 18, Top = 132, AutoSize = true }, preview, lblImage
        });

        var choose = new Button { Text = "Chọn ảnh…", Left = 280, Top = 135, Width = 90 };
        var remove = new Button { Text = "Bỏ ảnh", Left = 280, Top = 172, Width = 90 };
        var save = new Button { Text = "Lưu", Left = 200, Top = 315, Width = 80 };
        var cancel = new Button { Text = "Hủy", Left = 290, Top = 315, Width = 80, DialogResult = DialogResult.Cancel };
        choose.Click += (s, e) => ChooseImage();
        remove.Click += (s, e) => { ClearImage = true; SelectedImageSource = null; ClearImageRequested?.Invoke(); };
        save.Click += (s, e) => SaveRequested?.Invoke();
        Controls.AddRange(new Control[] { choose, remove, save, cancel });
        AcceptButton = save;
        CancelButton = cancel;
        lblImage.Text = existing?.ImagePath ?? "Chưa có ảnh";
        if (existing != null) ShowImage(existing.ImagePath);
    }

    void ChooseImage()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Chọn hình món",
            Filter = "Hình ảnh|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ClearImage = false;
        SelectedImageSource = dialog.FileName;
        ChooseImageRequested?.Invoke();
    }

    public void ShowCategories(IReadOnlyList<Category> categories, int? selectedCategoryId)
    {
        cbCategory.DataSource = categories.ToList();
        cbCategory.SelectedItem = categories.FirstOrDefault(category => category.Id == selectedCategoryId);
    }

    public void ShowFood(Food food)
    {
        txtName.Text = food.Name;
        nudPrice.Value = Math.Min(nudPrice.Maximum, food.Price);
        lblImage.Text = food.ImagePath ?? "Chưa có ảnh";
        ShowImage(food.ImagePath);
    }

    public void SetImageSource(string imagePath)
    {
        if (imagePath == null)
        {
            lblImage.Text = "Không dùng ảnh";
            ShowImage(null);
            return;
        }
        lblImage.Text = Path.GetFileName(imagePath);
        ShowImage(imagePath);
    }

    void ShowImage(string path)
    {
        var old = preview.Image;
        preview.Image = FoodImageStorage.LoadImage(path);
        old?.Dispose();
    }

    public void SetSavedFood(Food food)
    {
        Result = food;
        DialogResult = DialogResult.OK;
        Close();
    }

    public void ShowValidationMessage(string message)
        => MessageBox.Show(this, message, "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public void ShowError(string message)
        => MessageBox.Show(this, message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

    protected override void Dispose(bool disposing)
    {
        if (disposing) preview.Image?.Dispose();
        base.Dispose(disposing);
    }
}
