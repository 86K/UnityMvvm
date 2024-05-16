

using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public interface IAccountRepository
	{
		Task<Account> Get (string username);

		Task<Account> Save (Account account);

		Task<Account> Update (Account account);

		Task<bool> Delete (string username);
	}
}