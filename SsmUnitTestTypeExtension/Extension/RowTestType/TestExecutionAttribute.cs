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
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Specify a list of Ids/Descs to execute. All others on this TestMethod will be ignored.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class TestExecutionAttribute : Attribute
    {
        private Log.LogDetailsType? logType;
        private Log.LogDetailsContentType? logContentType;
        private bool? writeHeaders;
        private bool? executeInParallel;
        private int? maxDegreeOfParallelism;
        /// <summary>
        /// Gets or sets the log type.
        /// </summary>
        public Log.LogDetailsType LogType 
        {
            get { return logType.HasValue ? logType.Value : Log.LogDetailsType.TestContext; }
            set { this.logType = value; }
        }

        /// <summary>
        /// Gets os sets the log content type.
        /// </summary>
        public Log.LogDetailsContentType LogContentType  
        {
            get { return logContentType.HasValue ? logContentType.Value : Log.LogDetailsContentType.Text; }
            set { this.logContentType = value; }
        }

        /// <summary>
        /// Gets or sets whether ExtensionMethodInvoker should log any header or footer information.
        /// </summary>
        public bool WriteHeaders
        {
            get { return writeHeaders.HasValue ? writeHeaders.Value : true; }
            set { this.writeHeaders = value; }
        }

        /// <summary>
        /// Gets or sets whether ExtensionMethodInvoker should execute test rows in parallel.
        /// </summary>
        public bool ExecuteInParallel
        {
            get { return executeInParallel.HasValue ? executeInParallel.Value : false; }
            set { this.executeInParallel = value; }
        }

        /// <summary>
        /// Gets or sets the maximum degree of parallelism used by ExtensionMethodInvoker. Value 0 denotes default. Value must be in range from 0 to 63 inclusive.
        /// </summary>
        public int MaxDegreeOfParallelism
        {
            get { return maxDegreeOfParallelism.HasValue ? maxDegreeOfParallelism.Value : 0; }
            set { this.maxDegreeOfParallelism = value; }
        }

        public virtual ITestMethodInvoker OverrideTestMethodInvoker(ITestMethodInvoker testMethodInvoker, TestMethodInvokerContext originalInvokerContext, RowTestContext rowTestContext)
        {
            return testMethodInvoker;
        }

        internal static TestExecutionAttribute Aggregate(IEnumerable<TestExecutionAttribute> attributes)
        {
            return attributes.Aggregate(new TestExecutionAttribute(), Combine);
        }
        
        private static TestExecutionAttribute Combine(TestExecutionAttribute acc, TestExecutionAttribute item)
        {
            Contract.Requires(acc != null);
            Contract.Requires(item != null);

            if (item.logType.HasValue)
            {
                acc.logType = item.logType;
            }

            if (item.logContentType.HasValue)
            {
                acc.logContentType = item.logContentType;
            }

            if (item.writeHeaders.HasValue)
            {
                acc.writeHeaders = item.writeHeaders;
            }

            if (item.executeInParallel.HasValue)
            {
                acc.executeInParallel = item.executeInParallel;
            }

            if (item.maxDegreeOfParallelism.HasValue)
            {
                acc.maxDegreeOfParallelism = item.maxDegreeOfParallelism;
            }

            return acc;
        }
    }
}
