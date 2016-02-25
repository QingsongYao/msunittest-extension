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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Specify a list of Ids/Descs to execute. All others on this TestMethod will be ignored.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public abstract class RowFilterAttribute : Attribute
    {
        public RowFilterAttribute()
        {
        }

        public virtual FilterInfo GetFilterInfo(TestMethodInvokerContext invokerContext, DataRowValues dataRowValues)
        {
            return FilterInfo.None;
        }
    }

    /// <summary>
    /// Specify the filter info. 
    /// Row will be executed if there is at least one matching Include filter or no matching Exclude filters.
    /// </summary>
    [Flags]
    public enum FilterInfo
    {
        /// <summary>
        /// Default filter.
        /// </summary>
        None = 0,

        /// <summary>
        /// Include row.
        /// </summary>
        Include = 0x1,

        /// <summary>
        /// Exclude row.
        /// </summary>
        Exclude = 0x2,

        /// <summary>
        /// Hide skipped row.
        /// </summary>
        ShowSkipped = 0x4,

        /// <summary>
        /// Hide skipped row.
        /// </summary>
        HideSkipped = 0x8,
    }
}
