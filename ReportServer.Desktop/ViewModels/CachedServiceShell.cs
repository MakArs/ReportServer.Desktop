using System;
using System.Configuration;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using Domain0.Api.Client;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Application = System.Windows.Application;

namespace ReportServer.Desktop.ViewModels
{

    public class CachedServiceShell : Shell
    {
        private readonly ICachedService cachedService;
        private readonly IAuthenticationContext authContext;

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateTaskCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateOperTemplateCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateScheduleCommand { get; set; }

        public CachedServiceShell(ICachedService cachedService, IAuthenticationContext context)
        {
            this.cachedService = cachedService;
            authContext = context;

            RefreshCommand = ReactiveCommand.Create(this.cachedService.RefreshData);

            CreateTaskCommand = ReactiveCommand.Create(() =>
                ShowView<TaskEditorView>(new TaskEditorRequest
                    {
                        ViewId = "Creating new Task",
                        Task = new ApiTask {Id = 0}
                    },
                    new UiShowOptions {Title = "Creating new Task"}));

            CreateScheduleCommand = ReactiveCommand.Create(() =>
                ShowView<CronEditorView>(new CronEditorRequest
                    {
                        ViewId = "Creating new Schedule",
                        Schedule = new ApiSchedule {Id = 0}
                    },
                    new UiShowOptions {Title = "Creating new Schedule"}));

            CreateOperTemplateCommand = ReactiveCommand.Create(() =>
                ShowView<OperEditorView>(new OperEditorRequest
                    {
                        Oper = new ApiOperTemplate {Id = 0},
                        ViewId = "Creating new operation template"
                    },
                    new UiShowOptions {Title = "Creating new operation template"}));
        }

        private async Task LoginDomain0()
        {
            var mainview = Application.Current.MainWindow as MetroWindow;

            if (!await cachedService.Connect(ConfigurationManager.AppSettings["BaseServiceUrl"]))
            {
                await ShowMessageAsync("Cannot connect working service");
                mainview.Close();
            }

            if (!authContext.IsLoggedIn)
            {
                var logintask = mainview.ShowLoginAsync(
                    "Login",
                    "Enter your login and password",
                    new LoginDialogSettings
                    {
                        AffirmativeButtonText = "Login",
                        NegativeButtonText = "Cancel",
                        NegativeButtonVisibility = Visibility.Visible,
                        UsernameWatermark = "phone(71231234567) or e-mail (example@example.example)",
                        EnablePasswordPreview = true,
                        RememberCheckBoxVisibility = Visibility.Visible,
                    });

                var loginData = await logintask;

                if (loginData==null)
                    Application.Current.MainWindow?.Close();

                authContext.ShouldRemember = loginData.ShouldRemember;

                if (long.TryParse(loginData.Username, out long phone))
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

        public async Task InitCachedServiceAsync(int tries)
        {
            while (tries-- > 0)
            {
                await LoginDomain0();

                if (!authContext.IsLoggedIn)
                    continue;

                cachedService.Init(authContext.Token);

                ShowView<TaskManagerView>(
                    options: new UiShowOptions {Title = "Task Manager", CanClose = false});

                ShowView<OperTemplatesManagerView>(
                    options: new UiShowOptions
                    {
                        Title = "Operation template Manager",
                        CanClose = false
                    });

                ShowView<RecepientManagerView>(
                    options: new UiShowOptions {Title = "Recepient Manager", CanClose = false});

                ShowView<ScheduleManagerView>(
                    options: new UiShowOptions {Title = "Schedule Manager", CanClose = false});

                return;
            }

            Application.Current.MainWindow?.Close();
        }

        public async Task<bool> ShowWarningAffirmativeDialogAsync(
            string question, string title = "Warning",
            MessageDialogStyle dialogStyle = MessageDialogStyle.AffirmativeAndNegative)
        {
            var mainview = Application.Current.MainWindow as MetroWindow;

            var dialogResult = await mainview.ShowMessageAsync(title,
                question
                , dialogStyle);
            return dialogResult == MessageDialogResult.Affirmative;
        }

        public async Task ShowMessageAsync(string text, string title = "Warning")
        {
            var mainview = Application.Current.MainWindow as MetroWindow;

            await mainview.ShowMessageAsync(title, text);
        }
    }
}