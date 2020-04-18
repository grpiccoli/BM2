using BiblioMit.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace BiblioMit.Models
{
    public class Indexed : IHasBasicIndexer
    {
        [ParseSkip]
        public object this[string propertyName]
        {
            get
            {
                PropertyInfo propertyInfo = GetType().GetProperty(propertyName);
                if (propertyInfo == null) return null;
                return propertyInfo.GetValue(this, null);
            }
            set
            {
                PropertyInfo propertyInfo = GetType().GetProperty(propertyName);
                if (value == null)
                {
                    if (propertyInfo.PropertyType.IsGenericType)
                    {
                        if (propertyInfo.PropertyType.IsClass && propertyInfo.CanWrite)
                        {
                            SetValue(this, propertyInfo, propertyName, value);
                        }
                        else
                        {
                            var def = propertyInfo.PropertyType.GetGenericTypeDefinition();
                            if (propertyInfo.GetSetMethod() == null && def == typeof(ICollection<>))
                            {
                                var list = propertyInfo.GetValue(this, null);
                                propertyInfo.PropertyType.InvokeMember("Clear",
                                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                    null, list, null,
                                    CultureInfo.InvariantCulture);
                            }
                            else if (def == typeof(Nullable<>) && propertyInfo.CanWrite)
                            {
                                SetValue(this, propertyInfo, propertyName, value);
                            }
                            else
                            {
                                throw new InvalidCastException();
                            }
                        }
                    }
                }
                else
                {
                    var inputType = value.GetType();
                    if (inputType.IsGenericType)
                    {
                        var def = propertyInfo.PropertyType.GetGenericTypeDefinition();
                        if (propertyInfo.GetSetMethod() == null && def == typeof(ICollection<>))
                        {
                            var list = propertyInfo.GetValue(this, null);
                            propertyInfo.PropertyType.InvokeMember("Clear", 
                                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                null, list, null, 
                                CultureInfo.InvariantCulture);
                            foreach (var item in value as ICollection)
                                propertyInfo.PropertyType.InvokeMember("Add",
                                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                    null, list, new object[] { item },
                                    CultureInfo.InvariantCulture);
                        }
                    }
                    else if (inputType == propertyInfo.PropertyType && propertyInfo.CanWrite)
                    {
                        SetValue(this, propertyInfo, propertyName, value);
                    }
                    else
                    {
                        if (propertyInfo.PropertyType.IsGenericType)
                        {
                            var def = propertyInfo.PropertyType.GetGenericTypeDefinition();
                            if (def == typeof(Nullable<>) && propertyInfo.CanWrite)
                            {
                                var t = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                                if (inputType == t)
                                SetValue(this, propertyInfo, propertyName, value);
                            }
                        }
                        else
                        {
                            TypeConverter typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                            object propValue = typeConverter.ConvertFromString(value.ToString());
                            SetValue(this, propertyInfo, propertyName, propValue);
                        }
                    }
                }
            }
        }
        public static void SetValue(Indexed entity, PropertyInfo propertyInfo, string propertyName, object value)
        {
            if (propertyInfo == null) return;
            if (propertyInfo.GetSetMethod(true).IsPublic)
                propertyInfo.SetValue(entity, value, null);
            else
                propertyInfo.PropertyType.InvokeMember($"Set{propertyName}",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                null, entity, new object[] { value },
                CultureInfo.InvariantCulture);
        }
    }
}
