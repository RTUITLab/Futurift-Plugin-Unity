using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChairControl.ChairWork
{
    internal interface IDataSender
    {
        void SendData(byte[] data);
        void Start();
        void Stop();
    }
}
