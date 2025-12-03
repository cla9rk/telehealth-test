using Microsoft.AspNetCore.Http.HttpResults;
using org.cchmc.{{cookiecutter.namespace}}.endpoints.Endpoints;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.EndpointTests
{
    [TestClass]
    public sealed class SampleEndpointsUnitTests
    {
        [TestMethod]
        public void SampleGet_Success()
        {
            const string input = "test";
            var result = SampleEndpoints.SampleGet(input);
            
            Assert.IsNotNull(result?.Result);
            Assert.IsInstanceOfType<Ok<string>>(result.Result);
            var resultObject = result.Result as Ok<string>;
            Assert.IsNotNull(resultObject.Value);
            Assert.Contains(input, resultObject.Value);
        }
    }
}
