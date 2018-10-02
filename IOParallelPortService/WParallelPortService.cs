using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Configuration;
using SenApi.Services.ParallelPort;
using SenApi.ServicesContract.PubSubContract;

namespace IOParallelPortService
{
    public partial class WParallelPortService : ServiceBase
    {
        private ServiceHost host;
        private Thread t_worker; 
        ParallelPortServer wcfInstance;

        public WParallelPortService()
        {
            InitializeComponent();
            wcfInstance = new ParallelPortServer();
            host = new ServiceHost(wcfInstance);
           
            t_worker = new Thread(HostParallelPortAPI);
            t_worker.Name = "ParallelPortServe";
            t_worker.IsBackground = true;           
        }

        protected override void OnStart(string[] args)
        {                
            t_worker.Start();            
        }

        protected override void OnStop()
        {
            host.Close();
            t_worker.Abort();
        }
        
        private void HostParallelPortAPI() {            
            host.Open();           
        }
    }
}
