using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols;
using NLog;

namespace Race.Locator.Positioning.DeviceMan.Handler.Protocols
{
    public class Gt02WireProtocol : IScsWireProtocol
    {
        public Logger logger = LogManager.GetLogger("Gt02WireProtocol");

        /// <summary>
        /// Maximum length of a message.
        /// </summary>
        private const int MaxMessageLength = 32768; //32 Kbytes.

        /// <summary>
        /// This MemoryStream object is used to collect receiving bytes to build messages.
        /// </summary>
        private MemoryStream _receiveMemoryStream;

        public Gt02WireProtocol()
        {
            _receiveMemoryStream = new MemoryStream();
        }


        public IEnumerable<IScsMessage> CreateMessages(byte[] receivedBytes)
        {
            logger.Debug("receive data: {0}", BitConverter.ToString(receivedBytes));

            //Write all received bytes to the _receiveMemoryStream
            _receiveMemoryStream.Write(receivedBytes, 0, receivedBytes.Length);
            //Create a list to collect messages
            var messages = new List<IScsMessage>();
            //Read all available messages and add to messages collection
            while (ReadSingleMessage(messages)) { }
            //Return message list
            return messages;
        }


        public byte[] GetBytes(IScsMessage message)
        {
            var m = message as Protocols.GpsMessage;
            List<byte> bytes = new List<byte>();
            
            if (m.Data != null)
            {
                bytes.Add(0x54);
                bytes.Add(0x68);
                bytes.Add(0x1A);
                bytes.Add(0x0D);
                bytes.Add(0x0A);
                m.Data = bytes.ToArray();
            }
            logger.Debug("Data {0}", BitConverter.ToString(m.Data));
            return bytes.ToArray();
        }

        public void Reset()
        {
            if (_receiveMemoryStream.Length > 0)
                _receiveMemoryStream = new MemoryStream();
        }

        private bool ReadSingleMessage(ICollection<IScsMessage> messages)
        {
            
            //Go to the begining of the stream
            _receiveMemoryStream.Position = 0;

            //If stream has less than 5 bytes, that means we can not even read length of the message
            //So, return false to wait more bytes from remore application.
            if (_receiveMemoryStream.Length < 5)
            {
                return false;
            }
            
            int head1 = _receiveMemoryStream.ReadByte();
            int head2 = _receiveMemoryStream.ReadByte();
            // two head bytes of the expected package are 68H 68H
            if (head1 == 104 && head2 == 104)
            {
                logger.Debug("Received appropriate head bytes {0} {1}", head1.ToString(), head2.ToString());
            }
            else return false;

            var messageLength = _receiveMemoryStream.ReadByte();
            logger.Debug("message length is {0}", messageLength.ToString());
            
            /*
            int start1 = _receiveMemoryStream.ReadByte();

            var messageLength = ReadInt16(_receiveMemoryStream) & 0x7fff;
            */
            if (messageLength > MaxMessageLength)
                throw new Exception("Message is too big (" + messageLength + " bytes). Max allowed length is " + MaxMessageLength + " bytes.");
            
            // если длина сообщенния 0
            if (messageLength == 0)
            {
                // удаляем сообщение если одно
                if (_receiveMemoryStream.Length == 5)
                {
                    _receiveMemoryStream = new MemoryStream();
                    return false;
                }
                else
                {
                    // если есть еще данные, то сдвигаем
                    var bytes = _receiveMemoryStream.ToArray();
                    _receiveMemoryStream = new MemoryStream();
                    _receiveMemoryStream.Write(bytes, 5, bytes.Length - 5);
                    return true;
                }
            }
            
            // если не все байты сообщения еще пришли
            if (_receiveMemoryStream.Length < (5 + messageLength))
            {
                _receiveMemoryStream.Position = _receiveMemoryStream.Length;
                return false;
            }

            //Read bytes of serialized message and deserialize it
            var serializedMessageBytes = ReadByteArray(_receiveMemoryStream, messageLength + 2);
            foreach (var m in ParseMessages(serializedMessageBytes))
                messages.Add(m);

            //Read remaining bytes to an array
            var remainingBytes = ReadByteArray(_receiveMemoryStream, (int)(_receiveMemoryStream.Length - (5 + messageLength)));

            //Re-create the receive memory stream and write remaining bytes
            _receiveMemoryStream = new MemoryStream();
            _receiveMemoryStream.Write(remainingBytes, 0, remainingBytes.Length);

            //Return true to re-call this method to try to read next message
            return (remainingBytes.Length > 5);
        }

