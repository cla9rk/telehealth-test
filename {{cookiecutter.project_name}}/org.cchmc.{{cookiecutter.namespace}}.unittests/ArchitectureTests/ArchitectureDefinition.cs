using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using System.Linq;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.ArchitectureTests
{
    internal static class ArchitectureDefinition
    {
        public static readonly string BaseNamespace = "org.cchmc.{{cookiecutter.namespace}}";

        public static Architecture Architecture { get; }

        public static Assembly AppHostAssembly { get; }
        public static Assembly ApiAssembly { get; }
        public static Assembly AuthAssembly { get; }
        public static Assembly DataAssembly { get; }
        public static Assembly ModelAssembly { get; }
        public static Assembly UnitTestAssembly { get; }

        public static IObjectProvider<IType> EndpointClasses { get; }
        public static IObjectProvider<IType> ApiLayer { get; }
        public static IObjectProvider<IType> AuthClasses { get; }
        public static IObjectProvider<IType> RepositoryClasses { get; }
        public static IObjectProvider<IType> HealthCheckClasses { get; }
        public static IObjectProvider<Interface> Interfaces { get; }
        public static IObjectProvider<IType> ServiceClasses { get; }
        public static IObjectProvider<IType> UnitTestClasses { get; }
        public static IObjectProvider<IType> ClassesThatShouldBeTested { get; }

        static ArchitectureDefinition()
        {
            Architecture = new ArchLoader().LoadAssemblies(
                System.Reflection.Assembly.Load($"{BaseNamespace}.apphost"),
                System.Reflection.Assembly.Load($"{BaseNamespace}.auth"),
                System.Reflection.Assembly.Load($"{BaseNamespace}.data"),
                System.Reflection.Assembly.Load($"{BaseNamespace}.endpoints"),
                System.Reflection.Assembly.Load($"{BaseNamespace}.models"),
                System.Reflection.Assembly.Load($"{BaseNamespace}.unittests")
            ).Build();

            AppHostAssembly = Architecture.Assemblies.ToList().First(a => a.Name.StartsWith($"{BaseNamespace}.apphost"));
            ApiAssembly = Architecture.Assemblies.ToList().First(a => a.Name.StartsWith($"{BaseNamespace}.endpoints"));
            AuthAssembly = Architecture.Assemblies.ToList().First(a => a.Name.StartsWith($"{BaseNamespace}.auth"));
            DataAssembly = Architecture.Assemblies.ToList().First(a => a.Name.StartsWith($"{BaseNamespace}.data"));
            ModelAssembly = Architecture.Assemblies.ToList().First(a => a.Name.StartsWith($"{BaseNamespace}.models"));
            UnitTestAssembly = Architecture.Assemblies.ToList().First(a => a.Name.StartsWith($"{BaseNamespace}.unittests"));

            EndpointClasses = Classes()
                .That()
                .ArePublic()
                .And().ResideInNamespace($"{BaseNamespace}.endpoints.Endpoints")
                .Or().ResideInNamespace($"{BaseNamespace}.auth.Endpoints")
                .As("Endpoint Classes");

            ApiLayer = Types()
                .That()
                .ArePublic().And()
                .ResideInAssembly(ApiAssembly)
                .As("API Layer");

            RepositoryClasses = Classes()
                .That()
                .ArePublic().And()
                .ResideInNamespace($"{BaseNamespace}.data.Repositories")
                .As("Repository Classes");

            HealthCheckClasses = Classes()
                .That()
                .ArePublic().And()
                .ResideInNamespace($"{BaseNamespace}.data.HealthChecks")
                .As("HealthCheck Classes");

            Interfaces = Interfaces()
                .That()
                .ArePublic().And()
                .ResideInNamespace($"{BaseNamespace}.data.Interfaces")
                .As("Interfaces");

            ServiceClasses = Classes()
                .That()
                .ArePublic().And()
                .ResideInNamespace($"{BaseNamespace}.data.Services")
                .Or().ResideInNamespace($"{BaseNamespace}.auth.Services")
                .As("Service Classes");

            UnitTestClasses = Classes()
                .That()
                .ArePublic().And()
                .HaveNameEndingWith("Tests").Or().HaveAnyAttributes(typeof(TestClassAttribute))
                .As("Unit Test Classes");

            // These should be added to as new folders/namespaces are added to your project
            ClassesThatShouldBeTested = Types()
                .That().ResideInNamespace($"{BaseNamespace}.endpoints.Endpoints")
                .Or().ResideInNamespace($"{BaseNamespace}.auth.Endpoints")
                .Or().ResideInNamespace($"{BaseNamespace}.auth.Services")
                .Or().ResideInNamespace($"{BaseNamespace}.data.Repositories")
                .Or().ResideInNamespace($"{BaseNamespace}.data.Services")
                .Or().ResideInNamespace($"{BaseNamespace}.data.HealthChecks")
                .And().ArePublic()
                .As("Testable Classes");
        }
    }
}
