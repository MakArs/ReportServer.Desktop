using FluentValidation;
using ReportServer.Desktop.ViewModel;

namespace ReportServer.Desktop.Views.WpfResources
{
   public class TaskEditorValidator: AbstractValidator<TaskEditorViewModel>
    {
        public TaskEditorValidator()
        {
            RuleFor(ted => ted.TryCount)
                .Must(t => t < 101)
                .WithMessage("You can set max 100 tries for task");
        }
    }

    public class ReportEditorValidator : AbstractValidator<ReportEditorViewModel>
    {
        public ReportEditorValidator()
        {
            RuleFor(ted => ted.QueryTimeOut)
                .Must(t => t < 300)
                .WithMessage("You can set max 300 seconds timeout for query");
        }
    }
}
