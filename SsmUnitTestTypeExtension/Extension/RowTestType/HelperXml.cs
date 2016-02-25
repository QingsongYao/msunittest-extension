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
using System.Xml.XPath;


namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    /// <summary>
    /// Utilities for reading XML streams for data-driven row enumeration.
    /// </summary>
    internal static class HelperXml
    {
        /// <summary>
        /// Any XML stream source should use this method to yield row data.
        /// Both embedded and external XML files use this.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="stream"></param>
        /// <param name="source"></param>
        /// <param name="section"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static IEnumerable<DataRowValues> GetRows(HelperTestGridResults results, Stream stream, string source, string section, string row)
        {
            XPathDocument document = new XPathDocument(stream);
            XPathNavigator navigator = document.CreateNavigator();

            var sectionNode = navigator.SelectSingleNode("//" + section);

            if (sectionNode != null)
            {
                for (var rowNodes = sectionNode.Select(row); rowNodes.MoveNext(); )
                {
                    var rowNode = rowNodes.Current;

                    DataRowValues dataRowValues = ReadRow(results, rowNodes.Current);
                    yield return dataRowValues;
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("The section element '{0}' was not found in the XML stream from '{1}'.",
                    section, source));
            }
        }

        private static DataRowValues ReadRow(HelperTestGridResults results, XPathNavigator rowNode)
        {
            List<object> rowValuesList = new List<object>();

            DataRowValues dataRowValues = new DataRowValues();

            rowValuesList.Clear();

            for (var elementNodes = rowNode.Select("*"); elementNodes.MoveNext(); )
            {
                var elementNode = elementNodes.Current;

                switch (elementNode.Name)
                {
                    case "Desc": dataRowValues.Desc = elementNode.Value; break;
                    case "Ignore": dataRowValues.Ignore = elementNode.ValueAsBoolean; break;

                    // Parse Bug also for back compat before we renamed it to Defect
                    case "Bug":
                    case "Defect": dataRowValues.Defect = elementNode.ValueAsInt; break;
                    case "Id": dataRowValues.Id = elementNode.Value; break;

                    case "Properties":
                        dataRowValues.Properties = new Dictionary<string, object>();
                        for (var propertyNodes = elementNode.Select("*"); propertyNodes.MoveNext(); )
                        {
                            var propertyNode = propertyNodes.Current;
                            dataRowValues.Properties[propertyNode.Name] = propertyNode.Value;
                        }

                        break;

                    default:
                        // Each value is typed according to the test method formal args
                        // If params are present in the signature, then the type of every optional value is the params element type
                        Type dataType;
                        if (rowValuesList.Count < results.NonParamsArgCount)
                        {
                            dataType = results.FormalArgs[rowValuesList.Count].ParameterType;
                        }
                        else
                        {
                            dataType = results.ParamArgElementType;
                        }

                        object value = elementNode.ValueAs(dataType, null);
                        rowValuesList.Add(value);

                        break;
                }
            }

            dataRowValues.Values = rowValuesList.ToArray();
            return dataRowValues;
        }
    }
}
