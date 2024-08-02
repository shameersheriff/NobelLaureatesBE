using System.Net;
using Moq;
using Moq.Protected;
using NobelLaureatesBE.BusinessLogic.Services;
using NobelLaureatesBE.BusinessLogic.Interfaces;

namespace NobelLaureatesBE.Tests
{
    [TestFixture]
    public class NobelPrizeServiceTests : IDisposable
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private INobelPrizeService _nobelPrizeService;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _nobelPrizeService = new NobelPrizeService(_httpClient);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task GetNobelLaureatesAsync_ReturnsExpectedResult()
        {
            // Arrange
            var expectedResponse = "{\"result\":\"success\"}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            // Act
            var result = await _nobelPrizeService.GetNobelLaureatesAsync(10, 5, "male", "1900-01-01", "2000-01-01", "che");

            // Assert
            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public async Task GetNobelLaureateAsync_ReturnsExpectedResult()
        {
            // Arrange
            var expectedResponse = "{\"result\":\"success\"}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            // Act
            var result = await _nobelPrizeService.GetNobelLaureateAsync(1);

            // Assert
            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public void GetNobelLaureatesAsync_ThrowsException_WhenResponseIsNotSuccess()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                });

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(() => _nobelPrizeService.GetNobelLaureatesAsync(10, 5, "male", "1900-01-01", "2000-01-01", "che"));
        }

        [Test]
        public void GetNobelLaureateAsync_ThrowsException_WhenResponseIsNotSuccess()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                });

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(() => _nobelPrizeService.GetNobelLaureateAsync(1));
        }
    }
}
