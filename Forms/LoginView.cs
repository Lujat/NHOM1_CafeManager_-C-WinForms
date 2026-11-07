using CafeManager.Models;
using CafeManager.Views.Contracts;

namespace CafeManager.Forms;

public sealed class LoginView : Form, ILoginView
{
    readonly TextBox txtUser = new() { Left = 110, Top = 20, Width = 180 };
    readonly TextBox txtPassword = new() { Left = 110, Top = 55, Width = 180, UseSystemPasswordChar = true };

    public string UserName => txtUser.Text;
    public string Password => txtPassword.Text;
    public Account User { get; private set; }
    public event Action LoginRequested;

    public LoginView()
    {
        Text = "Đăng nhập - Quản lý quán cà phê";
        ClientSize = new Size(320, 150);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var login = new Button { Text = "Đăng nhập", Left = 110, Top = 95, Width = 110, Height = 32 };
        login.Click += (s, e) => LoginRequested?.Invoke();
        AcceptButton = login;
        Controls.AddRange(new Control[]
        {
            new Label { Text = "Tài khoản:", Left = 20, Top = 24, AutoSize = true },
            new Label { Text = "Mật khẩu:", Left = 20, Top = 59, AutoSize = true },
            txtUser, txtPassword, login
        });
    }

    public void LoginSucceeded(Account account)
    {
        User = account;
        DialogResult = DialogResult.OK;
    }

    public void ShowError(string message)
        => MessageBox.Show(this, message, "Đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
