using System;
using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ReportServer.Desktop.Entities
{
    public class DesktopTask : ReactiveObject
    {
        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Operations { get; set; }
        [Reactive] public string Schedule { get; set; }
    }

    public class DesktopTaskOper : ReactiveObject
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int TaskId { get; set; }
        public int? OperId { get; set; }
    }

    [DataContract]
    public class DesktopOper : ReactiveObject
    {
        [DataMember] public int Id { get; set; }
        [Reactive] [DataMember] public string Name { get; set; }
        [Reactive] [DataMember] public string ConnectionString { get; set; }
        [Reactive] [DataMember] public string ViewTemplate { get; set; }
        [Reactive] [DataMember] public string Query { get; set; }
        [Reactive] [DataMember] public ReportType ReportType { get; set; }
        [Reactive] [DataMember] public int QueryTimeOut { get; set; } //seconds
    }

    public class DesktopTaskInstance
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
    }

    public class DesktopOperInstance
    {
        public int Id { get; set; }
        public int OperId { get; set; }
        public string OperName { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
        public string DataSet { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum ReportType : byte
    {
        Common = 1,
        Custom = 2
    }

    public enum InstanceState 
    {
        InProcess = 1,
        Success = 2,
        Failed = 3
    }
}
