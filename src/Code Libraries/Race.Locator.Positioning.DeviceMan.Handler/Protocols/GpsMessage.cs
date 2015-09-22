using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;

namespace Race.Locator.Positioning.DeviceMan.Handler.Protocols
{
    public class GpsMessage : ScsMessage
    {
        public byte[] Data { get; set; }
        public Race.Locator.Positioning.DeviceMan.Model.Location Location { get; set; }
        
        public GpsMessage() 
        {
            Location = new Model.Location();
        }

        public override string ToString()
        {
            return String.Format("{0}", (Data != null ? BitConverter.ToString(Data) : ""));
        }
    }
}
