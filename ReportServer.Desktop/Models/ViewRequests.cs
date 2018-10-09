using System.Collections.Generic;
using ReportServer.Desktop.Entities;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Models
{
    public class TaskEditorRequest : ViewRequest
    {
        public ApiTask Task { get; set; }
        public List<ApiTaskOper> TaskOpers { get; set; }
    }

    public class OperEditorRequest : ViewRequest
    {
        public ApiOperTemplate Oper { get; set; }
    }

    public class CronEditorRequest : ViewRequest
    {
        public ApiSchedule Schedule { get; set; }
    }
}