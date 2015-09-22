using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Race.Locator.Positioning.DeviceMan.Model;
using NLog;

using Hik.Communication;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Communication.Messages;

namespace Race.Locator.Positioning.DeviceMan.Handler
{
    public class TestHandler : IDeviceHandler
    {
        private ManualResetEvent _shutdownEvent;
        private Logger logger = LogManager.GetLogger("TestHandler");

        public void Init(System.Xml.XmlElement handlerElement)
        {
            _shutdownEvent = new ManualResetEvent(false);
        }

        public event Action<Location> MessageEvent;


        public void Start()
        {
            Thread th = new Thread(new ThreadStart(ThreadStart));
            th.Start();
        }

        public void Stop()
        {
            _shutdownEvent.Set();
        }

        private void OnMessageEvent(Location e)
        {
            if (MessageEvent != null)
            {
                logger.Debug("event");
                MessageEvent(e);
            }
        }

        private void ThreadStart()
        {
            do
            {
                OnMessageEvent(new Location() { IMEI = "" });
            }
            while (!_shutdownEvent.WaitOne(1000));
        }
    }
}
