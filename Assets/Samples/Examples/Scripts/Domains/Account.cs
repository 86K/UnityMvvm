

using System;

namespace Fusion.Mvvm
{
	public class Account : ObservableObject
	{
		private string username;
		private string password;

		private DateTime created;

		public string Username {
			get => username;
			set => Set(ref username, value);
		}

		public string Password {
			get => password;
			set => Set(ref password, value);
		}

		public DateTime Created {
			get => created;
			set => Set(ref created, value);
		}
	}
}