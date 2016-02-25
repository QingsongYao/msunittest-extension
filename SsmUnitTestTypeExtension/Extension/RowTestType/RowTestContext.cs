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
using System.Linq;
using System.Text;
using Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest;
using Log = Microsoft.Test.VSUnitTest.TestTypeExtension.Log;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    public class RowTestContext
    {
        [ThreadStatic]
        private static RowTestContext current;

        private readonly Dictionary<string, object> properties;
        private readonly DataRowValues dataRowValues;
        private readonly Dictionary<string, string> customValues;

        public static RowTestContext Current
        {
            get { return current; }
            internal set { current = value; }
        }

        internal RowTestContext(DataRowValues dataRowValues)
        {
            Contract.Requires(dataRowValues != null);

            this.dataRowValues = dataRowValues;

            this.properties = dataRowValues.Properties != null ? new Dictionary<string, object>(dataRowValues.Properties) : new Dictionary<string, object>();

            this.customValues = new Dictionary<string, string>();
        }

        public DataRowValues DataRowValues
        {
            get { return this.dataRowValues; }
        }

        public Dictionary<string, object> Properties
        {
            get { return this.properties; }
        }

        public string ResultMessage { get; set; }

        public Log.ILogWriter LogWriter { get; internal set; }

        internal Dictionary<string, string> CustomValues
        {
            get { return this.customValues; }
        }

        internal bool Ignored { get;set; }
        internal bool ShowSkipped { get; set; }
        internal StringBuilder LogBuilder { get; set; }
        internal TestMethodInvokerResult InvokerResult { get; set; }
        internal bool WriteHeaders { get; set; }
        internal Log.LogDetailsType LogType { get; set; }
        internal Log.LogDetailsContentType LogContentType { get; set; }
        internal object[] ValuesArray { get; set; }
        internal IList<TestExecutionAttribute> TestExecutionAttributes { get; set; }
        internal Exception ResultException { get; set; }
    }
}
