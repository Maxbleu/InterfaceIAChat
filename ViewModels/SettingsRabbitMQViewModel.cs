using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using MauiApp_rabbit_mq_cliente_1.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MauiApp_rabbit_mq_cliente_1.ViewModels
{
    public class SettingsRabbitMQViewModel : INotifyPropertyChanged
    {
        //  CAMPOS
        private IModel _channel;
        private string _queueName;

        private bool _isEnabledButton = false;
        private string _hostName = "192.168.1.149";
        private EventingBasicConsumer _consumer;
        private string _exchangeName = "grupoChat";
        private bool _isRabbitMQServiceRunning = false;
        private string _appId = Guid.NewGuid().ToString();
        public event PropertyChangedEventHandler? PropertyChanged;

        //  BINDING ELEMENTS
        public IModel Channel
        {
            get => _channel;
            set
            {
                if (_channel != value)
                {
                    _channel = value;
                    OnPropertyChanged();
                }
            }
        }
        public string HostName
        {
            get => _hostName;
            set
            {
                if (_hostName != value)
                {
                    _hostName = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public string ExchangeName
        {
            get => _exchangeName;
            set
            {
                if (_exchangeName != value)
                {
                    _hostName = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public bool IsEnabledButton
        {
            get => _isEnabledButton;
            set
            {
                _isEnabledButton = value;
                OnPropertyChanged();
            }
        }
        public bool IsRabbitMQServiceRunning
        {
            get => _isRabbitMQServiceRunning;
            set
            {
                if (_isRabbitMQServiceRunning != value)
                {
                    _isRabbitMQServiceRunning = value;
                    OnPropertyChanged();
                }
            }
        }
        public EventingBasicConsumer Consumer
        {
            get => _consumer;
            set
            {
                if (_consumer != value)
                {
                    _consumer = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AppId
        {
            get => _appId;
            set
            {
                if (_appId != value)
                {
                    _appId = value;
                    OnPropertyChanged();
                }
            }
        }

        //  COMMANDS
        public ICommand SaveConfigurationCommand { get; }
        
        public SettingsRabbitMQViewModel()
        {
            this.SaveConfigurationCommand = new Command(this.SaveConfiguration);
        }

        /// <summary>
        /// Este método se encarga de configurar el broker
        /// RabbitMQ para que las IA puedan tener la conversion
        /// </summary>
        public void SetupRabbitMQ()
        {
            this.IsRabbitMQServiceRunning = false;
            try
            {
                var factory = new ConnectionFactory() { HostName = this.HostName };
                var connection = factory.CreateConnection();
                this.Channel = connection.CreateModel();

                this.Channel.ExchangeDeclare(exchange: this.ExchangeName, type: "fanout");

                this._queueName = this._channel.QueueDeclare().QueueName;

                this.Channel.QueueBind(queue: this._queueName,
                                  exchange: this.ExchangeName,
                                  routingKey: "");

                this.Consumer = new EventingBasicConsumer(this._channel);

                this.Channel.BasicConsume(queue: this._queueName,
                                     autoAck: false,
                                     consumer: this.Consumer);

                this.IsRabbitMQServiceRunning = true;
                this.IsEnabledButton = false;

                ThingsUtils.SendSnakbarMessage("Se ha guardado correctamente la configuración RabbitMQ");
            }
            catch (Exception ex)
            {
                this.IsEnabledButton = true;
                ThingsUtils.SendSnakbarMessage("No se ha encontrado el host para RabbitMQ");
            }
        }
        /// <summary>
        /// Este método se encarga de enviar el mensaje que 
        /// ha procesado nuestra IA
        /// </summary>
        public void PostMessageInExchange(string newMessageText)
        {
            if (!this.IsRabbitMQServiceRunning)
            {
                ThingsUtils.SendSnakbarMessage("Hay problemas en la configuración RabbitMQ. Por favor cámbiela");
                return;
            }
            if (string.IsNullOrWhiteSpace(newMessageText)) return;
            var body = Encoding.UTF8.GetBytes(newMessageText);
            var properties = this._channel.CreateBasicProperties();
            properties.AppId = this.AppId;
            this._channel.BasicPublish(exchange: this.ExchangeName, routingKey: "", basicProperties: properties, body: body);
        }
        /// <summary>
        /// Este método una vez que se han actualizado los valores de las propiedades
        /// reconfiguro RabbitMQ en base a las configuraciones impuestas
        /// </summary>
        private void SaveConfiguration()
        {
            SetupRabbitMQ();
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
