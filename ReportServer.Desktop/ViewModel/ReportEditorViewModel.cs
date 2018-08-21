using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class ReportEditorViewModel : ViewModelBase, IInitializableViewModel, ISaveableViewModel
    {
        private readonly IDialogCoordinator dialogCoordinator = DialogCoordinator.Instance;
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;

        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string ConnectionString { get; set; }
        [Reactive] public string ViewTemplate { get; set; }
        [Reactive] public string Query { get; set; }
        [Reactive] public ReportType ReportType { get; set; }
        [Reactive] public int QueryTimeOut { get; set; } //seconds

        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }
        [Reactive] public bool QueryRedactorOpened { get; set; }
        [Reactive] public bool ViewTemplateRedactorOpened { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }
        public ReactiveCommand OpenQueryTemplateWindowCommand { get; set; }
        public ReactiveCommand OpenViewTemplateWindowCommand { get; set; }

        public ReportEditorViewModel(ICachedService cachedService, IMapper mapper)
        {
            this.cachedService = cachedService;
            this.mapper = mapper;
            IsValid = true;
            validator = new ReportEditorValidator();

            OpenQueryTemplateWindowCommand = ReactiveCommand.Create(() =>
                QueryRedactorOpened = true);

            OpenViewTemplateWindowCommand = ReactiveCommand.Create(() =>
                ViewTemplateRedactorOpened = true);

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty,
                isd => isd == true);

            SaveChangesCommand = ReactiveCommand.CreateFromTask(async () => await Save(),
                canSave);

            CancelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (IsDirty)
                {
                    var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                        "Все несохранённые изменения пропадут. Действительно закрыть окно редактирования?"
                        , MessageDialogStyle.AffirmativeAndNegative);

                    if (dialogResult != MessageDialogResult.Affirmative)
                        return;
                }

                Close();
            });

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (viewRequest is ReportEditorRequest request)
            {
                FullTitle = request.FullId;
                mapper.Map(request.Report, this);
            }

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                IsDirty = true;
                if (Title.Last() != '*')
                    Title += '*';
            }

            PropertyChanged += Changed;

            IsDirty = false;

        }

        public async Task Save()
        {
            if (!IsValid) return;

            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                Id > 0
                    ? "Вы действительно хотите изменить этот отчёт?"
                    : "Вы действительно хотите создать отчёт?"
                , MessageDialogStyle.AffirmativeAndNegative);

            if (dialogResult != MessageDialogResult.Affirmative) return;

            var editedReport = new ApiReport();
            mapper.Map(this, editedReport);
            cachedService.CreateOrUpdateReport(editedReport);
            Close();
            cachedService.RefreshData();
        }
    }
}