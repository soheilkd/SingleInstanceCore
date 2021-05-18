# SingleInstanceCore
For single instance applications on .NET Core

NuGet Package: https://www.nuget.org/packages/SingleInstanceCore/
# Usage

Note: Usage examples are for WPF desktop applications. For other platforms/frameworks, inheritance and initialization should be done accordingly, not exactly like the examples.

The class that handles instance invokation should inherit ISingleInstance and implement OnInstanceInvoked method.

E.g. in App class (App.xaml.cs):
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
Initialization of instance should be done when application is starting, and cleanup method should be called on the exit point of the application. 

E.g. in App class (App.xaml.cs):
```csharp

	private void Application_Startup(object sender, StartupEventArgs e)
	{
		bool isFirstInstance = this.InitializeAsFirstInstance("soheilkd_ExampleIPC");
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
		SingleInstance.Cleanup();
	}
```
