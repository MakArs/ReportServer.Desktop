﻿using ReportServer.Desktop.ViewModels;
using ReportServer.Desktop.ViewModels.Editors;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Views
{
    public partial class TaskEditorView : IView
    {
        public TaskEditorView(TaskEditorViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
            ViewModel.FullTitle = options.Title;
        }

        public IViewModel ViewModel { get; set; }
    }
}
