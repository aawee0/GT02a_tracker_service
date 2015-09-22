using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Race.Locator.Positioning.DeviceMan.Model;
using NLog;

namespace Race.Locator.Positioning.DeviceMan.Handler
{
    public class HttpSendHandler : ISystemHandler
    {
        private Logger logger = LogManager.GetLogger("HttpSendHandler");

        private string _url;

        public void Init(System.Xml.XmlElement handlerElement)
        {
            _url = handlerElement["Url"].InnerText;
        }

        public void SetMessage(Location loc)
        {
            try
            {
                StringBuilder sb = new StringBuilder(_url);
                sb.AppendFormat("?boardId={0}", loc.IMEI);
                sb.AppendFormat("&version={0}", loc.Version);
                sb.AppendFormat("&channel=gprs");
                sb.AppendFormat("&msgNum={0}", loc.MsgNum);
                sb.AppendFormat("&Sensors={0}", loc.Sensors);
                sb.AppendFormat("&sumSensors={0}", loc.SumSensors);
                sb.AppendFormat("&longitude={0}.{1:000000}", (int)loc.Lon, (loc.Lon - (int)loc.Lon) * 10000 * 60);
                sb.AppendFormat("&latitude={0}.{1:000000}", (int)loc.Lat, (loc.Lat - (int)loc.Lat) * 10000 * 60);
                sb.AppendFormat("&boardTime={0}", loc.PositionDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                sb.AppendFormat("&gpsTime={0}", loc.PositionDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                sb.AppendFormat("&speed={0}", loc.Velocity * 100);
                sb.AppendFormat("&maxSpeed={0}", loc.MaxSpeed * 100);
                sb.AppendFormat("&course={0}", loc.Angle * 100);
                sb.AppendFormat("&isValid={0}", loc.IsValid);
                sb.AppendFormat("&satn={0}", loc.Satn);
                sb.AppendFormat("&satk={0}", loc.Satk);
                sb.AppendFormat("&gsmRssi={0}", loc.GsmRssi);
                sb.AppendFormat("&distance={0}", loc.Distance);
                sb.AppendFormat("&distanceDiff={0}", loc.DistanceDiff);
                sb.AppendFormat("&accelMaxNeg={0}", loc.AccelMaxNeg * 10);
                sb.AppendFormat("&accelMaxPos={0}", loc.AccelMaxPos * 10);
                sb.AppendFormat("&fsensor={0}", loc.FSensor);
                sb.AppendFormat("&aipVolt={0}", loc.AipVolt * 1000);
                sb.AppendFormat("&boardTemp={0}", loc.BoardTemp * 10);
                sb.AppendFormat("&powerVolt={0}", loc.PowerVolt * 1000);
                sb.AppendFormat("&HDop={0}", loc.HDop);
                sb.AppendFormat("&GpsSatNum={0}", loc.GpsSatNum);
                sb.AppendFormat("&GlonasSatNum={0}", loc.GlonasSatNum);

                logger.Debug("send to {0} message", sb.ToString());

                WebClient web = new WebClient();
                string s = web.DownloadString(sb.ToString());
                logger.Info("answer from server: {0}", s);
            }
            catch (Exception ex)
            {
                logger.ErrorException("set message error", ex);
            }

        }
    }
}
