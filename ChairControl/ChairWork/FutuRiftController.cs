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
using Assets.Plugins.UnityChairPlugin.ChairControl.ChairWork.Options;
using Assets.Plugins.UnityChairPlugin.ChairControl.ChairWork;
using Assets.Plugins.UnityChairPlugin.ChairControl.ChairWork.DataSenders;
using System.Collections.ObjectModel;

namespace ChairControl.ChairWork
{
    public class FutuRiftController
    {
        private readonly Timer timer;
        private float pitch;
        private float roll;
        private readonly byte[] buffer = new byte[33];

        private readonly ReadOnlyCollection<IDataSender> senders;

        public float Pitch { get => pitch; set => pitch = value.Clamp(-15, 21); }
        public float Roll { get => roll; set => roll = value.Clamp(-18, 18); }

        public FutuRiftController(ComPortOptions comPortOptions, FutuRiftOptions futuRiftOptions = null)
            : this(comPortOptions: comPortOptions, udpOptions: null, futuRiftOptions: futuRiftOptions)
        {
        }
        public FutuRiftController(UdpOptions udpOptions, FutuRiftOptions futuRiftOptions = null)
            : this(comPortOptions: null, udpOptions: udpOptions, futuRiftOptions: futuRiftOptions)
        {
        }
        public FutuRiftController(ComPortOptions comPortOptions, UdpOptions udpOptions, FutuRiftOptions futuRiftOptions = null)
        {
            futuRiftOptions = futuRiftOptions ?? new FutuRiftOptions();
            var sendersList = new List<IDataSender>();
            if (comPortOptions != null)
            {
                sendersList.Add(new ComPortSender(comPortOptions));
            }
            if (udpOptions != null)
            {
                sendersList.Add(new UdpPortSender(udpOptions));
            }
            senders = sendersList.AsReadOnly();
            buffer[0] = MSG.SOM;
            buffer[1] = 33;
            buffer[2] = 12;
            buffer[3] = (byte)Flag.OneBlock;
            timer = new Timer(futuRiftOptions.interval);
            timer.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            foreach (var sender in senders)
            {
                sender.Start();
            }
            timer.Start();
        }

        public void Stop()
        {
            foreach (var sender in senders)
            {
                sender.Stop();
            }
            timer.Stop();
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
            foreach (var dataSender in senders)
            {
                dataSender.SendData(buffer);
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
