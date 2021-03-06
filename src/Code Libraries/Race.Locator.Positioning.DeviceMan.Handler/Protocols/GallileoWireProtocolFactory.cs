﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols;

namespace Race.Locator.Positioning.DeviceMan.Handler.Protocols
{
    public class GallileoWireProtocolFactory : IScsWireProtocolFactory
    {
        public IScsWireProtocol CreateWireProtocol()
        {
            return new GallileoWireProtocol();
        }
    }
}
