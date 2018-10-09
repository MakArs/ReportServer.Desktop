using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
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
    public class OperEditorViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        private readonly IShell shell;

        private Dictionary<string, Type> DataImporters { get; set; }
        private Dictionary<string, Type> DataExporters { get; set; }

        public int? Id { get; set; }
        [Reactive] public OperMode Mode { get; set; }
        public ReactiveList<string> OperTemplates { get; set; }
        [Reactive] public string Type { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public object Configuration { get; set; }

        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }

        public OperEditorViewModel(ICachedService cachedService, IMapper mapper,
                                   IDialogCoordinator dialogCoordinator,IShell shell)
        {
            this.shell = shell;
            this.cachedService = cachedService;
            this.mapper = mapper;
            IsValid = true;
            validator = new OperEditorValidator();
            this.dialogCoordinator = dialogCoordinator;

            OperTemplates = new ReactiveList<string>();

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty,
                isd => isd == true);

            SaveChangesCommand = ReactiveCommand.CreateFromTask(async () => await Save(),
                canSave);

            CancelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (IsDirty)
                {
                    var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                        "All unsaved changes will be lost. Close window?"
                        , MessageDialogStyle.AffirmativeAndNegative);

                    if (dialogResult != MessageDialogResult.Affirmative)
                        return;
                }

                Close();
            });

            this.ObservableForProperty(s => s.Mode)
                .Subscribe(mode =>
                {
                    var templates = mode.Value == OperMode.Exporter
                        ? DataExporters.Select(pair => pair.Key)
                        : DataImporters.Select(pair => pair.Key);

                    OperTemplates.PublishCollection(templates);
                    Type = OperTemplates.First();
                });

            this.ObservableForProperty(s => s.Type)
                .Where(type => type.Value != null)
                .Subscribe(type =>
                {
                    var operType = Mode == OperMode.Exporter
                        ? DataExporters[type.Value]
                        : DataImporters[type.Value];
                    if (operType == null) return;

                    Configuration = Activator.CreateInstance(operType);
                    mapper.Map(cachedService, Configuration);
                });

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());
        }

        public void Initialize(ViewRequest viewRequest)
        {
            shell.AddVMCommand("File", "Save",
                    "SaveChangesCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.S);

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                if (IsDirty) return;
                IsDirty = true;
                Title += '*';
            }

            PropertyChanged += Changed;

            DataImporters = cachedService.DataImporters;
            DataExporters = cachedService.DataExporters;

            if (viewRequest is OperEditorRequest request)
            {
                FullTitle = request.ViewId;

                if (request.Oper.Id == 0)
                {
                    Mode = OperMode.Importer;
                    Name = "New operation";
                }

                else
                {
                    Mode = DataExporters.ContainsKey(request.Oper.Type)
                        ? OperMode.Exporter
                        : OperMode.Importer;

                    mapper.Map(request.Oper, this);
                    var type = Mode == OperMode.Exporter
                        ? DataExporters[Type]
                        : DataImporters[Type];

                    Configuration = JsonConvert.DeserializeObject(request.Oper.ConfigTemplate, type);
                }
            }
          }

        public async Task Save()
        {
            if (!IsValid || !IsDirty) return;

            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                Id > 0
                    ? "Save these operation parameters?"
                    : "Create this operation?"
                , MessageDialogStyle.AffirmativeAndNegative);

            if (dialogResult != MessageDialogResult.Affirmative) return;

            var editedReport = new ApiOperTemplate();

            mapper.Map(this, editedReport);

            cachedService.CreateOrUpdateOper(editedReport);
            Close();

            cachedService.RefreshData();
        }
    }
}