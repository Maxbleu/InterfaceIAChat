using MauiApp_rabbit_mq_cliente_1.ViewModels;

namespace MauiApp_rabbit_mq_cliente_1.Views;

public partial class SettingsRabbitMQPage : ContentPage
{
    public SettingsRabbitMQPage(SettingsRabbitMQViewModel settingsRabbitMQViewModel)
	{
		InitializeComponent();
        this.BindingContext = settingsRabbitMQViewModel;
    }
}