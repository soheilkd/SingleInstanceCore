using System;

namespace SingleInstanceCore
{
	public interface ISingleInstance
	{
		public void OnInstanceInvoked(string[] args);
	}
}
