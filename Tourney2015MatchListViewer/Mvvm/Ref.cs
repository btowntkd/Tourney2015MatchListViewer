using System;
using System.Reflection;

namespace SpiritMVVM
{
    /// <summary>
    /// Contains Getter and Setter methods for an externally-referenced value.
    /// </summary>
    public class Ref<T>
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;

        #region Constructors

        /// <summary>
        /// Create a new instance of the <see cref="Ref{T}"/> object,
        /// with the given 'parent' host of the property, and the given <see cref="PropertyInfo"/> object.
        /// </summary>
        /// <param name="parent">The object in which the 'target' property is contained.</param>
        /// <param name="propInfo">The PropertyInfo object describing the target property.</param>
        public Ref(object parent, PropertyInfo propInfo)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (propInfo == null)
                throw new ArgumentNullException("propInfo");

            _getter = () => (T)propInfo.GetValue(parent);
            _setter = (x) => propInfo.SetValue(parent, x);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Ref{T}"/> object, with the given Getter and Setter methods.
        /// </summary>
        /// <param name="getter">The method to use when retrieving the item's value.</param>
        /// <param name="setter">The method to use when setting the item's value.</param>
        public Ref(Func<T> getter, Action<T> setter)
        {
            if (getter == null)
                throw new ArgumentNullException("getter");
            if (setter == null)
                throw new ArgumentNullException("setter");

            _getter = getter;
            _setter = setter;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Get or Set the underlying value using the getter or setter methods.
        /// </summary>
        public T Value
        {
            get { return _getter(); }
            set { _setter(value); }
        }

        #endregion Public Properties

        /// <summary>
        /// Implicit conversion into the wrapped type.
        /// </summary>
        /// <param name="accessor">The accessor which wraps an underlying value.</param>
        /// <returns>Returns the underlying value.</returns>
        public static implicit operator T(Ref<T> accessor)
        {
            return accessor.Value;
        }
    }
}