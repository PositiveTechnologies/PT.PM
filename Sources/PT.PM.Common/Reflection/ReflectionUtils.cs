using System.Reflection;

namespace PT.PM.Common.Reflection
{
    public static class ReflectionUtils
    {
        public static bool IsActual(this Assembly assembly)
        {
            string assemblyName = assembly.GetName().Name;
            return assemblyName.StartsWith("PT.PM") && !assemblyName.Contains("Test");
        }
    }
}
