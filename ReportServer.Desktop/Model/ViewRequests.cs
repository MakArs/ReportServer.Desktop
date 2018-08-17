using ReportServer.Desktop.Interfaces;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Model
{
    public class TaskEditorRequest : ViewRequest
    {
        public DesktopFullTask Task { get; set; }
    }
}
