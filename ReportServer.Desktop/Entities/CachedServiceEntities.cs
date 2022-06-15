using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Entities
{
    public class TaskParameter : ViewModelBase, IDataErrorInfo
    {
        [Reactive] public string Name { get; set; }
        [Reactive] public object Value { get; set; }
        [Reactive] public bool IsDuplicate { get; set; }
        [Reactive] public bool HasErrors { get; set; }

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

    public class TaskParameterInfos
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public Validation Validation { get; set; }
    }

    public class Validation
    {
        public  List<ValidationRule> ValidationRules { get; set; }
    }
    
    public class ValidationRule
    {
        public class ValidationRuleConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                var jObject = JObject.Load(reader);
                if (objectType == typeof(ValidationRule))
                {
                    var obj = jObject.ToObject<DateRangeValidationRule>(serializer);
                    if (obj.ValidationRuleName.Equals("DateRangeValidationRule"))
                    {
                        return obj;
                    }
                }
                throw new NotImplementedException();
            }
    
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ValidationRule);
            }
        }
    }
    
    public class DateRangeValidationRule : ValidationRule
    {
        public string ValidationRuleName { get; set; }
        public string LinkedParameterName { get; set; }
        public int MaxDays { get; set; }
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
        public long Id { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string ParameterInfos { get; set; }
        public ApiTaskDependence[] DependsOn { get; set; }
        public int? ScheduleId { get; set; }
        public ApiOperation[] BindedOpers { get; set; }
    }

    public class ApiOperation
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string ImplementationType { get; set; }
        public bool IsDefault { get; set; }
        public string Config { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ApiTaskInstance
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
    }

    public class ApiOperInstance
    {
        public long Id { get; set; }
        public long TaskInstanceId { get; set; }
        public long OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public byte[] DataSet { get; set; }
        public string ErrorMessage { get; set; }
    }
}
