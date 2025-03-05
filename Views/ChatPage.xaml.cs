using MauiApp_rabbit_mq_cliente_1.ViewModels;

namespace MauiApp_rabbit_mq_cliente_1.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel chatViewModel)
	{
		InitializeComponent();
        this.BindingContext = chatViewModel;
    }
}