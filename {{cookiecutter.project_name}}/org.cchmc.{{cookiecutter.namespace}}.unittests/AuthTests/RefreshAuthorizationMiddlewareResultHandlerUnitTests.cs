using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using org.cchmc.{{cookiecutter.namespace}}.auth.Interfaces;
using org.cchmc.{{cookiecutter.namespace}}.auth.Models;
using org.cchmc.{{cookiecutter.namespace}}.auth.Settings;
using org.cchmc.{{cookiecutter.namespace}}.auth;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using HttpContextMoq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.AuthTests
{
    [TestClass]
    public sealed class RefreshAuthorizationMiddlewareResultHandlerUnitTests
    {
        RefreshAuthorizationMiddlewareResultHandler _handler;
        HttpContextMock _mockContext;
        Mock<ISingleSignOn> _mockSso;
        readonly AuthOptions _options = new()
        {
            ApplicationEnvironmentId = Guid.NewGuid().ToString(),
            IssuerSigningKey = "somegarbageheresomegarbageheresomegarbageheresomegarbageheresomegarbagehere"
        };

        Mock<RequestDelegate> _mockRequestDelegate;
        AuthorizationPolicy _mockPolicy;
        Mock<IAuthenticationService> _mockAuth;
        Mock<IServiceProvider> _mockServiceProvider;

        readonly SingleSignOnUser _user = new()
        {
            FirstName = "yu sdfadf",
            Email = "fubsvdfgad@yiubvsdf.com",
            Id = Guid.NewGuid().ToString(),
            LastName = "tyjubvdfg",
            LastSignIn = DateTime.UtcNow,
            Roles = ["dfgiunbf", "fyubyvdas"],
            UserName = "jminubysdfg",
            Department = "uknbjsdffasdfgnubhg",
            EmployeeNumber = "gynikjbdfsvgdfgujh",
            ThumbnailPhoto = "Unibsdvtsardmufngjhhfdg",
            Title = "gynibrsvdrgadsgfadf"
        };

        [TestInitialize]
        public void Initialize()
        {
            _mockSso = new Mock<ISingleSignOn>();

            _mockContext = new HttpContextMock();
            _mockContext.Mock.Setup(p => p.Request.Headers).Returns(new HeaderDictionary());
            _mockContext.Mock.Setup(p => p.Connection.RemoteIpAddress).Returns(IPAddress.Parse("10.36.72.7"));

            _mockRequestDelegate = new Mock<RequestDelegate>();
            _mockPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

            _mockAuth = new Mock<IAuthenticationService>();
            _mockAuth.Setup(p => p.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                     .Returns(Task.FromResult(true));
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(p => p.GetService(typeof(IAuthenticationService))).Returns(_mockAuth.Object);
            _mockContext.Mock.Setup(p => p.RequestServices).Returns(_mockServiceProvider.Object);

            _handler = new RefreshAuthorizationMiddlewareResultHandler(_mockSso.Object, _options, new Mock<ILogger<RefreshAuthorizationMiddlewareResultHandler>>().Object);
        }

        [TestMethod]
        public async Task HandleAsync_CookieStillValid_Continue()
        {
            string fakeToken = "ThisIsMyFakeRefreshToken";

            _mockContext.Mock.Setup(p => p.User).Returns(new ClaimsPrincipal());
            _mockContext.Mock.Setup(p => p.User.Claims).Returns([
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddMinutes(5).ToString("O")),
                new Claim(AuthCookieHelper.RefreshTokenClaimName, fakeToken)
            ]);

            await _handler.HandleAsync(_mockRequestDelegate.Object, _mockContext.Mock.Object, _mockPolicy, PolicyAuthorizationResult.Success());

            _mockSso.Verify(p => p.RefreshToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAsync_CookieExpired_RefreshFailed_401()
        {
            string fakeToken = "ThisIsMyFakeRefreshToken";
            int statusCodeSet = 0;
            _mockContext.Mock.Setup(p => p.User).Returns(new ClaimsPrincipal());
            _mockContext.Mock.Setup(p => p.User.Claims).Returns([
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddMinutes(-1).ToString("O")),
                new Claim(AuthCookieHelper.RefreshTokenClaimName, fakeToken)
            ]);

            _mockSso.Setup(p => p.RefreshToken(fakeToken, _options.ApplicationEnvironmentId, It.IsAny<string>()))
                    .ReturnsAsync((AuthResponse)null);
            _mockContext.Mock.SetupSet(p => p.Response.StatusCode = StatusCodes.Status401Unauthorized).Callback<int>(value => statusCodeSet = value);

            await _handler.HandleAsync(_mockRequestDelegate.Object, _mockContext.Mock.Object, _mockPolicy, PolicyAuthorizationResult.Success());

            _mockSso.Verify(p => p.RefreshToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, statusCodeSet);
        }

        [TestMethod]
        public async Task HandleAsync_CookieExpired_RefreshSuccessful()
        {
            string fakeToken = "ThisIsMyFakeRefreshToken";
            HeaderDictionary headers = [];
            _mockContext.Mock.Setup(p => p.User).Returns(new ClaimsPrincipal());
            _mockContext.Mock.Setup(p => p.User.Claims).Returns([
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddMinutes(-1).ToString("O")),
                new Claim(AuthCookieHelper.RefreshTokenClaimName, fakeToken)
            ]);

            _mockSso.Setup(p => p.RefreshToken(fakeToken, _options.ApplicationEnvironmentId, It.IsAny<string>()))
                      .ReturnsAsync(new AuthResponse()
                      {
                          ActiveAlerts = [],
                          ExpiresOnUtc = DateTime.UtcNow.AddMinutes(5),
                          RefreshToken = "newRefreshToken",
                          User = _user
                      });
            _mockContext.Mock.Setup(p => p.Response.Headers).Returns(headers);

            await _handler.HandleAsync(_mockRequestDelegate.Object, _mockContext.Mock.Object, _mockPolicy, PolicyAuthorizationResult.Success());

            _mockSso.Verify(p => p.RefreshToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockAuth.Verify(p => p.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
            Assert.AreEqual(1, headers.Count);
            Assert.IsTrue(headers["sso_user"].ToString().Length > 0);
        }

        [TestMethod]
        public async Task HandleAsync_MissingExpirationClaim_401()
        {
            string fakeToken = "ThisIsMyFakeRefreshToken";
            int statusCodeSet = 0;
            _mockContext.Mock.Setup(p => p.User).Returns(new ClaimsPrincipal());
            _mockContext.Mock.Setup(p => p.User.Claims).Returns([
                new Claim(AuthCookieHelper.RefreshTokenClaimName, fakeToken)
            ]);

            _mockContext.Mock.SetupSet(p => p.Response.StatusCode = StatusCodes.Status401Unauthorized).Callback<int>(value => statusCodeSet = value);

            await _handler.HandleAsync(_mockRequestDelegate.Object, _mockContext.Mock.Object, _mockPolicy, PolicyAuthorizationResult.Success());

            _mockSso.Verify(p => p.RefreshToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, statusCodeSet);
        }

        [TestMethod]
        public async Task HandleAsync_NoUserInContext_401()
        {
            int statusCodeSet = 0;

            _mockContext.Mock.SetupSet(p => p.Response.StatusCode = StatusCodes.Status401Unauthorized).Callback<int>(value => statusCodeSet = value);

            await _handler.HandleAsync(_mockRequestDelegate.Object, _mockContext.Mock.Object, _mockPolicy, PolicyAuthorizationResult.Success());

            _mockSso.Verify(p => p.RefreshToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, statusCodeSet);
        }
    }
}
