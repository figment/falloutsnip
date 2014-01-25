using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TESVSnip.Domain.Services
{
    public static class Alerts
    {
        public static void Show(string text)
        {
            if (OnAlert != null)
            {
                OnAlert(null, new AlertEventArgs(){Message = text});
            }
        }
        public static void Show(string format, params object[] args)
        {
            if (OnAlert != null)
            {
                OnAlert(null, new AlertEventArgs() { Message = string.Format(format, args) });
            }
        }

        public class AlertEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public static event EventHandler<AlertEventArgs> OnAlert;
    }
}
