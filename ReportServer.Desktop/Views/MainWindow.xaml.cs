using System;
using Ui.Wpf.Common;
using Xceed.Wpf.AvalonDock;

namespace ReportServer.Desktop.Views
{
    public partial class MainWindow : IDockWindow
    {
        public MainWindow(IShell shell)
        {
            Shell = shell;
            DataContext = Shell;
            InitializeComponent();
        }

        private IShell Shell { get; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Shell.AttachDockingManager(DockingManager);

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Shell?.Container.Dispose();
        }

        private void ActiveContentChanged(object sender, EventArgs e)
        {

        }
    }
}
