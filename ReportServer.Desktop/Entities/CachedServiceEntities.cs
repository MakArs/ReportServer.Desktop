using System;
using System.ComponentModel;
using System.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Entities
{
    public class TaskParameter : ViewModelBase, IDataErrorInfo
    {
        [Reactive] public string Name { get; set; }
        public object Value { get; set; }
        [Reactive] public bool IsDuplicate { get; set; }
        public bool HasErrors { get; set; }

        public TaskParameter()
        {
            validator = new TaskParameterValidator();
        }

        public new string Error
        {
            get
            {
                var results = validator?.Validate(this);

                if (results != null && results.Errors.Any())
                {
                    var errors = string.Join(Environment.NewLine,
                        results.Errors.Select(x => x.ErrorMessage).ToArray());
                    return errors;
                }

                return string.Empty;
            }
        }

        public new string this[string columnName]
        {
            get
            {
                if (columnName == "Value")
                    return "";

                var errs = validator?
                    .Validate(this).Errors;

                HasErrors = errs?.Any() ?? false;

                if (errs != null)
                    return validator != null
                        ? string.Join("; ", errs.Select(e => e.ErrorMessage))
                        : "";
                return "";
            }
        }
    }

    public class ApiOperTemplate
    {
        public int Id { get; set; }
        public string ImplementationType { get; set; }
        public string Name { get; set; }
        public string ConfigTemplate { get; set; }
    }

    public class ApiRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }

    public class ApiTelegramChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long ChatId { get; set; }
        public int Type { get; set; }
    }

    public class ApiSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class ApiTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public int? ScheduleId { get; set; }
        public ApiOperation[] BindedOpers { get; set; }
    }

    public class ApiOperation
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string ImplementationType { get; set; }
        public bool IsDefault { get; set; }
        public string Config { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ApiTaskInstance
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
    }

    public class ApiOperInstance
    {
        public int Id { get; set; }
        public int TaskInstanceId { get; set; }
        public int OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public byte[] DataSet { get; set; }
        public string ErrorMessage { get; set; }
        public string OperName { get; set; }
    }
}
