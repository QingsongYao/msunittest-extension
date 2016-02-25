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
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension
{
    public static class AttributeHelper
    {
        /// <summary>
        /// Returns the matching attributes on given method in order from *least* relevant to *most* relevant.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInfo"></param>
        /// <param name="deepInherit"></param>
        /// <returns></returns>
        public static IList<T> GetCustomAttributes<T>(MethodInfo methodInfo, bool deepInherit = false)
            where T : Attribute
        {
            List<ICustomAttributeProvider> attributeProviders = new List<ICustomAttributeProvider>();
            GetAllAttributeProviders(methodInfo, deepInherit, attributeProviders);

            return attributeProviders.SelectMany(ap => GetCustomAttributes<T>(ap, false)).ToArray();
        }

        public static IList<T> GetCustomAttributes<T>(ICustomAttributeProvider attributeProvider, bool inherit)
        {
            Contract.Requires(attributeProvider != null);

            return attributeProvider.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
        }

        private static void GetAllAttributeProviders(MethodInfo methodInfo, bool deepInherit, List<ICustomAttributeProvider> result)
        {
            if (methodInfo != null)
            {
                // Return attribute providers in order from least relevant to most relevant.

                if (deepInherit)
                {
                    GetAllAttributeProviders(methodInfo.ReflectedType, deepInherit, result);
                }

                if (!result.Contains(methodInfo))
                {
                    result.Add(methodInfo);
                }
            }
        }

        private static void GetAllAttributeProviders(Type type, bool deepInherit, List<ICustomAttributeProvider> result)
        {
            if (type != null)
            {
                // Return attribute providers in order from least relevant to most relevant.

                if (deepInherit)
                {
                    GetAllAttributeProviders(type.BaseType, deepInherit, result);
                    GetAllAttributeProviders(type.Assembly, deepInherit, result);
                    GetAllAttributeProviders(type.DeclaringType, deepInherit, result);
                }

                if (!result.Contains(type))
                {
                    result.Add(type);
                }
            }
        }

        private static void GetAllAttributeProviders(Assembly assembly, bool deepInherit, List<ICustomAttributeProvider> result)
        {
            if (assembly != null)
            {
                // Return attribute providers in order from least relevant to most relevant.

                if (!result.Contains(assembly))
                {
                    result.Add(assembly);
                }
            }
        }
    }
}
