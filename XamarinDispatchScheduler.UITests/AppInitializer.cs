using System;
using System.IO;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace XamarinDispatchScheduler.UITests
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp
                    .Android
                    .InstalledApp("com.XamarinDispatchScheduler.XamarinDispatchScheduler")
                    //.ApkFile("../../../XamarinDispatchScheduler.Tests.Android/bin/Release/com.XamarinDispatchScheduler.XamarinDispatchScheduler.apk")
                    .StartApp();
            }

            return ConfigureApp
                .iOS
                .StartApp();
        }
    }
}

