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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    public partial class RowDetails : UserControl
    {
        public RowDetails()
        {
            InitializeComponent();

            this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            this.webBrowser1.StatusTextChanged += new EventHandler(webBrowser1_StatusTextChanged);
        }

        public string CaptionText { get; set; }

        public void SetDetails(DataRow dataRow)
        {
            Debug.Assert(dataRow != null, "dataRow != null");

            this.CaptionText = String.Format("Test row details, id: {0}", dataRow["Id"]);
            this.webBrowser1.DocumentText = GetDocumentText(dataRow);
        }

        public void SetDetails(DataRowCollection allDataRows)
        {
            Debug.Assert(allDataRows != null, "allDataRows != null");

            this.CaptionText = String.Format("All test row details");

            StringBuilder sb = new StringBuilder();
            foreach (DataRow dataRow in allDataRows)
            {
                sb.Append(GetDocumentText(dataRow));
            }

            this.webBrowser1.DocumentText = sb.ToString();
        }

        void webBrowser1_StatusTextChanged(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = this.webBrowser1.StatusText;
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.webBrowser1_Navigating);
            
            if (string.IsNullOrWhiteSpace(this.webBrowser1.Document.Body.Style))
            {
                this.webBrowser1.Document.Body.Style = "font-family: calibri,sans-serif; font-size: 10pt";
            }   
            
            foreach (HtmlElement elem in this.webBrowser1.Document.Links)
            {
                elem.Click += new HtmlElementEventHandler(elem_Click);
            }
        }

        void elem_Click(object sender, HtmlElementEventArgs e)
        {
            HtmlElement elem = (HtmlElement)sender;

            string href = elem.GetAttribute("href");

            Uri hrefUri = new Uri(href);

            if (hrefUri.Scheme == "shell")
            {
                string arguments = System.Web.HttpUtility.UrlDecode(hrefUri.LocalPath.Substring(1));

                if (hrefUri.Host.ToLower() == "start")
                {
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(arguments);
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.Start();
                }
                else
                {
                    Process process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = hrefUri.Host;
                    process.StartInfo.Arguments = arguments;
                    process.Start();
                }

                e.ReturnValue = false;
                e.BubbleEvent = false;
            }   
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            e.Cancel = true;
        }

        private static string GetDocumentText(DataRow dataRow)
        {
            string detailsStr = dataRow.Table.Columns.Contains("Details") ? dataRow["Details"].ToString() : string.Empty;
            string detailsTypeStr = dataRow.Table.Columns.Contains("DetailsContentType") ? dataRow["DetailsContentType"].ToString() : string.Empty;
            
            switch (detailsTypeStr.ToLower())
            {
                case "html":
                    return detailsStr;
                default:
                    return detailsStr.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\n", "\n<br/>");
            }
        }
    }
}
