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


I borrowed from the [CoreDispatchScheduler From Rx.NET](https://github.com/Reactive-Extensions/Rx.NET/blob/ba98e6254c9a2f841cbc4169bf38590b133c8064/Rx.NET/Source/src/System.Reactive/Platforms/Windows/Concurrency/CoreDispatcherScheduler.cs) and
the [Xamarin.Forms BeginInvokeOnMainThread](https://github.com/xamarin/Xamarin.Forms/blob/d3d59ee4f0b3098457e1debe8d7b03d0d0061a53/Xamarin.Forms.Platform.iOS/Forms.cs) implementations
to create a combined Scheduler that will work Cross Platform for Xamarin.

