using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Microsoft.SqlServer.Test.VSUnitTest.SsmUnitTestTypeExtension.RowTest
{
    /// <summary>
    /// Specify which data input Ids to break on in the debugger.
    /// The debugger must already be attached for this to do anything.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class TestDebugAttribute : Attribute
    {
        // Holds all the id values provided in our attribute
        private int[] idValues;

        /// <summary>
        /// Create a list of Ids to break on in the debugger.
        /// </summary>
        /// <param name="idValues">The data input Id values to break on in the debugger.</param>
        public TestDebugAttribute(params int[] idValues)
        {
            this.idValues = idValues;
        }

        /// <summary>
        /// The stored Id values
        /// </summary>
        public int[] IdValues
        {
            get { return this.idValues; }
        }
    }
}
