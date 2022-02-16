using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MensajeroC
{
    public partial class Bot_MensajeroC : ServiceBase
    {
        public Bot_MensajeroC()
        {
            InitializeComponent();
            eventos_MensajeroC = new EventLog();
            if (!EventLog.SourceExists("Bot_MensajeroC"))
            {
                EventLog.CreateEventSource("Bot_MensajeroC", "Application");
            }
            eventos_MensajeroC.Source = "Bot_MensajeroC";
            eventos_MensajeroC.Log = "Application";
        }

        protected override void OnStart(string[] args)
        {
            // escribe en el log de eventos del sistema - inicio del mensajero
            eventos_MensajeroC.WriteEntry("Inicio del Bot_MensajeroC");
        }

        protected override void OnStop()
        {
            // escribe en el log de eventos del sistema - detención del mensajero
            eventos_MensajeroC.WriteEntry("Detención del Bot_MensajeroC");
        }
    }
}
