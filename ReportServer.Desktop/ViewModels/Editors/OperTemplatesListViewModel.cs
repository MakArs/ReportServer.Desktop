using System;
using System.Reactive;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels.Editors
{
    public class OperTemplatesListViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private TaskEditorViewModel taskEditor;

        [Reactive] public string OperationsSearchString { get; set; }
        public ReactiveCommand<ApiOperTemplate, Unit> SelectTemplateCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        public ReactiveCommand<ApiOperTemplate, Unit> AddFullTemplateCommand { get; set; }

        [Reactive] public ApiOperTemplate SelectedTemplate { get; set; }
        [Reactive] public ObservableCollectionExtended<ApiOperTemplate> OperTemplates { get; set; }

        public OperTemplatesListViewModel(ICachedService service)
        {
            cachedService = service;

            this.ObservableForProperty(s => s.OperationsSearchString)
                .Subscribe(sstr =>
                {
                    OperTemplates.Clear();

                    cachedService.OperTemplates.Connect().Filter(oper =>
                            oper.Name.IndexOf(sstr.Value, StringComparison.OrdinalIgnoreCase) >=
                            0)
                        .Bind(OperTemplates).Subscribe();
                });

            CancelCommand = ReactiveCommand.Create(Close);

            SelectTemplateCommand = ReactiveCommand.Create<ApiOperTemplate>(templ =>
            {
                taskEditor.ParseTemplate(templ);
                Close();
            });

            AddFullTemplateCommand = ReactiveCommand.Create<ApiOperTemplate>(templ =>
            {
                taskEditor.AddFullTemplate(templ);
                Close();
            });
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (viewRequest is OperTemplatesListRequest req)
                taskEditor = req.TaskEditor;

            OperTemplates = new ObservableCollectionExtended<ApiOperTemplate>();

            cachedService.OperTemplates.Connect()
                .Bind(OperTemplates).Subscribe();
        }
    }
}