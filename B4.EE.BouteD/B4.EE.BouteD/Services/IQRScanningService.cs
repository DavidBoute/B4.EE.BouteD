﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B4.EE.BouteD.Services
{
    public interface IQRScanningService
    {
        Task<string> ScanAsync();
    }
}
