using ArchUnitNET.Fluent;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitNET.MSTestV2;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.ArchitectureTests
{
    [TestClass]
    public sealed class EndpointArchitectureTests
    {
        [TestMethod]
        public void Endpoints_Should_End_With_Endpoints()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.EndpointClasses)
                .As("Classes in the `*.Endpoints` namespace should end with `Endpoints`.")
                .Should()
                .HaveNameEndingWith("Endpoints");

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void Endpoints_Should_Not_Reference_Repositories()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.EndpointClasses)
                .As("Endpoints should not directly reference repositories, instead reference the interface and rely on Dependency Injection.")
                .Should().NotDependOnAnyTypesThat().ResideInNamespace($"{ArchitectureDefinition.BaseNamespace}.data.Repositories");

            rule.Check(ArchitectureDefinition.Architecture);
        }

        [TestMethod]
        public void ApiLayer_Should_Not_Reference_EntityFramework()
        {
            IArchRule rule = Classes()
                .That().Are(ArchitectureDefinition.ApiLayer)
                .As("API layer should not directly Entity Framework.")
                .Should().NotDependOnAnyTypesThat().ResideInNamespace($"Microsoft.EntityFrameworkCore")
                .WithoutRequiringPositiveResults();

            rule.Check(ArchitectureDefinition.Architecture);
        }


        // TODO: Not sure how to identify endpoints without adding extra, otherwise unnecessary attributes
        //[TestMethod]
        //public void Controllers_Should_Depend_On_Logger()
        //{
        //    IArchRule rule = Classes()
        //        .That().Are(ArchitectureDefinition.EndpointClasses)
        //        .As("Endpoints should require an `ILogger<>`")
        //        .Should()
        //        .DependOnAny("Microsoft.Extensions.Logging.ILogger`1"); // Need this `1 because its really ILogger<T>

        //    rule.Check(ArchitectureDefinition.Architecture);
        //}

        //[TestMethod]
        //public void Api_Routes_Should_Have_UnitTests()
        //{
        //    IArchRule rule = MethodMembers()
        //        .That().AreDeclaredIn(ArchitectureDefinition.ClassesThatShouldBeTested)
        //        .And().ArePublic()
        //        .And().HaveReturnType("Results`[0-9]+", true)
        //        .As("Endpoint routes should have corresponding unit tests.")
        //        .Should().HaveUnitTests();

        //    rule.Check(ArchitectureDefinition.Architecture);
        //}

        //[TestMethod]
        //public void Controllers_Should_Be_In_Controller_Namespace()
        //{
        //    IArchRule rule = Classes()
        //        .That().Are(ArchitectureDefinition.EndpointClasses)
        //        .As("Controllers belong in the .Controllers namespace.")
        //        .Should()
        //        .ResideInNamespace($"{ArchitectureDefinition.BaseNamespace}.api.Controllers")
        //        .OrShould().ResideInNamespace($"{ArchitectureDefinition.BaseNamespace}.api.Controllers.v2");

        //    rule.Check(ArchitectureDefinition.Architecture);
        //}

        //[TestMethod]
        //public void Routes_Should_Always_Pass_Exception_And_Message_In_LogError()
        //{
        //    IArchRule rule = MethodMembers()
        //        .That().AreDeclaredIn(ArchitectureDefinition.ClassesThatShouldBeTested)
        //        .And().ArePublic()
        //        .And().HaveAnyAttributes(_attributes)
        //        .As("`.LogError` should always have the exception and the error message.")
        //        .Should().FollowCustomCondition(m =>
        //        {
        //            var logErrorCall = m.GetMethodCallDependencies().FirstOrDefault(c => c.TargetMember.NameStartsWith("LogError"));
        //            if (logErrorCall == null) return false;

        //            return logErrorCall.TargetMember is not MethodMember targetMember || targetMember.ParameterInstances.Count == 4; // logErrorCall should have 4 parameters
        //        }, "", "Method does not call LogError with 4 parameters.");

        //    rule.Check(ArchitectureDefinition.Architecture);
        //}

        //[TestMethod]
        //public void Routes_Should_Always_Have_ReturnType_TaskIActionResult()
        //{
        //    IArchRule rule = MethodMembers()
        //        .That().AreDeclaredIn(ArchitectureDefinition.ClassesThatShouldBeTested)
        //        .And().ArePublic()
        //        .And().HaveAnyAttributes(_attributes)
        //        .As("Controller routes should always return `Task<IActionResult>`")
        //        .Should().HaveReturnType(typeof(Task<IActionResult>), typeof(IActionResult));

        //    rule.Check(ArchitectureDefinition.Architecture);
        //}
    }
}
