using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Plugins.UnityChairPlugin.ChairControl.ChairWork
{
    internal interface IDataSender
    {
        void SendData(byte[] data);
        void Start();
        void Stop();
    }
}
