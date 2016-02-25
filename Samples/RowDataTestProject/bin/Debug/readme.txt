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


For VS UI test runs
-------------------
- This is part of the overall InstallSsmUnitTestTools.cmd, or can be run standalone.

- InstallSsmUnitTestTypeExtension.cmd is our own script that does the same job as the MSI, but needs a runas Admin shell to work
  (it pre-dates the MSI, but is instructive to keep to show what actually has to happen to install an extension)


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
A build of this project is sufficient to use the command-line enlistment version of mstest since the archived TestTools.xml already has it,
and the binplace to %TESTBIN% when you build it puts it in one of the prober path we cause to be used from our .testsettings <Directory> additions.

    :: This is just to document what must be present for other extensions
    :: Test execution outside VS from the command line version of mstest uses a virtualized Registry file in testsrc\mpu\tools\external\mstest\v11.0\bin\TestTools.xml
    :: Test execution inside VS UI uses the VS registry entries and extension dll copied to IDE\PrivateAssemblies, which our script or isntaller takes care of
    ::(i.e., the VS version of mstest in IDE folder must use the real Registry; the one in our enlistment is a special version with the virtualized registry from a file) 

    The virtualized entries in the XML file look like this, corresponding to what the real registry has in it for VS UI to use:

            <!-- SSM Unit Test Type Extension for RowTestClass (nest inside the UNIT TEST Key in TestTools.xml) for the virtualized registry version of mstest -->
            <Key name="TestTypeExtensions">
              <Key name="RowTestClassAttribute">
                <Value name="AttributeProvider" value="Microsoft.Test.VSUnitTest.UnitTestTypeExtension.RowTest.RowTestClassAttribute, Microsoft.Test.VSUnitTest.UnitTestTypeExtension" />
              </Key>
            </Key>
            <!-- SSM Unit Test Type Extension for RunAsTestClass (nest inside the UNIT TEST Key in TestTools.xml) for the virtualized registry version of mstest -->
            <Key name="TestTypeExtensions">
              <Key name="RunAsTestClassAttribute">
                <Value name="AttributeProvider" value="Microsoft.Test.VSUnitTest.UnitTestTypeExtension.RunAsTest.RunAsTestClassAttribute, Microsoft.Test.VSUnitTest.UnitTestTypeExtension" />
              </Key>
            </Key>


