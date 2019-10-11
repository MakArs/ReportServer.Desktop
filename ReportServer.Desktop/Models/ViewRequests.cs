using System.Collections.Generic;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.ViewModels.Editors;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Models
{
    public class TaskEditorRequest : ViewRequest
    {
        public ApiTask Task { get; set; }
        public List<ApiOperation> TaskOpers { get; set; }
        public List<DesktopTaskDependence> DependsOn { get; set; }
    }

    public class OperEditorRequest : ViewRequest
    {
        public ApiOperTemplate Oper { get; set; }
    }

    public class CronEditorRequest : ViewRequest
    {
        public ApiSchedule Schedule { get; set; }
    }

    public class RecepientEditorRequest : ViewRequest
    {
        public ApiRecepientGroup Group { get; set; }
    }

    public class OperTemplatesListRequest : ViewRequest
    {
        public TaskEditorViewModel TaskEditor { get; set; }
    }
}