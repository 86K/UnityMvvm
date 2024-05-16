

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if NETFX_CORE
using System.Threading.Tasks;
using System.Linq;
#endif

namespace Fusion.Mvvm
{
    public class TypeFinderUtils
    {
#if NETFX_CORE
        private static readonly ILog log = LogManager.GetLogger(typeof(TypeFinderUtils));
#endif

        private static readonly List<Assembly> assemblies = new List<Assembly>();

        public static Type FindType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
#if NETFX_CORE
            Task<List<Assembly>> task = GetAssemblies();
            task.Wait();
            List<Assembly> assemblies = task.Result;
#else
            List<Assembly> assemblies = GetAssemblies();
#endif
            foreach (Assembly assembly in assemblies)
            {
                Type type = assembly.GetType(typeName, false, false);
                if (type != null)
                    return type;
            }

            var name = $".{typeName}";
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.FullName.EndsWith(name))
                        return type;
                }
            }
            return null;
        }

#if NETFX_CORE
        private static async Task<List<Assembly>> GetAssemblies()
        {
            if (assemblies.Count > 0)
                return assemblies;

            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            if (files == null)
                return assemblies;

            foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe"))
            {
                try
                {
                    if (Regex.IsMatch(file.Name, "^((mscorlib)|(nunit)|(ucrtbased)|(Microsoft)|(ClrCompression)|(BridgeInterface)|(System)|(UnityEngine)|(UnityPlayer)|(Loxodon.Log)|(WinRTLegacy))"))
                        continue;

                    assemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
                }
                catch (Exception e) 
                {
                     if (log.IsWarnEnabled)
                        log.Warn("Loads assembly:{0}", e);
                }
            }
            return assemblies;
        }
#else
        private static List<Assembly> GetAssemblies()
        {
            if (assemblies.Count > 0)
                return assemblies;

            Assembly[] listAssembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in listAssembly)
            {
                var name = assembly.FullName;
                if (Regex.IsMatch(name, "^((mscorlib)|(nunit)|(System)|(UnityEngine)|(Loxodon.Log))"))
                    continue;

                assemblies.Add(assembly);
            }
            return assemblies;
        }
#endif
    }
}
