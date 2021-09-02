using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BiblioMit.Models.VM;

namespace BiblioMit.Extensions
{
    public static partial class EnumExtensions
    {
        //Get MultiSelect from Enum with selected / disabled values
        #region MultiSelect
        public static MultiSelectList Enum2MultiSelect<TEnum>(this TEnum @enum,
            IEnumerable<string> selectedValues = null,
            string name = null)
            where TEnum : struct, IConvertible, IFormattable => name switch
            {
                "Name" => @enum.Name2MultiSelect(selectedValues),
                "Description" => @enum.Description2MultiSelect(selectedValues),
                _ => @enum.Flag2MultiSelect(selectedValues)
            };
        public static MultiSelectList Description2MultiSelect<TEnum>(this TEnum _,
            IEnumerable<string> selectedValues = null)
            where TEnum : struct, IConvertible, IFormattable => 
            new(((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(s => new SelectListItem
                {
                    Value = s.ToString("d", null),
                    Text = s.GetAttrDescription()
                }), selectedValues);
        public static IEnumerable<ChoicesGroup> Enum2ChoicesGroup<TEnum>(this TEnum _, string prefix = null)
            where TEnum : struct, IConvertible, IFormattable =>
            ((TEnum[])Enum.GetValues(typeof(TEnum))).GroupBy(e => e.GetAttrGroupName())
            .OrderBy(g => g.Key)
            .Select((g, i) => new ChoicesGroup
                {
                    Label = g.Key,
                    Choices = g.Select(t => new ChoicesItem 
                    { 
                        Label = $"{t.GetAttrName()} ({t.GetAttrPrompt()})", 
                        Value = prefix + t.ToString("d", null)
                    })
                });
    public static MultiSelectList Name2MultiSelect<TEnum>(this TEnum _,
            IEnumerable<string> selectedValues = null)
            where TEnum : struct, IConvertible, IFormattable =>
            new(((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(s => new SelectListItem
                {
                    Value = s.ToString("d", null),
                    Text = s.GetAttrName()
                }), selectedValues);
        public static MultiSelectList Flag2MultiSelect<TEnum>(this TEnum _,
            IEnumerable<string> selectedValues = null)
            where TEnum : struct, IConvertible, IFormattable =>
            new(((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(s => new SelectListItem
                {
                    Value = s.ToString("d", null),
                    Text = s.ToString()
                }), selectedValues);
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
        public static IEnumerable<SelectListItem> Name2Select<TEnum>(this TEnum _)
            where TEnum : struct, IConvertible, IFormattable => ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(t => new SelectListItem
                {
                    Value = t.ToString("d", null),
                    Text = t.GetAttrName()
                });
        public static IEnumerable<SelectListItem> Description2Select<TEnum>(this TEnum _)
            where TEnum : struct, IConvertible, IFormattable => ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Select(t => new SelectListItem
                {
                    Value = t.ToString("d", null),
                    Text = t.GetAttrDescription()
                });
        public static IEnumerable<SelectListItem> Flag2Select<TEnum>(this TEnum _)
            where TEnum : struct, IConvertible, IFormattable => 
            ((TEnum[])Enum.GetValues(typeof(TEnum))).Select(t => 
            new SelectListItem
            {
                Value = t.ToString("d", null),
                Text = t.ToString()
            });
        public static IEnumerable<TEnum> Enum2List<TEnum>(this TEnum _)
            where TEnum : struct, IConvertible, IFormattable => 
            ((TEnum[])Enum.GetValues(typeof(TEnum))).Select(t => t);
        public static IEnumerable<string> Enum2ListNames<TEnum>(this TEnum _)
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
