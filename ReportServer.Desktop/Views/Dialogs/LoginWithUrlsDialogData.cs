using System;
using System.Security;

namespace ReportServer.Desktop.Views.Dialogs
{
    public class LoginWithUrlsDialogData
    {
        public string ServiceUrl { get; internal set; }

        public string AuthUrl { get; internal set; }

        public string Username { get; internal set; }

        public string Password
        {
            [SecurityCritical]
            get
            {
                IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.SecurePassword);
                try
                {
                    return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
                }
            }
        }

        public SecureString SecurePassword { get; internal set; }

        public bool ShouldRemember { get; internal set; }
    }
}
