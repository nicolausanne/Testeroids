// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorizeUnitTestFixturesAspectAttribute.cs" company="Testeroids">
//   � 2012 Testeroids. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Testeroids.Aspects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using NUnit.Framework;

    using PostSharp.Aspects;
    using PostSharp.Extensibility;
    using PostSharp.Reflection;

    /// <summary>
    ///   CategorizeUnitTestFixturesAspect injects the <see cref="NUnit.Framework.CategoryAttribute" /> attribute (to group all test fixtures under their respective SUT) and the <see
    ///    cref="NUnit.Framework.DescriptionAttribute" /> attribute (to transform the test name into an English sentence) into all test fixtures in an assembly (categorized by top-level specification class).
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
    [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.Strict)]
    public class CategorizeUnitTestFixturesAspectAttribute : MethodLevelAspect, 
                                                             IAspectProvider
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategorizeUnitTestFixturesAspectAttribute" /> class.
        /// </summary>
        public CategorizeUnitTestFixturesAspectAttribute()
        {
            this.AttributeTargetMemberAttributes = MulticastAttributes.Public | MulticastAttributes.Instance;
            this.AttributeTargetElements = MulticastTargets.Class;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   The compile time validate.
        /// </summary>
        /// <param name="method"> The method. </param>
        /// <returns> The System.Boolean. </returns>
        public override bool CompileTimeValidate(MethodBase method)
        {
            return TypeInvestigationService.IsTestMethod(method) && typeof(IContextSpecification).IsAssignableFrom(method.DeclaringType);
        }

        /// <summary>
        ///   The provide aspects.
        /// </summary>
        /// <param name="targetElement"> The target element. </param>
        /// <returns> The System.Collections.Generic.IEnumerable`1[T -&gt; PostSharp.Aspects.AspectInstance]. </returns>
        /// <remarks>
        ///   This method is called at build time and should just provide other aspects.
        /// </remarks>
        public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
        {
            var targetMethod = (MethodBase)targetElement;

            var categoryAttributes = targetMethod.GetCustomAttributes(typeof(CategoryAttribute), false).Cast<CategoryAttribute>().ToArray();
            var categoryName = string.Format("Specifications for {0}", GetTestedClassTypeName(targetMethod.DeclaringType));

            if (!categoryAttributes.Any() || categoryAttributes.All(x => x.Name != categoryName))
            {
                var categoryAttributeConstructorInfo = typeof(CategoryAttribute).GetConstructor(new[] { typeof(string) });
                var introduceCategoryAspect = new CustomAttributeIntroductionAspect(new ObjectConstruction(categoryAttributeConstructorInfo, categoryName));

                // Add the Category attribute to the type. 
                yield return new AspectInstance(targetMethod, introduceCategoryAspect);
            }

            if (targetMethod.IsDefined(typeof(DescriptionAttribute), false))
            {
                yield break;
            }

            var descriptionAttributeConstructorInfo = typeof(DescriptionAttribute).GetConstructor(new[] { typeof(string) });
            var introduceDescriptionAspect = new CustomAttributeIntroductionAspect(new ObjectConstruction(descriptionAttributeConstructorInfo, GetDescription(targetMethod)));

            // Add the Category attribute to the type. 
            yield return new AspectInstance(targetMethod, introduceDescriptionAspect);
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Get the description of the test out of the test class name.
        /// </summary>
        /// <param name="targetMethod"> The target test method. </param>
        /// <returns> The literal description out of the test class name. </returns>
        private static string GetDescription(MethodBase targetMethod)
        {
            if (targetMethod != null)
            {
                if (targetMethod.DeclaringType != null)
                {
                    var englishConditionName = targetMethod.DeclaringType.Name.Replace('_', ' ');
                    var englishAssertName = targetMethod.Name.Replace('_', ' ');

                    return string.Format("Test case for {0}: {1}, {2}.", GetTestedClassTypeName(targetMethod.DeclaringType), englishConditionName, englishAssertName);
                }

                return string.Format("Test case for {0}: {1}, {2}.", GetTestedClassTypeName(targetMethod.DeclaringType), string.Empty, string.Empty);
            }

            return string.Format("Test case for {0}: {1}, {2}.", string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        ///   Get the tested class type name.
        /// </summary>
        /// <param name="targetType"> The test class from which the tested class must be found. </param>
        /// <returns> The tested class name. </returns>
        private static string GetTestedClassTypeName(Type targetType)
        {
            var contextSpecificationType = typeof(ContextSpecification<>);
            var subjectInstantiationContextSpecificationType = typeof(SubjectInstantiationContextSpecification<>);

            while (targetType != null)
            {
                if (targetType.IsGenericType)
                {
                    var targetGenericTypeDefinition = targetType.GetGenericTypeDefinition();

                    if (targetGenericTypeDefinition == contextSpecificationType ||
                        targetGenericTypeDefinition == subjectInstantiationContextSpecificationType)
                    {
                        var typeTested = targetType.GetGenericArguments().Single();

                        return typeTested.Name;
                    }
                }

                targetType = targetType.BaseType;
            }

            return "Unknown";
        }

        #endregion
    }
}