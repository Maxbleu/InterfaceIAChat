﻿using MauiApp_rabbit_mq_cliente_1.ViewModels;
using MauiApp_rabbit_mq_cliente_1.Views;
using Microsoft.Extensions.Logging;

namespace MauiApp_rabbit_mq_cliente_1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
            
            builder.Services.AddSingleton<SettingsRabbitMQViewModel>();
            builder.Services.AddSingleton<SettingsModeloViewModel>();
            
            builder.Services.AddTransient<SettingsRabbitMQPage>();
            builder.Services.AddTransient<SettingsModeloPage>();

            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<AppShell>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
