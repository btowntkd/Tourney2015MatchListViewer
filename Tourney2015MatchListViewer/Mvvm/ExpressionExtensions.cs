using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiritMVVM
{
    /// <summary>
    /// Provides extension methods for lambda expressions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Retrieve the string name of the given property member expression.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertySelector">The property expression.</param>
        /// <returns>Returns the string name of the referred property.</returns>
        public static string PropertyName<TProperty>(this Expression<Func<TProperty>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression does not reference a valid member", "propertySelector");

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Retrieve the parent object of the specified member selector.
        /// </summary>
        /// <param name="memberExpression">An expression referencing a member.</param>
        /// <returns>Returns the parent object instance of the specified property.</returns>
        public static object GetParentInstance(this MemberExpression memberExpression)
        {
            var memberInfoStack = new Stack<MemberInfo>();
            
            //Descend to the root of the expression
            Expression currentExpr = memberExpression;
            while (currentExpr is MemberExpression)
            {
                var memberExpr = currentExpr as MemberExpression;
                memberInfoStack.Push(memberExpr.Member);
                currentExpr = memberExpr.Expression;
            }

            var constExpr = currentExpr as ConstantExpression;
            object currInstance = null;
            if (constExpr == null)
            {
                //Static field
                throw new ArgumentException("Unable to retrieve parent object when property selector references a static member.");
            }
            else
            {
                //Instance field
                currInstance = constExpr.Value;
            }

            //Pop back up the expression tree, retrieving the instance values for
            //each node until we reach the parent object
            while (memberInfoStack.Count > 1)
            {
                var memberInfo = memberInfoStack.Pop();

                var runtimeProperty = currInstance.GetType().GetRuntimeProperty(memberInfo.Name);
                if (runtimeProperty != null)
                {
                    currInstance = runtimeProperty.GetValue(currInstance);
                    continue;
                }
                var runtimeField = currInstance.GetType().GetRuntimeField(memberInfo.Name);
                if (runtimeField != null)
                {
                    currInstance = runtimeField.GetValue(currInstance);
                    continue;
                }
                
            }
            return currInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static Ref<TProperty> GetAccessorForProperty<TProperty>(this Expression<Func<TProperty>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if(memberExpression == null)
                throw new ArgumentException("Expression does not reference a valid property", "propertySelector");

            //PropertyInfo will be captured by the Accessor's get/set expression, and will not be garbage collected
            var propInfo = memberExpression.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("Expression does not reference a valid property", "propertySelector");

            object parentObj = memberExpression.GetParentInstance();
            var accessor = new Ref<TProperty>(
                () => (TProperty)propInfo.GetValue(parentObj),
                (x) => propInfo.SetValue(parentObj, x));

            return accessor;
        }
    }
}