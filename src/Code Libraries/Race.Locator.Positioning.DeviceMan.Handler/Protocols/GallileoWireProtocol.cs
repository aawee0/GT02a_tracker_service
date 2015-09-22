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
    public class GallileoWireProtocol : IScsWireProtocol
    {
        public Logger logger = LogManager.GetLogger("GallileoWireProtocol");

        /// <summary>
        /// Maximum length of a message.
        /// </summary>
        private const int MaxMessageLength = 32768; //32 Kbytes.

        /// <summary>
        /// This MemoryStream object is used to collect receiving bytes to build messages.
        /// </summary>
        private MemoryStream _receiveMemoryStream;

        public GallileoWireProtocol()
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
            logger.Debug("Data {0}", BitConverter.ToString(m.Data));
            if (m.Data != null)
            {
                bytes.Add(0x02);
                bytes.Add(m.Data[m.Data.Length - 2]);
                bytes.Add(m.Data[m.Data.Length - 1]);
                m.Data = bytes.ToArray();
            }
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

            int start = _receiveMemoryStream.ReadByte();

            var messageLength = ReadInt16(_receiveMemoryStream) & 0x7fff;

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
                int fisrtTag = 0;

                while (deserializeMemoryStream.Position < deserializeMemoryStream.Length - 2)
                {
                    int tag = ReadInt8(deserializeMemoryStream);
                    
                    // так как в одном сообщении передается много координат, а разделителя нет, делаем жесткий хак.
                    // запонинаем первый тэг пакета и его считаем разделителем
                    if (fisrtTag == 0)
                    {
                        fisrtTag = tag;
                        message = new GpsMessage() { Data = bytes };
                        messages.Add(message);
                    }
                    else if (tag == fisrtTag)
                    {
                        message = new GpsMessage();
                        messages.Add(message);
                    }

                    switch (tag)
                    {
                        case 0x01:
                            //logger.Debug("Версия железа: {0}", ReadInt8(deserializeMemoryStream));
                            break;
                        case 0x02:
                            message.Location.Version = ReadInt8(deserializeMemoryStream).ToString();
                            //logger.Debug("Версия прошивки: {0}", message.Location.Version);
                            break;
                        case 0x03:
                            message.Location.IMEI = System.Text.Encoding.ASCII.GetString(ReadByteArray(deserializeMemoryStream, 15));
                            //logger.Debug("IMEI: {0}", message.Location.IMEI);
                            break;
                        case 0x04:
                            ReadInt16(deserializeMemoryStream);
                            //logger.Debug("Идентификатор устройства: {0}", );
                            break;
                        case 0x10:
                            message.Location.MsgNum = ReadInt16(deserializeMemoryStream);
                            //logger.Debug("Номер записи в архиве: {0}", message.Location.MsgNum);
                            break;
                        case 0x20:
                            message.Location.TerminalDate = message.Location.PositionDate = new DateTime(1970, 1, 1).AddSeconds(ReadInt32(deserializeMemoryStream));
                            //logger.Debug("Дата и время: {0}", message.Location.PositionDate);
                            break;
                        case 0x30:
                            int state = ReadInt8(deserializeMemoryStream);
                            message.Location.Lat = ReadInt32(deserializeMemoryStream) / 1000000.0;
                            message.Location.Lon = ReadInt32(deserializeMemoryStream) / 1000000.0;
                            message.Location.IsValid = (state & 0xF0) >> 4 == 0;

                            //logger.Debug("Координаты: число: {0}, валидность: {1}, lat: {2}, lon: {3}", (state & 0x0F), (state & 0xF0) >> 4, message.Location.Lat, message.Location.Lon);
                            break;
                        case 0x33:
                            //uint sc = (uint)ReadInt32(deserializeMemoryStream);
                            int speed = ReadInt16(deserializeMemoryStream);
                            int cource = ReadInt16(deserializeMemoryStream);
                            message.Location.Velocity = (double)speed / 10;
                            message.Location.Angle = cource / 10;
                            //logger.Debug("Скорость {0} и направление: {1}", message.Location.Velocity, message.Location.Angle);
                            break;
                        case 0x34:
                            ReadInt16(deserializeMemoryStream);
                            //logger.Debug("Высота: {0}", );
                            break;
                        case 0x35:
                            message.Location.HDop = ReadInt8(deserializeMemoryStream);
                            //logger.Debug("HDOP: {0}", message.Location.HDop);
                            break;
                        case 0x40:
                            ReadInt16(deserializeMemoryStream);
                            //logger.Debug("Статус устройства: {0}", );
                            break;
                        case 0x41:
                            message.Location.PowerVolt = (double)ReadInt16(deserializeMemoryStream) / 1000.0;
                            //logger.Debug("Напряжение питания, мВ: {0}", message.Location.PowerVolt * 1000);
                            break;
                        case 0x42:
                            message.Location.AipVolt = (double)ReadInt16(deserializeMemoryStream) / 1000.0;
                            //logger.Debug("Напряжение аккумулятора, мВ: {0}", message.Location.AipVolt * 1000);
                            break;
                        case 0x43:
                            message.Location.BoardTemp = ReadInt8(deserializeMemoryStream);
                            //logger.Debug("Температура терминала, С: {0}", message.Location.BoardTemp);
                            break;
                        case 0x44:
                            ReadInt32(deserializeMemoryStream);
                            //logger.Debug("Ускорение: {0}", );
                            break;
                        case 0x45:
                            ReadInt16(deserializeMemoryStream);
                            //logger.Debug("Статус выходов: {0}", );
                            break;
                        case 0x46:
                            ReadInt16(deserializeMemoryStream);
                            //logger.Debug("Статус входов: {0}", ReadInt16(deserializeMemoryStream));
                            break;
                        case 0x50:
                        case 0x51:
                        case 0x52:
                        case 0x53:
                        case 0x58:
                        case 0x59:
                        case 0x70:
                        case 0x71:
                        case 0x72:
                        case 0x73:
                        case 0x74:
                        case 0x75:
                        case 0x76:
                        case 0x77:
                        case 0xD6:
                        case 0xD7:
                        case 0xD8:
                        case 0xD9:
                        case 0xDA:
                            ReadInt16(deserializeMemoryStream);
                            //logger.Debug("{0:x}: {1}", tag,  );
                            break;
                        case 0x90:
                        case 0xC0:
                        case 0xC1:
                        case 0xC2:
                        case 0xC3:
                        case 0xD3:
                        case 0xD4:
                        case 0xDB:
                        case 0xDC:
                        case 0xDD:
                        case 0xDE:
                        case 0xDF:
                            ReadInt32(deserializeMemoryStream);
                            //logger.Debug("{0:x}: {1}", tag,  );
                            break;
                        case 0xC4:
                        case 0xC5:
                        case 0xC6:
                        case 0xC7:
                        case 0xC8:
                        case 0xC9:
                        case 0xCA:
                        case 0xCB:
                        case 0xCC:
                        case 0xCD:
                        case 0xCE:
                        case 0xCF:
                        case 0xD0:
                        case 0xD1:
                        case 0xD2:
                        case 0xD5:
                            ReadInt8(deserializeMemoryStream);
                            //logger.Debug("{0:x}: {1}", tag,  );
                            break;
                    }
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
