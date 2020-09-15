using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ChairControl.ChairWork.Options
{
    [Serializable]
    public class FutuRiftOptions
    {
        /// <summary>
        /// Interval for sending commands, in milliseconds
        /// </summary>
        public double interval = 100;
    }
}
