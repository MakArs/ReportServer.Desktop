using System;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Views
{
    /// <summary>
    /// codebehind for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IDockWindow
    {
        public MainWindow(IShell shell)
        {
            Shell = shell;
            InitializeComponent();
        }

        private IShell Shell { get; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Shell.AttachDockingManager(DockingManager);

            DataContext = Shell;
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
