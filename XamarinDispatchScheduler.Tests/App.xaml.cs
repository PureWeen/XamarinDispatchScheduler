﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;

using Xamarin.Forms;

namespace XamarinDispatchScheduler.Tests
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

			MainPage = new XamarinDispatchScheduler.Tests.MainPage();
		}

		protected override void OnStart ()
        {
            XamarinDispatcherScheduler.Init();
            // Handle when your app starts
        }

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}