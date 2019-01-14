using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
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
        public CachedServiceShell Shell { get; }

        public ReadOnlyObservableCollection<ApiRecepientGroup> RecepientGroups { get; set; }
        [Reactive] public ApiRecepientGroup SelectedGroup { get; set; }

        [Reactive] public RecepientEditorViewModel EditorViewModel { get; set; }

        public ReactiveCommand<Unit, Unit> EditGroupCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; set; }

        public RecepientManagerViewModel(ICachedService cachedService, IShell shell)
        {
            CanClose = false;
            this.cachedService = cachedService;
            Shell = shell as CachedServiceShell;

            CreateGroupCommand = ReactiveCommand.Create(() =>
            {
                EditorViewModel = Shell.Container.Resolve<RecepientEditorViewModel>(
                    new NamedParameter("group", new ApiRecepientGroup()));
            }, Shell.CanEdit);

            EditGroupCommand = ReactiveCommand.Create(() =>
            {
                EditorViewModel = Shell.Container.Resolve<RecepientEditorViewModel>(
                    new NamedParameter("group", SelectedGroup));
            }, Shell.CanEdit);

            SaveChangesCommand = ReactiveCommand.Create(() =>
            {
                if (EditorViewModel == null || EditorViewModel.IsOpened == false)
                    return;
                EditorViewModel.Save();
            }, Shell.CanEdit);
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (Shell.Role == ServiceUserRole.Editor)
            {
                Shell.AddVMCommand("File", "Save",
                        "SaveChangesCommand", this)
                    .SetHotKey(ModifierKeys.Control, Key.S);

                Shell.AddVMCommand("Edit", "Change recepient group",
                    "EditGroupCommand", this);
            }

            RecepientGroups = cachedService.RecepientGroups.SpawnCollection();
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

        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

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

            CancelCommand = ReactiveCommand.Create(() =>
            {
                IsOpened = false;
            });

            this.WhenAnyObservable(s => s.AllErrors.CountChanged)
                .Subscribe(_ => IsValid = !AllErrors.Items.Any());

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