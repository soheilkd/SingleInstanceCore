using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using TinyIpc.Messaging;

namespace SingleInstanceCore
{
	public static class SingleInstance<TApplication>
		where TApplication : Application, ISingleInstance
	{
		private const string channelNameSufflix = ":SingeInstanceIPCChannel";
		private static Mutex singleMutex;
		private static TinyMessageBus messageBus;

		public static bool InitializeAsFirstInstance(string uniqueName)
		{
			var CommandLineArgs = GetCommandLineArgs(uniqueName);
			string applicationIdentifier = uniqueName + Environment.UserName;

			string channelName = $"{applicationIdentifier}{channelNameSufflix}";

			singleMutex = new Mutex(true, applicationIdentifier, out var firstInstance);
			if (firstInstance)
				CreateRemoteService(channelName);
			else
				SignalFirstInstance(channelName, CommandLineArgs);

			return firstInstance;
		}

		private static void SignalFirstInstance(string channelName, IList<string> commandLineArgs)
		{
			new TinyMessageBus(channelName).PublishAsync(commandLineArgs.Serialize());
		}

		private static void CreateRemoteService(string channelName)
		{
			messageBus = new TinyMessageBus(channelName);
			messageBus.MessageReceived += (_, e) =>
				(Application.Current as TApplication).OnInstanceInvoked(e.Message.Deserialize<string[]>());
			messageBus.MessageReceived += MessageBus_MessageReceived;
		}

		private static void MessageBus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
		{
			var app = Application.Current as TApplication;
			var args = e.Message.Deserialize<string[]>();
			app.OnInstanceInvoked(args);
		}

		private static string[] GetCommandLineArgs(string uniqueApplicationName)
		{
			string[] args = Environment.GetCommandLineArgs();
			if (args == null)
			{
				// Try getting commandline arguments from shared location in case of ClickOnce deployed application  
				string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);
				string cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
				if (File.Exists(cmdLinePath))
				{
					try
					{
						using var reader = new StreamReader(cmdLinePath, Encoding.Unicode);
						args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd());
						File.Delete(cmdLinePath);
					}
					catch (IOException)
					{
					}
				}
			}

			if (args == null)
				args = Array.Empty<string>();

			return args;
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
