using System;
using System.Timers;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class MailingService
    {
        private readonly Timer _updateTimer = new()
        {
            AutoReset = true,
        };

        public DateTime NextMail { get; set; }

        public event EventHandler MailReady;
        public event EventHandler TimeElapsed;

        public MailingService()
        {
            _updateTimer.Elapsed += UpdateTimerOnElapsed;
            NextMail = DateTime.MinValue;
        }

        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if(RemainingMailDelay <= 0)
            {
                MailReady?.Invoke(this, EventArgs.Empty);
                _updateTimer.Stop();
            }
            else
            {
                TimeElapsed?.Invoke(this, EventArgs.Empty);
            }
        }

        public int RemainingMailDelay => (int)(NextMail - DateTime.Now).TotalSeconds;

        public void SendMail()
        {
            _updateTimer.Start();
            NextMail = DateTime.Now.AddSeconds(6);
        }
    }
}
