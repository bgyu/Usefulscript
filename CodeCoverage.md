# How to Create C# Code Coverage Report
Assuming you have a C# Library project (.netstandard2.0)


## Create testing library project and unit test project
```c#
namespace MathLib
{
    public class Basic
    {
        public int Add(int x, int y)
        {
            return x + y;
        }

        public int Sub(int x, int y)
        {
            return x - y;
        }
    }
}
```

And a C# unit test (.netframework or .netcore)
```C#
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

using MathLib;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestAdd()
        {
            Basic b = new Basic();
            Assert.AreEqual(5, b.Add(2, 3));
        }

        [TestMethod]
        public void TestSub()
        {
            Basic b = new Basic();
            Assert.AreEqual(3, b.Sub(5, 2));
        }
    }
}
```

## Use Microsoft.CodeCovorage.Console.exe (VS Enterprise only) to collect test run coverage report
To Get Test coverage, compile above two projects, can be debug or release, x64/x86/anycpu.

Then use `Microsoft.CodeCoverage.Console.exe` (VS Enterprise) tool to generate code coverage (default is .coverage type, but can use `-f xml` to convert to xml type):
```bash
Microsoft.CodeCoverage.Console.exe collect -f xml -o "test.xml" vstest.console.exe UnitTestProject1\bin\x64\Release\UnitTestProject1.dll

# Help
Microsoft.CodeCoverage.Console.exe collect --help
```

Above code to use `Microsoft.CodeCoverage.Console.exe collect` to collect test coverage run by `vstest.console.exe`.

Then install reportgenerator (which converts xml code coverage report to human readable html web pages).
```dotnet tool install --global dotnet-reportgenerator-globaltool```

## Convert coverage/xml Report To Human Readable HTML report

If the report is in `.coverage` format, you can also use `Microsoft.CodeCoverage.Console.exe merge` to merge and convert `.converage` report to xml report:
```bash
Microsoft.CodeCoverage.Console.exe merge test1.coverage test2.coverage -o testcoverage.xml -f xml

# Help
Microsoft.CodeCoverage.Console.exe merge --help
```

Generate html report:
```bash
reportgenerator -reports:test.xml -targetdir:coverage

# Help
reportgenerator -h
```

In coverage folder, you will see lots of html files. You can open report start with index.html.

