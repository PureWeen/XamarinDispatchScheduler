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
        bool executed = false;
        TestScheduler scheduler = new TestScheduler();
        XamarinDispatcherScheduler.Current = (IScheduler)scheduler;
        XamarinDispatcherScheduler
                   .Current
                   .Schedule( () =>
                   {
                       executed = true;
                   });

        Assert.Equal(false, executed);
        scheduler.AdvanceBy(1000);
        Assert.Equal(true, executed);
    }
}
