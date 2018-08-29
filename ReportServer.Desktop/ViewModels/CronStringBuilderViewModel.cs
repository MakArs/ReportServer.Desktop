using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CronExpressionDescriptor;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels
{
    public class CronStringBuilderViewModel : ViewModelBase
    {
        [Reactive] public string FullExpression { get; set; }
        [Reactive] public string FullStringExpression { get; set; }
        private string currentExpression;
        public ReactiveList<CronCategory> Categories { get; set; }

        public CronStringBuilderViewModel()
        {
            Categories = new ReactiveList<CronCategory>();
            Categories.ChangeTrackingEnabled = true;


            this.WhenAnyObservable(conncr => conncr.Categories.ItemChanged)
                .Subscribe(_ => CreateCronString());

            this.WhenAnyObservable(conncr => conncr.Categories.Changed)
                .Subscribe(_ => CreateCronString());

            for (int i = 0; i < 5; i++)
                Categories.Add(new CronCategory(i));

            this.WhenAnyValue(conncr => conncr.FullExpression)
                .Where(value => !string.IsNullOrEmpty(value))
                .Subscribe(_ => UpdateCategories());
        }

        public void UpdateCategories()
        {
            if (FullExpression == currentExpression)
                return;

            Categories.ChangeTrackingEnabled = false;

                var categorieExpressions = FullExpression.Split(' ');

                for (int i = 0; i < 5; i++)
                    Categories[i].FillExpressionParts(categorieExpressions[i]);

            Categories.ChangeTrackingEnabled = true;
        }

        public void CreateCronString()
        {
            currentExpression = string.Join(" ",
                Categories.Select(category => category.GetCategoryExpression()));
            
            FullExpression = currentExpression;

            try
            {
                FullStringExpression = ExpressionDescriptor.GetDescription(FullExpression);
            }

            catch (Exception e)
            {
                FullStringExpression = e.Message;
            }
        }
    }

    public class CronCategory : ViewModelBase
    {
        public TimeType DescriprionType { get; set; }
        public ReactiveList<CronCategoryPart> ExpressionParts { get; set; }
        public ReactiveCommand AddCategoryCommand { get; set; }
        public ReactiveCommand<CronCategoryPart, Unit> RemoveCategoryCommand { get; set; }

        public CronCategory(int type)
        {
            ExpressionParts = new ReactiveList<CronCategoryPart>
                {new CronCategoryPart("*")};
            ExpressionParts.ChangeTrackingEnabled = true;

            DescriprionType = (TimeType) type;

            AddCategoryCommand = ReactiveCommand.Create(() =>
                ExpressionParts.Add(new CronCategoryPart("")));

            RemoveCategoryCommand = ReactiveCommand.Create<CronCategoryPart>(
                parseCategory =>
                    ExpressionParts.Remove(parseCategory));

            this.WhenAnyObservable(ce => ce.ExpressionParts.Changed)
                .Subscribe(_ => this.RaisePropertyChanged());

            this.WhenAnyObservable(ce => ce.ExpressionParts.ItemChanged)
                .Subscribe(_ => this.RaisePropertyChanged());
        }

        public void FillExpressionParts(string expression)
        {

            var parts = expression.Split(',').Select(part => new CronCategoryPart(part));

            foreach (var existingpart in ExpressionParts)
                existingpart.Dispose();

            ExpressionParts.PublishCollection(parts);
        }

        public string GetCategoryExpression()
        {
            var expressions = ExpressionParts.Select(part => part.GetPartxpression());

            return string.Join(",", expressions);
        }
    }

    public class CronCategoryPart : ReactiveObject,IDisposable
    {
        [Reactive] public ParsingCategory ParsingCategory { get; set; }
        private CompositeDisposable disposables;
        [Reactive] public string Value { get; set; }
        [Reactive] public bool HasStep { get; set; }
        [Reactive] public int? Step { get; set; }

        public CronCategoryPart(string obtval)
        {
            disposables = new CompositeDisposable();

            ParsingCategory = obtval.Contains("*") ? ParsingCategory.All :
                obtval.Contains('-') ? ParsingCategory.Range : ParsingCategory.Value;

            if (!obtval.Contains('/'))
                Value = obtval;
            else
            {
                var valueparts = obtval.Split('/');
                Value = valueparts[0];
                HasStep = true;
                Step = Int32.Parse(valueparts[1]);
            }

            disposables.Add(this.ObservableForProperty(cp => cp.ParsingCategory)
                .Select(x => x.Value)
                .Subscribe(val => Value = val == ParsingCategory.All ? "*"
                    : val == ParsingCategory.Range ? "0-0"
                    : ""));

            disposables.Add(this.ObservableForProperty(cp => cp.HasStep)
                .Select(x => x.Value)
                .Subscribe(val =>
                {
                    if (val)
                        Step = 1;

                    else
                        Step = null;
                }));
        }


        public string GetPartxpression()
        {
            return HasStep ? Value + '/' + Step : Value;
        }

        public void Dispose()
        {
            disposables?.Dispose();
        }
    }

    public enum TimeType
    {
        Minutes,
        Hours,
        Days,
        Month,
        WeekDay
    }

    public enum ParsingCategory
    {
        All,
        Value,
        Range
    }

}