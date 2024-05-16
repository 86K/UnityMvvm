using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public enum AccountEventType
    {
        Register,
        Update,
        Deleted,
        Login
    }
    public class AccountEventArgs
    {
        public AccountEventArgs(AccountEventType type, Account account)
        {
            Type = type;
            Account = account;
        }

        public AccountEventType Type { get; private set; }

        public Account Account { get; private set; }
    }

    public interface IAccountService
    {
        IMessenger Messenger { get; }

        Task<Account> Register(Account account);

        Task<Account> Update(Account account);

        Task<Account> Login(string username, string password);

        Task<Account> GetAccount(string username);
    }
}