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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// The status of a test method iteration.
    /// If skipped, the test method was never invoked.
    /// </summary>
    public enum HelperTestStatus
    {
        Passed = 0,
        Failed,
        Skipped
    }

    /// <summary>
    /// Helper Test Grid Result class
    /// </summary>
    public class HelperTestGridResults : IDisposable
    {
        /// <summary>
        /// Number of row variations in the TestMethod.
        /// </summary>
        private int rowCount = 0;
        /// <summary>
        /// Number of rows which passed.
        /// </summary>
        private int passedCount = 0;
        /// <summary>
        /// Number of rows which failed.
        /// </summary>
        private int failureCount = 0;
        /// <summary>
        /// Number of rows which were skipped.
        /// </summary>
        private int skippedCount = 0;
        /// <summary>
        /// Will be used when we provide the results for the results detail
        /// </summary>
        private XmlDocument doc;
        private XmlWriter xw;
        ParameterInfo[] formalArgs;

        // Whether the args have DataRowValues arg.
        bool isDataRowValuesArg;

        // The number of formal args after prior to a params arg. Regular non args are after any special args.
        int nonParamsArgCount;

        // Remember if the last parameter is a 'params', since it means we need to fabricate
        // a single column value representing all the param value, to keep it all rectangular and simple
        // when we bind it to the grid.
        bool isParams;
        // Also remember if it is 'params string[]' so we can generate "string1", "string2", ... for the grid
        // instead of as-is param1, param2, ...
        bool isParamsStringType;
        /// Gets the type of the last formal argument'e element type, if it is a param arg.
        /// This is null if there is no param formal arg at the end.
        Type paramArgElementType;

        // Stores the exception of the row test. If there was only one row test executed, this exception is used as the test result exception.
        Exception singleRowTestException;

        /// <summary>
        /// Make a new test results object to accumulate row results
        /// The doc ends up looking like this for a row variation TestMethod invocation:
        ///   RowTest
        ///     Row *
        ///       Icon
        ///       Status
        ///       Id
        ///       Defect
        ///       Desc
        ///       (dynamic_names)*
        ///       Message
        /// </summary>
        public HelperTestGridResults(MethodInfo mi)
        {
            // Generate the xml representing a row variations test
            this.doc = new XmlDocument();
            this.xw = this.doc.CreateNavigator().AppendChild();

            // Root element
            this.xw.WriteStartElement("RowTest");

            // Rows element containing all the Row outputs
            // The RowSummary element will follow all this at the end
            this.xw.WriteStartElement("Rows");

            this.formalArgs = mi.GetParameters();

            int idx = 0;

            // Check for DataRowValues arg.
            if (idx < this.formalArgs.Length && typeof(DataRowValues).IsAssignableFrom(this.formalArgs[idx].ParameterType))
            {
                this.isDataRowValuesArg = true;
                idx++;
            }

            // Add each of the dynamic column names from the method info formal parameter names
            this.nonParamsArgCount = this.formalArgs.Length - idx;

            // Check for params arg.
            if (this.nonParamsArgCount > 0)
            {
                ParameterInfo lastFormalArg = this.formalArgs.Last();
                this.isParams = Attribute.IsDefined(lastFormalArg, typeof(ParamArrayAttribute));
                if (this.isParams)
                {
                    this.nonParamsArgCount = this.formalArgs.Length - 1;
                    this.paramArgElementType = lastFormalArg.ParameterType.GetElementType();
                    this.isParamsStringType = (paramArgElementType == typeof(string));
                }
                else
                {
                    this.nonParamsArgCount = this.formalArgs.Length;
                }
            }
        }

        /// <summary>
        /// Get rid of the writer
        /// </summary>
        public void Dispose()
        {
            if (this.xw != null)
            {
                this.xw.Close();
                this.xw = null;
            }
        }
        
        /// <summary>
        /// Gets whether the test method signature has params-style arguments in it.
        /// </summary>
        public bool IsParams
        {
            get
            {
                return this.isParams;
            }
        }

        /// <summary>
        /// Gets the formal arguments parameter info.
        /// </summary>
        public ParameterInfo[] FormalArgs
        {
            get
            {
                return this.formalArgs;
            }
        }

        /// <summary>
        /// Gets the type of the last formal argument'e element type, if it is a param arg.
        /// This is null if there is no param formal arg at the end.
        /// </summary>
        public Type ParamArgElementType
        {
            get
            {
                return this.paramArgElementType;
            }
        }

        /// <summary>
        /// Gets the number of formal arguments preceding any params argument. If no params argument is present, this counts all formal arguments.
        /// </summary>
        public int NonParamsArgCount
        {
            get
            {
                return this.nonParamsArgCount;
            }
        }

        /// <summary>
        /// Adds a result to the list of results we have
        /// </summary>
        /// <param name="rowTestContext">The row context of the iteration</param>
        public void AddTestResult(RowTestContext rowTestContext)
        {
            Contract.Requires(rowTestContext != null);
            HelperTestStatus testStatus = HelperTestStatus.Passed;
            if (rowTestContext.Ignored)
            {
                // Skipped trumps the other statuses since it means the test method was not even invoked
                testStatus = HelperTestStatus.Skipped;
            }
            else if (rowTestContext.ResultException != null)
            {
                // The iteration is considered Failed if an exception was filled in
                testStatus = HelperTestStatus.Failed;
            }

            this.rowCount++;

            // Add header row
            // The row number is added to the row header later when we display the grid
            this.xw.WriteStartElement("Row");

            // The pass/fail/skip status
            this.xw.WriteStartElement("Status");
            switch (testStatus)
            {
                case HelperTestStatus.Failed:
                    this.failureCount++;
                    this.xw.WriteString("Failed");
                    break;

                case HelperTestStatus.Skipped:
                    this.skippedCount++;
                    this.xw.WriteString("Skipped");
                    break;

                case HelperTestStatus.Passed:
                    this.passedCount++;
                    this.xw.WriteString("Passed");
                    break;
            }         
            this.xw.WriteEndElement();

            // Add the Id, or leave blank if zero
            this.xw.WriteStartElement("Id");
            if (!string.IsNullOrWhiteSpace(rowTestContext.DataRowValues.Id) && rowTestContext.DataRowValues.Id != "0")
            {
                this.xw.WriteValue(rowTestContext.DataRowValues.Id);
            }
            this.xw.WriteEndElement();

            // Add the Defect, or leave blank if zero
            this.xw.WriteStartElement("Defect");
            if (rowTestContext.DataRowValues.Defect != 0)
            {
                this.xw.WriteValue(rowTestContext.DataRowValues.Defect);
            }
            this.xw.WriteEndElement();

            // Add the Desc
            this.xw.WriteStartElement("Desc");
            this.xw.WriteString(rowTestContext.DataRowValues.Desc);
            this.xw.WriteEndElement();

            StringBuilder parametersBuilder = new StringBuilder();

            // Add the method arg data columns for the row
            for (int i = 0; i < this.formalArgs.Length; i++)
            {
                if (i > 0) parametersBuilder.Append(", ");

                object value = rowTestContext.ValuesArray[i];

                parametersBuilder.Append(this.formalArgs[i].Name);
                parametersBuilder.Append("=");

                // If the very last formal arg is a 'params', we must concatenate all remaining values
                // into one big column value to keep this simple and rectangular in the grid
                if (i == this.formalArgs.Length - 1 && this.isParams)
                {
                    // Join the remaining params values into a single string to write out
                    IEnumerable<string> paramsStrings;
                    if (this.ParamArgElementType == typeof(string))
                    {
                        paramsStrings = (string[])value;
                    }
                    else
                    {
                        Array valueArray = (Array)value;
                        object[] paramsObjects = new object[valueArray.Length];
                        valueArray.CopyTo(paramsObjects, 0);

                        int paramsLength = paramsObjects.Length;
                        paramsStrings = paramsObjects.Select(o => o.ToString());
                    }

                    parametersBuilder.Append(string.Join(", ", paramsStrings));
                }
                else
                {
                    parametersBuilder.Append(value != null ? value.ToString() : "null");
                }
            }

            this.xw.WriteStartElement("Parameters");
            this.xw.WriteString(parametersBuilder.ToString());
            this.xw.WriteEndElement();

            // The last column is the exception message, if any
            this.xw.WriteStartElement("Message");
            if (rowTestContext.ResultException != null)
            {
                this.xw.WriteString(rowTestContext.ResultException.Message);
                this.singleRowTestException = rowTestContext.ResultException;
            }

            this.xw.WriteEndElement();

            if (rowTestContext.CustomValues != null)
            {
                foreach (var pair in rowTestContext.CustomValues)
                {
                    this.xw.WriteStartElement(pair.Key);
                    this.xw.WriteString(pair.Value);
                    this.xw.WriteEndElement();
                }
            }

            // Close the row
            this.xw.WriteEndElement();
        }
   
        /// <summary>
        /// Gets the full run results to return to the client
        /// </summary>
        /// <returns>TestMethodInvokerResult to return to client</returns>
        public TestMethodInvokerResult GetAllResults()
        {
            // This is the results that will be included in the .trx <ExtensionResult> element for the TestMethod
            // (i.e. all the row results are in it)
            TestMethodInvokerResult tmir = new TestMethodInvokerResult();

            // Close the Rows element
            this.xw.WriteEndElement();

            string summary = string.Format("{0} rows, {1} passed, {2} failed, {3} skipped. See test details.",
                this.rowCount, this.passedCount, this.failureCount, this.skippedCount);

            // Write the summary element after all the rows
            this.xw.WriteStartElement("RowSummary");
            this.xw.WriteString(summary);
            this.xw.WriteEndElement();

            // Close the RowTest root element
            this.xw.Close();

            // This is the whole XML section we made which gets serialized and output to the .trx
            // and used to populate the summary label and DataGridView if you show results on-screen
            tmir.ExtensionResult = this.doc.InnerXml;

            if (this.failureCount > 0)
            {
                tmir.Exception = this.rowCount == 1 ? this.singleRowTestException : new AssertFailedException(summary);              
            }

            return tmir;
        }

        /// <summary>
        /// Transform row input values if necessary for use in an invocation
        /// expecting a flat object[] array.
        /// </summary>
        /// <param name="dataRowValues">The input row values array.</param>
        /// <returns>An array of objects which can be invocation argsuments.</returns>
        public object[] PrepareRowValues(DataRowValues dataRowValues)
        {
            object[] valuesArray = new object[this.formalArgs.Length];
            int idx = 0;
            
            // Populate special args.
            if (this.isDataRowValuesArg)
            {
                valuesArray[idx++] = dataRowValues;
            }

            object[] inRowValues = dataRowValues.Values;
            int inRowIdx = 0;

            // Populate provided non params args.
            for (; idx < this.nonParamsArgCount && inRowIdx < inRowValues.Length; idx++, inRowIdx++)
            {
                valuesArray[idx] = inRowValues[inRowIdx];
            }

            // Populate optional non params args.
            for (; idx < this.nonParamsArgCount; idx++)
            {
                if (this.formalArgs[idx].IsOptional)
                {
                    valuesArray[idx] = this.formalArgs[idx].DefaultValue;
                }
                else if (this.formalArgs[idx].ParameterType.IsValueType)
                {
                    valuesArray[idx] = Activator.CreateInstance(this.formalArgs[idx].ParameterType);
                }
                else
                {
                    valuesArray[idx] = null;
                }
            }

            if (this.isParams)
            {
                int paramsLength = inRowValues.Length - inRowIdx;
                Array paramsArray = Array.CreateInstance(this.ParamArgElementType, paramsLength);
                Array.Copy(inRowValues, inRowIdx, paramsArray, 0, paramsLength);

                inRowIdx += paramsLength;
                valuesArray[idx++] = paramsArray;
            }

            return valuesArray;
        }
    }
}
