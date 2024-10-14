using System.Diagnostics;
using System;

namespace Kenedia.Modules.FashionManager.Models
{
    public class DataLoadProgress : IProgress<DataLoadReport>
    {
        public void Report(DataLoadReport report)
        {
            Debug.WriteLine($"{report.Message} | {report.CurrentCount} / {report.MaxCount} | {report.Percent} ");
        }
    }
}
