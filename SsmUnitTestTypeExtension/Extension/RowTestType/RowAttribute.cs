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
using System.Diagnostics;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Row attribute to access enumerable row data.
    /// Derived classes implement different enumeration sources and strategies.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class RowAttribute : Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="RowValues">The values we plan to hold for each iteration of the method</param>
        public RowAttribute(params object[] RowValues)
        {
            this.RowValues = RowValues;
            this.Desc = string.Empty;
        }

        /// <summary>
        /// The stored values
        /// </summary>
        [ReservedProperty]
        public object[] RowValues { get; set; }

        /// <summary>
        /// The collection of named properties for this data input attribute.
        /// </summary>
        [ReservedProperty]
        public string[] Properties { get; set; }

        /// <summary>
        /// Flag to indicate this attribute will need a custom list of key/value pairs from the test context for
        /// variable substitution in paths.
        /// </summary>
        [ReservedProperty]
        public virtual bool NeedsTestContextPairs
        {
            get
            {
                // The base RowAttribute does not need these prepared
                return false;
            }
        }

        /// <summary>
        /// The description for this data input attribute.
        /// </summary>
        public string Desc { get; set; }
        
        /// <summary>
        /// Skip this data input attribute if true.
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// The defect number for this data input attribute.
        /// </summary>
        public int Defect { get; set; }

        /// <summary>
        /// The expected exception for this row.
        /// Row will pass only if it throws the specified exception.
        /// </summary>
        public Type ExpectedException { get; set; }

        /// <summary>
        /// The id number of this data input.
        /// Also used to indicate which data inputs are to be debugged or executed on a run.
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// Perform any needed transformation of pathing data.
        /// Usually this entails expanding environment variables, any customVars passed in, or both.
        /// </summary>
        /// <param name="customExpand">Variable names and their expand values from the test context.</param>
        public virtual void SetExpandedPath(Dictionary<string, string> customExpand)
        {
            // The base RowAttribute does nothing since it doesn't use paths to access anything.
        }

        /// <summary>
        /// Enumerate row data.
        /// Each row this attribute represents is yielded.
        /// </summary>
        /// <param name="resourceAssembly">The assembly containing the resource stream to use. Used by derived row classes.</param>
        /// <param name="results">The helper to access param and formal arg type info. Used by derived row classes.</param>
        /// <returns>The data row values for this row to drive the test method.</returns>
        public virtual IEnumerable<DataRowValues> GetRowEnumerator(Assembly resourceAssembly, HelperTestGridResults results)
        {
            // The base RowAttribute just returns one row for any values it specifies
            // plus any metadata about the data row specified (Desc, Ignore, Defect, etc.).
            // Derived versions may open a data soruce and enumerate those.
            DataRowValues dataRowValues = new DataRowValues();
            dataRowValues.Values = this.RowValues;
            dataRowValues.Desc = this.Desc;
            dataRowValues.Ignore = this.Ignore;
            dataRowValues.Defect = this.Defect;
            dataRowValues.Id = Convert.ToString(this.Id);
            dataRowValues.ExpectedException = this.ExpectedException;
            dataRowValues.Properties = this.RetrieveProperties(this.Properties);

            yield return dataRowValues;
        }

        private Dictionary<string, object> RetrieveProperties(string[] properties)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            var allProperties = this.GetType().GetProperties();
            foreach (PropertyInfo property in allProperties
                .Where(p => p.CanRead && p.GetCustomAttributes(typeof(ReservedPropertyAttribute), true).Length == 0))
            {
                Debug.Assert(!result.ContainsKey(property.Name), "Duplicated property: '" + property.Name + "'");
                object value = property.GetValue(this, null);
                result.Add(property.Name, value);
            }

            if (properties != null)
            {
                foreach (string str in properties)
                {
                    int idx = str.IndexOf('=');

                    Debug.Assert(idx >= 0, "Property text does not contain = character: '" + str + "'");

                    string name = str.Substring(0, idx);
                    string value = str.Substring(idx + 1);

                    Debug.Assert(!result.ContainsKey(name), "Duplicated property: '" + name + "'");
                    result.Add(name, value);
                }
            }

            return result;
        }

        [AttributeUsage(AttributeTargets.Property)]
        protected class ReservedPropertyAttribute : Attribute
        {
        }
    }
}
