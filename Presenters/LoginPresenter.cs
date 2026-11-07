using CafeManager.Repositories;
using CafeManager.Views.Contracts;

namespace CafeManager.Presenters;
//kieem tra login 
public sealed class LoginPresenter
{
    readonly ILoginView _view;

    public LoginPresenter(ILoginView view)
    {
        _view = view;
        _view.LoginRequested += Login;
    }
    void Login()
    {
        try
        {
            var account = AccountRepository.Login(_view.UserName.Trim(), _view.Password);
            if (account == null)
            {
                _view.ShowError("Sai tài khoản hoặc mật khẩu.");
                return;
            }
            _view.LoginSucceeded(account);
        }
        catch (Exception ex) { _view.ShowError("Không kết nối được CSDL:\n" + ex.Message); }
    }
}
