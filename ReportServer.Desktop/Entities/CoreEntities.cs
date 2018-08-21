using System;
using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ReportServer.Desktop.Entities
{
    public class DesktopFullTask : ReactiveObject
    {
        public int Id { get; set; }
        public int TelegramChannelId { get; set; }
        [Reactive] public int ReportId { get; set; }
        [Reactive] public string ReportName { get; set; }
        [Reactive] public int ScheduleId { get; set; }
        [Reactive] public int RecepientGroupId { get; set; }
        [Reactive] public string Schedule { get; set; }
        [Reactive] public string ConnectionString { get; set; }
        [Reactive] public string RecepientGroup { get; set; }
        [Reactive] public string ViewTemplate { get; set; }
        [Reactive] public string Query { get; set; }
        [Reactive] public int TryCount { get; set; }
        [Reactive] public int QueryTimeOut { get; set; }
        [Reactive] public ReportType ReportType { get; set; }
        [Reactive] public bool HasHtmlBody { get; set; }
        [Reactive] public bool HasJsonAttachment { get; set; }
        [Reactive] public bool HasXlsxAttachment { get; set; }
    }

    public class DesktopInstanceCompact
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
        public int TryNumber { get; set; }
    }

    public class DesktopInstance
    {
        public int Id { get; set; }
        public string Data { get; set; } = "";
        public string ViewData { get; set; } = "";
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
        public int TryNumber { get; set; }
    }

    [DataContract]
    public class DesktopReport : ReactiveObject
    {
        [DataMember] public int Id { get; set; }
        [Reactive] [DataMember] public string Name { get; set; }
        [Reactive] [DataMember] public string ConnectionString { get; set; }
        [Reactive] [DataMember] public string ViewTemplate { get; set; }
        [Reactive] [DataMember] public string Query { get; set; }
        [Reactive] [DataMember] public ReportType ReportType { get; set; }
        [Reactive] [DataMember] public int QueryTimeOut { get; set; } //seconds
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
