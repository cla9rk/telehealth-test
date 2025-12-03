using System;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Models
{
    public class SingleSignOnAlert
    {
        public Guid AlertId { get; set; }
        public string MessageText { get; set; }
        public DateTime ActiveStartTime { get; set; } // Must have a start time
        public DateTime? ActiveEndTime { get; set; } // Optional end time, if the alert should be open until the user acknowledges it
        public AlertType AlertType { get; set; }
    }

    public enum AlertType
    {
        Info,
        Warning,
        Critical
    }
}
