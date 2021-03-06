﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.ViewModels.General;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels.Editors
{
    public class OperEditorViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        public CachedServiceShell Shell { get; }

        private Dictionary<string, Type> DataImporters { get; set; }
        private Dictionary<string, Type> DataExporters { get; set; }

        public int? Id { get; set; }
        public ReadOnlyObservableCollection<string> OperTemplates { get; set; }

        [Reactive] public OperMode Mode { get; set; }
        [Reactive] public string ImplementationType { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public object Configuration { get; set; }

        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

        public OperEditorViewModel(ICachedService cachedService, IMapper mapper, IShell shell)
        {
            Shell = shell as CachedServiceShell;
            this.cachedService = cachedService;
            this.mapper = mapper;
            IsValid = true;
            validator = new OperEditorValidator();

            var operTemplates = new SourceList<string>();

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty,
                isd => isd == true).Concat(Shell.CanEdit);

            SaveChangesCommand = ReactiveCommand.CreateFromTask(async () => await Save(),
                canSave);

            CancelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (IsDirty)
                {
                    if (!await Shell.ShowWarningAffirmativeDialogAsync
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

                    operTemplates.ClearAndAddRange(templates);

                    OperTemplates = operTemplates.SpawnCollection();

                    ImplementationType = operTemplates.First();
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

            this.WhenAnyObservable(s => s.AllErrors.CountChanged)
                .Subscribe(_ => IsValid = !AllErrors.Items.Any());
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (Shell.Role == ServiceUserRole.Editor)
                Shell.AddVMCommand("File", "Save",
                    "SaveChangesCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.S);

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                if (IsDirty|| Shell.Role == ServiceUserRole.Viewer) return;
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

            if (!await Shell.ShowWarningAffirmativeDialogAsync(Id > 0
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