using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CronExpressionDescriptor;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;
using static System.Int32;

namespace ReportServer.Desktop.Models
{
    public class CronStringCreator : ViewModelBase
    {
        [Reactive] public string FullExpression { get; set; }
        [Reactive] public string FullStringExpression { get; set; }
        public ReactiveList<CronCategoryExpression> Categories { get; set; }

        public CronStringCreator()
        {
            Categories = new ReactiveList<CronCategoryExpression>();

            for (int i = 0; i < 5; i++)
                Categories.Add(new CronCategoryExpression(i));

            this.WhenAnyObservable(conncr => conncr.Categories.ItemChanged)
                .Subscribe(_ => CreateCronString());

            this.WhenAnyValue(conncr => conncr.FullExpression)
                .Where(value => !string.IsNullOrEmpty(value))
                .Subscribe(_ => UpdateCategories());
        }

        public void UpdateCategories()
        {
            Categories.ChangeTrackingEnabled = false;
            try
            {
                var categorieExpressions = FullExpression.Split(' ');

                for (int i = 0; i < 4; i++)
                    Categories[i].FillExpressionParts(categorieExpressions[i]);

                FullStringExpression = ExpressionDescriptor.GetDescription(FullExpression);
            }
            catch (Exception e)
            {
                FullStringExpression = e.Message;
            }

            Categories.ChangeTrackingEnabled = true;
        }

        public void CreateCronString()
        {
            FullExpression = string.Join(" ",
                Categories.Select(category => category.GetCategoryExpression()));

            FullStringExpression = ExpressionDescriptor.GetDescription(FullExpression);
        }
    }

    public class CronCategoryExpression : ViewModelBase
    {
        public TimeType DescriprionType { get; set; }
        public ReactiveList<CronCategoryPart> ExpressionParts { get; set; }
        [Reactive] public string StringExpression { get; set; }
        public ReactiveCommand AddCategoryCommand { get; set; }
        public ReactiveCommand<CronCategoryPart, Unit> RemoveCategoryCommand { get; set; }

        public CronCategoryExpression(int type)
        {
            ExpressionParts = new ReactiveList<CronCategoryPart>
                {new CronCategoryPart("*")};

            DescriprionType = (TimeType) type;

            AddCategoryCommand = ReactiveCommand.Create(() =>
                ExpressionParts.Add(new CronCategoryPart("")));

            RemoveCategoryCommand = ReactiveCommand.Create<CronCategoryPart>(
                parseCategory =>
                    ExpressionParts.Remove(parseCategory));

        }

        public void FillExpressionParts(string expression)
        {
            var parts = expression.Split(',').Select(part => new CronCategoryPart(part));

            ExpressionParts.PublishCollection(parts);
        }

        public string GetCategoryExpression()
        {
            var expressions = ExpressionParts.Select(part => part.GetPartxpression());

            return string.Join(",", expressions);
        }
    }

    public class CronCategoryPart
    {
        [Reactive] public ParsingCategory ParsingCategory { get; set; }
        [Reactive] public string Value { get; set; }
        [Reactive] public bool HasStep { get; set; }
        [Reactive] public int? Step { get; set; }

        public CronCategoryPart(string value)
        {
            if (!value.Contains('/'))
                Value = value;
            else
            {
                var valueparts = value.Split('/');
                Value = valueparts[0];
                HasStep = true;
                Step = Parse(valueparts[1]);
            }

            ParsingCategory = value == "*" ? ParsingCategory.All :
                value.Contains('-') ? ParsingCategory.Range : ParsingCategory.Value;
        }

        public string GetPartxpression()
        {
            return HasStep ? Value + '/' + Step : Value;
        }
    }

    public enum TimeType
    {
        Minutes = 0,
        Hours = 1,
        Days = 2,
        Month = 3,
        WeekDay = 4
    }

    public enum ParsingCategory
    {
        All = 1,
        Value = 2,
        Range = 3
    }

}