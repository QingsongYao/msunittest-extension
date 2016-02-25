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
using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.RowTest
{
    public class SortableBindingList<T> : BindingList<T>
    {
        private readonly List<T> list;
        private ListSortDirection sortDirection;
        private PropertyDescriptor sortProperty;

        public SortableBindingList(List<T> list)
        {
            Debug.Assert(list != null, "list != null");

            this.list = list;

            bool oldRaiseListChangedEvents = this.RaiseListChangedEvents;
            this.RaiseListChangedEvents = false;

            try
            {
                this.SetItems(list);
            }
            finally
            {
                this.RaiseListChangedEvents = oldRaiseListChangedEvents;
                ResetBindings();
            }
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            Debug.Assert(property != null, "property != null");

            bool oldRaiseListChangedEvents = this.RaiseListChangedEvents;
            this.RaiseListChangedEvents = false;

            try
            {
                this.sortDirection = (property == sortProperty && sortDirection == ListSortDirection.Ascending) ?
                    ListSortDirection.Descending : ListSortDirection.Ascending;
                this.sortProperty = property;

                IEnumerable<T> orderedList;
                if (property.PropertyType == typeof(string))
                {
                    orderedList = this.list.OrderBy(x => property.GetValue(x), StringLogicalComparer.Instance);
                }
                else
                {
                    orderedList = this.list.OrderBy(x => property.GetValue(x));
                }

                var newList = this.sortDirection == ListSortDirection.Descending ? orderedList.Reverse().ToList() : orderedList.ToList();

                SetItems(newList);
            }
            finally
            {
                this.RaiseListChangedEvents = oldRaiseListChangedEvents;
                ResetBindings();
            }
        }

        protected override void RemoveSortCore()
        {
            SetItems(this.list);
        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return sortDirection; }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return sortProperty; }
        }

        private void SetItems(List<T> items)
        {
            Debug.Assert(items != null, "items != null");
            base.ClearItems();

            for (int i = 0; i < items.Count; i++)
            {
                base.Add(items[i]);
            }
        }
    }
}
