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
    /// EmbeddedXmlRows attribute to access an embedded XML data resource.
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class EmbeddedXmlRowsAttribute : RowAttribute
    {
        // The resource name to get an XML stream from
        private string xmlResourceName;
        // The element name representing the containing element for the row element children.
        private string xmlSectionElementName;
        // The element name representing each row to read
        private string xmlRowElementName;
        // List the valid resource names in the error message when the resource name is not found
        private bool listResourceNames;


        /// <summary>
        /// Construct an embedded XML row driver given an embedded resource name.
        /// </summary>
        /// <param name="xmlResourceName">The resource name to get an XML stream from.</param>
        /// <param name="xmlSectionElementName">The element name representing the containing element for the row element children.</param>
        /// <param name="xmlRowElementName">The element name representing each row to read.</param>
        public EmbeddedXmlRowsAttribute(string xmlResourceName, string xmlSectionElementName, string xmlRowElementName)
        {
            if (string.IsNullOrWhiteSpace(xmlResourceName))
            {
                throw new ArgumentException("The xmlResourceName cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(xmlSectionElementName))
            {
                throw new ArgumentException("The xmlSectionElementName cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(xmlRowElementName))
            {
                throw new ArgumentException("The xmlRowElementName cannot be null or empty");
            }

            this.xmlResourceName = xmlResourceName;
            this.xmlSectionElementName = xmlSectionElementName;
            this.xmlRowElementName = xmlRowElementName;
        }

        /// <summary>
        /// Construct an embedded XML row driver given an embedded resource name.
        /// </summary>
        /// <param name="xmlResourceName">The resource name to get an XML stream from.</param>
        /// <param name="xmlSectionElementName">The element name representing the containing element for the row element children.</param>
        /// <param name="xmlRowElementName">The element name representing each row to read.</param>
        /// <param name="listResourceNames">List the valid resource names in the error message when the resource name is not found.</param>
        public EmbeddedXmlRowsAttribute(string xmlResourceName, string xmlSectionElementName, string xmlRowElementName, bool listResourceNames)
            : this(xmlResourceName, xmlSectionElementName, xmlRowElementName)
        {
            // The default is not to list these
            this.listResourceNames = listResourceNames;
        }

        /// <summary>
        /// Flag to indicate this attribute will need a custom list of key/value pairs from the test context for
        /// variable substitution in paths.
        /// </summary>
        public override bool NeedsTestContextPairs
        {
            get
            {
                // Embedded resource paths currently do not need any expansions
                return false;
            }
        }

        /// <summary>
        /// Perform any needed transformation of pathing data.
        /// Usually this entails expanding environment variables, any customVars passed in, or both.
        /// </summary>
        /// <param name="customExpand">Variable names and their expand values from the test context.</param>
        public override void SetExpandedPath(Dictionary<string, string> customExpand)
        {
            // Embedded resource paths currently do not need any expansions
        }

        /// <summary>
        /// Enumerate row data.
        /// Each row this attribute represents is yielded.
        /// </summary>
        /// <param name="resourceAssembly">The assembly containing the resource stream to use.</param>
        /// <param name="results">The helper to access param and formal arg type info.</param>
        /// <returns>An enumeration of DataRowValues to drive test method.</returns>
        public override IEnumerable<DataRowValues> GetRowEnumerator(Assembly resourceAssembly, HelperTestGridResults results)
        {
            Stream stream = resourceAssembly.GetManifestResourceStream(this.xmlResourceName);

            if (stream == null)
            {
                // List the valid resource names one could use in the error message, if desired
                string resourceList = string.Empty;
                if (this.listResourceNames)
                {
                    resourceList = string.Format(" Valid resource names: {0}", string.Join(", ", resourceAssembly.GetManifestResourceNames()));
                }

                throw new FileNotFoundException(string.Format("The embedded resource '{0}' could not be found in the assembly '{1}'.{2}",
                    this.xmlResourceName, resourceAssembly.FullName, resourceList));
            }

            return HelperXml.GetRows(results, stream, this.xmlResourceName, this.xmlSectionElementName, this.xmlRowElementName);
        }
    }
}
