using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autofac;
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
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using IContainer = Autofac.IContainer;
using ItemCollection = Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection;

namespace ReportServer.Desktop.ViewModels
{
    public class OperEditorViewModel : ViewModelBase, IInitializableViewModel, ISaveableViewModel
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;

        private Dictionary<string, Type> DataImporters { get; set; }
        private Dictionary<string, Type> DataExporters { get; set; }

        public int? Id { get; set; }
        [Reactive] public OperType Type { get; set; }
        public ReactiveList<string> OperTemplates { get; set; }
        [Reactive] public string SelectedTemplateName { get; set; }
        [Reactive] public object Configuration { get; set; }

        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }

        public OperEditorViewModel(ICachedService cachedService, IMapper mapper,
                                   IDialogCoordinator dialogCoordinator)
        {
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

            this.ObservableForProperty(s => s.Type)
                .Subscribe(type =>
                {
                    var templates = type.Value == OperType.Exporter
                        ? DataExporters.Select(pair => pair.Key)
                        : DataImporters.Select(pair => pair.Key);

                    OperTemplates.PublishCollection(templates);
                    SelectedTemplateName = OperTemplates.First();
                });


            this.ObservableForProperty(s => s.SelectedTemplateName)
                .Where(templ => templ.Value != null)
                .Subscribe(templ =>
                {
                    var type = Type == OperType.Exporter
                        ? DataExporters[templ.Value]
                        : DataImporters[templ.Value];
                    if (type == null) return;

                    Configuration = Activator.CreateInstance(type);
                    mapper.Map(cachedService, Configuration);
                });

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());
        }

        public void Initialize(ViewRequest viewRequest)
        {
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
                FullTitle = request.FullId;
                mapper.Map(request.Oper, this);

                if (Id == 0)
                    Type = OperType.Importer;
                else
                {
                    var type = Type == OperType.Exporter
                        ? DataExporters[SelectedTemplateName]
                        : DataImporters[SelectedTemplateName];

                    Configuration = JsonConvert.DeserializeObject(request.Oper.Config, type);
                }
            }
          }

        public async Task Save()
        {
            if (!IsValid) return;

            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                Id > 0
                    ? "Save these operation parameters?"
                    : "Create this operation?"
                , MessageDialogStyle.AffirmativeAndNegative);

            if (dialogResult != MessageDialogResult.Affirmative) return;

            var editedReport = new ApiOper();

            mapper.Map(this, editedReport);

            cachedService.CreateOrUpdateOper(editedReport);
            Close();
            cachedService.RefreshData();
        }
    }
}