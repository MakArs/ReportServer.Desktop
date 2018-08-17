using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
   public class TaskEditorViewModel : ReactiveObject, IViewModel, IInitializableViewModel
    {
        public string Title { get; set; }
        public string FullTitle { get; set; }

        public bool IsDirty { get; set; }

        public TaskEditorViewModel()
        {

        }

        public void Initialize(ViewRequest viewRequest)
        {
            throw new NotImplementedException();
        }
    }
}
