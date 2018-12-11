using System;
using System.IO;
using Google.Protobuf;

namespace ClientGame.Net
{
    public class KcpNetPack
    {
        public ushort HeadID { get; protected set; }
        public ushort Length { get; protected set; }  //body的长度
        public ushort CrcValue { get; set; }
        public uint Index { get; set; }

        public CBuffer BodyBuffer;
        public CBuffer HeaderBuffer;
        private const int HeaderLength = 4;

        protected void _ParseHeader()
        {
            HeadID = BitConverter.ToUInt16(HeaderBuffer.Bytes, 0);
            Length = BitConverter.ToUInt16(HeaderBuffer.Bytes, 2);
        }

        protected void _AssemblyHeader()
        {
            byte[] msgID = BitConverter.GetBytes(HeadID);
            HeaderBuffer.Fill(msgID, 2);

            byte[] len = BitConverter.GetBytes(Length);
            HeaderBuffer.Fill(len, 2);
        }

        public KcpNetPack()
        {
            HeaderBuffer = CBuffer.Create(HeaderLength);
        }

        public KcpNetPack(ushort msgID, ushort msgLen)
        {
            HeadID = msgID;
            Length = msgLen;

            HeaderBuffer = CBuffer.Create(HeaderLength);
            _AssemblyHeader();
        }

        public bool FillPack(byte[] buffer, ref int offset, ref int count)
        {
            if (!this.HeaderBuffer.IsFull)
            {
                int fillSize = HeaderBuffer.Fill(buffer, offset, count, true);
                offset += fillSize;
                count -= fillSize;

                if (HeaderBuffer.IsFull)
                {
                    _ParseHeader();//解析头以后才知道body的长度
                    BodyBuffer = CBuffer.Create((int)this.Length);
                }
                else
                {
                    //头部数据没接收完成，继续解析
                    return false;
                }
            }

            if (!this.BodyBuffer.IsFull)
            {
                int fillSize = BodyBuffer.Fill(buffer, offset, count, true);
                offset += fillSize;
                count -= fillSize;
            }

            return this.BodyBuffer.IsFull;
        }

        public CBuffer Assembly()
        {
            CBuffer buf = CBuffer.Create(HeaderLength + BodyBuffer.Length);
            buf.Fill(HeaderBuffer);
            buf.Fill(BodyBuffer);
            return buf;
        }

        //public T Deserialize<T>()
        //{
        //    using (var ms = new MemoryStream(this.BodyBuffer.Bytes, 0, this.BodyBuffer.BufSize))
        //        return ProtoBuf.Serializer.Deserialize<T>(ms);
        //}
        //void Send<T>(Protocol type, T content) where T : Google.Protobuf.IMessage
        //{
        //    GameMessage message = new GameMessage();
        //    message.Type = type;
        //    message.Data = ByteString.CopyFrom(content.ToByteArray());
        //    _UnityUdpSocket.Send(message.ToByteArray());
        //}
        public static KcpNetPack SerializeToPack<T>(T content, ushort msg_id) where T : Google.Protobuf.IMessage
        {
            int size = content.CalculateSize();//注意检查长度不能超过  65,535-HeaderLength
            KcpNetPack pack = new KcpNetPack(msg_id, (ushort)size);
            pack.BodyBuffer = CBuffer.Create(size);
            pack.BodyBuffer.Fill(content.ToByteArray(), size);
            return pack;
        }
    }
}