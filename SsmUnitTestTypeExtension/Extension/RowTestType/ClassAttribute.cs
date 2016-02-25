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
    /// Attribute that will be used to denote our custom test class
    /// </summary>
    [Serializable]
    public class RowTestClassAttribute : TestClassExtensionAttribute
    {
        /// <summary>
        /// Id of this extention attribute
        /// </summary>
        private static readonly Uri uri = new Uri("urn:RowTestClassAttribute");

        public RowTestClassAttribute()
        {
        }

        /// <summary>
        /// Unique extension id to identify this extension attribute
        /// </summary>
        public override Uri ExtensionId
        {
            get { return uri; }
        }

        /// <summary>
        /// Gets the execution object for this extension object.  This object will contain 
        /// interfaces for our execution code
        /// </summary>
        /// <returns>Execution Extension object</returns>
        public override TestExtensionExecution GetExecution()
        {
            return new ExecutionExtension();
        }

        /// <summary>
        /// Gets any special UI code to display in the detailed test results
        /// </summary>
        /// <returns>Client Side Extension</returns>
        public override object GetClientSide()
        {
            return new ExtensionClientSide();
        }
    }
}
