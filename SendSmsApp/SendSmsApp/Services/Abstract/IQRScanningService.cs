using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSenderApp.Services.Abstract
{
    public interface IQRScanningService
    {
        Task<string> ScanAsync();
    }
}
