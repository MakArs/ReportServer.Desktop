using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels.Editors
{
    public class RecepientEditorViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;

        public int? Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Addresses { get; set; }
        [Reactive] public string AddressesBcc { get; set; }
        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

        public RecepientEditorViewModel(ICachedService cachedService, IMapper mapper)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.mapper = mapper;
            validator = new RecepientGroupEditorValidator();

            IsValid = true;

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty,
                isd => isd == true);

            SaveChangesCommand = ReactiveCommand.Create(Save, canSave);

            CancelCommand = ReactiveCommand.Create(Close);

            this.WhenAnyObservable(s => s.AllErrors.CountChanged)
                .Subscribe(_ => IsValid = !AllErrors.Items.Any());


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

            var editedGroup=new ApiRecepientGroup();

            mapper.Map(this, editedGroup);

            if (string.IsNullOrEmpty(AddressesBcc))
                editedGroup.AddressesBcc = null;

            cachedService.CreateOrUpdateRecepientGroup(editedGroup);
            cachedService.RefreshRecepientGroups();
            Close();
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (viewRequest is RecepientEditorRequest request)
            {
                mapper.Map(request.Group, this);
            }
        }
    }
}
