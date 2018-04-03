using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChairControl.ChairWork
{
    enum Flag : byte
    {
        NextBlock = 0,
        FirstBlock = 1,
        LastBlock = 2,
        OneBlock = 3,
        ErrBlock = 4,
    }
}
