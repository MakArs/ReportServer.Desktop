using System;
using System.ComponentModel;
using System.Drawing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.ViewModels;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

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

    public interface IOperationConfig
    {
        string DataSetName { get; set; }
    }

    public class DbExporterConfig : IOperationConfig
    {
        [Reactive] public string DataSetName { get; set; }
        [Reactive] public string ConnectionString { get; set; }
        [Reactive] public string TableName { get; set; }
        [Reactive] public int DbTimeOut { get; set; }
        [Reactive] public bool DropBefore { get; set; }
    }

    public class ReportInstanceExporterConfig : IOperationConfig
    {
        [Reactive] public string DataSetName { get; set; }
        [Reactive] public string ReportName { get; set; }
        [Reactive] public string ConnectionString { get; set; }
        [Reactive] public string TableName { get; set; }
        [Reactive] public int DbTimeOut { get; set; }
    }

    public class EmailExporterConfig : IOperationConfig
    {
        [Reactive] public string DataSetName { get; set; }
        [Reactive] public bool HasHtmlBody { get; set; }
        [Reactive] public bool HasJsonAttachment { get; set; }
        [Reactive] public bool HasXlsxAttachment { get; set; }

        [ItemsSource(typeof(BadGates))]
        [Description("Select recepient group for exporter")]
        [Reactive] public int RecepientGroupId { get; set; }
        [Reactive] public string ViewTemplate { get; set; }
        [Reactive] public string ReportName { get; set; }
    }

    public class TelegramExporterConfig : IOperationConfig
    {
        [Reactive] public string DataSetName { get; set; }
        [Reactive] public int TelegramChannelId { get; set; }
        [Reactive] public string ReportName { get; set; }
    }

    public class ExcelImporterConfig : IOperationConfig
    {
        [Reactive] public string DataSetName { get; set; }
        [Reactive] public string FilePath { get; set; }
        [Reactive] public string ScheetName { get; set; }
        [Reactive] public bool SkipEmptyRows { get; set; }
        [Reactive] public string[] ColumnList { get; set; }
        [Reactive] public bool UseColumnNames { get; set; }
        [Reactive] public int FirstDataRow { get; set; }
        [Reactive] public int MaxRowCount { get; set; }
    }

    public class DbImporterConfig : IOperationConfig
    {
        [Reactive] public string DataSetName { get; set; }
        [Reactive] public string ConnectionString { get; set; }
        [Reactive] public string Query { get; set; }
        [Reactive] public int TimeOut { get; set; }
    }

    public enum OperType : byte
    {
        Importer = 1,
        Exporter = 2
    }

    public enum InstanceState 
    {
        InProcess = 1,
        Success = 2,
        Failed = 3
    }
}
