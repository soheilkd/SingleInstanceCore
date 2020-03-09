# SingleInstanceCore
Single instance application - For WPF on .NET Core
# Usage
The App class (on App.xaml.cs) should inherit ISingleInstance and implement OnInstanceInvoked method:
```csharp
public partial class App : Application, ISingleInstance
{
	public void OnInstanceInvoked(string[] args)
	{
			//What to do with the args another instance has sent
	}
	...
}
```
The initialization of the instance should be done on application startup or main window's startup
Cleanup method should be called on the exit point of the application. 
```csharp

	private void Application_Startup(object sender, StartupEventArgs e)
	{
		bool isFirstInstance = SingleInstance<App>.InitializeAsFirstInstance("soheilkd_EPlayerIPC"))
		if (!isFirstInstance)
		{
			//If it's not the first instance, arguments are automatically passed to the first instance
			//OnInstanceInvoked will be raised on the first instance
			//You may shut down the current instance
			Current.Shutdown();
		}
	}
		
	private void Application_Exit(object sender, ExitEventArgs e)
	{
		//Do not forget to cleanup
		SingleInstance<App>.Cleanup();
	}
```
