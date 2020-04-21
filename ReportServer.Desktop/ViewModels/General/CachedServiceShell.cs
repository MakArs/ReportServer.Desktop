using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Domain0.Api.Client;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views;
using ReportServer.Desktop.Views.Dialogs;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace ReportServer.Desktop.ViewModels.General
{

    public class CachedServiceShell : Shell
    {
        private readonly ICachedService cachedService;
        private readonly IAuthenticationContext authContext;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IAppConfigStorage _appConfigStorage;

        [Reactive] public ServiceUserRole Role { get; set; }
        public IObservable<bool> CanEdit;
        public IObservable<bool> CanStopRun;
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateTaskCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateOperTemplateCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateScheduleCommand { get; set; }

        public CachedServiceShell(
            ICachedService cachedService, 
            IAuthenticationContext context, 
            IDialogCoordinator dialogCoordinator,
            IAppConfigStorage appConfigStorage)
        {
            this.cachedService = cachedService;
            authContext = context;
            _dialogCoordinator = dialogCoordinator;
            _appConfigStorage = appConfigStorage;

            RefreshCommand = ReactiveCommand.Create(this.cachedService.RefreshData);

            CanEdit = this.WhenAnyValue(shl => shl.Role, role => role == ServiceUserRole.Editor);

            CanStopRun = this.WhenAnyValue(shl => shl.Role, role => role == ServiceUserRole.Editor ||
                                                                    role == ServiceUserRole.StopRunner);

            CreateTaskCommand = ReactiveCommand.Create(() =>
                ShowView<TaskEditorView>(new TaskEditorRequest
                    {
                        ViewId = "Creating new Task",
                        Task = new ApiTask {Id = 0}
                    },
                    new UiShowOptions {Title = "Creating new Task"}), CanEdit);

            CreateScheduleCommand = ReactiveCommand.Create(() =>
                ShowView<CronEditorView>(new CronEditorRequest
                    {
                        ViewId = "Creating new Schedule",
                        Schedule = new ApiSchedule {Id = 0}
                    },
                    new UiShowOptions {Title = "Creating new Schedule"}), CanEdit);

            CreateOperTemplateCommand = ReactiveCommand.Create(() =>
                ShowView<OperEditorView>(new OperEditorRequest
                    {
                        Oper = new ApiOperTemplate {Id = 0},
                        ViewId = "Creating new operation template"
                    },
                    new UiShowOptions {Title = "Creating new operation template"}), CanEdit);
        }

        private async Task ConnectAndLogin()
        {
            if (!authContext.IsLoggedIn)
            {
                var config = await _appConfigStorage.Load();
                var loginData = await ShowLoginDialog(config);

                if (loginData == null)
                {
                    ShutDown();
                    return;
                }

                config.ServiceUrl = loginData.ServiceUrl;
                config.AuthUrl = loginData.AuthUrl;
                config.Username = loginData.Username;
                config.ShouldRemember = loginData.ShouldRemember;

                await _appConfigStorage.Save(config);

                if (!await cachedService.Connect(config.ServiceUrl))
                {
                    await ShowMessageAsync("Cannot connect to working service");
                    return;
                }

                authContext.HostUrl = loginData.AuthUrl;
                authContext.ShouldRemember = loginData.ShouldRemember;

                if (long.TryParse(loginData.Username, out var phone))
                {
                    try
                    {
                        await authContext
                            .LoginByPhone(phone, loginData.Password);
                    }
                    catch (Exception e)
                    {
                        await ShowMessageAsync(e.InnerException?.Message ?? e.Message);
                    }
                }

                else
                {
                    try
                    {
                        await authContext
                            .LoginByEmail(loginData.Username, loginData.Password);
                    }
                    catch (Exception e)
                    {
                        await ShowMessageAsync(e.InnerException?.Message ?? e.Message);
                    }
                }
            }
        }

        private async Task<LoginWithUrlsDialogData> ShowLoginDialog(AppConfig config)
        {
            var settings = new LoginWithUrlsDialogSettings
            {
                InitialServiceUrl = config.ServiceUrl,
                InitialAuthUrl = config.AuthUrl,
                InitialUsername = config.Username,
                AnimateShow = true,
                AnimateHide = true,
                AffirmativeButtonText = "Login",
                NegativeButtonText = "Cancel",
                NegativeButtonVisibility = Visibility.Visible,
                UsernameWatermark = "phone(71231234567) or e-mail (example@example.example)",
                EnablePasswordPreview = true,
                RememberCheckBoxVisibility = Visibility.Visible,
                RememberCheckBoxChecked = config.ShouldRemember,
            };

            var loginDialog = new LoginWithUrlsDialog(null, settings)
            {
                Title = "Login",
                Message = "Enter your login and password",
            };
            await _dialogCoordinator.ShowMetroDialogAsync(this, loginDialog);
            var result = await loginDialog.WaitForButtonPressAsync();
            await _dialogCoordinator.HideMetroDialogAsync(this, loginDialog);

            return result;
        }

        public async Task InitCachedServiceAsync(int tries)
        {
            while (tries-- > 0)
            {
                await ConnectAndLogin();

                if (!authContext.IsLoggedIn)
                    continue;

                cachedService.Init(authContext.Token);

                Role = await cachedService.GetUserRole(authContext.Token);

                if (Role ==  ServiceUserRole.NoRole)
                {
                    authContext.Logout();
                    await ShowMessageAsync("You have not enough rights for using service.Contact administrator to get it");
                    continue;
                }

                ShowView<OperTemplatesManagerView>(
                    options: new UiShowOptions
                    {
                        Title = "Operation template Manager",
                        CanClose = false
                    });

                ShowView<RecepientManagerView>(
                    options: new UiShowOptions {Title = "Recipient Manager", CanClose = false});

                ShowView<ScheduleManagerView>(
                    options: new UiShowOptions {Title = "Schedule Manager", CanClose = false});
                
                ShowView<TaskManagerView>(
                    options: new UiShowOptions {Title = "Task Manager", CanClose = false});

                return;
            }

            ShutDown();
        }

        public async Task<bool> ShowWarningAffirmativeDialogAsync(
            string question, string title = "Warning",
            MessageDialogStyle dialogStyle = MessageDialogStyle.AffirmativeAndNegative)
        {
            var dialogResult = await _dialogCoordinator.ShowMessageAsync(this, title, question, dialogStyle);
            return dialogResult == MessageDialogResult.Affirmative;
        }

        public Task ShowMessageAsync(string text, string title = "Warning")
            => _dialogCoordinator.ShowMessageAsync(this, title, text);

        private void ShutDown() 
            => (Container.Resolve<IDockWindow>() as Window)?.Close();
    }
}
