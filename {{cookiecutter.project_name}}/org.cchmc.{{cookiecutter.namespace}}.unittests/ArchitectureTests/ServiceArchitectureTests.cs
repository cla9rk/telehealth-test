using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using System;
using System.Linq;
using ArchUnitNET.MSTestV2;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.ArchitectureTests
{
    [TestClass]
    public sealed class ServiceArchitectureTests
    {
        [TestMethod]
        public void Services_Should_Implement_Interface()
        {
            var results = ArchitectureDefinition.ServiceClasses
                .GetObjects(ArchitectureDefinition.Architecture)
                .Select(r =>
                {
                    var entity = r.Name.Split("Service")[0];
                    return new Tuple<string, bool>(r.Name,
                        r.IsAssignableTo($"{ArchitectureDefinition.BaseNamespace}.data.Interfaces.I{entity}")
                        || r.IsAssignableTo($"{ArchitectureDefinition.BaseNamespace}.auth.Interfaces.I{entity}"));
                })
                .ToList();

            Assert.IsTrue(results.All(r => r.Item2));
        }

        [TestMethod]
        public void Services_Should_End_With_Service()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.ServiceClasses)
                .As("Services should end with `Service`.")
                .Should()
                .HaveNameMatching("Service(`[0-9]+)*");

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void Services_Should_Not_Reference_Endpoint_Project()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.ServiceClasses)
                .As("Services should not depend on any classes in the `.endpoint` project.")
                .Should().NotDependOnAny(ArchitectureDefinition.ApiLayer);

            rule.Check(ArchitectureDefinition.Architecture);
        }
    }
}
