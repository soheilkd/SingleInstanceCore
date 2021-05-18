using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TinyIpc.Messaging;

namespace SingleInstanceCore
{
	public static class SingleInstance
	{
		private const string channelNameSufflix = ":SingeInstanceIPCChannel";
		//For detecting if mutex is locked (first instance is already up then)
		private static Mutex singleMutex;
		//Message bus for communication between instances
		private static TinyMessageBus messageBus; 

		/// <summary>
		/// Intended to be on app startup
		/// Initializes service if the call is from first instance
		/// Signals the first instance if it already exists
		/// </summary>
		/// <param name="uniqueName">A unique name for IPC channel</param>
		/// <returns>Whether the call is from application's first instance</returns>
		public static bool InitializeAsFirstInstance<T>(this T instance, string uniqueName) where T:ISingleInstance
		{
			var CommandLineArgs = GetCommandLineArgs(uniqueName);
			var applicationIdentifier = uniqueName + Environment.UserName;
			var channelName = $"{applicationIdentifier}{channelNameSufflix}";
			singleMutex = new Mutex(true, applicationIdentifier, out var firstInstance);

			if (firstInstance)
				CreateRemoteService(instance, channelName);
			else
				SignalFirstInstance(channelName, CommandLineArgs);

			return firstInstance;
		}

		private static void SignalFirstInstance(string channelName, IList<string> commandLineArgs)
		{
			var bus = new TinyMessageBus(channelName);
			var serializedArgs = commandLineArgs.Serialize();
			bus.PublishAsync(serializedArgs);
		}

		private static void CreateRemoteService(ISingleInstance instance, string channelName)
		{
			messageBus = new TinyMessageBus(channelName);
			messageBus.MessageReceived += (_, e) =>
			{
				instance.OnInstanceInvoked(e.Message.Deserialize<string[]>());
			};
		}

		private static string[] GetCommandLineArgs(string uniqueApplicationName)
		{
			var args = Environment.GetCommandLineArgs();
			if (args == null)
			{
				// Try getting commandline arguments from shared location in case of ClickOnce deployed application  
				var appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);
				var cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
				if (File.Exists(cmdLinePath))
				{
					try
					{
						using var reader = new StreamReader(cmdLinePath, Encoding.Unicode);
						args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd());
						File.Delete(cmdLinePath);
					}
					catch (IOException) { }
				}
			}
			return args ?? Array.Empty<string>();
		}

		public static void Cleanup()
		{
			if (messageBus != null)
			{
				messageBus.Dispose();
				messageBus = null;
			}
			if (singleMutex != null)
			{
				singleMutex.Close();
				singleMutex = null;
			}
		}
	}
}
