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
}
