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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Security.Principal;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RunAsTest
{
    public class ExtensionMethodInvoker : ITestMethodInvoker
    {
        private TestMethodInvokerContext context;
        public ExtensionMethodInvoker(TestMethodInvokerContext context)
        {
            Debug.Assert(context != null);
            this.context = context;
        }

        /// Is called when a test is run
        public TestMethodInvokerResult Invoke(params object[] args)
        {
            // Log the ID of the test user
            Trace.WriteLine("Begin Invoke: current user is " + WindowsIdentity.GetCurrent().Name);

            HelperImpersonate runas = null;

            bool runAsNormalUser;
            Boolean.TryParse(this.context.TestContext.Properties["RunAsNormalUser"] as string, out runAsNormalUser);

            if (runAsNormalUser)
            {
                Trace.WriteLine("Creating user: " + USER);
                HelperUserAccounts.CreateUserInMachine(USER, PASSWORD, HelperUserAccounts.GroupType.Users);

                // Impersonate a user with minimal privileges
                Trace.WriteLine("Impersonating user: " + USER);
                runas = new HelperImpersonate(USER, DOMAIN, PASSWORD);
            }

            // Invoke the user's test method
            Trace.WriteLine("Invoking test method");

            try
            {
                return this.context.InnerInvoker.Invoke();
            }
            finally
            {
                if (runas != null)
                {
                    // Undo the impersonation
                    Trace.WriteLine("Undoing impersonation of user: " + USER);
                    runas.Dispose();

                    Trace.WriteLine("Removing user: " + USER);
                    HelperUserAccounts.RemoveUserFromMachine(USER);
                }

                // Log the ID of the test user
                Trace.WriteLine("End Invoke: current user is " + WindowsIdentity.GetCurrent().Name);
            }
        }

        #region member variables
        private const string USER = "DemoUser";
        private const string DOMAIN = "";
        private const string PASSWORD = "abc123!!";
        #endregion
    }
}
