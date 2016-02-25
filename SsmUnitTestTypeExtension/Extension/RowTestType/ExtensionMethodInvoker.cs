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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Diagnostics.Contracts;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Invoker class object
    /// </summary>
    public class ExtensionMethodInvoker : ITestMethodInvoker
    {
        /// <summary>
        /// Test Context member
        /// </summary>
        private TestMethodInvokerContext invokerContext;

        /// <summary>
        /// Invoker constructor.  Allows you to save off the context
        /// </summary>
        /// <param name="InvokerContext">context of the test/run</param>
        public ExtensionMethodInvoker(TestMethodInvokerContext InvokerContext)
        {
            this.invokerContext = InvokerContext;
        }

        /// <summary>
        /// The method that gets called when a test is executed
        /// </summary>
        /// <param name="args">arguments that are passed from the test signature</param>
        /// <returns></returns>
        public TestMethodInvokerResult Invoke(params object[] args)
        {
            // Our helper results class to aggregate our test results
            // Make sure we dispose it when done to get rid of our XmlTextWriter
            using (HelperTestGridResults results = new HelperTestGridResults(this.invokerContext.TestMethodInfo))
            {
                Assembly testMethodAssembly = this.invokerContext.TestMethodInfo.DeclaringType.Assembly;

                // Gets the custom attribute off the method
                RowAttribute[] rows = (RowAttribute[])this.invokerContext.TestMethodInfo.GetCustomAttributes(typeof(RowAttribute), false);

                IList<RowFilterAttribute> rowFilters = AttributeHelper.GetCustomAttributes<RowFilterAttribute>(this.invokerContext.TestMethodInfo, true);

                IList<TestExecutionAttribute> testExecutionAttributes = AttributeHelper.GetCustomAttributes<TestExecutionAttribute>(this.invokerContext.TestMethodInfo, true);
                TestExecutionAttribute testExecutionAggregateAttr = TestExecutionAttribute.Aggregate(testExecutionAttributes);

                Log.LogWriter logWriter = CreateLogWriter(testExecutionAggregateAttr);

                SetExpandedPaths(rows, this.invokerContext);
                IEnumerable<DataRowValues> dataRowValuesCollection = GetAllDataRowValues(rows, this.invokerContext, results);

                IEnumerable<RowTestContext> inputRowContexts = dataRowValuesCollection.Select(dataRowValues =>
                    {
                        object[] valuesArray = results.PrepareRowValues(dataRowValues);

                        RowTestContext rowTestContext = new RowTestContext(dataRowValues)
                        {
                            LogType = testExecutionAggregateAttr.LogType,
                            LogContentType = testExecutionAggregateAttr.LogContentType,
                            LogWriter = logWriter,
                            WriteHeaders = testExecutionAggregateAttr.WriteHeaders,
                            ValuesArray = valuesArray,
                            TestExecutionAttributes = testExecutionAttributes
                        };

                        PopulateRowSkippedInfo(invokerContext, rowTestContext, rowFilters);

                        return rowTestContext;
                    });

                IEnumerable<RowTestContext> outputRowContexts;

                if (testExecutionAggregateAttr.ExecuteInParallel)
                {
                    ParallelQuery<RowTestContext> query = inputRowContexts.AsParallel().AsOrdered();
                    if (testExecutionAggregateAttr.MaxDegreeOfParallelism > 0)
                    {
                        query = query.WithDegreeOfParallelism(testExecutionAggregateAttr.MaxDegreeOfParallelism);
                    }

                    outputRowContexts = query.Select(this.InvokeSingleRow);
                }
                else
                {
                    outputRowContexts = inputRowContexts.Select(this.InvokeSingleRow);
                }

                
                // enumerate row data and invoke the test case with each yield they give us
                foreach (RowTestContext rowTestContext in outputRowContexts)
                {
                    // Add results to our aggregator if it is either executed, or skipped but we are showing skipped rows in results
                    if (!rowTestContext.Ignored || rowTestContext.ShowSkipped)
                    {
                        results.AddTestResult(rowTestContext);
                    }
                }

                // Return the aggregated results for the row variatons in the TestMethod 
                return results.GetAllResults();
            }
        }
        

        private RowTestContext InvokeSingleRow(RowTestContext rowTestContext)
        {
            if (rowTestContext.Ignored)
            {
                return rowTestContext;
            }

            if (rowTestContext.LogType == Log.LogDetailsType.RowDetails)
            {
                rowTestContext.LogBuilder = new StringBuilder();
                rowTestContext.LogWriter = Log.LogWriter.CreateStringBuilderLogWriter(rowTestContext.LogBuilder, rowTestContext.LogType, rowTestContext.LogContentType);
            }

            if (rowTestContext.WriteHeaders)
            {
                if (rowTestContext.LogContentType == Log.LogDetailsContentType.Html)
                {
                    rowTestContext.LogWriter.WriteLine("<pre>");
                    rowTestContext.LogWriter.Write("<b>");
                }

                // Output header info as to which data input row we are now on,
                // so any WriteLines or exception messages are diusplayed in-context under it.
                // This is the best we can do to partition the output per data input row, since MSTest does not know
                // that one of its Testmethods is really logically broken down into N rows.

                rowTestContext.LogWriter.Write("[Test {0}] {1}", rowTestContext.DataRowValues.Id, rowTestContext.DataRowValues.Desc);

                if (rowTestContext.LogContentType == Log.LogDetailsContentType.Html)
                {
                    rowTestContext.LogWriter.Write("</b>");
                }

                rowTestContext.LogWriter.WriteLine(string.Empty);
            }

            // Execute the parameterize test method for the current data input row values we parsed.
            // The invocation context will have a non-null Exception property if something threw within it
            // (since MSTest catches it first to record it for their purposes).
            // So no try/catch wrapper or rethrowing is necessary on our part.
            RowTestContext.Current = rowTestContext;

            // Give every TestExecutionAttribute an option to override default Test Method Invoker.
            ITestMethodInvoker testMethodInvoker = rowTestContext.TestExecutionAttributes.Aggregate(
                this.invokerContext.InnerInvoker,
                (ITestMethodInvoker invoker, TestExecutionAttribute attr) =>
                    attr.OverrideTestMethodInvoker(invoker, this.invokerContext, rowTestContext));

            rowTestContext.InvokerResult = testMethodInvoker.Invoke(rowTestContext.ValuesArray);

            RowTestContext.Current = null;

            rowTestContext.ResultException = rowTestContext.InvokerResult.Exception;

            // Use the inner exception as the meaningful one for us, unless there isn't one.
            // Normally the "target of invocation" outer exception is just noise for our purposes,
            // unless there is nothing else to show.
            if (rowTestContext.ResultException != null && 
                rowTestContext.ResultException.GetType() == typeof(TargetInvocationException) && 
                rowTestContext.ResultException.InnerException != null)
            {
                rowTestContext.ResultException = rowTestContext.ResultException.InnerException;
            }

            // Check for expected exception.
            if (rowTestContext.DataRowValues.ExpectedException != null)
            {
                if (rowTestContext.ResultException != null)
                {
                    if (rowTestContext.DataRowValues.ExpectedException.IsAssignableFrom(rowTestContext.ResultException.GetType()))
                    {
                        rowTestContext.ResultException = null;
                    }
                    else
                    {
                        rowTestContext.ResultException = new AssertFailedException("Expected '" + rowTestContext.DataRowValues.ExpectedException.FullName + "' exception, but different exception has been thrown. See inner exception.", rowTestContext.ResultException);
                    }
                }
                else
                {
                    rowTestContext.ResultException = new AssertFailedException("Expected '" + rowTestContext.DataRowValues.ExpectedException.FullName + "' exception, but no exception has been thrown.");
                }
            }

            // Log exception
            if (rowTestContext.ResultException != null)
            {
                rowTestContext.ResultMessage = rowTestContext.ResultException.Message;

                if (rowTestContext.WriteHeaders)
                {
                    StringBuilder sb = new StringBuilder();
                    WriteException(sb, rowTestContext.LogContentType, rowTestContext.ResultException);
                    rowTestContext.LogWriter.WriteLine(sb.ToString());
                }
            }

            if (rowTestContext.WriteHeaders)
            {
                // Output data input row end
                rowTestContext.LogWriter.WriteStringLine("--------------------------");
                rowTestContext.LogWriter.WriteStringLine(string.Empty);

                if (rowTestContext.LogContentType == Log.LogDetailsContentType.Html)
                {
                    rowTestContext.LogWriter.WriteStringLine("</pre>");
                }
            }

            ((Log.LogWriter)rowTestContext.LogWriter).Flush();

            if (rowTestContext.LogType == Log.LogDetailsType.RowDetails)
            {
                rowTestContext.CustomValues.Add("Details", rowTestContext.LogBuilder.ToString());
                rowTestContext.CustomValues.Add("DetailsContentType", rowTestContext.LogContentType.ToString());
            }

            return rowTestContext;
        }


        private Log.LogWriter CreateLogWriter(TestExecutionAttribute testExecutionAttr)
        {
            switch (testExecutionAttr.LogType)
            {
                case Log.LogDetailsType.TestContext:
                    return Log.LogWriter.CreateTestContextLogWriter(this.invokerContext.TestContext, testExecutionAttr.LogType, testExecutionAttr.LogContentType);

                case Log.LogDetailsType.Console:
                    return Log.LogWriter.CreateConsoleLogWriter(testExecutionAttr.LogType, testExecutionAttr.LogContentType);

                case Log.LogDetailsType.RowDetails:
                    return null;

                default:
                    Debug.Fail("Unexpected log type:" + testExecutionAttr.LogType);
                    return Log.LogWriter.ConsoleWriter;
            }
        }

        private static void WriteException(StringBuilder sb, Log.LogDetailsContentType contentType, Exception exception)
        {
            WriteException(sb, contentType, exception, "Exception");
        }

        private static void WriteException(StringBuilder sb, Log.LogDetailsContentType contentType, Exception exception, string header)
        {
            if (contentType == Log.LogDetailsContentType.Html)
            {
                sb.Append("<b>");
            }

            sb.AppendFormat("[{0}] {1}: {2}", header, exception.GetType().Name, exception.Message);

            if (contentType == Log.LogDetailsContentType.Html)
            {
                sb.Append("</b>");
            }

            sb.AppendLine();
            sb.AppendLine(exception.StackTrace);

            if (exception is AggregateException)
            {
                int idx = 0;
                foreach (var innerException in ((AggregateException)exception).InnerExceptions)
                {
                    idx++;
                    WriteException(sb, contentType, innerException, "Inner Exception (" + idx + " of " + ((AggregateException)exception).InnerExceptions.Count + ")");
                }
            }
            else if (exception.InnerException != null)
            {
                WriteException(sb, contentType, exception.InnerException, "Inner Exception");
            }
        }

        private static void SetExpandedPaths(RowAttribute[] rows, TestMethodInvokerContext invokerContext)
        {
            Debug.Assert(rows != null, "rows != null");
            Debug.Assert(invokerContext != null, "invokerContext != null");

            Dictionary<string, string> testContextPairs = null;

            // Process each RowAttribute, or derived attribute
            foreach (RowAttribute attr in rows)
            {
                // Prepare a custom list of test context key/value pairs is the row attribute needs it
                if (attr.NeedsTestContextPairs)
                {
                    if (testContextPairs == null)
                    {
                        testContextPairs = new Dictionary<string, string>();

                        foreach (object key in invokerContext.TestContext.Properties.Keys)
                        {
                            object value = invokerContext.TestContext.Properties[key];
                            testContextPairs.Add(key.ToString(), value.ToString());
                        }
                    }

                    // Let the row driver adjust its paths for its own purposes
                    attr.SetExpandedPath(testContextPairs);
                }
            }
        }

        private static void PopulateRowSkippedInfo(TestMethodInvokerContext invokerContext, RowTestContext rowTestContext, IList<RowFilterAttribute> filters)
        {
            FilterInfo filterInfo = filters.Select(filter => filter.GetFilterInfo(invokerContext, rowTestContext.DataRowValues))
                .Aggregate(FilterInfo.None, (acc, f) => acc | f);

            rowTestContext.Ignored = false;
            rowTestContext.ShowSkipped = true;

            // Process execution flags.
            if ((filterInfo & FilterInfo.Exclude) != FilterInfo.None)
            {
                rowTestContext.Ignored = true;
            }

            if ((filterInfo & FilterInfo.Include) != FilterInfo.None)
            {
                rowTestContext.Ignored = false;
            }

            // Process display flags
            if ((filterInfo & FilterInfo.HideSkipped) != FilterInfo.None)
            {
                rowTestContext.ShowSkipped = false;
            }

            if ((filterInfo & FilterInfo.ShowSkipped) != FilterInfo.None)
            {
                rowTestContext.ShowSkipped = true;
            }

            if (rowTestContext.DataRowValues.Ignore)
            {
                rowTestContext.Ignored = true;
            }
        }

        private static IEnumerable<DataRowValues> GetAllDataRowValues(RowAttribute[] rows, TestMethodInvokerContext invokerContext, HelperTestGridResults results)
        {
            Debug.Assert(rows != null, "rows != null");
            Debug.Assert(invokerContext != null, "invokerContext != null");
            Debug.Assert(results != null, "results != null");

            if (rows.Length > 0)
            {
                Assembly testMethodAssembly = invokerContext.TestMethodInfo.DeclaringType.Assembly;

                // Process each RowAttribute, or derived attribute
                foreach (RowAttribute attr in rows)
                {
                    // enumerate row data and invoke the test case with each yield they give us
                    foreach (DataRowValues dataRowValues in attr.GetRowEnumerator(testMethodAssembly, results))
                    {
                        if (attr.Ignore)
                        {
                            dataRowValues.Ignore = true;
                        }

                        yield return dataRowValues;
                    }
                }
            }
            else
            {
                yield return new DataRowValues();
            }
        }

        /// <summary>
        /// Get the formatted text for an exception, including its inner exceptions and stack traces at each level.
        /// </summary>
        /// <param name="e">The exception to format.</param>
        /// <returns>The formatted exception text.</returns>
        internal static string GetExceptionText(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            while (e != null)
            {
                sb.Append(sb.Length == 0 ? "[Error Message] " : "[Inner Exception] ");
                string s = e.Message;
                if (s.StartsWith(e.GetType().Name))
                {
                    sb.AppendLine(s);
                }
                else
                {
                    sb.AppendLine(string.Format(", Inner exception {0}: {1}", e.GetType().Name, s));
                }


                // Output the stack trace if we have one
                if (!string.IsNullOrWhiteSpace(e.StackTrace))
                {
                    sb.AppendLine("[Error Stack Trace]");
                    sb.Append(e.StackTrace);
                }

                e = e.InnerException;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escape any format string { and } to {{ and }} since TextContext.WriteLine's only overload
        /// will throw if it sees substitution points without data to sub.
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns></returns>
        private string EscapeWriteLineText(string s)
        {
            // Since the one and only WriteLine in TestContext expects a format string, we must
            // escape any single '{' as '{{' and single '}' as '}}' before passing it on.
            //
            // For example, if the msg really tries to display a literal "{0}" as part of its text,
            // it will throw an error unless we escape (i.e. double-up) the brackets first.
            return s.Replace("{", "{{").Replace("}", "}}");
        }
    }
}
