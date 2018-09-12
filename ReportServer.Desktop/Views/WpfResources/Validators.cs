using System;
using CronExpressionDescriptor;
using FluentValidation;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.ViewModels;

namespace ReportServer.Desktop.Views.WpfResources
{
    public class TaskEditorValidator : AbstractValidator<TaskEditorViewModel>
    {
        public TaskEditorValidator()
        {
            RuleFor(ted => ted.ScheduleId)
                .NotNull()
                .WithMessage("You should select Schedule");

            RuleFor(red => red.Name)
                .Must(name => !string.IsNullOrEmpty(name))
                .WithMessage("Name cannot be empty");
        }
    }

    public class ReportEditorValidator : AbstractValidator<OperEditorViewModel>
    {
        public ReportEditorValidator()
        {
            RuleFor(red => red.QueryTimeOut)
                .Must(t => t < 1000 && t > 0)
                .WithMessage("You should set min 1 and max 1000 seconds timeout for query");

            RuleFor(red => red.Name)
                .Must(name => !string.IsNullOrEmpty(name))
                .WithMessage("Name cannot be empty");

            RuleFor(red => red.ConnectionString)
                .Must((red, connstr) => red.ReportType == ReportType.Custom ||
                                        !string.IsNullOrEmpty(connstr))
                .WithMessage("Connection string cannot be empty");

            RuleFor(red => red.ViewTemplate)
                .Must(viewTemplate => !string.IsNullOrEmpty(viewTemplate))
                .WithMessage("View template cannot be empty");

            RuleFor(red => red.Query)
                .Must(query => !string.IsNullOrEmpty(query))
                .WithMessage("Query cannot be empty");
        }
    }

    public class RecepientGroupEditorValidator : AbstractValidator<RecepientEditorViewModel>
    {
        public RecepientGroupEditorValidator()
        { 
            RuleFor(red => red.Name)
                .Must(name => !string.IsNullOrEmpty(name))
                .WithMessage("Name cannot be empty");

            RuleFor(red => red.Addresses)
                .Must(addresses => !string.IsNullOrEmpty(addresses))
                .WithMessage("This field cannot be empty");
        }
    }

    public class CronEditorValidator : AbstractValidator<CronEditorViewModel>
    {
        public CronEditorValidator()
        {
            RuleFor(red => red.Name)
                .Must(name => !string.IsNullOrEmpty(name))
                .WithMessage("Name cannot be empty");

            RuleFor(red => red.FullExpression)
                .Must(TryGetDescription)
                .WithMessage("Can not decrypt such schedule");

        }

        private bool TryGetDescription(string expr)
        {
            try
            {
                var _ = ExpressionDescriptor.GetDescription(expr);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}