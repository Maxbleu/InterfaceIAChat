namespace MauiApp_rabbit_mq_cliente_1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = IPlatformApplication.Current.Services.GetService<AppShell>();
        }
    }
}
