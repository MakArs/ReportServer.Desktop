﻿using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace ReportServer.Desktop.ViewModels
{

    public class CachedServiceShell : Shell
    {
        private readonly ICachedService cachedService;

        public ReactiveCommand RefreshCommand { get; set; }
        public ReactiveCommand CreateTaskCommand { get; set; }
        public ReactiveCommand CreateOperTemplateCommand { get; set; }
        public ReactiveCommand CreateScheduleCommand { get; set; }

        public CachedServiceShell(ICachedService cachedService)
        {
            this.cachedService = cachedService;

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

        public async Task InitCachedServiceAsync(int tries)
        {
            while (tries-- > 0)
            {
                var mainview = Application.Current.MainWindow as MetroWindow;

                var serviceUri = await mainview.ShowInputAsync(
                    "Login",
                    "Enter working Report service instance url",
                    new MetroDialogSettings
                    {
                        DefaultText = "http://localhost:12345/",
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });

                if (!cachedService.Init(serviceUri))
                    continue;

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