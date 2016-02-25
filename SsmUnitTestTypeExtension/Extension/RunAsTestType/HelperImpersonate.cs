/*
 * 
 * 
 * ------------------------------------------ START OF LICENSE -----------------------------------------
MsTest UnitTestTypeExtension 
Copyright (c) Microsoft Corporation
All rights reserved. 
MIT License
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
----------------------------------------------- END OF LICENSE ------------------------------------------
 * 
 */


using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RunAsTest
{

    /**/
    public enum LogonType
    {
        Interactive = 2,
        Network = 3,
        Batch = 4,
        Service = 5,
        Unlock = 7,
        NetworkClearText = 8,
        NewCredentials = 9,
    }

    /**/
    public enum LogonProvider
    {
        Default = 0,
        WinNT35 = 1,
        WinNT40 = 2,
        WinNT50 = 3
    }

    /**/
    public sealed class HelperImpersonate : IDisposable
    {
        WindowsImpersonationContext impersonationContext = null;

        /**/
        public HelperImpersonate(string user, string domain, string password)
        {
            Impersonate(user, domain, password, LogonType.Interactive, LogonProvider.Default);
        }
        /**/
        public HelperImpersonate(string user, string domain, string password, LogonType logonType)
        {
            Impersonate(user, domain, password, logonType, LogonProvider.Default);
        }
        /**/
        public HelperImpersonate(string user, string domain, string password, LogonType logonType, LogonProvider logonProvider)
        {
            Impersonate(user, domain, password, logonType, logonProvider);
        }
        /**/
        ~HelperImpersonate()
        {
            // HACK, lechg, fxCop expects to have a finalizer if IDisposable is implemented by the class.
            // Can't do actually nothing, because the finalizer thread != constructor thread.
        }
        /**/
        public void Dispose()
        {
            if (constructorThread != Thread.CurrentThread)
            {
                throw new ApplicationException("Dispose should be called on the same thread as instance constructor.");
            }
            Exception ex = null;
            if (IntPtr.Zero != token)
            {
                impersonationContext.Undo();
                if (!NativeMethods.CloseHandle(token))
                {
                    ex = new Win32Exception(Marshal.GetLastWin32Error());
                }
                token = IntPtr.Zero;
            }
            GC.KeepAlive(this);
            GC.SuppressFinalize(this);
            if (ex != null) throw ex;
        }
        /**/
        private void Impersonate(string user, string domain, string password, LogonType logonType, LogonProvider logonProvider)
        {
            if (null == user) throw new ArgumentNullException();
            if (null == domain) throw new ArgumentNullException();
            if (null == password) throw new ArgumentNullException();
            //
            if (!NativeMethods.LogonUser(user, domain, password, logonType, logonProvider, out token))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            impersonationContext = WindowsIdentity.Impersonate(token);
            if (impersonationContext == null)
            {
                NativeMethods.CloseHandle(token);
                token = IntPtr.Zero;
                throw new Exception("Failed to impersonate specified user");
            }

            constructorThread = Thread.CurrentThread;
            GC.KeepAlive(this);
        }
        /**/
        private IntPtr token = IntPtr.Zero;
        private Thread constructorThread = null;
    }



    abstract class NativeMethods
    {

        /**/
        [DllImport("advapi32.dll", SetLastError = true)]
        internal extern static bool LogonUser(string user, string domain, string password, LogonType logonType, LogonProvider provider, out IntPtr token);
        /**/
        [DllImport("kernel32.dll", SetLastError = true)]
        internal extern static bool CloseHandle(IntPtr handle);
        /**/
        [DllImport("advapi32.dll", SetLastError = true)]
        internal extern static bool ImpersonateLoggedOnUser(IntPtr token);
        /**/
        [DllImport("advapi32.dll", SetLastError = true)]
        internal extern static bool RevertToSelf();

    }
}
