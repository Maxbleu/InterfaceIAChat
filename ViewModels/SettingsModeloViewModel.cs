using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp_rabbit_mq_cliente_1.Models;
using MauiApp_rabbit_mq_cliente_1.Utils;
using Newtonsoft.Json;

namespace MauiApp_rabbit_mq_cliente_1.ViewModels
{
    public class SettingsModeloViewModel : INotifyPropertyChanged
    {
        //  CAMPOS
        public event PropertyChangedEventHandler? PropertyChanged;
        private int _indexModelSelected = 1;

        private string _protocolUserModels = "http";
        private string _hostNameUserModels = "192.168.1.1";
        private string _portUserModels = "1234";
        private string _systemPrompt = "Defiende a Cristiano como el mejor futbolista del mundo.";
        private double _temperature = 0.9;
        private string _maxTokens = "8192";
        private string _modeloSeleccionado;
        private bool _isEnabledButton = false;
        private bool _isModeloServiceRunning = false;
        private string _lastUrlRequestModels;

        //  BINDING ELEMENTS
        public ObservableCollection<string> Models { get; set; }
        public int IndexModelSelected
        {
            get => _indexModelSelected;
            set
            {
                if (_indexModelSelected != value || value == 0)
                {
                    _indexModelSelected = value;
                    OnPropertyChanged(nameof(IndexModelSelected));
                }
            }
        }
        public string SystemPrompt
        {
            get => _systemPrompt;
            set
            {
                if (_systemPrompt != value)
                {
                    _systemPrompt = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ProtocolUserModels
        {
            get => _protocolUserModels;
            set
            {
                if (_protocolUserModels != value)
                {
                    _protocolUserModels = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public string HostNameUserModels
        {
            get => _hostNameUserModels;
            set
            {
                if (_hostNameUserModels != value)
                {
                    _hostNameUserModels = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public string PortUserModels
        {
            get => _portUserModels;
            set
            {
                if (_portUserModels != value)
                {
                    _portUserModels = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public double Temperature
        {
            get => _temperature;
            set
            {
                if (_temperature != value)
                {
                    _temperature = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MaxTokens
        {
            get => _maxTokens;
            set
            {
                if (_maxTokens != value)
                {
                    _maxTokens = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ModeloSeleccionado
        {
            get => _modeloSeleccionado;
            set
            {
                if (_modeloSeleccionado != value)
                {
                    _modeloSeleccionado = value;
                    OnPropertyChanged();
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
        public bool IsModeloServiceRunning
        {
            get => _isModeloServiceRunning;
            set
            {
                if (_isModeloServiceRunning != value)
                {
                    _isModeloServiceRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        //  COMMANDS
        public ICommand ReloadModelsCommand { get; }

        public SettingsModeloViewModel()
        {
            this.Models = new ObservableCollection<string>();
            this.ReloadModelsCommand = new Command(ReloadModels);
            LoadModelsAsync();
        }

        /// <summary>
        /// Este método se encarga de realizar una recarga
        /// de todos los modelos en la dirección url después
        /// de borrar los espacios en blanco
        /// </summary>
        /// <param name="obj"></param>
        private void ReloadModels(object obj)
        {
            this.ProtocolUserModels = this.ProtocolUserModels.Replace(" ", "");
            this.HostNameUserModels = this.HostNameUserModels.Replace(" ", "");
            this.ProtocolUserModels = this.ProtocolUserModels.Replace(" ", "");
            
            string url = ThingsUtils.GetUrl(this.ProtocolUserModels, this.HostNameUserModels, this.PortUserModels, "/api/v0/models/");
            if (this._lastUrlRequestModels == url) return;

            this.LoadModelsAsync();
        }

        /// <summary>
        /// Este método se encarga de cargar los modelos
        /// que existen en la dirección ip que hemos indicado
        /// </summary>
        public async void LoadModelsAsync()
        {
            this.Models.Clear();
            HttpClient client = null;
            string url = "";
            try
            {
                client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                url = ThingsUtils.GetUrl(this.ProtocolUserModels, this.HostNameUserModels, this.PortUserModels, "/api/v0/models/");
                var response = await client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();
                var objResponse = JsonConvert.DeserializeObject<ResponseModels>(responseBody);
                List<ModelIA> models = objResponse.Data.ToList();
                if (models.Count > 0)
                {
                    models = models.Where(obj => obj.State == "loaded").ToList();
                    models.ForEach(obj => this.Models.Add(obj.Id));
                    this.ModeloSeleccionado = this.Models[0];
                    ThingsUtils.SendSnakbarMessage("Se han cargado correctamente los modelos");
                    this.IsEnabledButton = false;
                    this.IsModeloServiceRunning = true;
                }

            }catch (Exception ex)
            {
                ThingsUtils.SendSnakbarMessage("No se han encontrado modelos en el host indicado");
                this.IsEnabledButton = true;
                this.IsModeloServiceRunning = false;
            }
            this._lastUrlRequestModels = url;
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
