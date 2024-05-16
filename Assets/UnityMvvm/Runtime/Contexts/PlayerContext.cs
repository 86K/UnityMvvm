

namespace Fusion.Mvvm
{
    public class PlayerContext : Context
    {
        private readonly string username;
        public string Username => username;

        public PlayerContext(string username) : this(username, null)
        {
            this.username = username;
        }

        public PlayerContext(string username, IServiceContainer container) : base(container, GetApplicationContext())
        {
            this.username = username;
        }
    }
}
