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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Test.VSUnitTest.TestTypeExtension.Log
{
    public abstract class LogWriter : ILogWriter
    {
        private static readonly LogWriter consoleWriter = Log.LogWriter.CreateConsoleLogWriter(LogDetailsType.Console, LogDetailsContentType.Text);

        private readonly LogDetailsType logType;
        private readonly LogDetailsContentType contentType;

        protected LogWriter(LogDetailsType logType, LogDetailsContentType contentType)
        {
            this.logType = logType;
            this.contentType = contentType;
        }

        public static LogWriter ConsoleWriter
        {
            get { return consoleWriter; }
        }

        public static LogWriter CreateTestContextLogWriter(TestContext testContext, LogDetailsType logType, LogDetailsContentType contentType)
        {
            return new TestContextLogWriter(testContext, logType, contentType);
        }

        public static LogWriter CreateTextWriterLogWriter(TextWriter textWriter, LogDetailsType logType, LogDetailsContentType contentType)
        {
            return new TextWriterLogWriter(textWriter, logType, contentType);
        }

        public static LogWriter CreateStringBuilderLogWriter(StringBuilder stringBuilder, LogDetailsType logType, LogDetailsContentType contentType)
        {
            return new StringBuilderLogWriter(stringBuilder, logType, contentType);
        }

        public static LogWriter CreateConsoleLogWriter(LogDetailsType logType, LogDetailsContentType contentType)
        {
            return new ConsoleLogWriter(logType, contentType);
        }

        public LogDetailsType LogType
        {
            get { return this.logType; }
        }

        public LogDetailsContentType LogContentType 
        {
            get { return this.contentType; }
        }

        public abstract void Flush();
        
        protected abstract void WriteLineToOutput(string str);
        protected abstract void WriteToOutput(string str);

        protected void WriteLineInternal(string str)
        {
            switch (this.contentType)
            {
                case LogDetailsContentType.Text:
                case LogDetailsContentType.Html:
                    this.WriteLineToOutput(str);
                    break;

                default:
                    Debug.Fail("Unexpected content type: " + this.contentType);
                    this.WriteLineToOutput(str);
                    break;
            }
        }

        protected void WriteInternal(string str)
        {
            this.WriteToOutput(str);
        }

        #region ILogWriter Members

        public void Write(string format, params object[] args)
        {
            this.WriteInternal(String.Format(format, args));
        }

        public void WriteString(string str)
        {
            this.WriteInternal(str);
        }

        public void WriteLine(string format, params object[] args)
        {
            this.WriteLineInternal(String.Format(format, args));
        }

        public void WriteLine()
        {
            this.WriteLineInternal(string.Empty);
        }

        public void WriteStringLine(string str)
        {
            this.WriteLineInternal(str);
        }

        #endregion

        #region Private Classes

        private sealed class TestContextLogWriter : LogWriter
        {
            private readonly TestContext testContext;
            private StringBuilder lineBuilder;

            public TestContextLogWriter(TestContext testContext, LogDetailsType logType, LogDetailsContentType contentType)
                : base(logType, contentType)
            {
                Debug.Assert(testContext != null, "testContext != null");

                this.testContext = testContext;
                this.lineBuilder = new StringBuilder();
            }

            protected override void WriteLineToOutput(string str)
            {
                if (this.lineBuilder.Length > 0)
                {
                    this.lineBuilder.Append(str);
                    this.testContext.WriteLine("{0}", this.lineBuilder.ToString());
                    this.lineBuilder.Clear();
                }
                else
                {
                    this.testContext.WriteLine("{0}", str);
                }
            }

            protected override void WriteToOutput(string str)
            {
                this.lineBuilder.Append(str);
            }

            public override void Flush()
            {
                if (lineBuilder.Length > 0)
                {
                    this.testContext.WriteLine("{0}", this.lineBuilder.ToString());
                    this.lineBuilder.Clear();
                }
            }
        }

        private sealed class TextWriterLogWriter : LogWriter
        {
            private readonly TextWriter textWriter;

            public TextWriterLogWriter(TextWriter textWriter, LogDetailsType logType, LogDetailsContentType contentType)
                : base(logType, contentType)
            {
                Debug.Assert(textWriter != null, "textWriter != null");

                this.textWriter = textWriter;
            }

            protected override void WriteLineToOutput(string str)
            {
                this.textWriter.WriteLine("{0}", str);
            }

            protected override void WriteToOutput(string str)
            {
                this.textWriter.Write("{0}", str);
            }

            public override void Flush()
            {
            }
        }

        private sealed class StringBuilderLogWriter : LogWriter
        {
            private readonly StringBuilder stringBuilder;

            public StringBuilderLogWriter(StringBuilder stringBuilder, LogDetailsType logType, LogDetailsContentType contentType)
                : base(logType, contentType)
            {
                Debug.Assert(stringBuilder != null, "stringBuilder != null");

                this.stringBuilder = stringBuilder;
            }

            protected override void WriteLineToOutput(string str)
            {
                this.stringBuilder.AppendLine(str);
            }

            protected override void WriteToOutput(string str)
            {
                this.stringBuilder.Append(str);
            }

            public override void Flush()
            {
            }
        }

        private sealed class ConsoleLogWriter : LogWriter
        {
            public ConsoleLogWriter(LogDetailsType logType, LogDetailsContentType contentType)
                : base(logType, contentType)
            {
            }

            protected override void WriteLineToOutput(string str)
            {
                Console.WriteLine("{0}", str);
            }

            protected override void WriteToOutput(string str)
            {
                Console.Write("{0}", str);
            }

            public override void Flush()
            {
            }
        }

        #endregion
    }
}
