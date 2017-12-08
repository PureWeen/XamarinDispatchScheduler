using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace XamarinDispatchScheduler.UITests
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void WaitForChangeMeToBeSet()
        {

            app.WaitForElement(x => x.Marked("lblChangeMe"));
            app.WaitForElement(x => x.Marked("Changed"));
        }

        [Test]
        public void SlowlyScheduled()
        {

            app.WaitForElement(x => x.Marked("lblSlowScheduler"));
            app.WaitForElement(x => x.Marked("slowly scheduled"));
        }


        [Test]
        public void TimerSchedule()
        {
            app.WaitForElement(x => x.Marked("lblTimer"));

            List<string> matches = new List<string>();

            for (int i= 0; i < 3; i++)
            {
                matches.Add(
                        app.Query("lblTimer")
                            .First()
                            .Text
                    );

                Assert.AreEqual(matches.Count, matches.Distinct().Count());
                System.Threading.Thread.Sleep(3000);
            }


            app.Tap("btnStop");

            matches.Clear();
            for (int i = 0; i < 3; i++)
            {
                matches.Add(
                        app.Query("lblTimer")
                            .First()
                            .Text
                    );

                Assert.AreEqual(1, matches.Distinct().Count());
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}

