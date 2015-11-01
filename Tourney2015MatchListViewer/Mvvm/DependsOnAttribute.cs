using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SpiritMVVM
{
    /// <summary>
    /// The <see cref="DependsOnAttribute"/> assigns dependencies to properties.
    /// When any of a property's dependencies changes its value, it is implied
    /// that the attributed property's value will change as well.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class DependsOnAttribute : Attribute
    {
        /// <summary>
        /// String used to specify a "Wild Card" dependency.
        /// Assigning the wild card as a dependency means that
        /// the attributed property's value is dependent on all
        /// other properties in the class.
        /// </summary>
        public const string WildCard = "*";

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="DependsOnAttribute"/> with the given property name as a dependency.
        /// </summary>
        /// <param name="property">
        /// The name of the property on which the current property is dependent.
        /// </param>
        public DependsOnAttribute(String property)
        {
            this.Property = property;
        }

        #endregion Constructor

        #region Public Properties

        /// <summary>
        /// The property on which the attributed property is dependent.
        /// </summary>
        public String Property { get; private set; }

        #endregion Public Properties

        #region Public Static Methods

        /// <summary>
        /// Locate all properties which depend on the provided property.
        /// Only returns the properties which directly depend on the provided one
        /// (i.e. only one level deep).
        /// </summary>
        /// <param name="targetType">The Type in which the properties reside.</param>
        /// <param name="propertyName">
        /// The name of the property for which to collect all directly-dependent properties.
        /// </param>
        /// <returns>
        /// Returns a list of all properties which are directly dependent on the given property name.
        /// </returns>
        public static IEnumerable<PropertyInfo> GetDirectDependants(Type targetType, string propertyName)
        {
            return from property in targetType.GetRuntimeProperties()
                   where property.Name != propertyName //Ignore the original property, if it depends on itself
                   where property.GetCustomAttributes(typeof(DependsOnAttribute), true)
                         .Cast<DependsOnAttribute>()
                         .Any(attribute => (attribute.Property == propertyName) || (attribute.Property == WildCard))
                   select property;
        }

        /// <summary>
        /// Recursively gathers the list of all properties which depend
        /// on the provided property, whether directly or indirectly.
        /// Each dependent property will only be included once, so
        /// multiple or circular dependencies will not result in multiple
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> events.
        /// </summary>
        /// <param name="targetType">The object in which the properties reside.</param>
        /// <param name="propertyName">
        /// The name of the property for which to collect all dependent properties.
        /// </param>
        /// <returns>
        /// Returns the list of all properties which are directly or
        /// indirectly dependent on the original property.
        /// </returns>
        public static IEnumerable<PropertyInfo> GetAllDependants(Type targetType, string propertyName)
        {
            //Retrieve the Property Info for the specified property
            var propertyInfo = targetType.GetRuntimeProperties()
                .First(x => x.Name == propertyName);

            IEnumerable<PropertyInfo> oldResults = null;
            IEnumerable<PropertyInfo> results = new[] { propertyInfo };
            do
            {
                oldResults = results;

                var dependancies = from input in results
                                   from dependancy in GetDirectDependants(targetType, input.Name)
                                   select dependancy;

                //Create union of current results with "new" results,
                //making sure to remove duplicates
                results = results.Union(dependancies)
                    .GroupBy((x) => x.Name)
                    .Select(grp => grp.First());
            }
            while (results.Count() > oldResults.Count());

            //Return results not including the original property name
            return results.Where(x => (x.Name != propertyName));
        }

        /// <summary>
        /// Gather the list of all properties on which the given property depends.
        /// </summary>
        /// <param name="targetType">The object in which the properties reside.</param>
        /// <param name="propertyName">The property for which to collect dependencies.</param>
        /// <returns>
        /// Returns the list of all properties on which the given
        /// property directly depends.
        /// </returns>
        public static IEnumerable<PropertyInfo> GetDirectDependencies(Type targetType, string propertyName)
        {
            //Retrieve the Property Info for the specified property
            var propertyInfo = targetType.GetRuntimeProperties()
                .First(x => x.Name == propertyName);

            //Get the names of all properties specified in the target property's DependsOnAttributes
            var dependencyNames = from attribute in propertyInfo
                                      .GetCustomAttributes(typeof(DependsOnAttribute), true)
                                      .Cast<DependsOnAttribute>()
                                  where attribute.Property != propertyName //Ignore the original property, if it depends on itself
                                  select attribute.Property;

            //Return the matching PropertyInfo objects
            return targetType.GetRuntimeProperties().Where(x => dependencyNames.Any(y => y == x.Name));
        }

        /// <summary>
        /// Recursively gathers the list of all properties on which the specified
        /// property depends, whether directly or indirectly.
        /// </summary>
        /// <param name="targetType">The object in which the properties reside.</param>
        /// <param name="propertyName">
        /// The property for which to collect all dependencies.
        /// </param>
        /// <returns>
        /// Returns the list of all properties on which the given
        /// property directly or indirectly depends.
        /// </returns>
        public static IEnumerable<PropertyInfo> GetAllDependencies(Type targetType, string propertyName)
        {
            //Retrieve the Property Info for the specified property
            var propertyInfo = targetType.GetRuntimeProperties()
                .First(x => x.Name == propertyName);

            IEnumerable<PropertyInfo> oldResults = null;
            IEnumerable<PropertyInfo> results = new[] { propertyInfo };
            do
            {
                oldResults = results;

                var dependancies = from input in results
                                   from dependancy in GetDirectDependencies(targetType, input.Name)
                                   select dependancy;

                //Create union of current results with "new" results,
                //making sure to remove duplicates
                results = results.Union(dependancies)
                    .GroupBy((x) => x.Name)
                    .Select(grp => grp.First());
            }
            while (results.Count() > oldResults.Count());

            return results.Where(x => (x.Name != propertyName));
        }

        #endregion Public Static Methods
    }
}