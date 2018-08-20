using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Views;
using ReportServer.Desktop.Views.WpfResources;

namespace ReportServer.Desktop.ViewModel
{
    public class Core : ReactiveObject
    {
        private readonly IDialogCoordinator dialogCoordinator = DialogCoordinator.Instance;
 
        public ReactiveList<string> ViewTemplates { get; set; }
        public ReactiveList<string> QueryTemplates { get; set; }

        public ReactiveCommand OpenViewTemplateWindowCommand { get; set; }
        public ReactiveCommand OpenQueryTemplateWindowCommand { get; set; }
    
      }
}