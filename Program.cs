using CafeManager.Forms;
using CafeManager.Presenters;

namespace CafeManager;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        while (true)
        {
            using var login = new LoginView();
            _ = new LoginPresenter(login);
            if (login.ShowDialog() != DialogResult.OK) break;

            using var main = new OrderView(login.User);
            _ = new OrderPresenter(main, main);
            Application.Run(main);
            if (!main.LoggedOut) break;   
            // dangxuat = ve login
        }
    }
}
