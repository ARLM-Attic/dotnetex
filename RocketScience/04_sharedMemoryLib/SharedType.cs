﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _04_sharedMemoryLib
{
    public class SharedType
    {
        private int x;

        public int GetX()
        {
            return x;
        }

        public void SetX(int val)
        {
            x = val;
        }
    }
}
