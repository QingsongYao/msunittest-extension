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
    /// Execution Extension object
    /// </summary>
    public class ExecutionExtension : TestExtensionExecution
    {
        /// <summary>
        /// Returns the object that will be used to invoke the test method
        /// </summary>
        /// <param name="InvokerContext">Contains information about the run and the test being executed</param>
        /// <returns>Invoker object</returns>
        public override ITestMethodInvoker CreateTestMethodInvoker(TestMethodInvokerContext InvokerContext)
        { 
            return new ExtensionMethodInvoker(InvokerContext);
        }

        /// <summary>
        /// Clean up any native resources you have created here
        /// </summary>
        public override void Dispose()
        {
            //TODO: Free, release or reset native resources
        }

        /// <summary>
        /// Initialize any resources or objects you may need for the test to run.
        /// You can also wire up event handlers you may want to intercept
        /// </summary>
        /// <param name="Execution"></param>
        public override void Initialize(TestExecution Execution)
        {
            //TODO: Wire up event handlers for test events if needed
        }
    }


}
