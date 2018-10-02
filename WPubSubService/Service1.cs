using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.Threading;
using System.ServiceModel;

using SenApi.Services.PubSubServices;

namespace WPubSubService
{
    public partial class Service1 : ServiceBase
    {
        Thread t;      
        public Service1()
        {
            InitializeComponent();
            t = new Thread(StarService);
        }

        void StarService(object o) {
            ServiceHost h = new ServiceHost(new PubSubService());
            h.Open();
        }

        protected override void OnStart(string[] args)
        {
            t.Start();
        }

        protected override void OnStop()
        {
            t.Abort();
        }
    }
}
