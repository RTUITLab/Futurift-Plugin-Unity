using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChairControl.Extensions
{
    static class IComparableExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }
    }
}
