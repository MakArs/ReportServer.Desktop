using ReportServer.Desktop.Entities;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Models
{
    public class TaskEditorRequest : ViewRequest
    {
        public DesktopFullTask Task { get; set; }
        public string FullId { get; set; }
    }

    public class ReportEditorRequest : ViewRequest
    {
        public DesktopReport Report { get; set; }
        public string FullId { get; set; }
    }

    public class CronEditorRequest : ViewRequest
    {
        public ApiSchedule Schedule { get; set; }
        public string FullId { get; set; }
    }
}