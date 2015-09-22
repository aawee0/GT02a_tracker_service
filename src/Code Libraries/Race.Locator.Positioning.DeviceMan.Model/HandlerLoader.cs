using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Race.Locator.Positioning.DeviceMan.Model
{
    public class HandlerLoader
    {
        /// <summary>
        /// Прогрузить плагины 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="workingDir"></param>
        public static IList<IDeviceHandler> LoadDeviceHandlers(string fileName, string workingDir)
        {
            if (workingDir == null)
                throw new ApplicationException("Working directory can't be null!");

            fileName = Path.Combine(workingDir, fileName);
            if (!File.Exists(fileName))
                    throw new Exception(string.Format("Cannot find configuration file {0} ", fileName));

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            // инициализируем обработчик 
            var items = doc.SelectNodes("/handlers/handler");
            if (items == null || items.Count == 0)
                throw new Exception("Invalid configuration file format. Missing <handler> element in device handlers.");

            List<IDeviceHandler> list = new List<IDeviceHandler>();
            foreach (XmlElement handlerElement in items)
            {
                IDeviceHandler handler = CreateObject<IDeviceHandler>(handlerElement);
                handler.Init(handlerElement);
                list.Add(handler);
            }

            return list;
        }

        /// <summary>
        /// Прогрузить плагины 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="workingDir"></param>
        public static IList<ISystemHandler> LoadSystemHandlers(string fileName, string workingDir)
        {
            if (workingDir == null)
                throw new ApplicationException("Working directory can't be null!");

            fileName = Path.Combine(workingDir, fileName);
            if (!File.Exists(fileName))
                throw new Exception(string.Format("Cannot find configuration file {0} ", fileName));

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            // инициализируем обработчик 
            var items = doc.SelectNodes("/handlers/handler");
            if (items == null || items.Count == 0)
                throw new Exception("Invalid configuration file format. Missing <handler> element in system handlers.");

            List<ISystemHandler> list = new List<ISystemHandler>();
            foreach (XmlElement handlerElement in items)
            {
                ISystemHandler handler = CreateObject<ISystemHandler>(handlerElement);
                handler.Init(handlerElement);
                list.Add(handler);
            }

            return list;
        }

        /// <summary>
        /// Создать объект 
        /// </summary>
        /// <param name="handlerElement"></param>
        /// <returns></returns>
        private static T CreateObject<T>(XmlElement handlerElement)
        {
            if (handlerElement == null)
                throw new ApplicationException("Configuration element is null");
            XmlAttribute attribute = handlerElement.Attributes["type"];
            if (attribute == null)
                throw new ApplicationException("Missing handler class type.");
            string typeName = attribute.Value;
            if (string.IsNullOrEmpty(typeName))
                throw new ApplicationException("Type attribute is null");

            Type type = Type.GetType(typeName, true, false);

            T handler = default(T);
            if (type != null)
            {
                var obj = Activator.CreateInstance(type);
                if (obj is T)
                    handler = (T)obj;
                else
                    throw new ApplicationException(string.Format(@"Cannot create type {0}. Object is null.", typeName));
            }

            return handler;
        }
    }
}
