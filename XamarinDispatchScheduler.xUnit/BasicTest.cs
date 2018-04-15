using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Xam.Reactive.Concurrency;
using Xunit;

public class BasicTest
{
    [Fact]
    public void Test()
    {
        TestScheduler scheduler = new TestScheduler();
        XamarinDispatcherScheduler.Current = scheduler;
        XamarinDispatcherScheduler
                   .Current
                   .Schedule( () =>
                   {
                      
                   });
    }
}
