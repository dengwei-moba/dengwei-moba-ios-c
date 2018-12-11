using System;
using System.IO;
using Google.Protobuf;

namespace ClientGame.Net
{
    public class NetPack
    {
        public uint HeadID { get; protected set; }
        public uint CMD { get; protected set; }
        public uint Length { get; protected set; }  //body的长度
        public ushort CrcValue { get; set; }
        public uint Index { get; set; }

        public CBuffer BodyBuffer;
        public CBuffer HeaderBuffer;
        private const int HeaderLength = 12;

        protected void _ParseHeader()
        {
            HeadID = BitConverter.ToUInt32(HeaderBuffer.Bytes, 0);
            CMD = BitConverter.ToUInt32(HeaderBuffer.Bytes, 4);
            Length = BitConverter.ToUInt32(HeaderBuffer.Bytes, 8);
        }

        protected void _AssemblyHeader()
        {
            byte[] msgID = BitConverter.GetBytes(HeadID);
            HeaderBuffer.Fill(msgID, 4);

            byte[] cmd = BitConverter.GetBytes(CMD);
            HeaderBuffer.Fill(cmd, 4);

            byte[] len = BitConverter.GetBytes(Length);
            HeaderBuffer.Fill(len, 4);
        }

        public NetPack()
        {
            HeaderBuffer = CBuffer.Create(HeaderLength);
        }

        public NetPack(uint msgID, uint cmd, uint msgLen)
        {
            HeadID = msgID;
            CMD = cmd;
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
        public static NetPack SerializeToPack<T>(T content, uint msg_id, uint cmd) where T : Google.Protobuf.IMessage
        {
            int size = content.CalculateSize();
            NetPack pack = new NetPack(msg_id, cmd, (uint)size);
            pack.BodyBuffer = CBuffer.Create(size);
            pack.BodyBuffer.Fill(content.ToByteArray(), size);
            return pack;
        }
    }
}

