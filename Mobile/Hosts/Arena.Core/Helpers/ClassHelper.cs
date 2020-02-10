using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arena.Core.Helpers
{
    public static class ClassHelper
    {
        public static T CopyClass<T>(T obj)
        {
            T objcpy = (T)Activator.CreateInstance(typeof(T));
            foreach (var prop in obj.GetType().GetProperties())
            {
                var value = prop.GetValue(obj);
                objcpy.GetType().GetProperty(prop.Name).SetValue(objcpy, value);
            }
            return objcpy;
        }

        /// <summary>
        /// Determine if two objects agree property-by-property.
        /// <br />
        /// See: https://stackoverflow.com/questions/506096/comparing-object-properties-in-c-sharp
        /// </summary>
        /// 
        public static bool PropertiesEqual<T>(this T self, T to,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance,
            params string[] ignore) where T : class
        {
            if (self != null && to != null)
            {
                var type = typeof(T);
                var ignoreList = ignore?.ToList() ?? new List<string>();
                var unequalProperties =
                    from pi in type.GetProperties(bindingFlags)
                    where !ignoreList.Contains(pi.Name) && pi.GetUnderlyingType().IsSimpleType() && pi.GetIndexParameters().Length == 0
                    let selfValue = type.GetProperty(pi.Name).GetValue(self, null)
                    let toValue = type.GetProperty(pi.Name).GetValue(to, null)
                    where selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue))
                    select selfValue;
                return !unequalProperties.Any();
            }
            return self == to;
        }
    }

    public static class TypeExtensions
    {
        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc) 
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(
           this Type type)
        {
            return
               type.IsValueType ||
               type.IsPrimitive ||
               new[]
               {
               typeof(String),
               typeof(Decimal),
               typeof(DateTime),
               typeof(DateTimeOffset),
               typeof(TimeSpan),
               typeof(Guid)
               }.Contains(type) ||
               (Convert.GetTypeCode(type) != TypeCode.Object);
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                       "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
    }
}
