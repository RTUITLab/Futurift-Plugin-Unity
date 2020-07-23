using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using ChairControl.Extensions;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace ChairControl.ChairWork
{
    public class FutuRiftController
    {
        private static FutuRiftController defaultController = new FutuRiftController(6);

        private readonly SerialPort port;
        private readonly Timer timer;
        private float pitch;
        private float roll;
        private readonly byte[] buffer = new byte[33];
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ep;
        private bool InHomeMode = false;
        private IPAddress Ip;
        private int Port;


        public float Pitch { get => pitch; set => pitch = value.Clamp(-15, 21); }
        public float Roll { get => roll; set => roll = value.Clamp(-18, 18); }
        public bool IsConnected => port.IsOpen;
        public FutuRiftController(int portNumber)
        {
            port = new SerialPort()
            {
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadBufferSize = 4096,
                WriteBufferSize = 4096,
                ReadTimeout = 500,
                PortName = "COM" + portNumber,
            };
            buffer[0] = MSG.SOM;
            buffer[1] = 33;
            buffer[2] = 12;
            buffer[3] = (byte)Flag.OneBlock;
            timer = new Timer(100);
            timer.Elapsed += Timer_Elapsed;
            this.InHomeMode = false;
        }

        public FutuRiftController(int port, IPAddress Ip)
        {
            this.InHomeMode = true;
            timer = new Timer(100);
            timer.Elapsed += Timer_Elapsed;
            this.Port = port;
            this.Ip = Ip;
        }

        public void Start()
        {
            if (InHomeMode)
            {
                IPAddress broadcast = Ip;
                ep = new IPEndPoint(broadcast, Port);
            }
            else
            {
                port.Open();
            }
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
            if (!InHomeMode)
            {
                port.Close();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            byte index = 4;
            Fill(ref index, pitch);
            Fill(ref index, roll);
            buffer[index++] = 0;
            buffer[index++] = 0;
            buffer[index++] = 0;
            buffer[index++] = 0;
            Fill(ref index, FullCRC(buffer, 1, index));
            buffer[index++] = MSG.EOM;
            string GG = BitConverter.ToString(buffer);
            //UnityEngine.Debug.Log(Convert.ToInt32(buffer[8]));
            UnityEngine.Debug.Log(GG);
            if (InHomeMode)
            {
                s.SendTo(Encoding.ASCII.GetBytes($"pr {pitch} {roll}"), ep);
            }
            else
            {
                port.Write(buffer, 0, index);
            }
        }

        private void Fill(ref byte index, float value)
        {
            var arr = BitConverter.GetBytes(value);
            for (var i = 0; i < arr.Length; i++)
            {
                AddByte(ref index, arr[i]);
            }
        }
        private void Fill(ref byte index, ushort value)
        {
            var arr = BitConverter.GetBytes(value);
            for (var i = 0; i < arr.Length; i++)
            {
                AddByte(ref index, arr[i]);
            }
        }
        private void AddByte(ref byte index, byte value)
        {
            if (value >= MSG.ESC)
            {
                buffer[index++] = MSG.ESC;
                buffer[index++] = (byte)(value - MSG.ESC);
            }
            else
            {
                buffer[index++] = value;
            }
        }

        private static ushort FullCRC(byte[] p, int start, int end)
        {
            ushort crc = 58005;
            for (var i = start; i < end; i++)
            {
                if (p[i] == MSG.ESC)
                {
                    i++;
                    crc = CRC16(crc, (byte)(p[i] + MSG.ESC));
                }
                else
                    crc = CRC16(crc, p[i]);
            }
            return crc;
        }

        private static ushort CRC16(ushort crc, byte b)
        {
            var num1 = (ushort)(byte.MaxValue & (crc >> 8 ^ b));
            var num2 = (ushort)(num1 ^ (uint)num1 >> 4);
            return (ushort)((crc ^ num2 << 4 ^ num2 >> 3) << 8 ^ (num2 ^ num2 << 5) & byte.MaxValue);
        }
    }
}
