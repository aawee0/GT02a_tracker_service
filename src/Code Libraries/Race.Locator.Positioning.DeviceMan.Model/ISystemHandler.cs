using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Race.Locator.Positioning.DeviceMan.Model
{
    /// <summary>
    /// Интерфейс для обработчика входящих сообщений и сохранений в базу
    /// </summary>
    public interface ISystemHandler
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        void Init(XmlElement handlerElement);

        /// <summary>
        /// Входящее сообщение
        /// </summary>
        /// <param name="message"></param>
        void SetMessage(Location message);
    }
}
