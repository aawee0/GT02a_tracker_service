using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using NLog;

namespace Race.Locator.Positioning.DeviceMan.Model
{
    public class RequestDispatcher
    {
        private volatile bool _started = true;
        private Logger logger = LogManager.GetLogger("RequestDispatcher");
        private IList<IDeviceHandler> _deviceHandlers;
        private IList<ISystemHandler> _systemHandlers;

        /// <summary>
        /// Запуск обработки запросов
        /// </summary>
        public void Start()
        {
            logger.Info("Service starting...");
            _started = true;

            foreach (var handler in _deviceHandlers)
            {
                handler.Start();
                logger.Info("Device handler {0} started", handler.GetType());
            }
        }

        /// <summary>
        /// Закончить обработку запросов 
        /// </summary>
        public void Stop()
        {
            logger.Info("Service stopping...");
            _started = false;
            
            foreach (var handler in _deviceHandlers)
            {
                handler.Stop();
                logger.Info("Device handler {0} stopped", handler.GetType());
            }
            
            Thread.Sleep(5000);
            logger.Info("Service stopped.");
        }

        /// <summary>
        /// Инициализация сервиса
        /// </summary>
        public bool Init()
        {
            logger.Info("Initialization started");
            try
            {
                //загрузка плагинов устройств
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                logger.Info("Executing path : {0}", path);

                string fileName = Settings.Current.DeviceHandlersFileName;
                logger.Info("Device handlers file path : {0}", fileName);
                _deviceHandlers = HandlerLoader.LoadDeviceHandlers(fileName, path);

                foreach (var handler in _deviceHandlers)
                {
                    handler.MessageEvent += handler_MessageEvent;
                    logger.Info("Device handler {0} loaded", handler.GetType());
                }


                //загрузка плагинов продукта
                fileName = Settings.Current.SystemHandlersFileName;
                logger.Info("System handlers file path : {0}", fileName);
                _systemHandlers = HandlerLoader.LoadSystemHandlers(fileName, path);

                foreach (var handler in _systemHandlers)
                    logger.Info("System handler {0} loaded", handler.GetType());
                
                logger.Info("Initialization comlete");
                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorException("Initialization error.", ex);
                return false;
            }
        }

        void handler_MessageEvent(Location message)
        {
            logger.Debug("incoming message: {0}", ObjectLogFormatter.ToString(message, false));
            
            foreach (var handler in _systemHandlers)
            {
                Task.Factory.StartNew(() =>
                {
                    handler.SetMessage(message);
                });
            }
        }
    }
}
