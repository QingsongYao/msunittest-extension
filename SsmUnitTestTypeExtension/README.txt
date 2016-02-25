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



Create and deploy the unit test type extension assembly and related registration.


For enlistment use on command line trun use
-------------------------------------------

- Build Extension project either in VS2012 or via msbuild or build.exe (it uses the same .csproj for any of those methods).

- The resulting assembly is published to bin\debug or bin\release folder. We run it from there for comand line test execution.


For either use
--------------
These need to be in the runtime path somewhere for either command line or VS use

    Microsoft.VisualStudio.QualityTools.Common.dll (from IDE\PrivateAseemblies)
    Microsoft.VisualStudio.QualityTools.Vsip.dll (from the VS SDK)
Both VS2012 files are copied and checked-in into resource folder.

Only built on-demand
--------------------
- The SampleTestProject is just an example of how to use this extension. Not necessary to build.


Details on what is going on during our script (or the equivalent installer) for VS UI use
---------------------------------------------------------------------------------------
The VS copy and reg must be performed once on a given machine with VS 2011 on it to register our extension to use [RowTestClass] and [RunAsTestClass] in tests.
    :: This is just to document what must be present for other extensions
    :: Test execution inside VS UI uses the VS registry entries and extension dll copied to IDE\PrivateAssemblies, which our script or isntaller takes care of