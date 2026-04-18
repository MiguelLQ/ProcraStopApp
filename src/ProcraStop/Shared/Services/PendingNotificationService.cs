using ProcraStop.Shared.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Shared.Services
{
    public static class PendingNotificationService
    {
        private static string? _message;
        private static SnackbarType _type;

        public static void Set(string message, SnackbarType type = SnackbarType.Success)
        {
            _message = message;
            _type = type;
        }

        public static (string? message, SnackbarType type) Consume()
        {
            var msg = _message;
            var type = _type;
            _message = null; // limpia después de consumir
            return (msg, type);
        }
    }
}
