using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp_rabbit_mq_cliente_1.Models
{
    public class ModelIA
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public string Type { get; set; }
        public string Publisher { get; set; }
        public string Arch { get; set; }
        public string CompatibilityType { get; set; }
        public string Quantization { get; set; }
        public string State { get; set; }
        public int MaxContextLength { get; set; }
        public int LoadedContextLength { get; set; }
    }
}
