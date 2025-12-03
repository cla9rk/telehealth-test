using ArchUnitNET.Fluent.Slices;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitNET.MSTestV2;
using ArchUnitNET.Fluent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.ArchitectureTests
{
    [TestClass]
    public sealed class OverallArchitectureTests
    {
        [TestMethod]
        public void Application_Should_Be_Free_Of_Cycles()
        {
            SliceRule rule = SliceRuleDefinition.Slices()
                .Matching($"{ArchitectureDefinition.BaseNamespace}.(*)")
                .Should().BeFreeOfCycles();

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void Classes_That_Should_Be_Tested_Should_Have_UnitTests()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.ClassesThatShouldBeTested)
                .As("Testable classes require unit tests.")
                .Should()
                .HaveUnitTests();

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void Method_Names_Should_Start_With_Caps()
        {
            IArchRule rule = MethodMembers()
                .That().AreDeclaredIn(ArchitectureDefinition.ClassesThatShouldBeTested)
                .And().AreNoConstructors()
                .And().DoNotHaveAnyAttributes(typeof(SpecialNameAttribute), typeof(CompilerGeneratedAttribute))
                .As("Methods should start with capital letters.")
                .Should().HaveNameMatching(@"^[A-Z]");

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void Testable_Classes_Should_Never_Be_Excluded_From_Code_Coverage()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.ClassesThatShouldBeTested)
                .As("Classes that are testable should never be excluded from code coverage.")
                .Should()
                .NotHaveAnyAttributes(typeof(ExcludeFromCodeCoverageAttribute));

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void UnitTests_Should_Be_In_UnitTest_Layer()
        {
            var rule = Classes().That().Are(ArchitectureDefinition.UnitTestClasses).Should().ResideInAssembly(ArchitectureDefinition.UnitTestAssembly);

            rule.Check(ArchitectureDefinition.Architecture);
        }
    }
}
