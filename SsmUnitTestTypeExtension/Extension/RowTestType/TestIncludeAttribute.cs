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
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Specify a list of Ids to execute. All others on this TestMethod will be ignored.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class TestIncludeAttribute : TestIncludeAttributeBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TestIncludeAttribute()
        {
        }

        /// <summary>
        /// Create a list of Ids to execute tests for. All others on this TestMethod/TestClass will be ignored.
        /// </summary>
        /// <param name="idValues">The data input Id values to execute tests for.</param>
        public TestIncludeAttribute(params int[] idValues)
            : base(idValues)
        {
        }

        /// <summary>
        /// Create a list of Ids to execute tests for. All others on this TestMethod/TestClass will be ignored.
        /// </summary>
        /// <param name="idValues">The data input Id values to execute tests for.
        /// Ids can be in format of either [test id] or [method name].[test id]</param>
        public TestIncludeAttribute(params string[] idValues)
            : base(idValues)
        {
        }

        /// <summary>
        /// Show any skipped data inputs in the results.
        /// The default if not specified is to only show the data inputs executed.
        /// </summary>
        public bool ShowSkipped { get; set; }

        /// <summary>
        /// Gets or sets whether the specified tests should be included even if they are excluded by other filters.
        /// </summary>
        public bool ForceInclude { get; set; }

        public override FilterInfo GetFilterInfo(TestMethodInvokerContext invokerContext, DataRowValues dataRowValues)
        {
            FilterInfo result = FilterInfo.None;

            if (this.Matches(invokerContext, dataRowValues))
            {
                if (this.ForceInclude)
                {
                    result |= FilterInfo.Include;
                }
            }
            else 
            {
                result |= FilterInfo.Exclude;

                if (this.ShowSkipped)
                {
                    result |= FilterInfo.ShowSkipped;
                }
                else
                {
                    result |= FilterInfo.HideSkipped;
                }
            }

            return result;
        }
    }
}
