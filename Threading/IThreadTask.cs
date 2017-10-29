﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLib.Threading
{
    public interface IThreadTask
    {
        void RunTask();
        void Start();
        void Stop(bool force);
    }
}
