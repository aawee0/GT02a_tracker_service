using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using NLog;
using Race.Locator.Positioning.DeviceMan.Model;

namespace Race.Locator.Positioning.DeviceMan.Service
{
    public partial class DeviceManService : ServiceBase
    {
        public Logger logger = LogManager.GetLogger("DeviceManService");
        private RequestDispatcher _dispatcher;

        public DeviceManService()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            this.OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            logger.Info("Starting service.");

            _dispatcher = new RequestDispatcher();
            if (_dispatcher.Init())
            {
                _dispatcher.Start();
                logger.Info("Service started.");
            }
            else
            {
                logger.Info("Initialization failed - stopping.");
                throw new ApplicationException("Initialization failure");
            }
        }

        protected override void OnStop()
        {
            logger.Info("Stopping service.");
            _dispatcher.Stop();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error(string.Format("Unhandled Exception occured:{0}", (e.ExceptionObject)));
        }

    }
}
