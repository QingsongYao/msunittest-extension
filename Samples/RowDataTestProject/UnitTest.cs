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
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
//
using Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace Microsoft.SqlServer.Test.Samples.Common.HelperTests1.UnitTest
{
    /// <summary>
    /// Summary of the DataTest unit test class.
    /// </summary>
    [TestClass]
    public class DataTest
    {
        #region TestContext

        /// <summary>
        /// Test class constructor.
        /// </summary>
        public DataTest() { }

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

        [TestMethod]
        public void TestMethod1()
        {
        }
    }

    /// <summary>
    /// Summary of the RowDataTest unit test class.
    /// </summary>
    [RowTestClass]
    public class RowDataTest
    {
        #region TestContext

        /// <summary>
        /// Test class constructor.
        /// </summary>
        public RowDataTest() { }

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

        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        [Row(1, 2, 3)]
        [Row(4, 5, 9)]
        public void TestRow(int number1, int number2, int result)
        {
            Assert.AreEqual(result, number1 + number2);
        }
    }
}
