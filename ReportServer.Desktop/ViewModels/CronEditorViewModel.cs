using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CronExpressionDescriptor;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels
{
    public class CronEditorViewModel : ViewModelBase, IInitializableViewModel
    {
        private string currentExpression;
        private readonly ICachedService cachedService;
        private readonly CachedServiceShell shell;

        public int? Id { get; set; }
        [Reactive] public string FullExpression { get; set; }
        [Reactive] public string FullStringExpression { get; set; }
        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }
        [Reactive] public string Name { get; set; }

        public ReactiveList<CronCategory> Categories { get; set; }
        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }

        public CronEditorViewModel(ICachedService cachedService, IShell shell)
        {
            this.shell = shell as CachedServiceShell;
            this.cachedService = cachedService;
            Categories = new ReactiveList<CronCategory> {ChangeTrackingEnabled = true};
            IsValid = true;
            validator = new CronEditorValidator();

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty,
                isd => isd == true);

            SaveChangesCommand = ReactiveCommand.CreateFromTask(async () => await Save(),
                canSave);

            CancelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (IsDirty)
                {
                    if (!await this.shell.ShowWarningAffirmativeDialogAsync
                        ("All unsaved changes will be lost. Close window?"))
                        return;
                }

                Close();
            });


            this.WhenAnyObservable(conncr => conncr.Categories.ItemChanged)
                .Subscribe(_ => CreateCronString());

            this.WhenAnyObservable(conncr => conncr.Categories.Changed)
                .Subscribe(_ => CreateCronString());

            for (int i = 0; i < 5; i++)
                Categories.Add(new CronCategory(i));

            this.WhenAnyValue(conncr => conncr.FullExpression)
                .Where(value => !string.IsNullOrEmpty(value))
                .Subscribe(_ => UpdateCategories());

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());
        }

        public void UpdateCategories()
        {
            if (FullExpression == currentExpression)
                return;

            Categories.ChangeTrackingEnabled = false;

            var categories = new[] {"*", "*", "*", "*", "*"};

            var categorieExpressions =
                FullExpression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < categorieExpressions.Length; i++)
            {
                categories[i] = categorieExpressions[i];
            }

            for (int i = 0; i < 5; i++)
                Categories[i].FillExpressionParts(categories[i]);

            Categories.ChangeTrackingEnabled = true;

            try
            {
                FullStringExpression = ExpressionDescriptor.GetDescription(FullExpression);
            }

            catch (Exception e)
            {
                FullStringExpression = e.Message;
            }
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

        public void Initialize(ViewRequest viewRequest)
        {
            shell.AddVMCommand("File", "Save",
                    "SaveChangesCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.S);

            if (viewRequest is CronEditorRequest request)
            {
                Id = request.Schedule.Id;

                if (Id == 0)
                    Name = "New schedule";

                else
                {
                    FullExpression = request.Schedule.Schedule;
                    Name = request.Schedule.Name;
                }
            }

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                IsDirty = true;
                if (Title.Last() != '*')
                    Title += '*';
            }

            PropertyChanged += Changed;
        }

        public async Task Save()
        {
            if (!IsValid || !IsDirty) return;

            if (!await shell.ShowWarningAffirmativeDialogAsync(Id > 0
                ? "Save these schedule parameters?"
                : "Create this schedule?"))
                return;

            var editedSchedule =
                new ApiSchedule {Id = Id ?? 0, Name = Name, Schedule = FullExpression};

            cachedService.CreateOrUpdateSchedule(editedSchedule);
            Close();
            cachedService.RefreshData();
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

    public class CronCategoryPart : ReactiveObject, IDisposable
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
                    : val                     == ParsingCategory.Range ? "0-0"
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
}