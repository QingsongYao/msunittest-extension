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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// XmlFileRows attribute to access an external XML data resource.
    /// The path will expand environment vairiables and test context variables.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class XmlFileRowsAttribute : RowAttribute
    {
        // The external file path to get an XML stream from
        private string xmlPath;
        // The expanded external file path with any %envvar% and %testcontext% vars expanded
        private string xmlExpandedPath;
        // The element name representing the containing element for the row element children.
        private string xmlSectionElementName;
        // The element name representing each row to read
        private string xmlRowElementName;


        /// <summary>
        /// Construct an embedded XML row driver given an external file path.
        /// </summary>
        /// <param name="xmlPath">The external file path to get an XML stream from.</param>
        /// <param name="xmlSectionElementName">The element name representing the containing element for the row element children.</param>
        /// <param name="xmlRowElementName">The element name representing each row to read.</param>
        public XmlFileRowsAttribute(string xmlPath, string xmlSectionElementName, string xmlRowElementName)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                throw new ArgumentException("The xmlPath cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(xmlSectionElementName))
            {
                throw new ArgumentException("The xmlSectionElementName cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(xmlRowElementName))
            {
                throw new ArgumentException("The xmlRowElementName cannot be null or empty");
            }

            this.xmlPath = xmlPath;
            this.xmlExpandedPath = xmlPath;
            this.xmlSectionElementName = xmlSectionElementName;
            this.xmlRowElementName = xmlRowElementName;
        }

        /// <summary>
        /// Flag to indicate this attribute will need a custom list of key/value pairs from the test context for
        /// variable substitution in paths.
        /// </summary>
        public override bool NeedsTestContextPairs
        {
            get
            {
                // External file paths do use the custom list for expanding values
                return true;
            }
        }

        /// <summary>
        /// Perform any needed transformation of pathing data.
        /// Usually this entails expanding environment variables, any customVars passed in, or both.
        /// </summary>
        /// <param name="customExpand">Variable names and their expand values from the test context.</param>
        public override void SetExpandedPath(Dictionary<string, string> customExpand)
        {
            string newPath = this.xmlPath;

            // Expand any %var% which are matched in the custom list first
            foreach (string key in customExpand.Keys)
            {
                newPath = newPath.Replace("%" + key + "%", customExpand[key]);
            }

            // Expand any environment variables last
            newPath = Environment.ExpandEnvironmentVariables(newPath);

            this.xmlExpandedPath = newPath;
        }

        /// <summary>
        /// Enumerate row data.
        /// Each row this attribute represents is yielded.
        /// </summary>
        /// <param name="resourceAssembly">The assembly containing the resource stream. Unused for external file access.</param>
        /// <param name="results">The helper to access param and formal arg type info.</param>
        /// <returns>An enumeration of DataRowValues to drive test method.</returns>
        public override IEnumerable<DataRowValues> GetRowEnumerator(Assembly resourceAssembly, HelperTestGridResults results)
        {
            Stream stream = File.OpenRead(this.xmlExpandedPath);

            if (stream == null)
            {
                throw new FileNotFoundException(string.Format("The path '{0}' expanded from '{1}' could not be found.",
                    this.xmlExpandedPath, this.xmlPath));
            }

            return HelperXml.GetRows(results, stream, this.xmlExpandedPath, this.xmlSectionElementName, this.xmlRowElementName);
        }
    }
}
