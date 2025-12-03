using Moq.Protected;
using Moq;
using Newtonsoft.Json;
using org.cchmc.{{cookiecutter.namespace}}.auth.Models;
using org.cchmc.{{cookiecutter.namespace}}.auth.Services;
using System;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Linq;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.AuthTests
{
    [TestClass]
    public sealed class SingleSignOnServiceUnitTests
    {
        private SingleSignOnService _service;
        private Mock<ILogger<SingleSignOnService>> _logger;
        private HttpClient _httpClient;
        private Mock<HttpMessageHandler> _mockMessageHandler;

        [TestInitialize]
        public void Initialize()
        {
            _mockMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            _logger = new Mock<ILogger<SingleSignOnService>>();
            _service = new SingleSignOnService(_httpClient, _logger.Object);
        }

        private static HttpResponseMessage BuildMockMessage<T>(HttpStatusCode statusCode, T response)
        {
            var resp = new HttpResponseMessage(statusCode) { Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json") };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        #region CompleteSignIn
        [TestMethod]
        public async Task CompleteSignIn_Success()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";

            var jwt = new AuthResponse()
            {
                ExpiresOnUtc = DateTime.UtcNow.AddHours(20),
                RefreshToken = "Fhgjkjsdfasdtyfgg",
                User = new SingleSignOnUser()
                {
                    FirstName = "hjlmnhgh",
                    Department = "haaefasd",
                    Email = "Funby",
                    EmployeeNumber = "imkytfg",
                    Id = Guid.NewGuid().ToString(),
                    LastName = "ilnkbyhg",
                    LastSignIn = DateTime.UtcNow.AddSeconds(-1),
                    Roles = ["fnukjfdg", "yilnhjh"],
                    ThumbnailPhoto = "awevawetser",
                    Title = "dbjtotukjsf",
                    UserName = "minubfgfads"
                }
            };

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/CompleteSignIn")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(BuildMockMessage(HttpStatusCode.OK, jwt))
                               .Verifiable();

            var result = await _service.CompleteSignIn(token, ip);

            Assert.IsNotNull(result);
            Assert.AreEqual(jwt.ExpiresOnUtc, result.ExpiresOnUtc);
            Assert.AreEqual(jwt.RefreshToken, result.RefreshToken);
            Assert.IsNotNull(result.User);
            Assert.AreEqual(jwt.User.Department, result.User.Department);
            Assert.AreEqual(jwt.User.Email, result.User.Email);
            Assert.AreEqual(jwt.User.EmployeeNumber, result.User.EmployeeNumber);
            Assert.AreEqual(jwt.User.FirstName, result.User.FirstName);
            Assert.AreEqual(jwt.User.Id, result.User.Id);
            Assert.AreEqual(jwt.User.LastName, result.User.LastName);
            Assert.AreEqual(jwt.User.LastSignIn, result.User.LastSignIn);
            Assert.AreEqual(string.Join(",", jwt.User.Roles), string.Join(",", result.User.Roles));
            Assert.AreEqual(jwt.User.ThumbnailPhoto, result.User.ThumbnailPhoto);
            Assert.AreEqual(jwt.User.Title, result.User.Title);
            Assert.AreEqual(jwt.User.UserName, result.User.UserName);
        }

        [TestMethod]
        public async Task CompleteSignIn_ServiceBadResponse_ReturnsNull()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/CompleteSignIn")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(BuildMockMessage<AuthResponse>(HttpStatusCode.Forbidden, null))
                               .Verifiable();

            var result = await _service.CompleteSignIn(token, ip);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CompleteSignIn_ServiceException_Rethrows()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/CompleteSignIn")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .Throws(new Exception())
                               .Verifiable();

            bool caughtException = false;

            try
            {
                await _service.CompleteSignIn(token, ip);
            }
            catch (Exception)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);
        }
        #endregion

        #region RefreshToken
        [TestMethod]
        public async Task RefreshToken_Success()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";
            var appKey = "gfhjhdsdfadgfk";

            var jwt = new AuthResponse()
            {
                ExpiresOnUtc = DateTime.UtcNow.AddHours(20),
                RefreshToken = "Fhgjkjsdfasdtyfgg",
                User = new SingleSignOnUser()
                {
                    FirstName = "hjlmnhgh",
                    Department = "haaefasd",
                    Email = "Funby",
                    EmployeeNumber = "imkytfg",
                    Id = Guid.NewGuid().ToString(),
                    LastName = "ilnkbyhg",
                    LastSignIn = DateTime.UtcNow.AddSeconds(-1),
                    Roles = ["fnukjfdg", "yilnhjh"],
                    ThumbnailPhoto = "awevawetser",
                    Title = "dbjtotukjsf",
                    UserName = "minubfgfads"
                }
            };

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/RefreshToken")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(appKey)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(BuildMockMessage(HttpStatusCode.OK, jwt))
                               .Verifiable();

            var result = await _service.RefreshToken(token, appKey, ip);

            Assert.IsNotNull(result);
            Assert.AreEqual(jwt.ExpiresOnUtc, result.ExpiresOnUtc);
            Assert.AreEqual(jwt.RefreshToken, result.RefreshToken);
            Assert.IsNotNull(result.User);
            Assert.AreEqual(jwt.User.Department, result.User.Department);
            Assert.AreEqual(jwt.User.Email, result.User.Email);
            Assert.AreEqual(jwt.User.EmployeeNumber, result.User.EmployeeNumber);
            Assert.AreEqual(jwt.User.FirstName, result.User.FirstName);
            Assert.AreEqual(jwt.User.Id, result.User.Id);
            Assert.AreEqual(jwt.User.LastName, result.User.LastName);
            Assert.AreEqual(jwt.User.LastSignIn, result.User.LastSignIn);
            Assert.AreEqual(string.Join(",", jwt.User.Roles), string.Join(",", result.User.Roles));
            Assert.AreEqual(jwt.User.ThumbnailPhoto, result.User.ThumbnailPhoto);
            Assert.AreEqual(jwt.User.Title, result.User.Title);
            Assert.AreEqual(jwt.User.UserName, result.User.UserName);
        }

        [TestMethod]
        public async Task RefreshToken_ServiceBadResponse_ReturnsNull()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";
            var appKey = "gfhjhdsdfadgfk";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/RefreshToken")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(appKey)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(BuildMockMessage<AuthResponse>(HttpStatusCode.Forbidden, null))
                               .Verifiable();

            var result = await _service.RefreshToken(token, appKey, ip);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RefreshToken_ServiceException_Rethrows()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";
            var appKey = "gfhjhdsdfadgfk";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/RefreshToken")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(appKey)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .Throws(new Exception())
                               .Verifiable();

            bool caughtException = false;

            try
            {
                await _service.RefreshToken(token, appKey, ip);
            }
            catch (Exception)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);
        }
        #endregion

        #region SignOut
        [TestMethod]
        public async Task SignOut_Success()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";
            var appKey = "gfhjhdsdfadgfk";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/RevokeToken")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(appKey)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(BuildMockMessage(HttpStatusCode.OK, "doesntmatter"))
                               .Verifiable();

            var result = await _service.SignOut(token, appKey, ip);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SignOut_ServiceBadResponse_ReturnsNull()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";
            var appKey = "gfhjhdsdfadgfk";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/RevokeToken")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(appKey)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(BuildMockMessage(HttpStatusCode.Forbidden, "y'ain't supposed to be here"))
                               .Verifiable();

            var result = await _service.SignOut(token, appKey, ip);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task SignOut_ServiceException_Rethrows()
        {
            var ip = "sdhsfgjsadf";
            var token = "iksrvarfasfdhfghj";
            var appKey = "gfhjhdsdfadgfk";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains("Auth/RevokeToken")
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(appKey)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(ip)
                                                                                                 && x.Content.ReadAsStringAsync().Result.Contains(token)),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .Throws(new Exception())
                               .Verifiable();

            bool caughtException = false;

            try
            {
                await _service.SignOut(token, appKey, ip);
            }
            catch (Exception)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);
        }
        #endregion

        #region GetThumbnail
        [TestMethod]
        public async Task GetThumbnail_Success()
        {
            var userName = "hjadsg";
            var thumbnaildata = "fjkjadfhfgjadsgadfg";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains($"Auth/GetThumbnail/{userName}")),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(thumbnaildata) })
                               .Verifiable();

            var result = await _service.GetThumbnail(userName);

            Assert.AreEqual(thumbnaildata, result);
        }

        [TestMethod]
        public async Task GetThumbnail_BadResponse_ReturnsNull()
        {
            var userName = "hjadsg";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains($"Auth/GetThumbnail/{userName}")),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(BuildMockMessage(HttpStatusCode.Forbidden, "y'ain't supposed to be here"))
                               .Verifiable();

            var result = await _service.GetThumbnail(userName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetThumbnail_ServiceException_Rethrows()
        {
            var userName = "hjadsg";

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains($"Auth/GetThumbnail/{userName}")),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .Throws(new Exception())
                               .Verifiable();

            bool caughtException = false;
            try
            {
                await _service.GetThumbnail(userName);
            }
            catch (Exception)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);
        }
        #endregion

        #region AcknowledgeAlert
        [TestMethod]
        public async Task AcknowledgeAlert_Success()
        {
            var alertId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains($"Auth/AcknowledgeAlert/{alertId}")
                                                                                                 && x.Method == HttpMethod.Post
                                                                                                 && x.Headers.Any(h => h.Key == "SSO-User-Key" && h.Value.Contains(userId.ToString()))
                                                                                              ),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("true") })
                               .Verifiable();

            var result = await _service.AcknowledgeAlert(alertId.ToString(), userId.ToString());

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task AcknowledgeAlert_BadResponseReturnsFalse()
        {
            var alertId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains($"Auth/AcknowledgeAlert/{alertId}")
                                                                                                 && x.Method == HttpMethod.Post
                                                                                                 && x.Headers.Any(h => h.Key == "SSO-User-Key" && h.Value.Contains(userId.ToString()))
                                                                                              ),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest))
                               .Verifiable();

            var result = await _service.AcknowledgeAlert(alertId.ToString(), userId.ToString());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task AcknowledgeAlert_ServiceException_Rethrows()
        {
            var alertId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.PathAndQuery.Contains($"Auth/AcknowledgeAlert/{alertId}")
                                                                                                 && x.Method == HttpMethod.Post
                                                                                                 && x.Headers.Any(h => h.Key == "SSO-User-Key" && h.Value.Contains(userId.ToString()))
                                                                                              ),
                                                                 ItExpr.IsAny<CancellationToken>())
                               .Throws(new Exception())
                               .Verifiable();

            bool caughtException = false;
            try
            {
                await _service.AcknowledgeAlert(alertId.ToString(), userId.ToString());
            }
            catch (Exception)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);
        }
        #endregion
    }
}
