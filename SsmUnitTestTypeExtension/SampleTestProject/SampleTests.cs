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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.SqlServer.Test.VSUnitTest.SsmUnitTestTypeExtension.RowTest;
using Microsoft.SqlServer.Test.VSUnitTest.SsmUnitTestTypeExtension.RunAsTest;
using System.Security.Principal;
using System.IO;
using System.Diagnostics;

namespace SampleTestProject
{
    #region RowTests
    [RowTestClass]
    public class RowTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Row(1,2,3)]
        [Row(0, 1, 1)]
        [Row(-1,1,0)]
        [Row(-1,-1,-2)]
        public void TestRowAdd(int number1, int number2, int result)
        {
            Assert.AreEqual(result, number1 + number2);
        }
    }
    #endregion

    #region RunAs
    #region TraditionalTestClass
    [TestClass]
    public class TraditionalTestClass
    {
        [TestMethod]
        public void TestTraditional()
        {
            Evilness.WriteToWinDir();
        }
    }
    #endregion

    #region DemoTestClass
    [RunAsTestClass]
    public class RunAsTestClass
    {
        [TestMethod]
        [TestProperty("RunAsNormalUser", "true")]
        public void TestRunAs()
        {
            Evilness.WriteToWinDir();
        }
    }
    #endregion

    #region Evilness
    static class Evilness
    {
        public static void WriteToWinDir()
        {
            Trace.WriteLine("WriteToWinDir: executing as " + WindowsIdentity.GetCurrent().Name);
            string winDir = Environment.GetEnvironmentVariable("windir");
            using (StreamWriter sw = File.CreateText(Path.Combine(winDir, "IDontBelongHere.txt")))
            {
                sw.WriteLine("Line of text");
            }
        }
    }
    #endregion

    #endregion

}
