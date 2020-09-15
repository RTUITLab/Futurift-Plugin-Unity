using ChairControl.ChairWork;
using ChairControl.ChairWork.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChairControl
{
    class SimpleChairControl // : MonoBehaviour
    {

        public float pitch = 0;
        public float roll = 0;
        public int portNumber = 6;

        private FutuRiftController controller;
        void Start()
        {
            controller = new FutuRiftController(new ComPortOptions { ComPort = 6 })
            {
                Pitch = pitch,
                Roll = roll
            };
            controller.Start();
        }

        void OnApplicationQuit()
        {
            controller.Stop();
        }

    }
}
