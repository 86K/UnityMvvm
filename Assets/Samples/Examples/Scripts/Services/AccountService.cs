using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository repository;
        private readonly IMessenger messenger;

        public IMessenger Messenger => messenger;

        public AccountService(IAccountRepository repository)
        {
            this.repository = repository;
            messenger = new Messenger();
        }


        public virtual async Task<Account> Register(Account account)
        {
            await repository.Save(account);
            messenger.Publish(new AccountEventArgs(AccountEventType.Register, account));
            return account;
        }

        public virtual async Task<Account> Update(Account account)
        {
            await repository.Update(account);
            messenger.Publish(new AccountEventArgs(AccountEventType.Update, account));
            return account;
        }

        public virtual async Task<Account> Login(string username, string password)
        {
            Account account = await GetAccount(username);
            if (account == null || !account.Password.Equals(password))
                return null;

            messenger.Publish(new AccountEventArgs(AccountEventType.Login, account));
            return account;
        }

        public virtual Task<Account> GetAccount(string username)
        {
            return repository.Get(username);
        }
    }
}