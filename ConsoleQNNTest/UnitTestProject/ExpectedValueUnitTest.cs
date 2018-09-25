using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace XUnitTestProject
{
    public class QNNChunkedMeasureEntanglementTests
    {
        [Fact]
        public void DriverTargets()
        {
            int increment = 50;

            for (int i=1; i<=1; i++)
            {
                int count = increment * i;

                string args = string.Join(" ", "-c", count.ToString());

                // Program.Main(args);
            }
        }
    }
}
