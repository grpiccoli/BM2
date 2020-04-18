using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.ComponentModel;
using System.Globalization;

namespace BiblioMit.Extensions
{
    public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceKey;
        private readonly ResourceManager _resource;
        public LocalizedDescriptionAttribute(
            string resourceKey,
            Type resourceType)
        {
            _resource = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }
        public override string Description
        {
            get
            {
                string displayName = _resource.GetString(_resourceKey, CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(displayName)
                    ? string.Format(CultureInfo.InvariantCulture, "{0}", _resourceKey)
                    : displayName;
            }
        }
    }
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Extensions require 'this' parameter")]
    public static partial class EnumExtensions
    {
        //Get MultiSelect from Enum with selected / disabled values
        #region MultiSelect
        public static MultiSelectList Enum2MultiSelect<TEnum>(this TEnum @enum,
            IDictionary<string, List<string>> Filters = null,
            string name = null)
            where TEnum : struct, IConvertible, IFormattable => name switch
            {
                "Name" => @enum.Name2MultiSelect(Filters),
                "Description" => @enum.Description2MultiSelect(Filters),
                _ => @enum.Flag2MultiSelect(Filters)
            };
        public static MultiSelectList Description2MultiSelect<TEnum>(this TEnum @enum,
            IDictionary<string, List<string>> Filters = null)
            where TEnum : struct, IConvertible, IFormattable => 
            new MultiSelectList(((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(async s => new SelectListItem
                { 
                    Value = s.ToString("d", null),
                    Text = await s.GetAttrDescriptionAsync().ConfigureAwait(false)
                }).Select(t => t.Result),
                Filters != null
                && Filters.ContainsKey(typeof(TEnum).ToString()) ? Filters[typeof(TEnum).ToString()] : null);
        public static MultiSelectList Name2MultiSelect<TEnum>(this TEnum @enum,
            IDictionary<string, List<string>> Filters = null)
            where TEnum : struct, IConvertible, IFormattable =>
            new MultiSelectList(((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(async s => new SelectListItem
                {
                    Value = s.ToString("d", null),
                    Text = await s.GetAttrNameAsync().ConfigureAwait(false)
                }).Select(t => t.Result),
                Filters != null
                && Filters.ContainsKey(typeof(TEnum).ToString()) ? Filters[typeof(TEnum).ToString()] : null);
        public static MultiSelectList Flag2MultiSelect<TEnum>(this TEnum @enum,
            IDictionary<string, List<string>> Filters = null)
            where TEnum : struct, IConvertible, IFormattable =>
            new MultiSelectList(((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(s => new SelectListItem
                {
                    Value = s.ToString("d", null),
                    Text = s.ToString()
                }), Filters != null
        && Filters.ContainsKey(typeof(TEnum).ToString()) ? Filters[typeof(TEnum).ToString()] : null);
        #endregion
        //Get SelectList of Attributes from Enum
        #region Enum2Select
        public static IEnumerable<SelectListItem> Enum2Select<TEnum>(this TEnum @enum, string name = null)
    where TEnum : struct, IConvertible, IFormattable => name switch
            {
                "Name" => @enum.Name2Select(),
                "Description" => @enum.Description2Select(),
                _ => @enum.Flag2Select()
            };
        public static IEnumerable<SelectListItem> Name2Select<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(async t => new SelectListItem
                {
                    Value = t.ToString("d", null),
                    Text = await t.GetAttrNameAsync().ConfigureAwait(false)
                }).Select(t => t.Result);
        public static IEnumerable<SelectListItem> Description2Select<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(async t => new SelectListItem
                {
                    Value = t.ToString("d", null),
                    Text = await t.GetAttrDescriptionAsync().ConfigureAwait(false)
                }).Select(t => t.Result);
        public static IEnumerable<SelectListItem> Flag2Select<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(t => new SelectListItem
                {
                    Value = t.ToString("d", null),
                    Text = t.ToString()
                });
        public static IEnumerable<TEnum> Enum2List<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(t => t);
        #endregion
        //Get Enum Attributes
        #region EnumAttributes
        public static async Task<string> GetAttrDescriptionAsync<TEnum>(this TEnum e) =>
            await e.LocalizeStringAsync(e.GetType()
        .GetMember(e.ToString())
      .FirstOrDefault()
      ?.GetCustomAttribute<DisplayAttribute>(false)
      ?.Description
      ?? e.ToString()).ConfigureAwait(false);
        public static async Task<string> GetAttrPromptAsync<TEnum>(this TEnum e) =>
            await e.LocalizeStringAsync(e.GetType()
                .GetMember(e.ToString())
              .FirstOrDefault()
              ?.GetCustomAttribute<DisplayAttribute>(false)
              ?.Prompt
              ?? e.ToString()).ConfigureAwait(false);
        public static async Task<string> GetAttrNameAsync<TEnum>(this TEnum e) =>
            await e.LocalizeStringAsync(e.GetType()
                .GetMember(e.ToString())
              .FirstOrDefault()
              ?.GetCustomAttribute<DisplayAttribute>(false)
              ?.Name
              ?? e.ToString()).ConfigureAwait(false);
        public static async Task<string> GetAttrGroupNameAsync<TEnum>(this TEnum e) =>
            await e.LocalizeStringAsync(e.GetType()
                .GetMember(e.ToString())
              .FirstOrDefault()
              ?.GetCustomAttribute<DisplayAttribute>(false)
              ?.GroupName
              ?? e.ToString()).ConfigureAwait(false);
        #endregion
        public static string GetDescription(this Enum enumValue)
        {
            FieldInfo fi = enumValue?.GetType().GetField(enumValue.ToString());
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);
            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return enumValue.ToString();
        }

        public static string Localize(this Enum e, string attribute, string lang)
        {
            if (lang != null && lang.Contains("es", StringComparison.InvariantCultureIgnoreCase))
            {
                //var rm = new ResourceManager(typeof(EnumResources));
                //var name = e?.GetType().Name + "_" + e + "_" + attribute;
                //var resourceDisplayName = rm.GetString(name, CultureInfo.InvariantCulture);
                //return string.IsNullOrWhiteSpace(resourceDisplayName) ? GetAttr(e, attribute) : resourceDisplayName;
                return GetAttr(e, attribute);
            }
            return GetAttr(e, attribute);
        }

        public static string GetAttr(this Enum e, string attribute) =>
            attribute switch
            {
                "Prompt" => e?.GetType()
                            .GetMember(e.ToString())
                          .FirstOrDefault()
                          ?.GetCustomAttribute<DisplayAttribute>(false)
                          ?.Prompt
                          ?? e.ToString(),
                "Description" =>
                        e?.GetType()
                            .GetMember(e.ToString())
                          .FirstOrDefault()
                          ?.GetCustomAttribute<DisplayAttribute>(false)
                          ?.Description
                          ?? e.ToString(),
                "GroupName" =>
                        e?.GetType()
                            .GetMember(e.ToString())
                          .FirstOrDefault()
                          ?.GetCustomAttribute<DisplayAttribute>(false)
                          ?.GroupName
                          ?? e.ToString(),
                _ =>
                        e?.GetType()
                            .GetMember(e.ToString())
                          .FirstOrDefault()
                          ?.GetCustomAttribute<DisplayAttribute>(false)
                          ?.Name
                          ?? e.ToString()
            };
        public static string GetDisplayName(this Enum e, string lang)
        {
            if (e == null)
            {
                return null;
            }
            if (lang != null && lang.Contains("es", StringComparison.InvariantCulture))
            {
                //var rm = new ResourceManager(typeof(EnumResources));
                //var name = e.GetType().Name + "_" + e;
                //var resourceDisplayName = rm.GetString(name, CultureInfo.InvariantCulture);
                //return string.IsNullOrWhiteSpace(resourceDisplayName) ? string.Format(CultureInfo.InvariantCulture, "{0}", e) : resourceDisplayName;
                return string.Format(CultureInfo.InvariantCulture, "{0}", e);
            }
            else
            {
                return e.GetType()
                    .GetMember(e.ToString())
                  .FirstOrDefault()
                  ?.GetCustomAttribute<DisplayAttribute>(false)
                  ?.Name
                  ?? e.ToString();
            }
        }
    }
}
