using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BiblioMit.Controllers;
using BiblioMit.Models.VM;

namespace BiblioMit.Extensions
{
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
                .Select(s => new SelectListItem
                { 
                    Value = s.ToString("d", null),
                    Text = s.GetAttrDescription()
                }),
                Filters != null
                && Filters.ContainsKey(typeof(TEnum).ToString()) ? Filters[typeof(TEnum).ToString()] : null);
        public static IEnumerable<ChoicesGroup> Enum2ChoicesGroup<TEnum>(this TEnum @enum, string prefix = null)
            where TEnum : struct, IConvertible, IFormattable =>
            ((TEnum[])Enum.GetValues(typeof(TEnum))).GroupBy(e => e.GetAttrGroupName())
            .OrderBy(g => g.Key)
            .Select((g, i) => new ChoicesGroup
                {
                    Label = g.Key,
                    //Id = i,
                    Choices = g.Select(t => new ChoicesItem 
                    { 
                        Label = $"{t.GetAttrName()} ({t.GetAttrPrompt()})", 
                        Value = prefix + t.ToString("d", null)
                    })
                });
    public static MultiSelectList Name2MultiSelect<TEnum>(this TEnum @enum,
            IDictionary<string, List<string>> Filters = null)
            where TEnum : struct, IConvertible, IFormattable =>
            new MultiSelectList(((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(s => new SelectListItem
                {
                    Value = s.ToString("d", null),
                    Text = s.GetAttrName()
                }),
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
                .Select(t => new SelectListItem
                {
                    Value = t.ToString("d", null),
                    Text = t.GetAttrName()
                });
        public static IEnumerable<SelectListItem> Description2Select<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(t => new SelectListItem
                {
                    Value = t.ToString("d", null),
                    Text = t.GetAttrDescription()
                });
        public static IEnumerable<SelectListItem> Flag2Select<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => 
            ((TEnum[])Enum.GetValues(typeof(TEnum))).Select(t => 
            new SelectListItem
            {
                Value = t.ToString("d", null),
                Text = t.ToString()
            });
        public static IEnumerable<TEnum> Enum2List<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => 
            ((TEnum[])Enum.GetValues(typeof(TEnum))).Select(t => t);
        public static IEnumerable<string> Enum2ListNames<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible, IFormattable => 
            ((TEnum[])Enum.GetValues(typeof(TEnum))).Select(t => t.ToString());
        #endregion
        //Get Enum Attributes
        #region EnumAttributes
        public static string GetAttribute<TEnum>(this TEnum e, string attr)
        {
            var display = e.GetType().GetMember(e.ToString())
                  .FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>(false);
            try
            {
                return display?.GetType().InvokeMember($"Get{attr}",
                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, display, null,
                    CultureInfo.InvariantCulture) as string ?? e.ToString();
            }
            catch (TargetInvocationException)
            {
                return display?.GetType().GetProperty(attr).GetValue(display, null) as string ?? e.ToString();
            }
        }
        public static string GetAttrDescription<TEnum>(this TEnum e) => e.GetAttribute("Description");
        public static string GetAttrPrompt<TEnum>(this TEnum e) => e.GetAttribute("Prompt");
        public static string GetAttrName<TEnum>(this TEnum e) => e.GetAttribute("Name");
        public static string GetAttrGroupName<TEnum>(this TEnum e) => e.GetAttribute("GroupName");
        public static IEnumerable<string> GetNamesList<TEnum>(this TEnum e) =>
            e.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.Static)
              .Select(m => m
              .GetCustomAttribute<DisplayAttribute>(false)
              ?.GetName() ?? m.ToString());
        #endregion
    }
}
