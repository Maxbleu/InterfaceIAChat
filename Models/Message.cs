using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp_rabbit_mq_cliente_1.Models
{
    public class Message
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Sender { get; set; }
    }
}
