using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace ChairControl.ChairWork
{
    internal class FutuRiftController
    {

        private SerialPort port;
        Timer timer;
        private float _pitch;
        private float _roll;

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
            //port.Close();
        }

        public static FutuRiftController Default
        {
            get
            {
                return new FutuRiftController(6);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            var packet = new byte[]
            {
                33,
                12,
                (byte)Flag.OneBlock
            }
            .Concat(BitConverter.GetBytes(_pitch))
            .Concat(BitConverter.GetBytes(_roll))
            .Concat(BitConverter.GetBytes(0f))
            .ToArray();
            var byteList = EncodePacket(packet).ToArray();
            Debug.Log($"{Pitch} {Roll}");
            Debug.Log(string.Join(" ", byteList.Select(b => b.ToString()).ToArray()));
            //port.Write(byteList, 0, byteList.Length);
        }
        private IEnumerable<byte> EncodePacket(byte[] packet)
        {
            yield return MSG.SOM;
            yield return packet[0];
            yield return packet[1];
            yield return packet[2];
            var crc = BitConverter.GetBytes(FullCRC(packet, packet[1] + 3));
            foreach (var item in Clear(packet.Skip(3).Concat(new byte[] { crc[0], crc[1] })))
            {
                yield return item;
            }
            yield return MSG.EOM;
        }


        private IEnumerable<byte> Clear(IEnumerable<byte> source)
        {
            foreach (var b in source)
            {
                if (b >= MSG.ESC)
                {
                    yield return MSG.ESC;
                    yield return (byte)(b - MSG.ESC);
                }
                else
                    yield return b;
            }
        }

        private static ushort FullCRC(byte[] p, int pSize)
        {
            ushort crc = 58005;
            for (int index = 0; index <= pSize - 1; ++index)
                crc = CRC16(crc, p[index]);
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
