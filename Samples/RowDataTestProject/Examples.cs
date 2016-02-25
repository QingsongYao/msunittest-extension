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

#region Using
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Test.VSUnitTest.TestTypeExtension.Log;
using Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
#endregion

namespace Microsoft.SqlServer.Test.Samples.Common.HelperTests1.UnitTest
{

    /// <summary>
    /// Examples of data-driven and parameterized test methods.
    /// </summary>
    [RowTestClass]
    public class DataExamples
    {
        #region TestContext

        /// <summary>
        /// Test class constructor.
        /// </summary>
        public DataExamples() { }

        private TestContext testContext;
        public TestContext TestContext
        {
            get { return this.testContext; }
            set { this.testContext = value; }
        }

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Inline Data Examples

        [TestMethod]
        [Row(1, 2, 3)]
        [Row(4, 5, 9)]
        public void TestRow(int number1, int number2, int result)
        {
            Assert.AreEqual(result, number1 + number2);
        }

        [TestMethod]
        [Row(42, "No extra params")]
        [Row(99, "A", "few", "words")]
        [Row(200, "A", "few", "more", "words", Properties= new string[] {
            "property1=Some value",
            "property2=123"
        })]
        [Row(201, "A", "few", "other", "words", Properties= new string[] {
            "property3=Some value 2",
            "property4=123456"
        })]
        public void TestRowParams(int i, string text, params string[] words)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("i={0}, text={1}, ", i, text));
            foreach (var s in words)
            {
                sb.Append(s);
                sb.Append(" ");
            }
            TestContext.WriteLine(sb.ToString());
        }

        [TestMethod]
        [Row(1, 2, 3)]
        [Row(3, 4, 7)]
        [Row(4, 5, 18, 2)]
        [Row(1, 2, 18, 3, 4, 5)]
        public void TestDataRowValues(DataRowValues drv, int number1, int number2, int result, int mult=1, params int[] moreNumbers)
        {
            Assert.IsTrue(drv.Values.Length >= 3);

            int moreNumbersSum = moreNumbers.Length > 0 ? moreNumbers.Aggregate((x,y) => x+y) : 0;
            Assert.AreEqual(result, (number1 + number2) * mult + moreNumbersSum);
        }

        #endregion

        #region Embedded Resource Examples

        [TestMethod]
        [EmbeddedXmlRows("Microsoft.SqlServer.Test.Samples.Common.HelperTests1.UnitTest.TestData.EmbeddedFile.xml", "RowSection", "Row")]
        [EmbeddedXmlRows("Microsoft.SqlServer.Test.Samples.Common.HelperTests1.UnitTest.TestData.EmbeddedFile.xml", "ParamsSection", "Row")]
        [EmbeddedXmlRows("Microsoft.SqlServer.Test.Samples.Common.HelperTests1.UnitTest.TestData.EmbeddedFile.xml", "PropertiesSection", "Row")]

        [TestInclude(11)]
        public void TestEmbeddedParams(int i, string text, params string[] words)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("i={0}, text={1}, ", i, text));
            foreach (var s in words)
            {
                sb.Append(s);
                sb.Append(" ");
            }

            sb.AppendLine();

            foreach (var key in TestContext.Properties.Keys)
            {
                sb.AppendLine("TestContext Property: " + key + "=" + testContext.Properties[key]);
            }

            TestContext.WriteLine(sb.ToString());
        }
        #endregion

        #region Variable-path Data File Examples

        #region Available test context variables
        // These may be used in the paths below along with environment variables for %...% subsitution.
        // They are taken from the TestContext and have MSDN-documented meanings:
        //
        // %DeploymentDirectory%
        // %ResultsDirectory%
        // %TestDeploymentDir%
        // %TestDir%
        // %TestLogsDir%
        // %TestName%
        // %TestResultsDirectory%
        // %TestRunDirectory%
        // %TestRunResultsDirectory%
        #endregion

        //TODO: Enable this tests
        // [TestMethod]
        //[XmlFileRows(@"..\..\common\inputs\SampleFile.xml", "RowSection", "Row")]
        //[XmlFileRows(@"..\..\common\inputs\SampleFile.xml", "ParamsSection", "Row")]
        // %TestDir% is relative to the Out deployment folder, not the project folder.
        // To uncomment the following lines, enable deployment and use the Copy to Output Directory file property to place it there.
        //[XmlFileRows(@"%TestDir%\..\..\HelperExample\TestData\EmbeddedFile.xml", "ParamsSection", "Row")]
        public void TestFileParams(int i, string text, params string[] words)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("i={0}, text={1}, ", i, text));
            foreach (var s in words)
            {
                sb.Append(s);
                sb.Append(" ");
            }
            TestContext.WriteLine(sb.ToString());
        }
        #endregion

        #region Deployed Data Source Examples

        #region [DataSource] usage
        // [DataSource] may use the special '|DataDirectory|' prefix for the Output folder, but ...
        // - no environment or test context '%variable%'s are supported.
        // - path after the special prefix is relative to the Output folder, which VS defaults to solution/TestResults/__/Out, but trun puts elsewhere.
        // - path portion under the project folder is preserved under '|DataDirectory|' when copied.
        // - path should not use '..'  or assume the relative location of the Output folder.
        //
        // ... '|DataDirectory|' msut be used with copy deployment enabled (i.e. <Deployment enabled="true" /> in the .testsettings).
        // - editing a .testsettings file outside of the VS Test Settings Editor requires a solution reload due to VS caching.
        // - [DeploymentItem] and Copy to Output Folder file properties are processed by MSTest in this mode.
        #endregion

        // To uncomment the following, ensure the SQL Express drive is present, and run from VS or trun with deployment enabled.
        //[TestMethod]
        //[DeploymentItem("HelperExample\\TestData\\MyWorkbook2007.xlsx")]
        //[DataSource("System.Data.SqlClient", "Server=.\\SQLExpress;AttachDbFilename=|DataDirectory|\\TestData\\SampleDatabase.mdf;Database=SampleDatabase;Driver={SQL Native Client};Trusted_Connection=Yes;Integrated Security=True;Connect Timeout=30;User Instance=True", "SampleTable", DataAccessMethod.Sequential)]
        public void TestDeployedSqlDataSource()
        {
        }

        // To uncomment the following ensure the Excel driver is present, and run from VS or trun with deployment enabled.
        //[TestMethod]
        [DeploymentItem("HelperExample\\TestData\\DeployedWorkbook2007.xlsx")]
        [DataSource("System.Data.OleDb", "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='|DataDirectory|\\TestData\\DeployedWorkbook2007.xlsx';Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"", "Sheet1$", DataAccessMethod.Sequential)]
        public void TestDeployedExcel2007DataSource()
        {
            int result = int.Parse(TestContext.DataRow["result"].ToString());
            int number1 = int.Parse(TestContext.DataRow["number1"].ToString());
            int number2 = int.Parse(TestContext.DataRow["number2"].ToString());
            TestContext.WriteLine("{0} + {1} = {2}", number1, number2, result);
            Assert.AreEqual(result, number1 + number2);
        }

        // To uncomment the following, run from VS or trun with deployment enabled.
        //[TestMethod]
        [DeploymentItem("HelperExample\\TestData\\DeployedData.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData\\DeployedData.xml", "Row", DataAccessMethod.Sequential)]
        public void TestDeployedXmlDataSource()
        {
            int result = int.Parse(TestContext.DataRow["result"].ToString());
            int number1 = int.Parse(TestContext.DataRow["number1"].ToString());
            int number2 = int.Parse(TestContext.DataRow["number2"].ToString());
            TestContext.WriteLine("{0} + {1} = {2}", number1, number2, result);
            Assert.AreEqual(result, number1 + number2);
        }
        #endregion

        #region Local Data Source Examples

        #region [DataSource] usage
        // [DataSource] may use relative paths, but ...
        // - no environment or test context '%variable%'s are supported.
        // - path is relative to the Output folder, which VS defaults to solution/TestResults/__/Out, but trun puts elsewhere.
        // - path using '..' should not assume the relative location of the Output folder.
        //
        // ... usually relative paths are attempted when copy deployment is disabled (i.e. <Deployment enabled="false" /> in the .testsettings).
        // - editing a .testsettings file outside of the VS Test Settings Editor requires a solution reload due to VS caching.
        // - [DeploymentItem] and Copy to Output Folder file properties are ignored by MSTest in this mode.
        #endregion

        // To uncomment the following, ensure the SQL Express driver is present, and run from VS but not trun.
        //[TestMethod]
        [DataSource("System.Data.SqlClient", "Server=.\\SQLExpress;AttachDbFilename=..\\..\\..\\TestData\\SampleDatabase.mdf;Database=SampleDatabase;Driver={SQL Native Client};Trusted_Connection=Yes;Integrated Security=True;Connect Timeout=30;User Instance=True", "SampleTable", DataAccessMethod.Sequential)]
        public void TestLocalSqlDataSource()
        {
            int result = int.Parse(TestContext.DataRow["result"].ToString());
            int number1 = int.Parse(TestContext.DataRow["number1"].ToString());
            int number2 = int.Parse(TestContext.DataRow["number2"].ToString());
            Assert.AreEqual(result, number1 + number2);
        }

        // To uncomment the following ensure the Excel driver is present, the data source relative path is correct, and run from VS but not trun.
        //[TestMethod]
        [DataSource("System.Data.OleDb", "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='..\\..\\..\\TestData\\Workbook2007.xlsx';Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"", "Sheet1$", DataAccessMethod.Sequential)]
        public void TestLocalExcel2007DataSource()
        {
            int result = int.Parse(TestContext.DataRow["result"].ToString());
            int number1 = int.Parse(TestContext.DataRow["number1"].ToString());
            int number2 = int.Parse(TestContext.DataRow["number2"].ToString());
            Assert.AreEqual(result, number1 + number2);
        }


        // To uncomment the following, ensure the data source relative path is correct, and run from VS but not trun.
        //[TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "..\\..\\..\\TestData\\Data.xml", "Row", DataAccessMethod.Sequential)]
        public void TestLocalXmlDataSource()
        {
            int result = int.Parse(TestContext.DataRow["result"].ToString());
            int number1 = int.Parse(TestContext.DataRow["number1"].ToString());
            int number2 = int.Parse(TestContext.DataRow["number2"].ToString());
            Assert.AreEqual(result, number1 + number2);
        }
        #endregion

        #region Runtime Rows Example

        public class MyRowAttribute : RowAttribute
        {
            public override IEnumerable<DataRowValues> GetRowEnumerator(System.Reflection.Assembly resourceAssembly, HelperTestGridResults results)
            {
                for (int i = 0; i < 10; i++)
                {
                    DataRowValues dataRowValues = new DataRowValues();
                    dataRowValues.Id = (i + 1).ToString();
                    dataRowValues.Values = new object[] {i};
                    yield return dataRowValues;
                }
            }
        }

        [TestMethod]
        [MyRowAttribute]
        [TestInclude(2,3,4)]
        public void TestMyRowAttribute(int i)
        {    
            TestContext.WriteLine("i = {0}", i);
        }

        #endregion

        #region Advanced Logging Examples

        // It is possible to use HTML in the log output, including shell execute extension, to provide additional tools (i.e. opening a windiff, updating a baselines, etc.) from the VS UI.
        [TestMethod, TestExecution(LogType = LogDetailsType.RowDetails, LogContentType = LogDetailsContentType.Html, WriteHeaders = false)]
        [Row(1, 2, 3)]
        [Row(4, 5, 9)]
        public void TestLogToRowDetails(int number1, int number2, int result)
        {
            RowTestContext.Current.LogWriter.Write("<h3>Test row {0}:</h3>\n", RowTestContext.Current.DataRowValues.Id);
            RowTestContext.Current.LogWriter.WriteLine("<span style='color: red'>{0}</span> + <u>{1}</u> = <span style='font-size: 18pt'>{2}</span>", number1, number2, result);

            RowTestContext.Current.LogWriter.WriteLine("<b>Click the following link to open a notepad:</b> <a href='shell://notepad.exe/foobar.txt'>notepad.exe foobar.txt</a>");

            Assert.AreEqual(result, number1 + number2);
        }

        [TestMethod, TestExecution(LogType = LogDetailsType.TestContext)]
        [Row(1, 2, 3)]
        [Row(4, 5, 9)]
        public void TestLogToTestContext(int number1, int number2, int result)
        {
            RowTestContext.Current.LogWriter.WriteLine("{0} + {1} = {2}", number1, number2, result);

            Assert.AreEqual(result, number1 + number2);
        }

        [TestMethod, TestExecution(LogType = LogDetailsType.Console)]
        [Row(1, 2, 3)]
        [Row(4, 5, 9)]
        public void TestLogToTestConsole(int number1, int number2, int result)
        {
            RowTestContext.Current.LogWriter.WriteLine("{0} + {1} = {2}", number1, number2, result);

            Assert.AreEqual(result, number1 + number2);
        }

        #endregion

        #region Parallel Execution Example

        public class BigRowAttribute : RowAttribute
        {
            public override IEnumerable<DataRowValues> GetRowEnumerator(System.Reflection.Assembly resourceAssembly, HelperTestGridResults results)
            {
                for (int i = 0; i < 1000; i++)
                {
                    DataRowValues dataRowValues = new DataRowValues();
                    dataRowValues.Id = (i + 1).ToString();
                    dataRowValues.Values = new object[] { i };
                    yield return dataRowValues;
                }
            }
        }

        // It takes 4000ms to execute all rows in sequence. Parallel execution will finish much faster on a machine with more procesor cores.
        [TestMethod, TestExecution(LogType= LogDetailsType.RowDetails, ExecuteInParallel=true)]
        [BigRowAttribute]
        public void TestParallelExecution(int i)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 2) ;
            RowTestContext.Current.LogWriter.WriteLine("i = {0}", i);
            while (sw.ElapsedMilliseconds < 4) ;
        }

        // It takes 4000ms to execute all rows in sequence. Parallel execution with 2 workers will take only 2000ms on a machine with more procesor cores.
        [TestMethod, TestExecution(LogType = LogDetailsType.RowDetails, ExecuteInParallel = true, MaxDegreeOfParallelism=2)]
        [BigRowAttribute]
        public void TestParallelExecution2(int i)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 2) ;
            RowTestContext.Current.LogWriter.WriteLine("i = {0}", i);
            while (sw.ElapsedMilliseconds < 4) ;
        }

        #endregion

        #region Test Filtering Examples

        [RowTestClass]
        [TestExclude("Exclude.Row2", ShowSkipped = true)]
        [TestExclude("AlwaysFail.*", ShowSkipped = true)]
        public class FilterExcample
        {
            [TestMethod]
            [Row(3, 4, Id = "Row1")]
            [Row(9, 1, Id = "Row2")]
            [TestInclude("Row1")]
            public void Include(int x, int y)
            {
                Assert.IsTrue(x < y);
            }

            [TestMethod]
            [Row(3, 4, Id = "Row1")]
            [Row(9, 1, Id = "Row2")]
            [Row(9, 2, Id = "Row3")]
            [TestExclude("Row3", ShowSkipped = false)]
            public void Exclude(int x, int y)
            {
                Assert.IsTrue(x < y);
            }

            [TestMethod]
            [Row(3, 4, Id = "Row1")]
            [Row(9, 1, Id = "Row2")]
            public void AlwaysFail(int x, int y)
            {
                Assert.IsTrue(false);
            }
        }

        #endregion

        #region Test Execution Override Example

        internal class ExecuteTwiceAttribute : TestExecutionAttribute
        {
            public override ITestMethodInvoker OverrideTestMethodInvoker(ITestMethodInvoker testMethodInvoker, TestMethodInvokerContext originalInvokerContext, RowTestContext rowTestContext)
            {
                return new Invoker() { OriginalInvoker = testMethodInvoker };
            }

            class Invoker : ITestMethodInvoker
            {
                public ITestMethodInvoker OriginalInvoker { get; set; }

                public TestMethodInvokerResult Invoke(params object[] parameters)
                {
                    this.OriginalInvoker.Invoke(parameters);
                    return this.OriginalInvoker.Invoke(parameters);
                }
            }
        }

        [RowTestClass]
        [ExecuteTwice]
        public class ExecuteTwiceTest
        {
            [TestMethod]
            public void Test1()
            {
                // This test method will be called twice.
                RowTestContext.Current.LogWriter.WriteLine("Test1");
            }

            [TestMethod]
            [Row(1)]
            [Row(2)]
            public void Test2(int x)
            {
                // This test method will be called twice for every row.
                RowTestContext.Current.LogWriter.WriteLine("Test2: {0}", x);
            }
        }

        #endregion
    }
}

