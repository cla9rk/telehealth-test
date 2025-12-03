using static ArchUnitNET.Fluent.ArchRuleDefinition;
using System;
using System.Linq;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.MSTestV2;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.ArchitectureTests
{
    [TestClass]
    public sealed class RepositoryArchitectureTests
    {
        [TestMethod]
        public void Repositories_Should_Implement_Interface()
        {
            var results = ArchitectureDefinition.RepositoryClasses
                .GetObjects(ArchitectureDefinition.Architecture)
                .Select(r =>
                {
                    if (r.Name.Contains("BaseRepository")) // workaround for Generic Type
                        return new Tuple<string, bool>(r.Name, true);

                    var entity = r.Name.Split("Repository")[0];
                    return new Tuple<string, bool>(r.Name,
                        r.IsAssignableTo($"{ArchitectureDefinition.BaseNamespace}.data.Interfaces.I{entity}"));
                })
                .ToList();

            Assert.IsTrue(results.All(r => r.Item2));
        }

        [TestMethod]
        public void Repositories_Should_End_With_Repository()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.RepositoryClasses)
                .As("Repositories should end with `Repository`.")
                .Should()
                .HaveNameMatching("Repository(`[0-9]+)*");

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void Repositories_Should_Not_Reference_Endpoint_Project()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.RepositoryClasses)
                .As("Repositories should not depend on any classes in the `.endpoint` Project.")
                .Should().NotDependOnAny(ArchitectureDefinition.ApiLayer);

            rule.Check(ArchitectureDefinition.Architecture);
        }
    }
}
