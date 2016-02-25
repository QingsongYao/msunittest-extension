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
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.Common;
using System.Diagnostics;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    public partial class ExtensionViewer : UserControl, Microsoft.VisualStudio.TestTools.Vsip.ITestTypeExtensionResultViewer
    {
        private static System.Drawing.Icon TestPassedIcon = Properties.Resources.TestPassed;
        private static System.Drawing.Icon TestFailedIcon = Properties.Resources.TestFailed;
        private static System.Drawing.Icon TestSkippedIcon = Properties.Resources.Unknown;

        private readonly int statusGridColumnIndex;

        private BindingList<ExtensionGridRow> bindingList;
        private DataTable dataTable;
        
        public ExtensionViewer()
        {
            InitializeComponent();

            dataGridView1.AutoGenerateColumns = false;
            this.InitializeDataGridColumns();

            this.statusGridColumnIndex = this.dataGridView1.Columns.IndexOf(this.dataGridView1.Columns["Status"]);
            dataGridView1.CellContentClick += new DataGridViewCellEventHandler(dataGridView1_CellContentClick);
        }

        private void InitializeDataGridColumns()
        {
            DataGridViewColumn iconColumn = new DataGridViewImageColumn();
            iconColumn.Name = "Icon";
            iconColumn.DataPropertyName = "Icon";
            iconColumn.HeaderText = "";
            iconColumn.ReadOnly = true;
            iconColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            iconColumn.Resizable = DataGridViewTriState.True;
            dataGridView1.Columns.Add(iconColumn);

            DataGridViewColumn statusColumn = new DataGridViewLinkColumn();
            statusColumn.Name = "Status";
            statusColumn.DataPropertyName = "Status";
            statusColumn.HeaderText = "Status";
            statusColumn.ReadOnly = true;
            statusColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            statusColumn.Resizable = DataGridViewTriState.True;
            dataGridView1.Columns.Add(statusColumn);

            DataGridViewColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.Name = "Id";
            idColumn.DataPropertyName = "Id";
            idColumn.HeaderText = "Id";
            idColumn.ReadOnly = true;
            idColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            idColumn.Resizable = DataGridViewTriState.True;
            dataGridView1.Columns.Add(idColumn);

            DataGridViewColumn descriptionColumn = new DataGridViewTextBoxColumn();
            descriptionColumn.Name = "Description";
            descriptionColumn.DataPropertyName = "Description";
            descriptionColumn.HeaderText = "Description";
            descriptionColumn.ReadOnly = true;
            descriptionColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            descriptionColumn.Resizable = DataGridViewTriState.True;
            dataGridView1.Columns.Add(descriptionColumn);

            DataGridViewColumn messageColumn = new DataGridViewTextBoxColumn();
            messageColumn.Name = "Message";
            messageColumn.DataPropertyName = "Message";
            messageColumn.HeaderText = "Message";
            messageColumn.ReadOnly = true;
            messageColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            messageColumn.Resizable = DataGridViewTriState.True;
            dataGridView1.Columns.Add(messageColumn);

            DataGridViewColumn parametersColumn = new DataGridViewTextBoxColumn();
            parametersColumn.Name = "Parameters";
            parametersColumn.DataPropertyName = "Parameters";
            parametersColumn.HeaderText = "Parameters";
            parametersColumn.ReadOnly = true;
            parametersColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            parametersColumn.Resizable = DataGridViewTriState.True;
            dataGridView1.Columns.Add(parametersColumn);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == this.statusGridColumnIndex && e.RowIndex >= 0)
            {
                ExtensionGridRow gridRow = this.bindingList[e.RowIndex];

                RowDetailsForm rowDetailsForm = new RowDetailsForm(this.dataTable.Rows[gridRow.DataRowIndex]);
                rowDetailsForm.ShowDialog();
            }
        }

        #region ITestTypeExtensionResultViewer Members

        public void Initialize(TestResult result)
        {
            try
            {
                ITestResultExtension rowResult = result as ITestResultExtension;
                if (rowResult != null)
                {
                    string xml = rowResult.ExtensionResult as string;

                    // Bind the test result data to the grid
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml((string)rowResult.ExtensionResult);

                    // RowTest is our root element
                    //   RowSummary is the first child (for the label)
                    //   Rows is the second child (containing all the Row results to databind to the grid)
                    XmlNode rowTest = xdoc.DocumentElement;
                    XmlNode rows = rowTest.SelectSingleNode("Rows");
                    XmlNode rowSummary = rowTest.SelectSingleNode("RowSummary");

                    // Put the summary string into the label above the grid
                    label1.Text = rowSummary.InnerText;

                    // Data bind the Rows to the DataGridView, but only if there is any data there
                    // (empty result sets do not generate any row data within the <Rows> element)
                    if (rows.HasChildNodes)
                    {
                        StringReader sr = new StringReader(rows.OuterXml);
                        XmlTextReader xr = new XmlTextReader(sr);

                        DataSet ds = new DataSet("RowTest");
                        ds.ReadXml(xr);

                        this.dataTable = ds.Tables[0];

                        List<ExtensionGridRow> gridRows = GetGridRows(this.dataTable);
                        this.bindingList = new SortableBindingList<ExtensionGridRow>(gridRows);
                        this.dataGridView1.DataSource = this.bindingList;

                        this.allRowDetails.SetDetails(dataTable.Rows);

                        if (gridRows.Count == 1)
                        {
                            this.tabControl1.SelectedTab = allTestRowDetailsPage;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string debug = Environment.GetEnvironmentVariable("VSUNIT_DEBUGUNITTESTTYPEEXTENSION");
                if (debug != null && debug == "1")
                {
                    string msg = "ExtensionViewer.Initialize(TestResult) threw an exception:\n\n" + ExtensionMethodInvoker.GetExceptionText(e);
                    MessageBox.Show(msg, "SsmUnitTypeExtension Error");
                }
                else
                {
                    // Rethrow and do nothing unless the env var flag is set
                    throw;
                }
            }           
        }

        private static List<ExtensionGridRow> GetGridRows(DataTable dataTable)
        {
            Debug.Assert(dataTable != null, "dataTable != null");

            List<ExtensionGridRow> gridRows = new List<ExtensionGridRow>(dataTable.Rows.Count);

            int statusColumnIndex = dataTable.Columns.IndexOf(dataTable.Columns["Status"]);
            int idColumnIndex = dataTable.Columns.IndexOf(dataTable.Columns["Id"]);
            int descColumnIndex = dataTable.Columns.IndexOf(dataTable.Columns["Desc"]);
            int messageColumnIndex = dataTable.Columns.IndexOf(dataTable.Columns["Message"]);
            int parametersColumnIndex = dataTable.Columns.IndexOf(dataTable.Columns["Parameters"]);

            for (int idx = 0; idx < dataTable.Rows.Count; idx++)
            {
                DataRow dataRow = dataTable.Rows[idx];
                ExtensionGridRow gridRow = new ExtensionGridRow(idx);

                switch (dataRow.Field<string>(statusColumnIndex))
                {
                    case "Passed": gridRow.Icon = TestPassedIcon; break;
                    case "Skipped": gridRow.Icon = TestSkippedIcon; break;
                    default: gridRow.Icon = TestFailedIcon; break;
                }

                gridRow.Status = dataRow.Field<string>(statusColumnIndex);
                gridRow.Id = dataRow.Field<string>(idColumnIndex);
                gridRow.Description = dataRow.Field<string>(descColumnIndex);
                gridRow.Message = dataRow.Field<string>(messageColumnIndex);
                gridRow.Parameters = dataRow.Field<string>(parametersColumnIndex);

                gridRows.Add(gridRow);
            }

            return gridRows;
        }

        #endregion
    }
}
