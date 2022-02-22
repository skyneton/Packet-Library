﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PacketSocket.Utils
{
    public class ByteBuf
    {
        private byte[] _readBuf;
        private readonly List<byte> _buf = new ();
        public int Available => ReadLength + WriteLength - Position;
        public int WriteLength => _buf.Count;
        public int ReadLength => _readBuf?.Length ?? 0;
        public int Position = 0;

        public ByteBuf(byte[] data)
        {
            _readBuf = data;
        }
        public ByteBuf() {}

        public static IEnumerable<byte> GetVarInt(int integer)
        {
            var buf = new List<byte>();
            while ((integer & -128) != 0)
            {
                buf.Add((byte) (integer & 127 | 128));
                integer = (int) ((uint) integer >> 7);
            }
            
            buf.Add((byte) integer);
            return buf.ToArray();
        }
        
        public static int ReadVarInt(NetworkStream stream)
        {
            var value = 0;
            var size = 0;
            int b;

            while (((b = stream.ReadByte()) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    throw new IOException("VarInt too long.");
                }
            }

            return value | ((b & 0x7F) << (size * 7));
        }
        
        public static int ReadVarInt(byte[] data)
        {
            var value = 0;
            var size = 0;
            int b;

            while (((b = data[size]) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    throw new IOException("VarInt too long.");
                }
            }

            return value | ((b & 0x7F) << (size * 7));
        }

        public int ReadByte()
        {
            return Read(1)[0];
        }

        public byte[] Read(int length)
        {
            var buffer = new byte[length];
            if (Position + length > ReadLength)
            {
                var readable = ReadLength - Position;
                if (readable > 0)
                {
                    Buffer.BlockCopy(_readBuf, Position, buffer, 0, readable);
                    length -= readable;
                    readable = 0;
                }

                var offset = ReadLength - Position;
                Buffer.BlockCopy(_buf.ToArray(), readable * -1, buffer, offset < 0 ? 0 : offset, length);
                // var pos = _readBuf?.Length ?? 0 - Position;
                // var target = pos < 0 ? 0 : pos;
                //
                // Debug.WriteLine(target);
                // if(_readBuf != null && target > 0)
                //     Buffer.BlockCopy(_readBuf, Position, buffer, 0, target);
                //
                // Buffer.BlockCopy(_buf.ToArray(), Position + target - _readBuf?.Length ?? 0, buffer,target,length - target);
            }else
                Buffer.BlockCopy(_readBuf, Position, buffer, 0, length);
            Position += length;
            
            return buffer;
        }

        public byte[] Peek(int length)
        {
            var pos = Position;
            var buffer = Read(length);
            Position = pos;
            
            return buffer;
        }

        public int PeekVarInt()
        {
            var pos = Position;
            var result = ReadVarInt();
            Position = pos;
            return result;
        }

        public bool ReadBool()
        {
            return ReadByte() != 0;
        }
        
        public int ReadVarInt()
        {
            var value = 0;
            var size = 0;
            int b;

            while (((b = ReadByte()) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    throw new IOException("VarInt too long.");
                }
            }

            return value | ((b & 0x7F) << (size * 7));
        }

        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(Read(4), 0);
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(Read(4), 0);
        }

        public string ReadString()
        {
            return Encoding.UTF8.GetString(Read(ReadVarInt()));
        }

        public long ReadLong()
        {
            return BitConverter.ToInt64(Read(8), 0);
        }

        public short ReadShort()
        {
            return BitConverter.ToInt16(Read(2), 0);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(Read(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(Read(8), 0);
        }

        public void Write(IEnumerable<byte> data)
        {
            _buf.AddRange(data);
        }

        public void Write(byte[] data, int length)
        {
            if (length == data.Length)
            {
                Write(data);
                return;
            }
            var arr = new byte[length];
            Buffer.BlockCopy(data, 0, arr, 0, length);
            _buf.AddRange(arr);
        }

        public void WriteByte(byte b)
        {
            _buf.Add(b);
        }

        public void WriteVarInt(int integer)
        {
            _buf.AddRange(GetVarInt(integer));
        }

        public void WriteInt(int data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteString(string data)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(data);
            WriteVarInt(stringData.Length);
            _buf.AddRange(stringData);
        }

        public void WriteShort(short data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteUShort(ushort data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteUInt(uint data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteBool(bool data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteDouble(double data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
        }

        public void WriteFloat(float data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
            // buf.AddRange(HostToNetworkOrder(data));
        }

        public void WriteLong(long data)
        {
            _buf.AddRange(BitConverter.GetBytes(data));
        }

        public byte[] Flush()
        {
            _buf.InsertRange(0, GetVarInt(_buf.Count));
            var data = _buf.ToArray();

            _readBuf = null;
            _buf.Clear();

            return data;
        }

        public byte[] GetBytes()
        {
            return _buf.ToArray();
        }

        public byte[] GetReadBytes()
        {
            return _readBuf;
        }
    }
}