using System;

namespace Kenedia.Modules.FashionManager.Models
{
    public class DataLoadReport
    {
        public DataLoadReport(string message, int currentCount, int maxCount, double percent)
        {
            Message = message;
            Percent = Math.Round(percent, 4);
            MaxCount = maxCount;
            CurrentCount = currentCount;
        }

        public string Message { get; }

        public double Percent { get; }

        public int MaxCount { get; }

        public int CurrentCount { get; }
    }
}
