## Xamarin Main Thread Dispatcher


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

