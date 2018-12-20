using System;
using System.ComponentModel;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Views.WpfResources;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ReportServer.Desktop.Entities
{
    public interface IOperationConfig
    {
    }

    public class DesktopTask : ReactiveObject
    {
        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Operations { get; set; }
        [Reactive] public string Schedule { get; set; }
    }

    public class DesktopOperation : ReactiveObject
    {
        [Reactive] public int? Id { get; set; }
        [Reactive] public int TaskId { get; set; }
        [Reactive] public int Number { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string ImplementationType { get; set; }
        [Reactive] public bool IsDefault { get; set; }
        [Reactive] public string Config { get; set; }
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
        public int OperationId { get; set; }
        public string OperName { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
        public string DataSet { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DbExporterConfig : IOperationConfig
    {
        [DisplayName("Package name")]
        [Description("Package which exporter needs for work")]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [DisplayName("Create table if not exists")]
        [Reactive]
        public bool CreateTable { get; set; }

        [DisplayName("Connection string")]
        [Reactive]
        public string ConnectionString { get; set; }

        [DisplayName("Table name")] [Reactive] public string TableName { get; set; }

        [DisplayName("Database operation timeout")]
        [DefaultValue(60)]
        [Reactive]
        public int DbTimeOut { get; set; }

        [DisplayName("Clean table before run")]
        [Description("Set if needed clearance before export")]
        [Reactive]
        public bool DropBefore { get; set; }
    }

    public class B2BExporterConfig : IOperationConfig
    {
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [DisplayName("Report name")]
        [Reactive]
        public string ReportName { get; set; }

        [Reactive] public string Description { get; set; }

        [DisplayName("Connection string")]
        [Reactive]
        public string ConnectionString { get; set; }

        [DisplayName("Export table name")]
        [Reactive]
        public string ExportTableName { get; set; }

        [DisplayName("Export instance table name")]
        [Reactive]
        public string ExportInstanceTableName { get; set; }

        [DisplayName("Database operation timeout")]
        [DefaultValue(60)]
        [Reactive]
        public int DbTimeOut { get; set; }
    }


    public class EmailExporterConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Html body")]
        [Reactive]
        public bool HasHtmlBody { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Xlsx attachement")]
        [Reactive]
        public bool HasXlsxAttachment { get; set; }

        [DisplayName("Json attachement")]
        [Reactive]
        public bool HasJsonAttachment { get; set; }

        [ItemsSource(typeof(RecepGroupsSource))]
        [DisplayName("Recepient group")]
        [Reactive]
        public int RecepientGroupId { get; set; }

        [DisplayName("Recepients package")]
        [Description("Package must contain dataset consisting of with'RecType' column with value" +
                     " 'To' or 'Bcc' and 'Address' column with recepient address")]
        [Reactive]
        public string RecepientsDatasetName { get; set; }

        [DisplayName("View template")]
        [Editor(typeof(MultilineTextBoxEditor), typeof(MultilineTextBoxEditor))]
        [Reactive]
        public string ViewTemplate { get; set; }

        [DisplayName("Report name")]
        [Description("Will be displayed in messages")]
        [Reactive]
        public string ReportName { get; set; }
    }

    public class TelegramExporterConfig : IOperationConfig
    {
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [ItemsSource(typeof(TelegramChannelsSource))]
        [DisplayName("Telegram channel")]
        [Reactive]
        public int TelegramChannelId { get; set; }

        [DisplayName("Report name")]
        [Description("Will be displayed in messages")]
        [Reactive]
        public string ReportName { get; set; }
    }

    public class ExcelImporterConfig : IOperationConfig
    {
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Path to file")]
        [Reactive]
        public string FilePath { get; set; }

        [DisplayName("Name of sheet in file")]
        [Reactive]
        public string ScheetName { get; set; }

        [DisplayName("Skip empty rows")]
        [Description("Set if need auto skipping empty rows in table")]
        [Reactive]
        public bool SkipEmptyRows { get; set; }

        [DisplayName("Using columns")]
        [Description("Set here names of columns which will be used")]
        public ObservableCollectionExtended<string> ColumnList { get; set; }

        [DisplayName("First data row")]
        [Description("First row in selected columns that will be read")]
        [Reactive]
        public int FirstDataRow { get; set; }

        [DisplayName("Use column names")]
        [Description("First data row will be used as column names if set")]
        [Reactive]
        public bool UseColumnNames { get; set; }

        [DisplayName("Max row count")]
        [Description("Max count of rows that will be read")]
        [Reactive]
        public int MaxRowCount { get; set; }
    }

    public class DbImporterConfig : IOperationConfig
    {
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Connection string")]
        [Reactive]
        public string ConnectionString { get; set; }

        [DisplayName("Query")]
        [Editor(typeof(MultilineTextBoxEditor), typeof(MultilineTextBoxEditor))]
        [Reactive]
        public string Query { get; set; }

        [DisplayName("Database operation timeout")]
        [DefaultValue(60)]
        [Reactive]
        public int TimeOut { get; set; }
    }

    public class CsvImporterConfig : IOperationConfig
    {
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("File path")]
        [Reactive]
        public string FilePath { get; set; }

        public int MaxRowCount;
    }

    public class CustomDbImporterConfig : IOperationConfig
    {
    }

    public class CustomEmailSenderConfig : IOperationConfig
    {
        [PropertyOrder(1)]
        [DisplayName("Html body")]
        [Reactive]
        public bool HasHtmlBody { get; set; }


        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Xlsx attachement")]
        [Reactive]
        public bool HasXlsxAttachment { get; set; }

        [DisplayName("Json attachement")]
        [Reactive]
        public bool HasJsonAttachment { get; set; }

        [DisplayName("Recepients package")]
        [Description("Package must contain dataset consisting of with'RecType' column with value" +
                     " 'To' or 'Bcc' and 'Address' column with recepient address")]
        [Reactive]
        public string RecepientsDatasetName { get; set; }

        [ItemsSource(typeof(RecepGroupsSource))]
        [DisplayName("Recepient group")]
        [Reactive]
        public int RecepientGroupId { get; set; }
    }

    public class CustomTelegramExporterConfig : IOperationConfig
    {
        [ItemsSource(typeof(TelegramChannelsSource))]
        [DisplayName("Telegram channel")]
        [Reactive]
        public int TelegramChannelId { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }
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
}