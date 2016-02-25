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
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Specify a list of Ids to execute. All others on this TestMethod will be ignored.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class TestIncludeAttributeBase : RowFilterAttribute
    {
        // Holds all the id values provided in our attribute
        protected readonly HashSet<string> idValues;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TestIncludeAttributeBase()
        {
        }

        /// <summary>
        /// Create a list of Ids to execute tests for. All others on this TestMethod will be ignored.
        /// </summary>
        /// <param name="idValues">The data input Id values to execute tests for.</param>
        public TestIncludeAttributeBase(params int[] idValues)
        {
            if (idValues.Length > 0)
            {
                this.idValues = new HashSet<string>(idValues.Select(id => Convert.ToString(id)));
            }
        }

        /// <summary>
        /// Create a list of Descs to execute tests for. All others on this TestMethod will be ignored.
        /// </summary>
        /// <param name="idValues">The data input Id values to execute tests for.</param>
        public TestIncludeAttributeBase(params string[] idValues)
        {
            if (idValues.Length > 0)
            {
                this.idValues = new HashSet<string>(idValues);
            }
        }

        protected bool Matches(TestMethodInvokerContext invokerContext, DataRowValues dataRowValues)
        {
            if (this.idValues != null &&
                (this.idValues.Contains(dataRowValues.Id) || this.idValues.Contains("*") || 
                this.idValues.Contains(invokerContext.TestMethodInfo.Name + "." + dataRowValues.Id) ||
                this.idValues.Contains(invokerContext.TestMethodInfo.Name + ".*")))
            {
                return true;
            }

            return false;
        }
    }
}