        protected virtual IList<IScsMessage> ParseMessages(byte[] bytes)
        {
            List<IScsMessage> messages = new List<IScsMessage>();

            //Create a MemoryStream to convert bytes to a stream
            using (var deserializeMemoryStream = new MemoryStream(bytes))
            {
                GpsMessage message = null;
                message = new GpsMessage() { Data = bytes };
                messages.Add(message);

                // reserved bytes in location package or voltage+gsm level in heartbeat package
                byte[] reserved = ReadByteArray(deserializeMemoryStream, 2);
                //logger.Debug("Reserved bytes: {0}", BitConverter.ToString(reserved));

                // read imea bytes, convert into string without dashes
                byte[] imei_raw = ReadByteArray(deserializeMemoryStream, 8);
                string imei = (BitConverter.ToString(imei_raw)).Replace("-", "");
                message.Location.IMEI = imei;
                //logger.Debug("Device imei: {0}", imei);

                int pack_num = (int)deserializeMemoryStream.ReadByte() * 16 
                                        + (int)deserializeMemoryStream.ReadByte();
                message.Location.MsgNum = pack_num;
                //logger.Debug("Package number: {0}", pack_num.ToString());

                int protocol_num = deserializeMemoryStream.ReadByte();
                //logger.Debug("Protocol number: {0}", protocol_num.ToString());

                if (protocol_num == 16) // LOCATION
                {
                    //logger.Debug("Parsing location package ({0} bytes)", message.Data.Length);
                 
                    byte[] dateB = ReadByteArray(deserializeMemoryStream, 6);
                    // date: YY MM DD HH MM SS
                    DateTime date = new DateTime(dateB[0]+2000, dateB[1], dateB[2], dateB[3], dateB[4], dateB[5]);
                    message.Location.TerminalDate = message.Location.PositionDate = date;
                    logger.Debug("Date: {0}", date.ToString());

                    int lat = (deserializeMemoryStream.ReadByte() << 24) + (deserializeMemoryStream.ReadByte() << 16) +
                        (deserializeMemoryStream.ReadByte() << 8) + (deserializeMemoryStream.ReadByte());
                    double latd = lat / 30000.0;
                    latd /= 60.0;
                    message.Location.Lat = latd;
                    logger.Debug("Latitude: {0}", latd.ToString());

                    int lon = (deserializeMemoryStream.ReadByte() << 24) + (deserializeMemoryStream.ReadByte() << 16) +
                        (deserializeMemoryStream.ReadByte() << 8) + (deserializeMemoryStream.ReadByte());
                    double lond = lon / 30000.0;
                    lond /= 60.0;
                    message.Location.Lon = lond;
                    logger.Debug("Longitude: {0}", lond.ToString());

                    message.Location.IsValid = true;

                    int speed = deserializeMemoryStream.ReadByte();
                    message.Location.Velocity = speed;
                    logger.Debug("Speed: {0}", speed.ToString());

                    int direction = (deserializeMemoryStream.ReadByte() << 8) + (deserializeMemoryStream.ReadByte());
                    message.Location.Angle = direction;
                    logger.Debug("Direction: {0}", direction.ToString());
                }
                else if (protocol_num == 26) // HEARTBEAT
                {
                    logger.Debug("Parsing heartbeat package ({0} bytes)", message.Data.Length);

                    message.Location.IsValid = false;

                    // voltage+gsm level, been read before
                    int voltage = reserved[0];
                    logger.Debug("Voltage level (0~6): {0}", voltage.ToString());
                    int signal = reserved[1];
                    logger.Debug("GSM signal level (0~4): {0}", signal.ToString());

                    int loc_status = deserializeMemoryStream.ReadByte();
                    logger.Debug("Locating status: {0}", loc_status.ToString());

                    int num_satell = deserializeMemoryStream.ReadByte();
                    logger.Debug("Number of satellites: {0}", loc_status.ToString());

                }
                
                //Go to head of the stream
                deserializeMemoryStream.Position = 0;
            }
                 

            return messages;
        }

        private static int ReadInt8(Stream stream)
        {
            var buffer = ReadByteArray(stream, 1);
            return buffer[0];
        }

        private static int ReadInt16(Stream stream)
        {
            var buffer = ReadByteArray(stream, 2);
            return ((buffer[1] << 8) |
                    (buffer[0]));
        }

        private static int ReadInt32(Stream stream)
        {
            var buffer = ReadByteArray(stream, 4);
            return (
                    (buffer[3] << 24) |
                    (buffer[2] << 16) |
                    (buffer[1] << 8) |
                    (buffer[0])
                   );
        }

        private static byte[] ReadByteArray(Stream stream, int length)
        {
            var buffer = new byte[length];
            var totalRead = 0;
            while (totalRead < length)
            {
                var read = stream.Read(buffer, totalRead, length - totalRead);
                if (read <= 0)
                    throw new EndOfStreamException("Can not read from stream! Input stream is closed.");
                totalRead += read;
            }
            return buffer;
        }
    }
}
