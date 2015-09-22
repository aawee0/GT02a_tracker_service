using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Race.Locator.Positioning.DeviceMan.Model
{
    public class Settings
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        private Settings() { }

        #region Static Members
        private static readonly object critical = new object();
        private static Settings current;

        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        public static Settings Current
        {
            get
            {
                if (null == current)
                {
                    lock (critical)
                    {
                        if (null == current) // double check after lock
                        {
                            Settings local = new Settings();
                            local.DeviceHandlersFileName = ConfigurationManager.AppSettings["DeviceHandlersFileName"];
                            local.SystemHandlersFileName = ConfigurationManager.AppSettings["SystemHandlersFileName"];
                            current = local;
                        }
                    }
                }
                return current;
            }
        }
        #endregion
        
        /// <summary>
        /// Файл с настройками плагинов для устройств 
        /// </summary>
        public string DeviceHandlersFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Файл с настройками плагинов куда сохранять данные в систему
        /// </summary>
        public string SystemHandlersFileName
        {
            get;
            set;
        }

    }
}
