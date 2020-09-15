using ChairControl.ChairWork;
using ChairControl.ChairWork.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
namespace ChairControl.ChairWork.DataSenders
{
    internal class UdpPortSender : IDataSender
    {
        private readonly UdpClient udpClient;
        private readonly IPEndPoint endPoint;
        public UdpPortSender(UdpOptions options)
        {
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse(options.ip), options.port);
        }
        public void SendData(byte[] data)
        {
            udpClient.Send(data, data.Length, endPoint);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
