﻿using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ReportServer.Desktop.Views.Dialogs
{
    public class LoginWithUrlsDialogSettings : MetroDialogSettings
    {
        private const string DefaultUsernameWatermark = "Username...";
        private const string DefaultPasswordWatermark = "Password...";
        private const string DefaultRememberCheckBoxText = "Remember";

        public LoginWithUrlsDialogSettings()
        {
            this.UsernameWatermark = DefaultUsernameWatermark;
            this.UsernameCharacterCasing = CharacterCasing.Normal;
            this.PasswordWatermark = DefaultPasswordWatermark;
            this.NegativeButtonVisibility = Visibility.Collapsed;
            this.ShouldHideUsername = false;
            this.AffirmativeButtonText = "Login";
            this.EnablePasswordPreview = false;
            this.RememberCheckBoxVisibility = Visibility.Collapsed;
            this.RememberCheckBoxText = DefaultRememberCheckBoxText;
            this.RememberCheckBoxChecked = false;
        }

        public string InitialServiceUrl { get; set; }

        public string InitialAuthUrl { get; set; }

        public string InitialUsername { get; set; }

        public string InitialPassword { get; set; }

        public string UsernameWatermark { get; set; }

        public CharacterCasing UsernameCharacterCasing { get; set; }

        public bool ShouldHideUsername { get; set; }

        public string PasswordWatermark { get; set; }

        public Visibility NegativeButtonVisibility { get; set; }

        public bool EnablePasswordPreview { get; set; }

        public Visibility RememberCheckBoxVisibility { get; set; }

        public string RememberCheckBoxText { get; set; }

        public bool RememberCheckBoxChecked { get; set; }
    }
}
