using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.Fluent.Syntax.Elements.Members.MethodMembers;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.ArchitectureTests
{
    public static partial class HaveUnitTestExtensionMethods
    {
        /// <summary>
        /// Checks that the given classes have corresponding unit test class
        /// </summary>
        public static IArchRule HaveUnitTests(this ClassesShould should, string unitTestClassSuffix = "UnitTests")
        {
            var description = "Have unit tests " + should.Description;
            return should.FollowCustomCondition(new ArchitectureCondition<Class>(Condition, description));

            IEnumerable<ConditionResult> Condition(IEnumerable<Class> classes, Architecture architecture)
            {
                foreach (var classItem in classes)
                {
                    var result = GetClassHasUnitTestResult(architecture, classItem, unitTestClassSuffix);
                    if (result is null) continue;

                    yield return result;
                }
            }
        }

        private static ConditionResult GetClassHasUnitTestResult(Architecture architecture, Class classItem, string unitTestClassSuffix)
        {
            var unitTestType = GetUnitTestTypeFromClass(architecture, classItem, unitTestClassSuffix);
            return unitTestType is null ? new ConditionResult(classItem, false) : new ConditionResult(classItem, true);
        }

        internal static IType GetUnitTestTypeFromClass(Architecture architecture, IType type, string unitTestClassSuffix)
        {
            // classes that are typed (e.g. `BaseRepository<T1, T2>`) will reflect as Type`N where N is the number of types it takes so we need to filter that out
            // This is also way more readable than the nested OR, no matter what CodeQL might think
            return type.Name.IndexOf('`') > 0
                ? architecture.Classes.FirstOrDefault(x => x.NameStartsWith(type.Name[..type.Name.IndexOf('`')]) && x.NameEndsWith(unitTestClassSuffix))
                : architecture.Classes.FirstOrDefault(x => x.NameStartsWith(type.Name) && x.NameEndsWith(unitTestClassSuffix));
        }


        /// <summary>
        /// Checks that the given methods have public unit tests
        /// </summary>
        public static IArchRule HaveUnitTests(this MethodMembersShould should, string unitTestClassSuffix = "UnitTests")
        {
            var description = "Have unit tests " + should.Description;
            return should.FollowCustomCondition(new ArchitectureCondition<MethodMember>(Condition, description));

            IEnumerable<ConditionResult> Condition(IEnumerable<MethodMember> methods, Architecture architecture)
            {
                foreach (var method in methods)
                {
                    var result = GetMethodHasUnitTestResult(architecture, method, unitTestClassSuffix);
                    if (result is null) continue;

                    yield return result;
                }
            }
        }

        private static ConditionResult GetMethodHasUnitTestResult(Architecture architecture, MethodMember method, string unitTestClassSuffix)
        {
            if (method.DeclaringType is null) return null;

            var unitTestType = GetUnitTestTypeFromClass(architecture, method.DeclaringType, unitTestClassSuffix);
            if (unitTestType is null)
                return new ConditionResult(method, false, $"Class '{method.DeclaringType.FullName}' has no corresponding '{method.DeclaringType}{unitTestClassSuffix}' class.");

            if (!UnitHasTestMethod(unitTestType, method))
                return new ConditionResult(method, false);

            return new ConditionResult(method, true);
        }

        private static bool UnitHasTestMethod(IType unitTestType, MethodMember method)
        {
            var methodName = UnitTestRegex().Match(method.Name).Groups[1].Value;

            return unitTestType
                .Members.OfType<MethodMember>()
                .Any(x => Regex.IsMatch(x.Name, $"^{Regex.Escape(methodName)}(_|$)", RegexOptions.IgnoreCase));
        }

        [GeneratedRegex(@"^\.?(\w+)\W")]
        private static partial Regex UnitTestRegex();
    }
}
