using System.Collections.ObjectModel;
using System.ComponentModel;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Views.WpfResources;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ReportServer.Desktop.Entities
{
    public interface IOperationConfig
    {
    }

    public interface IPackagedImporterConfig
    {
        string PackageName { get; set; }
    }

    public class DbExporterConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Description("Package which exporter needs for work")]
        [Editor(typeof(IncomingPackagesControl), typeof(IncomingPackagesControl))]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [DisplayName("Create table if not exists")]
        [Reactive]
        public bool CreateTable { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Connection string")]
        [Reactive]
        public string ConnectionString { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Table name")]
        [Reactive]
        public string TableName { get; set; }

        [PropertyOrder(3)]
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
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Editor(typeof(IncomingPackagesControl), typeof(IncomingPackagesControl))]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [PropertyOrder(5)]
        [DisplayName("Report name")]
        [Reactive]
        public string ReportName { get; set; }

        [Reactive] public string Description { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Connection string")]
        [Reactive]
        public string ConnectionString { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Export table name")]
        [Reactive]
        public string ExportTableName { get; set; }

        [PropertyOrder(3)]
        [DisplayName("Export instance table name")]
        [Reactive]
        public string ExportInstanceTableName { get; set; }

        [PropertyOrder(4)]
        [DisplayName("Database operation timeout")]
        [DefaultValue(60)]
        [Reactive]
        public int DbTimeOut { get; set; }
    }


    public class EmailExporterConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Editor(typeof(IncomingPackagesControl), typeof(IncomingPackagesControl))]
        [Reactive]
        public string PackageName { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Display execution date in name")]
        [Reactive]
        public bool DateInName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [PropertyOrder(3)]
        [DisplayName("Html body")]
        [Reactive]
        public bool HasHtmlBody { get; set; }

        [PropertyOrder(4)]
        [DisplayName("Xlsx attachement")]
        [Reactive]
        public bool HasXlsxAttachment { get; set; }

        [PropertyOrder(5)]
        [DisplayName("Use all package data sets")]
        [Reactive]
        public bool UseAllSetsXlsx { get; set; }

        [PropertyOrder(6)]
        [DisplayName("Json attachement")]
        [Reactive]
        public bool HasJsonAttachment { get; set; }

        [PropertyOrder(7)]
        [DisplayName("Use all package data sets")]
        [Reactive]
        public bool UseAllSetsJson { get; set; }

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

        [PropertyOrder(1)]
        [DisplayName("Report name")]
        [Description("Will be displayed in messages")]
        [Reactive]
        public string ReportName { get; set; }
    }

    public class TelegramExporterConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Editor(typeof(IncomingPackagesControl), typeof(IncomingPackagesControl))]
        [Reactive]
        public string PackageName { get; set; }

        [DisplayName("Use all package data sets")]
        [Reactive]
        public bool UseAllSets { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [ItemsSource(typeof(TelegramChannelsSource))]
        [DisplayName("Telegram channel")]
        [Reactive]
        public int TelegramChannelId { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Report name")]
        [Description("Will be displayed in messages")]
        [Reactive]
        public string ReportName { get; set; }
    }

    public class SshExporterConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Editor(typeof(IncomingPackagesControl), typeof(IncomingPackagesControl))]
        [Reactive]
        public string PackageName { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Display execution date in names of created files")]
        [Reactive]
        public bool DateInName { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Path to file folder")]
        [Reactive]
        [Editor(typeof(PathEditor), typeof(PathEditor))]
        public string SourceFileFolder { get; set; }

        [PropertyOrder(3)]
        [DisplayName("File name")]
        [Reactive]
        public string FileName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [PropertyOrder(4)]
        [DisplayName("Save package as xlsx file")]
        [Reactive]
        public bool ConvertPackageToXlsx { get; set; }

        [PropertyOrder(5)]
        [DisplayName("Save package as json file")]
        [Reactive]
        public bool ConvertPackageToJson { get; set; }

        [PropertyOrder(6)]
        [DisplayName("Save package as csv file")]
        [Reactive]
        public bool ConvertPackageToCsv { get; set; }

        [PropertyOrder(7)]
        [DisplayName("Use all package data sets")]
        [Reactive]
        public bool UseAllSets { get; set; }

        [PropertyOrder(8)]
        [DisplayName("Package file name")]
        [Description("Will be displayed for files maked from package")]
        [Reactive]
        public string PackageRename { get; set; }

        [DisplayName("Server host")]
        [Reactive]
        public string Host { get; set; }

        [DisplayName("Server user login")]
        [Reactive]
        public string Login { get; set; }

        [DisplayName("Server user password")]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Reactive]
        public string Password { get; set; }

        [DisplayName("Folder at server")]
        [Reactive]
        public string FolderPath { get; set; }

        [DisplayName("Clear interval")]
        [Description("Delete all files older than this number in days on execute")]
        [Reactive]
        [Editor(typeof(ClearIntervalEditor), typeof(ClearIntervalEditor))]
        public int ClearInterval { get; set; }
    }


    public class FtpExporterConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Editor(typeof(IncomingPackagesControl), typeof(IncomingPackagesControl))]
        [Reactive]
        public string PackageName { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Display execution date in names of created files")]
        [Reactive]
        public bool DateInName { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Path to file folder")]
        [Reactive]
        [Editor(typeof(PathEditor), typeof(PathEditor))]
        public string SourceFileFolder { get; set; }

        [PropertyOrder(3)]
        [DisplayName("File name")]
        [Reactive]
        public string FileName { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [PropertyOrder(4)]
        [DisplayName("Save package as xlsx file")]
        [Reactive]
        public bool ConvertPackageToXlsx { get; set; }

        [PropertyOrder(5)]
        [DisplayName("Save package as json file")]
        [Reactive]
        public bool ConvertPackageToJson { get; set; }

        [PropertyOrder(6)]
        [DisplayName("Save package as csv file")]
        [Reactive]
        public bool ConvertPackageToCsv { get; set; }

        [PropertyOrder(7)]
        [DisplayName("Use all package data sets")]
        [Reactive]
        public bool UseAllSets { get; set; }

        [PropertyOrder(8)]
        [DisplayName("Package file name")]
        [Description("Will be displayed for files maked from package")]
        [Reactive]
        public string PackageRename { get; set; }

        [DisplayName("Server host")]
        [Reactive]
        public string Host { get; set; }

        [DisplayName("Server user login")]
        [Reactive]
        public string Login { get; set; }

        [DisplayName("Server user password")]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Reactive]
        public string Password { get; set; }

        [DisplayName("Folder at server")]
        [Reactive]
        public string FolderPath { get; set; }

        [DisplayName("Clear interval")]
        [Description("Delete all files older than this number in days on execute")]
        [Reactive]
        [Editor(typeof(ClearIntervalEditor), typeof(ClearIntervalEditor))]
        public int ClearInterval { get; set; }
    }


    public class ExcelImporterConfig : IOperationConfig, IPackagedImporterConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Path to file folder")]
        [Reactive]
        [Editor(typeof(PathEditor), typeof(PathEditor))]
        public string FileFolder { get; set; }

        [PropertyOrder(2)]
        [DisplayName("File name")]
        [Reactive]
        public string FileName { get; set; }

        [PropertyOrder(3)]
        [Description("List of semi-colon separated column numbers on which it is needed to group data")]
        [DisplayName("Group columns list")]
        [Reactive]
        public string GroupNumbers { get; set; }

        [DisplayName("Name of sheet in file")]
        [Reactive]
        public string ScheetName { get; set; }

        [DisplayName("Skip empty rows")]
        [Description("Set if need auto skipping empty rows in table")]
        [Reactive]
        public bool SkipEmptyRows { get; set; }

        [DisplayName("Using columns")]
        [Description("Set here names of columns which will be used")]
        public ObservableCollection<string> ColumnList { get; set; }

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

        [DisplayName("Send error if package is empty")]
        [Reactive]
        public bool SendVoidPackageError { get; set; }
    }

    public class SshImporterConfig : IOperationConfig
    {
        [DisplayName("Server host")]
        [Reactive]
        public string Host { get; set; }

        [DisplayName("Server user login")]
        [Reactive]
        public string Login { get; set; }

        [DisplayName("Server user password")]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Reactive]
        public string Password { get; set; }

        [DisplayName("File path at server")]
        [Reactive]
        public string FilePath { get; set; }
    }

    public class EmailImporterConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Server host")]
        [Reactive]
        public string ServerHost { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Server port")]
        [Reactive]
        public int Port { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Login")]
        [Reactive]
        public string Email { get; set; }

        [PropertyOrder(3)]
        [DisplayName("Password for this login")]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Reactive]
        public string Password { get; set; }

        [PropertyOrder(4)]
        [DisplayName("Sender email")]
        [Reactive]
        public string SenderEmail { get; set; }

        [PropertyOrder(5)]
        [DisplayName("Attachment name")]
        [Reactive]
        public string AttachmentName { get; set; }

        [Description("Number of days to search for emails, by default 0 (only execution day)")]
        [DisplayName("History check depth")]
        [Reactive]
        public int SearchDays { get; set; }
    }

    public class DbImporterConfig : IOperationConfig, IPackagedImporterConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [PropertyOrder(1)]
        [Description("List of semi-colon separated dataset names for query results " +
                     "(successively assigned to the resulting sets)")]
        [DisplayName("Dataset names list")]
        [Reactive]
        public string DataSetNames { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Connection string")]
        [Reactive]
        public string ConnectionString { get; set; }

        [PropertyOrder(3)]
        [DisplayName("Query")]
        [Editor(typeof(MultilineTextBoxEditor), typeof(MultilineTextBoxEditor))]
        [Reactive]
        public string Query { get; set; }

        [PropertyOrder(4)]
        [Description("List of semi-colon separated column numbers on which it is needed to group data")]
        [DisplayName("Group columns list")]
        [Reactive]
        public string GroupNumbers { get; set; }

        [PropertyOrder(5)]
        [DisplayName("Database operation timeout")]
        [DefaultValue(60)]
        [Reactive]
        public int TimeOut { get; set; }

        [DisplayName("Send error if package is empty")]
        [Reactive]
        public bool SendVoidPackageError { get; set; }
    }

    public class CsvImporterConfig : IOperationConfig, IPackagedImporterConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Package name")]
        [Reactive]
        public string PackageName { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Dataset name")]
        [Reactive]
        public string DataSetName { get; set; }

        [PropertyOrder(2)]
        [DisplayName("Path to file folder")]
        [Reactive]
        [Editor(typeof(PathEditor), typeof(PathEditor))]
        public string FileFolder { get; set; }

        [PropertyOrder(3)]
        [DisplayName("File name")]
        [Reactive]
        public string FileName { get; set; }

        [PropertyOrder(4)]
        [Description("List of semi-colon separated column numbers on which it is needed to group data")]
        [DisplayName("Group columns list")]
        [Reactive]
        public string GroupNumbers { get; set; }

        [ItemsSource(typeof(DelimitersSource))]
        [Reactive]
        public string Delimiter { get; set; }

        [DisplayName("Send error if package is empty")]
        [Reactive]
        public bool SendVoidPackageError { get; set; }
    }

    public class CustomDbImporterConfig : IOperationConfig
    {
    }

    public class CustomEmailSenderConfig : IOperationConfig
    {
        [PropertyOrder(0)]
        [DisplayName("Html body")]
        [Reactive]
        public bool HasHtmlBody { get; set; }

        [DisplayName("Run if data package is void")]
        [Reactive]
        public bool RunIfVoidPackage { get; set; }

        [PropertyOrder(1)]
        [DisplayName("Xlsx attachement")]
        [Reactive]
        public bool HasXlsxAttachment { get; set; }

        [PropertyOrder(2)]
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
}
