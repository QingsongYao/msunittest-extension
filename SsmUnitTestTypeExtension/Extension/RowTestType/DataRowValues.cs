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

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Provide data row values with some additional values such as a description, ignore state and defect number.
    /// Each data row equates to one parameterized method call.
    /// </summary>
    public class DataRowValues
    {
        private static object[] emptyValues = new object[0];
        private object[] values;
        private string desc;
        private bool ignore;
        private int defect;
        private string id;
        private Type expectedException;
        private Dictionary<string, object> properties;
        
        /// <summary>
        ///  Construct a new data row values.
        /// </summary>
        /// <param name="values">The array of values for the data row.</param>
        public DataRowValues()
        {
            this.values = emptyValues;
            this.desc = string.Empty;
        }

        /// <summary>
        /// The data row values for this data row input.
        /// </summary>
        public object[] Values
        {
            get { return this.values; }
            set { this.values = value; }
        }
        
        /// <summary>
        /// The description for this data input.
        /// </summary>
        public string Desc
        {
            get { return this.desc; }
            set { this.desc = value; }
        }
        
        /// <summary>
        /// Skip this data input if true.
        /// </summary>
        public bool Ignore
        {
            get { return this.ignore; }
            set { this.ignore = value; }
        }

        /// <summary>
        /// The defect number for this data input.
        /// </summary>
        public int Defect
        {
            get { return this.defect; }
            set { this.defect = value; }
        }

        /// <summary>
        /// The id number of this data input.
        /// Also used to indicate which data inputs are to be debugged or executed on a run.
        /// </summary>
        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// The expected exception for this row.
        /// Row will pass only if it throws the specified exception.
        /// </summary>
        public Type ExpectedException
        {
            get { return this.expectedException; }
            set { this.expectedException = value; }
        }

        /// <summary>
        /// The collection of named properties for this data input attribute.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get { return this.properties; }
            set { this.properties = value; }
        }
    }
}
