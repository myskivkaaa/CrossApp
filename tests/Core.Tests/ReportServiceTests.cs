using Xunit;
using System.Linq;
using Core.Services;
using Core.Reports;
using Core;

public class ReportServiceTests
{
    [Theory]
    [InlineData(10, 2)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        Assert.Equal(expected, ReportService.LowStock(TestData.Items(), threshold).Count);
    }

    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        Assert.Equal(
            new[] { "SKU-BIG", "SKU-MID" },
            ReportService.TopByQuantity(TestData.Items(), 2).Select(i => i.Sku)
        );
    }
}