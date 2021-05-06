using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Semio.ClientService.Data
{
    ///<summary>
    /// Provides support for extracting property information based on a property expression.
    ///</summary>
    public static class PropertySupport
    {
        /// <summary>
        /// Extracts the property name from a property expression.
        /// </summary>
        /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
        /// <returns>The name of the property.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyExpression"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the expression is:<br/>
        ///     Not a <see cref="MemberExpression"/><br/>
        ///     The <see cref="MemberExpression"/> does not represent a property.<br/>
        ///     Or, the property is static.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(@"The expression is not a member access expression", nameof(propertyExpression));
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(@"The member access expression does not access property", nameof(propertyExpression));
            }

            var getMethod = property.GetGetMethod(true);
            if (getMethod.IsStatic)
            {
                throw new ArgumentException(@"The referenced property is a static property", nameof(propertyExpression));
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Creates an action to execute the getter of the property specified by the expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>Get Action</returns>
        public static Action<T, TProperty> GetSetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var setMethod = property.GetSetMethod();

            return (i, p) => setMethod.Invoke(i, new object[] { p });
        }

        /// <summary>
        /// Creates an action to execute the getter of the property specified by the expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="owner"></param>
        /// <returns>Get Action</returns>
        public static Action<TProperty> GetSetter<T, TProperty>(Expression<Func<TProperty>> expression, T owner)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var setMethod = property.GetSetMethod();

            return p => setMethod.Invoke(owner, new object[] { p });
        }

        /// <summary>
        /// Creates a function to execute the setter of the property specified by the expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>Set Function</returns>
        public static Func<T, TProperty> GetGetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var getMethod = property.GetGetMethod();

            return i => (TProperty)getMethod.Invoke(i, null);
        }

        /// <summary>
        /// Creates a function to execute the setter of the property specified by the expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="owner"></param>
        /// <returns>Set Function</returns>
        public static Func<TProperty> GetGetter<T, TProperty>(Expression<Func<TProperty>> expression, T owner)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var getMethod = property.GetGetMethod();

            return () => (TProperty)getMethod.Invoke(owner, null);
        }
    }
}