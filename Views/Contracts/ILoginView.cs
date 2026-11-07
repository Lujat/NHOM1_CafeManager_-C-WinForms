using CafeManager.Models;

namespace CafeManager.Views.Contracts;

public interface ILoginView
{
    event Action LoginRequested;
    string UserName { get; }
    string Password { get; }
    Account User { get; }
    void LoginSucceeded(Account account);
    void ShowError(string message);
}
