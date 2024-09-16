using System;

namespace Kenedia.Modules.Core.Models
{
    public class ConditionalNotification
    {
        public ConditionalNotification()
        {

        }

        public ConditionalNotification(string notificationText, Func<bool> condition)
        {
            NotificationText = notificationText;
            Condition = condition;
        }

        public ConditionalNotification(Func<string> notificationText, Func<bool> condition)
        {
            SetLocalizedNotificationText = notificationText;
            Condition = condition;
        }

        public event EventHandler ConditionMatched;

        public bool IsConditionMatched { get; private set; } = false;

        public Func<bool> Condition { get; set; }

        public Func<string> SetLocalizedNotificationText { get; set; }

        public string NotificationText { get => SetLocalizedNotificationText?.Invoke(); set => SetLocalizedNotificationText = () => value; }

        public void CheckCondition()
        {
            if (Condition?.Invoke() ?? false)
            {
                IsConditionMatched = true;
                ConditionMatched?.Invoke(this, new EventArgs());
            }
        }
    }
}
