using FluentValidation;
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
            RuleFor(ted => ted.QueryTimeOut)
                .Must(t => t < 300 && t > 0)
                .WithMessage("You should set min 1 and max 300 seconds timeout for query");

            RuleFor(ted => ted.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage("This field cannot be empty");

            RuleFor(ted => ted.ViewTemplate)
                .NotNull()
                .NotEmpty()
                .WithMessage("This field cannot be empty");

            RuleFor(ted => ted.Query)
                .NotNull()
                .NotEmpty()
                .WithMessage("This field cannot be empty");
        }
    }
}