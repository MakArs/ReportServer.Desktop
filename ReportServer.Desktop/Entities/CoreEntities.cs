using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ReportServer.Desktop.Entities
{
    public class DesktopTask : ReactiveObject
    {
        public long Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Operations { get; set; }
        [Reactive] public string Schedule { get; set; }
        [Reactive] public string GroupName { get; set; }
    }



    public class DesktopOperation : ReactiveObject
    {
        [Reactive] public long? Id { get; set; }
        [Reactive] public long TaskId { get; set; }
        [Reactive] public int Number { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string ImplementationType { get; set; }
        [Reactive] public bool IsDefault { get; set; }
        [Reactive] public string Config { get; set; }
    }

    public class DesktopTaskInstance
    {
        public long Id { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
    }

    public class DesktopTaskNameId
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
    
    public class DesktopOperInstance
    {
        public long Id { get; set; }
        public long OperationId { get; set; }
        public string OperName { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
        public string DataSet { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum OperMode : byte
    {
        Importer = 1,
        Exporter = 2
    }

    public enum ParsingCategory
    {
        All,
        Value,
        Range
    }

    public enum InstanceState
    {
        InProcess = 1,
        Success = 2,
        Failed = 3,
        Canceled = 4
    }

    public enum ServiceUserRole
    {
        Viewer,
        Editor,
        StopRunner, //executor?
        NoRole
    }
}