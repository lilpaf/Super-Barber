using Microsoft.AspNetCore.Hosting;
using Moq;


namespace SuperBarber.UnitTests.Mocks
{
    public static class IWebHostEnviromentMock
    {
        public static Mock<IWebHostEnvironment> MockIWebHostEnvironment()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            //The setup web path skips this path SuperBarber.Tests/bin/debug/net6.0
            mockEnvironment
                .Setup(m => m.WebRootPath)
                .Returns("../../../../SuperBarber/wwwroot");

            return mockEnvironment;
        }
    }
}
