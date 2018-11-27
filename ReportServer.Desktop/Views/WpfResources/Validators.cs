using System;
using System.Linq;
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
            RuleFor(red => red.Name)
                .Must(name => !string.IsNullOrEmpty(name))
                .WithMessage("Name cannot be empty");

            RuleFor(red => red.TaskParameters)
                .Must(pairs => pairs
                    .Items.All(pair => pair.Name.StartsWith("@RepPar")))
                .WithMessage("Parameter name must starting with @RepPar");
        }
    }

    public class TaskParameterValidator : AbstractValidator<TaskParameter>
    {
        public TaskParameterValidator()
        {
            RuleFor(par => par.IsDuplicate)
                .Must(isd => isd == false)
                .WithMessage("Task can not contain parameters with same names");

            RuleFor(par => par.Name)
                .Must(name => name.StartsWith("@RepPar"))
                .WithMessage("Parameter name must starting with @RepPar");
        }
    }

    public class OperEditorValidator : AbstractValidator<OperEditorViewModel>
    {
        public OperEditorValidator()
        {
            RuleFor(red => red.Name)
                .Must(name => !string.IsNullOrEmpty(name))
                .WithMessage("Name cannot be empty");
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