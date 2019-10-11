using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ReportServer.Desktop.Entities
{
    public class DesktopTaskDependence : ReactiveObject
    {
        [Reactive] public long TaskId { get; set; }
        [Reactive] public long MaxSecondsPassed { get; set; }
        public ReadOnlyObservableCollection<DesktopTaskNameId> Tasks { get; set; }
    }
}