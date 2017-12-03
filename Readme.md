## Xamarin Main Thread Dispatcher

#### UWP Setup

Just Call
```
protected override void OnLaunched(LaunchActivatedEventArgs e)
{
    XamarinDispatcherScheduler.Init();
```

Or with Xamarin Forms
```
protected override void OnStart ()
{
    XamarinDispatcherScheduler.Init();
```

The current dispatcher has to be obtained while on the UI Thread


#### Examples

```
Observable
    .Timer(TimeSpan.FromSeconds(2))
    .ObserveOn(XamarinDispatcherScheduler.Current)
    .Subscribe(_ =>
    {
        lblChangeMe.Text = "Changed";
    });
```

```
//Helper to check if you're on the main thread
if (!XamarinDispatcherScheduler.OnMainThread())
{
    throw new Exception("Scheduler fail");
}
```