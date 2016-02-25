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
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using System.Globalization;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Permissions;
using System.DirectoryServices;
using System.Security.Principal;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RunAsTest
{
    public class HelperUserAccounts
    {
        public enum GroupType
        {
            Administrators,
            Backup_Operators,
            Guests,
            HelpServicesGroup,
            Network_Configuration_Operators,
            Power_Users,
            Remote_Desktop_Users,
            Replicator,
            Users
        };
        static string[] strGroupType ={ "Administrators",
              "Backup Operators",
              "Guests",
              "HelpServicesGroup",    
              "Network Configuration Operators", 
              "Power Users", 
              "Remote Desktop Users", 
              "Replicator", 
              "Users"};

        static string[] sidGroupType ={ "S-1-5-32-544",
                             "S-1-5-32-551",
                             "S-1-5-32-546",
                             "S-1-5-21-2333387076-1144227834-250145053-1000",
                             "S-1-5-32-556",
                             "S-1-5-32-547",
                             "S-1-5-32-555",
                             "S-1-5-32-552",
                             "S-1-5-32-545"
                        };

        //This is for localization runs where Administrators might be Administrateurs etc
        static int findIndexForGroupType(string groupType)
        {
            for (int i = 0; i < strGroupType.Length; i++)
            {

                if (strGroupType[i].ToLower().Equals(groupType.ToLower()))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string GetLocalGroupString(string groupType)
        {

            int index = findIndexForGroupType(groupType);
            if (index < 0)
                throw new ArgumentException("groupType");
            SecurityIdentifier sid = new SecurityIdentifier(sidGroupType[index]);
            NTAccount ntaccount = sid.Translate(typeof(NTAccount)) as NTAccount;

            string[] accountTokens = ntaccount.ToString().Split(new char[] { '\\' });
            switch (accountTokens.Length)
            {
                case 2:
                    return accountTokens[1];
                case 1:
                    return accountTokens[0];
                default:
                    throw new Exception("Account Token not in the known format- Time to research");
            }

        }

        public static void CreateUserInMachine(string User, string Password, GroupType groupType)
        {
            InternalCreateUserInMachine(User, Password, groupType);
        }
        static void InternalCreateUserInMachine(string User, string Password, GroupType groupType)
        {
            try
            {
                InternalRemoveUserFromMachine(User);
            }
            catch { }

            DirectoryEntry AD = new DirectoryEntry("WinNT://" +
                                Environment.MachineName + ",computer");
            DirectoryEntry NewUser = AD.Children.Add(User, "user");
            NewUser.Invoke("SetPassword", new object[] { Password });
            //NewUser.Invoke("SetDomain", new object[] { "redmond" });
            NewUser.Invoke("Put", new object[] { "Description", "Test User from .NET" });
            //NewUser.Properties["userPrincipalName"].Value = User+ "@redmond";
            NewUser.CommitChanges();

            DirectoryEntry grp;

            grp = AD.Children.Find(GetLocalGroupString(strGroupType[(int)groupType]), "group");
            if (grp != null) { grp.Invoke("Add", new object[] { NewUser.Path.ToString() }); }

        }

        public static void RemoveUserFromMachine(string User)
        {
            InternalRemoveUserFromMachine(User);
        }

        static void InternalRemoveUserFromMachine(string User)
        {
            try
            {
                DirectoryEntry AD = new DirectoryEntry("WinNT://" +
                                    Environment.MachineName + ",computer");
                DirectoryEntry UserToRemove = AD.Children.Find(User, "user");
                AD.Children.Remove(UserToRemove);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
