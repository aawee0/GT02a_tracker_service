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
    public class GallileoHandler : IDeviceHandler
    {
        private ManualResetEvent _shutdownEvent;
        private Logger logger = LogManager.GetLogger("Gt02Handler");

        private int _port;
        IScsServer serverClient;
        System.Collections.Concurrent.ConcurrentDictionary<IScsServerClient, string> clients = new System.Collections.Concurrent.ConcurrentDictionary<IScsServerClient, string>();

        public void Init(System.Xml.XmlElement handlerElement)
        {
            _shutdownEvent = new ManualResetEvent(false);

            _port = Convert.ToInt32(handlerElement["Port"].InnerText);

            serverClient = Hik.Communication.Scs.Server.ScsServerFactory.CreateServer(new ScsTcpEndPoint(_port));
            serverClient.WireProtocolFactory = new Protocols.GallileoWireProtocolFactory();
            serverClient.ClientConnected += new EventHandler<ServerClientEventArgs>(ClientConnected);
            serverClient.ClientDisconnected += new EventHandler<ServerClientEventArgs>(ClientDisconnected);
        }

        public event Action<Location> MessageEvent;


        public void Start()
        {
            serverClient.Start();
        }

        public void Stop()
        {
            serverClient.Stop();
        }

        private void OnMessageEvent(Location e)
        {
            if (MessageEvent != null)
            {
                //logger.Debug("event");
                MessageEvent(e);
            }
        }

        void ClientConnected(object sender, ServerClientEventArgs e)
        {
            e.Client.MessageReceived += new EventHandler<MessageEventArgs>(ClientMessageReceived);
            e.Client.MessageSent += new EventHandler<MessageEventArgs>(ClientMessageSent);
            logger.Debug("Client Connected: {0}", e.Client.ClientId);
        }

        void ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            e.Client.MessageReceived -= new EventHandler<MessageEventArgs>(ClientMessageReceived);
            e.Client.MessageSent -= new EventHandler<MessageEventArgs>(ClientMessageSent);

            try
            {
                string name;
                clients.TryRemove(e.Client, out name);
                logger.Debug("Client Disconnected: {0}, {1}", e.Client.ClientId, name);
            }
            catch (Exception ex)
            {
                logger.Debug(ex.ToString());
            }
        }

        void ClientMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var connect = (IScsServerClient)sender;
                var message = e.Message as Protocols.GpsMessage;
                
                if (message.Data != null)
                    connect.SendMessage(message);

                OnMessageEvent(message.Location);
            }
            catch (Exception ex)
            {
                logger.Debug(ex.ToString());
            }
        }

        void ClientMessageSent(object sender, MessageEventArgs e)
        {
            logger.Debug("gallileo sent: " + e.Message.ToString());
        }
    }
}
