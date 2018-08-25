using FluentValidation;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.ViewModel;

namespace ReportServer.Desktop.Views.WpfResources
{
    public class TaskEditorValidator : AbstractValidator<TaskEditorViewModel>
    {
        public TaskEditorValidator()
        {
            RuleFor(ted => ted.TryCount)
                .Must(t => t < 101 && t > 0)
                .WithMessage("You should set min 1 and max 100 tries for task");

            RuleFor(ted => ted.SelectedReport)
                .NotNull()
                .WithMessage("You should select report");

            RuleFor(ted => ted.RecepientGroupId)
                .NotNull()
                .WithMessage("You should select RecepientGroup");

            RuleFor(ted => ted.ScheduleId)
                .NotNull()
                .WithMessage("You should select Schedule");
        }
    }

    public class ReportEditorValidator : AbstractValidator<ReportEditorViewModel>
    {
        public ReportEditorValidator()
        {
            RuleFor(red => red.QueryTimeOut)
                .Must(t => t < 300 && t > 0)
                .WithMessage("You should set min 1 and max 300 seconds timeout for query");

            RuleFor(red => red.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage("This field cannot be empty");

            RuleFor(red => red.ConnectionString)
                .Must((red, connstr) => red.ReportType == ReportType.Custom ||
                                        !string.IsNullOrEmpty(connstr))
                .WithMessage("This field cannot be empty");

            RuleFor(red => red.ViewTemplate)
                .NotNull()
                .WithMessage("This field cannot be empty");

            RuleFor(red => red.Query)
                .NotNull()
                .WithMessage("This field cannot be empty");
        }
    }
}