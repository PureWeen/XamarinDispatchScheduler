using Xamarin.Forms.Platform.WPF;

namespace XamarinDispatchScheduler.Tests.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();

            Xamarin.Forms.Forms.Init();
            var app = new XamarinDispatchScheduler.Tests.App();
            LoadApplication(app);
        }
    }
}
