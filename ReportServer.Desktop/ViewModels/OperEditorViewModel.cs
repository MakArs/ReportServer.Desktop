using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
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
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        private readonly CachedServiceShell shell;

        private Dictionary<string, Type> DataImporters { get; set; }
        private Dictionary<string, Type> DataExporters { get; set; }

        public int? Id { get; set; }
        public ReactiveList<string> OperTemplates { get; set; }
        [Reactive] public OperMode Mode { get; set; }
        [Reactive] public string ImplementationType { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public object Configuration { get; set; }

        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }

        public OperEditorViewModel(ICachedService cachedService, IMapper mapper, IShell shell)
        {
            this.shell = shell as CachedServiceShell;
            this.cachedService = cachedService;
            this.mapper = mapper;
            IsValid = true;
            validator = new OperEditorValidator();

            OperTemplates = new ReactiveList<string>();

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

            this.ObservableForProperty(s => s.Mode)
                .Subscribe(mode =>
                {
                    var templates = mode.Value == OperMode.Exporter
                        ? DataExporters.Select(pair => pair.Key)
                        : DataImporters.Select(pair => pair.Key);

                    OperTemplates.PublishCollection(templates);
                    ImplementationType = OperTemplates.First();
                });

            this.ObservableForProperty(s => s.ImplementationType)
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
                    Mode = DataExporters.ContainsKey(request.Oper.ImplementationType)
                        ? OperMode.Exporter
                        : OperMode.Importer;

                    mapper.Map(request.Oper, this);
                    var type = Mode == OperMode.Exporter
                        ? DataExporters[ImplementationType]
                        : DataImporters[ImplementationType];

                    Configuration =
                        JsonConvert.DeserializeObject(request.Oper.ConfigTemplate, type);
                }
            }
        }

        public async Task Save()
        {
            if (!IsValid || !IsDirty) return;

            if (!await shell.ShowWarningAffirmativeDialogAsync(Id > 0
                ? "Save these operation parameters?"
                : "Create this operation?"))
                return;

            var editedReport = new ApiOperTemplate();

            mapper.Map(this, editedReport);

            cachedService.CreateOrUpdateOper(editedReport);
            Close();

            cachedService.RefreshData();
        }
    }
}