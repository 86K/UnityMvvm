﻿/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using Loxodon.Framework.Messaging;
using System.Threading.Tasks;

namespace Loxodon.Framework.Examples
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