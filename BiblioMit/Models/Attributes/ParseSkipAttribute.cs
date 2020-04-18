using System;

namespace BiblioMit.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ParseSkipAttribute : Attribute
    {
    }
}
