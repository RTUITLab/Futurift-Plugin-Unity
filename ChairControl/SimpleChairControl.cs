using ChairControl.ChairWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChairControl
{
    class SimpleChairControl : MonoBehaviour
    {

        public float pitch = 0;
        public float roll = 0;


        private FutuRiftController controller;
        void Start()
        {
            controller = FutuRiftController.Default;
            controller.Pitch = pitch;
            controller.Roll = roll;
            controller.Start();
        }

        void OnApplicationQuit()
        {
            controller.Stop();
        }

    }
}
