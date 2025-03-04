using MauiApp_rabbit_mq_cliente_1.ViewModels;

namespace MauiApp_rabbit_mq_cliente_1.Views;

public partial class SettingsModeloPage : ContentPage
{
    public SettingsModeloPage(SettingsModeloViewModel settingsModeloViewModel)
	{
		InitializeComponent();
        this.BindingContext = settingsModeloViewModel;
    }
}