using BiblioMit.Services;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BiblioMit.Models;
using System.Threading.Tasks;

namespace BiblioMit.Extensions
{
    public static class EntityExtensions
    {
        [SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "includes circular definition")]
        private static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        public static bool IsBoxed<T>(this T value) =>
            (typeof(T).IsInterface || typeof(T) == typeof(object)) &&
            value != null &&
            value.GetType().IsValueType;
        public static void AddChanges<T>(this T val1, T val2) where T : class, IHasBasicIndexer
        {
            if (val1 != null && val2 != null)
            {
                var fi = val1.GetType().GetProperties(BindingFlags).Where(f => !f.PropertyType.IsClass && !f.PropertyType.IsInterface);
                foreach (var f in fi)
                {
                    var var1 = f.GetValue(val1);
                    var var2 = f.GetValue(val2);
                    if ((var1 == null && var2 != null) || (var1 != null && !var1.Equals(var2)))
                        val1[f.Name] = val2[f.Name];
                }
            }
        }
        public static void AddToPhyto(this Phytoplankton fito, double ce, Ear? e)
        {
            if (fito == null) return;
            fito.C += ce;
            if (e.HasValue && !(fito.EAR.HasValue && fito.EAR.Value >= e))
                fito.EAR = e;
        }
        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            if (@this is null) return null;
            Task task = (Task)@this.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }
    }
}
