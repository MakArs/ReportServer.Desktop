using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Autofac;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels
{
    public class RecepientManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IShell shell;

        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        [Reactive] public ApiRecepientGroup SelectedGroup { get; set; }

        [Reactive] public RecepientEditorViewModel EditorViewModel { get; set; }

        public ReactiveCommand EditGroupCommand { get; set; }
        public ReactiveCommand CreateGroupCommand { get; set; }

        public RecepientManagerViewModel(ICachedService cachedService, IShell shell)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.shell = shell;

            CreateGroupCommand = ReactiveCommand.Create(() =>
            {
                EditorViewModel = this.shell.Container.Resolve<RecepientEditorViewModel>(
                    new NamedParameter("group", new ApiRecepientGroup()));
            });

            EditGroupCommand = ReactiveCommand.Create(() =>
            {
                EditorViewModel = this.shell.Container.Resolve<RecepientEditorViewModel>(
                    new NamedParameter("group", SelectedGroup));
            });
        }

        public void Initialize(ViewRequest viewRequest)
        {
            shell.AddVMCommand("File", "Save",
                    "EditorViewModel?.SaveChangesCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.S);

            RecepientGroups = cachedService.RecepientGroups;
        }
    }


    public class RecepientEditorViewModel : ViewModelBase
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;

        public int? Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Addresses { get; set; }
        [Reactive] public string AddressesBcc { get; set; }
        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }
        [Reactive] public bool IsOpened { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }

        public RecepientEditorViewModel(ICachedService cachedService, IMapper mapper,
                                        ApiRecepientGroup group)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.mapper = mapper;
            validator = new RecepientGroupEditorValidator();

            IsValid = true;

            mapper.Map(group, this);

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty,
                isd => isd == true);

            SaveChangesCommand = ReactiveCommand.Create(Save, canSave);

            CancelCommand = ReactiveCommand.Create(() => IsOpened = false);

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());

            IsOpened = true;

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                if (IsDirty || e.PropertyName == "IsDirty") return;
                IsDirty = true;
            }

            PropertyChanged += Changed;
        }

        public void Save()
        {
            if (!IsValid || !IsDirty) return;

            var editedGroup = new ApiRecepientGroup();

            mapper.Map(this, editedGroup);

            if (string.IsNullOrEmpty(AddressesBcc))
                editedGroup.AddressesBcc = null;

            cachedService.CreateOrUpdateRecepientGroup(editedGroup);
            cachedService.RefreshData();
            IsOpened = false;
            Close();
        }
    }
}