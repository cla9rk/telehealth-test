using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using org.cchmc.{{cookiecutter.namespace}}.auth;
using org.cchmc.{{cookiecutter.namespace}}.auth.Endpoints;
using org.cchmc.{{cookiecutter.namespace}}.auth.Interfaces;
using org.cchmc.{{cookiecutter.namespace}}.auth.Models;
using org.cchmc.{{cookiecutter.namespace}}.auth.Settings;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.AuthTests
{
    [TestClass]
    public sealed class AuthEndpointsUnitTests
    {
        private readonly string _localIpAddress = "127.0.0.1";
        private readonly SingleSignOnUser _user = new()
        {
            FirstName = "Gkjsdfgadfg",
            Department = "fgundhkfhgj",
            Email = "go,mfhjhasdfasdgsffuhsdg",
            EmployeeNumber = "gnjbvdf",
            Id = Guid.NewGuid().ToString(),
            LastName = "fyimngubyd",
            LastSignIn = DateTime.Now.AddMinutes(-30),
            Roles = ["sdfgvsdfg", "hlmjhgg"],
            ThumbnailPhoto = "ymkjndsffadf",
            Title = "Fguntyin",
            UserName = "ukbjsdgf"
        };
        private readonly AuthOptions _authOptions = new()
        {
            ApplicationEnvironmentId = "inbghsdfgsdvfgs",
            ExpirationMinutes = 20,
            IssuerSigningKey = "h,klmndbfyvtgaetnydfmhjlghgjsdfg",
            RefreshTokenCookieName = "singlesignon-proofofconcept",
            SingleSignOnEndpoint = "fgnubhvdasmdfghlmkfhg",
            SingleSignOnRedirectUrl = "gimndbtyvgdfsgsdfhdtg"
        };
        private Mock<ISingleSignOn> _ssoService;
        private Mock<ILogger<AuthEndpoints>> _logger;
        private DefaultHttpContext _httpContext;

        [TestInitialize]
        public void Initialize()
        {
            _ssoService = new Mock<ISingleSignOn>();
            _logger = new Mock<ILogger<AuthEndpoints>>();
            _httpContext = SetupHttpRequestMock(_localIpAddress);
            _httpContext.User = new ClaimsPrincipal(identities: [
                new ClaimsIdentity(claims: [
                    new Claim(ClaimTypes.Name, "myUserName"),
                    new Claim(JwtRegisteredClaimNames.NameId, "myId"),
                    new Claim(JwtRegisteredClaimNames.GivenName, "firstName"),
                    new Claim(JwtRegisteredClaimNames.FamilyName, "lastName"),
                    new Claim(JwtRegisteredClaimNames.UniqueName, "myUserName"),
                    new Claim("sso_empnum", "123456"),
                    new Claim("sso_email", "myName@email"),
                    new Claim("sso_title", "myTitle"),
                    new Claim("sso_department", "myDep"),
                    new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddMinutes(6).ToString("O")),
                    new Claim("refresh_token", "sfgjdghksdfhgadfhadfh"),
                    new Claim("sso_alerts", "[]"),
                    new Claim(ClaimTypes.Role, "myRole")
                ])
            ]);
        }

        private static DefaultHttpContext SetupHttpRequestMock(string ipAddress)
        {
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.FromResult((object)null));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            var context = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
            context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);

            return context;
        }

        #region SignIn
        [TestMethod]
        public void SignIn_Success()
        {
            var result = AuthEndpoints.SignIn(_authOptions);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<Ok<SingleSignOnRedirectOptions>>(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(_authOptions.SingleSignOnRedirectUrl, result.Value.SingleSignOnUrl);
            Assert.AreEqual(_authOptions.ApplicationEnvironmentId, result.Value.EnvironmentId);
        }
        #endregion

        #region CompleteSignIn
        [TestMethod]
        public async Task CompleteSignIn_Success()
        {
            var signInToken = "ulmntdgsdfgu,imjkhsdgfgjkdfgsgd";
            var refreshToken = "Fmknbsdfadsfmjfgjnksdffsdfghsdfg";
            _ssoService.Setup(p => p.CompleteSignIn(signInToken, It.IsAny<string>()))
                       .ReturnsAsync(new AuthResponse()
                       {
                           ExpiresOnUtc = DateTime.UtcNow.AddHours(3),
                           RefreshToken = refreshToken,
                           User = _user,
                           ActiveAlerts = []
                       }).Verifiable();

            var result = await AuthEndpoints.CompleteSignIn(signInToken, _ssoService.Object, _logger.Object, _httpContext, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok<LoginResponse>>(result.Result);
            var resultObject = result.Result as Ok<LoginResponse>;
            Assert.IsNotNull(resultObject.Value.User);
            Assert.AreEqual(_user.Department, resultObject.Value.User.Department);
            Assert.AreEqual(_user.Email, resultObject.Value.User.Email);
            Assert.AreEqual(_user.EmployeeNumber, resultObject.Value.User.EmployeeNumber);
            Assert.AreEqual(_user.FirstName, resultObject.Value.User.FirstName);
            Assert.AreEqual(_user.Id, resultObject.Value.User.Id);
            Assert.AreEqual(_user.LastName, resultObject.Value.User.LastName);
            Assert.AreEqual(_user.LastSignIn, resultObject.Value.User.LastSignIn);
            Assert.AreEqual(string.Join(',', _user.Roles), string.Join(',', resultObject.Value.User.Roles));
            Assert.AreEqual(_user.ThumbnailPhoto, resultObject.Value.User.ThumbnailPhoto);
            Assert.AreEqual(_user.Title, resultObject.Value.User.Title);
            Assert.AreEqual(_user.UserName, resultObject.Value.User.UserName);
            _ssoService.VerifyAll();
        }

        [TestMethod]
        public async Task CompleteSignIn_CatchesException_500()
        {
            var signInToken = "ulmntdgsdfgu,imjkhsdgfgjkdfgsgd";
            _ssoService.Setup(p => p.CompleteSignIn(signInToken, It.IsAny<string>())).Throws(new Exception()).Verifiable();

            var result = await AuthEndpoints.CompleteSignIn(signInToken, _ssoService.Object, _logger.Object, _httpContext, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<ProblemHttpResult>(result.Result);
            _ssoService.VerifyAll();
        }

        [TestMethod]
        public async Task CompleteSignIn_ReturnsNull_401()
        {
            var signInToken = "ulmntdgsdfgu,imjkhsdgfgjkdfgsgd";
            _ssoService.Setup(p => p.CompleteSignIn(signInToken, It.IsAny<string>())).ReturnsAsync((AuthResponse)null).Verifiable();

            var result = await AuthEndpoints.CompleteSignIn(signInToken, _ssoService.Object, _logger.Object, _httpContext, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<UnauthorizedHttpResult>(result.Result);
            _ssoService.VerifyAll();
        }
        #endregion

        #region SsoSignOut
        [TestMethod]
        public async Task SsoSignOut_Success()
        {
            var refreshToken = "fghimngyusdfsdfg";
            var user = new ClaimsPrincipal(new ClaimsIdentity([new(AuthCookieHelper.RefreshTokenClaimName, refreshToken)]));
            _httpContext.User = user;
            _httpContext.Connection.RemoteIpAddress = IPAddress.Parse(_localIpAddress);

            _ssoService.Setup(p => p.SignOut(refreshToken, _authOptions.ApplicationEnvironmentId, It.IsAny<string>())).ReturnsAsync(true).Verifiable();

            var result = await AuthEndpoints.SsoSignOut(_httpContext, _ssoService.Object, _logger.Object, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok>(result.Result);
            var resultObject = result.Result as Ok;
            Assert.AreEqual(200, resultObject.StatusCode);
            _ssoService.VerifyAll();
        }

        [TestMethod]
        public async Task SsoSignOut_CatchesException_500()
        {
            var refreshToken = "fghimngyusdfsdfg";
            var user = new ClaimsPrincipal(new ClaimsIdentity([new(AuthCookieHelper.RefreshTokenClaimName, refreshToken)]));
            _httpContext.User = user;
            _httpContext.Connection.RemoteIpAddress = IPAddress.Parse(_localIpAddress);

            _ssoService.Setup(p => p.SignOut(refreshToken, _authOptions.ApplicationEnvironmentId, It.IsAny<string>())).Throws(new Exception()).Verifiable();

            var result = await AuthEndpoints.SsoSignOut(_httpContext, _ssoService.Object, _logger.Object, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<ProblemHttpResult>(result.Result);
            var resultObject = result.Result as ProblemHttpResult;
            Assert.AreEqual(500, resultObject.StatusCode);
            _ssoService.VerifyAll();
        }

        [TestMethod]
        public async Task SsoSignOut_NoCookie_Success()
        {
            var result = await AuthEndpoints.SsoSignOut(_httpContext, _ssoService.Object, _logger.Object, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok>(result.Result);
            var resultObject = result.Result as Ok;
            Assert.AreEqual(200, resultObject.StatusCode);
            _ssoService.VerifyAll();
        }

        [TestMethod]
        public async Task SsoSignOut_NoRefreshToken_Succes()
        {
            var user = new ClaimsPrincipal();
            _httpContext.User = user;
            _httpContext.Connection.RemoteIpAddress = IPAddress.Parse(_localIpAddress);

            var result = await AuthEndpoints.SsoSignOut(_httpContext, _ssoService.Object, _logger.Object, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok>(result.Result);
            var resultObject = result.Result as Ok;
            Assert.AreEqual(200, resultObject.StatusCode);
            _ssoService.VerifyAll();
        }
        #endregion

        #region AcknowledgeAlert
        [TestMethod]
        public async Task AcknowledgeAlert_Success()
        {
            Guid alertId = Guid.NewGuid();

            _ssoService.Setup(p => p.AcknowledgeAlert(alertId.ToString(), "myId")).ReturnsAsync(true).Verifiable();

            var result = await AuthEndpoints.AcknowledgeAlert(alertId, _httpContext, _ssoService.Object, _logger.Object);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok<bool>>(result.Result);
            var resultObject = result.Result as Ok<bool>;
            Assert.IsTrue(resultObject.Value);
        }

        [TestMethod]
        public async Task AcknowledgeAlert_NoSuchClaim_ContinuesOn()
        {
            Guid alertId = Guid.NewGuid();
            _httpContext.User = null;

            _ssoService.Setup(p => p.AcknowledgeAlert(alertId.ToString(), It.IsAny<string>())).ReturnsAsync(false).Verifiable();

            var result = await AuthEndpoints.AcknowledgeAlert(alertId, _httpContext, _ssoService.Object, _logger.Object);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok<bool>>(result.Result);
            var resultObject = result.Result as Ok<bool>;
            Assert.IsFalse(resultObject.Value);
        }

        [TestMethod]
        public async Task AcknowledgeAlert_CatchesException_500()
        {
            Guid alertId = Guid.NewGuid();

            _ssoService.Setup(p => p.AcknowledgeAlert(alertId.ToString(), "myId")).Throws(new Exception()).Verifiable();

            var result = await AuthEndpoints.AcknowledgeAlert(alertId, _httpContext, _ssoService.Object, _logger.Object);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<ProblemHttpResult>(result.Result);
        }
        #endregion

        #region GetUserInformation
        [TestMethod]
        public async Task GetUserInformation_Success()
        {
            string thumbnail = "myThumbnailValue";

            _ssoService.Setup(p => p.GetThumbnail("myUserName")).ReturnsAsync(thumbnail).Verifiable();

            var result = await AuthEndpoints.GetUserInformation(_httpContext, _ssoService.Object, _logger.Object, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok<LoginResponse>>(result.Result);
            var resultValue = (result.Result as Ok<LoginResponse>).Value;
            Assert.IsNotNull(resultValue.User);
            Assert.AreEqual(thumbnail, resultValue.User.ThumbnailPhoto);
            Assert.IsNotNull(resultValue.Alerts);
            Assert.AreEqual(1, _httpContext.Response.Headers.Count);
            Assert.IsTrue(_httpContext.Response.Headers["sso_user"].ToString().Length > 0);
            _ssoService.VerifyAll();
        }

        [TestMethod]
        public async Task GetUserInformation_CatchesException_500()
        {
            _ssoService.Setup(p => p.GetThumbnail("myUserName")).Throws(new Exception()).Verifiable();

            var result = await AuthEndpoints.GetUserInformation(_httpContext, _ssoService.Object, _logger.Object, _authOptions);

            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<ProblemHttpResult>(result.Result);
            _ssoService.VerifyAll();
        }
        #endregion
    }
}
