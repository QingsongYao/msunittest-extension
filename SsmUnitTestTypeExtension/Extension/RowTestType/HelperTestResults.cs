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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Helper Test Result class
    /// </summary>
    public class HelperTestResults
    {
        /// <summary>
        /// Member variables
        /// </summary>
        private int rowCount = 0;
        /// <summary>
        /// Lets us know if there is a failure somewhere in the list of results
        /// </summary>
        private bool hasTestFailures = false;
        /// <summary>
        /// Will be used when we provide the results for the results detail
        /// </summary>
        private StringBuilder resultStringBuilder = new StringBuilder();

        /// <summary>
        /// Adds a result to the list of results we have
        /// </summary>
        /// <param name="Result">The result of the iteration</param>
        /// <param name="RowValues">Row Values that were used for the iteration</param>
        public void AddTestResult(TestMethodInvokerResult Result, object[] RowValues)
        {
            this.rowCount++;
            writeResultHeader(RowValues);
            writeTestOutcome(Result);
        }

        /// <summary>
        /// Gets the full run results to return to the client
        /// </summary>
        /// <returns>TestMethodInvokerResult to return to client</returns>
        public TestMethodInvokerResult GetAllResults()
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Tested {0} rows \n\n", this.rowCount);
            sb.AppendLine();
            sb.Append(this.resultStringBuilder.ToString());

            // This is the results that will be included in the .trx <ExtensionResult> element for the TestMethod
            // (i.e. all the row results are in it)
            TestMethodInvokerResult tmir = new TestMethodInvokerResult();
            tmir.ExtensionResult = sb.ToString();

            if (this.hasTestFailures)
            {
                tmir.Exception = new Exception("see test details");
            }

            return tmir;
        }

        /// <summary>
        /// Writes failure information if iteration failed
        /// </summary>
        /// <param name="Result">failed result incl any exceptionh</param>
        private void writeFailureMessage(TestMethodInvokerResult Result)
        {
            this.resultStringBuilder.AppendLine("failed");
            Exception ex = Result.Exception;
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            this.resultStringBuilder.AppendLine(ex.Message);
        }

        /// <summary>
        /// Writes the result of the iteration to the results string builder
        /// </summary>
        /// <param name="Result">The result for this iteration</param>
        private void writeTestOutcome(TestMethodInvokerResult Result)
        {
            this.resultStringBuilder.Append("Outcome: ");

            if (Result.Exception != null)
            {
                this.hasTestFailures = true;
                writeFailureMessage(Result);
            }
            else
            {
                this.resultStringBuilder.AppendLine("passed");
            }
            this.resultStringBuilder.AppendLine("--------------------------");
            this.resultStringBuilder.AppendLine();
            return;
        }

        /// <summary>
        /// writes header information to our results string builder
        /// </summary>
        /// <param name="RowValues">Row values that were used when executing the test</param>
        private void writeResultHeader(object[] RowValues)
        {
            this.resultStringBuilder.AppendFormat("Row {0} execution \n", this.rowCount);
            this.resultStringBuilder.Append("Row input values: ");
            foreach (var o in RowValues)
            {
                this.resultStringBuilder.AppendFormat(o.ToString() + " ");
            }

            this.resultStringBuilder.AppendLine();
        }

    }
}
