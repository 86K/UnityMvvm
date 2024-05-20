using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository repository;

        public AccountService(IAccountRepository repository)
        {
            this.repository = repository;
        }


        public virtual async Task<Account> Register(Account account)
        {
            await repository.Save(account);
            return account;
        }

        public virtual async Task<Account> Update(Account account)
        {
            await repository.Update(account);
            return account;
        }

        public virtual async Task<Account> Login(string username, string password)
        {
            Account account = await GetAccount(username);
            if (account == null || !account.Password.Equals(password))
                return null;
            
            return account;
        }

        public virtual Task<Account> GetAccount(string username)
        {
            return repository.Get(username);
        }
    }
}