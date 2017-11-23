using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace XamarinFormsDispatcher.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            XamarinFormsDispatcher.MainPage.OnMainThread = () => Dispatcher.HasThreadAccess;
             

            this.InitializeComponent();

            LoadApplication(new XamarinFormsDispatcher.App());
            
        }
    }
}
