using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace ChairControl.ChairWork
{
    public class FutuRiftController
    {
        private static FutuRiftController defaultController = new FutuRiftController(6);


        private SerialPort port;
        Timer timer;
        private float _pitch;
        private float _roll;
        private byte[] buffer = new byte[33];


        public float Pitch { get => _pitch; set => _pitch = Mathf.Clamp(value, -15, 21); }
        public float Roll { get => _roll; set => _roll = Mathf.Clamp(value, -18, 18); }
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
        }

        public void Start()
        {
            try
            {
                port.Open();
                timer.Start();

            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError("Can't open port for Chair");
            }
        }

        public void Stop()
        {
            timer.Stop();
            port.Close();
        }

        public static FutuRiftController Default => defaultController;

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            byte index = 4;
            Fill(ref index, _pitch);
            Fill(ref index, _roll);
            buffer[index++] = 0;
            buffer[index++] = 0;
            buffer[index++] = 0;
            buffer[index++] = 0;

            var crc = BitConverter.GetBytes(FullCRC(buffer, 4, index));

            buffer[index++] = crc[0];
            buffer[index++] = crc[1];
            buffer[index++] = MSG.EOM;
            
            port.Write(buffer, 0, index);
        }

        private void Fill(ref byte index, float value)
        {
            var arr = BitConverter.GetBytes(value);
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] >= MSG.ESC)
                {
                    buffer[index++] = MSG.ESC;
                    buffer[index++] = (byte)(arr[i] - MSG.ESC);
                }
                else
                    buffer[index++] = arr[i];
            }

        }

        private static ushort FullCRC(byte[] p, int start, int end)
        {
            ushort crc = 58005;
            for (int index = start; index <= end - 1; ++index)
            {
                if (p[index] == MSG.ESC)
                {
                    crc = CRC16(crc, (byte)(p[++index] + MSG.ESC));
                }
                else
                    crc = CRC16(crc, p[index]);
            }
            return crc;
        }

        private static ushort CRC16(ushort crc, byte b)
        {
            ushort num1 = (ushort)(byte.MaxValue & (crc >> 8 ^ b));
            ushort num2 = (ushort)(num1 ^ (uint)num1 >> 4);
            return (ushort)((crc ^ num2 << 4 ^ num2 >> 3) << 8 ^ (num2 ^ num2 << 5) & byte.MaxValue);
        }

    }
}
