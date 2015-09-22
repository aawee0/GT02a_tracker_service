using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Race.Locator.Positioning.DeviceMan.Model
{
    public class Location
    {
        public string IMEI { get; set; }

        /// <summary>
        /// Является ли данная локация валидной 
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Лонгитуда
        /// </summary>
        public double Lon { get; set; }

        /// <summary>
        /// латитуда 
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Направление движения 
        /// </summary>
        public int Angle { get; set; }

        /// <summary>
        /// Скорость 
        /// </summary>
        public double Velocity { get; set; }

        /// <summary>
        /// Дата локации по gps
        /// </summary>
        public DateTime PositionDate { get; set; }

        /// <summary>
        /// Датчеги
        /// </summary>
        public int Sensors { get; set; }

        /// <summary>
        /// Версия борта
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// SATN
        /// </summary>
        public int Satn { get; set; }

        /// <summary>
        /// SATK
        /// </summary>
        public int Satk { get; set; }

        /// <summary>
        /// Максимальное отрицательное ускорение 
        /// </summary>
        public double AccelMaxNeg { get; set; }

        /// <summary>
        /// Максимальное положительное ускорение 
        /// </summary>
        public double AccelMaxPos { get; set; }

        /// <summary>
        /// Максимальная скорость
        /// </summary>
        public int MaxSpeed { get; set; }

        /// <summary>
        /// Пробег общий
        /// </summary>
        public int Distance { get; set; }

        /// <summary>
        /// Пробег с последней отметки 
        /// </summary>
        public int DistanceDiff { get; set; }

        /// <summary>
        /// Уровень сигнала gsm 
        /// </summary>
        public int GsmRssi { get; set; }

        /// <summary>
        /// Номер посылки 
        /// </summary>
        public int MsgNum { get; set; }

        /// <summary>
        /// Дата и время борта
        /// </summary>
        public DateTime TerminalDate { get; set; }

        /// <summary>
        /// ИД борта
        /// </summary>
        public int TerminalID { get; set; }

        /// <summary>
        /// Время создания записи в БД
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// температура выдаваемая датчиком уровня топлива, в градусах 	-100..100 
        /// </summary>
        public int FTemp { get; set; }
        /// <summary>
        /// температура терминала градусы * 10 	0.. 4294967295 
        /// </summary>
        public double BoardTemp { get; set; }
        /// <summary>
        /// напряжение АИП в Вольт * 1000 	0.. 4294967295 
        /// </summary>
        public double AipVolt { get; set; }
        /// <summary>
        /// напряжение Бортсети в Вольт * 1000 	0.. 
        /// </summary>
        public double PowerVolt { get; set; }
        /// <summary>
        /// уровень топлива   0…1023
        /// </summary>
        public int FSensor { get; set; }
        /// <summary>
        /// Битовая маска изменений датчиков 	0.. 4294967295 
        /// </summary>
        public int SumSensors { get; set; }
        /// <summary>
        /// параметр, отвечающий за погрешность локации, выдаваемый GPS либо ГЛОНАС приемником *10. 	0..65535 
        /// </summary>
        public int HDop { get; set; }
        /// <summary>
        /// Количество gps спутников, участвующих в расчѐте локации 	0..256 
        /// </summary>
        public int GpsSatNum { get; set; }
        /// <summary>
        /// Количество ГЛОНАС спутников, участвующих в расчѐте локации 	0..256 
        /// </summary>
        public int GlonasSatNum { get; set; }
        
        public Dictionary<string, object> Parameters { get; set; }
    }
}
