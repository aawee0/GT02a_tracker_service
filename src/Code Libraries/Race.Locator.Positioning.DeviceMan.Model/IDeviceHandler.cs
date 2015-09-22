using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Race.Locator.Positioning.DeviceMan.Model
{
    /// <summary>
    /// Интерфейс для обработчика сообщений от устройств
    /// </summary>
    public interface IDeviceHandler
    {
        event Action<Location> MessageEvent;

        /// <summary>
        /// Инициализация
        /// </summary>
        void Init(XmlElement handlerElement);

        void Start();
        void Stop();
    }
}
