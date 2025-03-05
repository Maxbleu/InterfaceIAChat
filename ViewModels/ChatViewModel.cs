using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using MauiApp_rabbit_mq_cliente_1.Models;
using MauiApp_rabbit_mq_cliente_1.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client.Events;

namespace MauiApp_rabbit_mq_cliente_1.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {

        //  CAMPOS
        private string _nombreChat;
        private string _newMessageText;
        public event PropertyChangedEventHandler PropertyChanged;
        private SettingsModeloViewModel _settingsModeloViewModel;
        private SettingsRabbitMQViewModel _settingsRabbitMQViewModel;

        //  BINDING UI ELEMENTS
        public ObservableCollection<Message> Messages { get; set; }
        public string NombreChat
        {
            get => _nombreChat;
            set
            {
                if (_nombreChat != value)
                {
                    _nombreChat = value;
                    OnPropertyChanged();
                }
            }
        }
        public string NewMessageText
        {
            get => _newMessageText;
            set
            {
                if (_newMessageText != value)
                {
                    _newMessageText = value;
                    OnPropertyChanged();
                }
            }
        }

        // COMMANDS
        public ICommand SendMessageCommand { get; }

        public ChatViewModel()
        {
            this.Messages = new ObservableCollection<Message>();
            this.SendMessageCommand = new Command(SendMessage);

            this._settingsModeloViewModel = IPlatformApplication.Current.Services.GetService<SettingsModeloViewModel>();
            this._settingsModeloViewModel.LoadModelsAsync();

            this._settingsRabbitMQViewModel = IPlatformApplication.Current.Services.GetService<SettingsRabbitMQViewModel>();
            this.NombreChat = ThingsUtils.CapitalizeFirstLetter(this._settingsRabbitMQViewModel.ExchangeName);
            this._settingsRabbitMQViewModel.PropertyChanged += SettingsRabbitMQViewModel_PropertyChanged;
            this._settingsRabbitMQViewModel.SetupRabbitMQ();
        }


        //  EVENTOS SUBCRIPTOS
        /// <summary>
        /// Este método se encarga de comprobar si la configuración
        /// de rabbit habia sido guardada anteriormente si es
        /// así se reconfiguraría el broker RabbitMQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsRabbitMQViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "ExhangeName")
            {
                this.NombreChat = this._settingsRabbitMQViewModel.ExchangeName;
            }

            if(e.PropertyName == "Consumer")
            {
                this._settingsRabbitMQViewModel.Consumer.Received += OnReceived;
            }
        }
        /// <summary>
        /// Este método se encarga de recibir todos los mensajes
        /// que son recibidos en el exchange al que esta subcripto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceived(object sender, BasicDeliverEventArgs e)
        {
            if (e.BasicProperties.AppId == this._settingsRabbitMQViewModel.AppId) return;
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Messages.Add(new Message
            {
                Id = DateTime.Now.Ticks.ToString(),
                Text = message,
                IsCurrentUser = false
            });

            if (this._settingsRabbitMQViewModel.IsRabbitMQServiceRunning && this._settingsModeloViewModel.IsModeloServiceRunning)
            {
                this.NewMessageText = SendMessageToAIAsync(message).Result;
                SendMessage();
            }

            this._settingsRabbitMQViewModel.Channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
        }

        //  MENSAJES
        /// <summary>
        /// Este método se encarga de mostrar los
        /// mensajes de la conversación
        /// </summary>
        private void ShowMessage()
        {
            var now = DateTime.Now;
            var newMessage = new Message
            {
                Id = now.Ticks.ToString(),
                Text = NewMessageText,
                IsCurrentUser = true,
            };

            Messages.Add(newMessage);
            NewMessageText = string.Empty;
        }
        /// <summary>
        /// Este método se encarga de enviar el mensaje que
        /// ha recibido en el exchange a nuestra IA y
        /// obtener la respuesta que ha procesado
        /// </summary>
        /// <returns></returns>
        private async Task<string> SendMessageToAIAsync(string message)
        {
            string url = ThingsUtils.GetUrl(this._settingsModeloViewModel.ProtocolUserModels, this._settingsModeloViewModel.HostNameUserModels, this._settingsModeloViewModel.PortUserModels, "/v1/chat/completions");

            var conversation = new List<object>
            {
                new { role = "system", content = this._settingsModeloViewModel.SystemPrompt },
                new { role = "user", content = message }
            };

            var requestData = new
            {
                model = this._settingsModeloViewModel.ModeloSeleccionado,
                messages = conversation.ToArray(),
                temperature = this._settingsModeloViewModel.Temperature,
                max_tokens = this._settingsModeloViewModel.MaxTokens,
            };

            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string aiMessage = string.Empty;
                try
                {
                    var response = await client.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var responseJson = JObject.Parse(responseBody);

                    if (responseJson["choices"] != null && responseJson["choices"].HasValues)
                    {
                        aiMessage = responseJson["choices"][0]["message"]?["content"]?.ToString();
                        Console.WriteLine("Respuesta de la IA: " + aiMessage);
                    }
                    else
                    {
                        Console.WriteLine("No se encontró 'choices' en la respuesta.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                return aiMessage;
            }
        }
        /// <summary>
        /// Este método se encarga de enviar el mensaje
        /// de respuesta del modelo al exchange
        /// </summary>
        private void SendMessage()
        {
            this._settingsRabbitMQViewModel.PostMessageInExchange(this.NewMessageText);
            ShowMessage();
        }


        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
