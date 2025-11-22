using System.Reflection;
using System.Linq;
using System.IO;
using Cruddy.Cli.Core;
using Cruddy.Core.Attributes;

namespace Cruddy.Cli.Commands
{
    public class ScanEntitiesCommand 
    {
        public string Name => "scan";
        public void ExecuteAsync(string[] args)
        {
            if (!IsProjectDirectoryValid())
            {
                Console.WriteLine("[Cruddy] No .csproj file found in the current directory. Please run this command from your project root.");
                return;
            }
            var metadataDir = Path.Combine(Directory.GetCurrentDirectory(), "Cruddy.Metadata");
            if (!Directory.Exists(metadataDir))
            {
                Directory.CreateDirectory(metadataDir);
                Console.WriteLine($"[Cruddy] Created metadata folder at: {metadataDir}");
            }

            var assemblies = LoadAssembliesFromCurrentDirectory().ToList();
            Console.WriteLine($"[Cruddy] Discovered {assemblies.Count} assemblies. Starting scan...");

            foreach (var assembly in assemblies)
            {
                Console.WriteLine($"[Cruddy] Scanning assembly: {assembly.GetName().Name}");
                var entityTypes = assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes(typeof(EntityAttribute), inherit: false).Any());

                foreach (var type in entityTypes)
                {
                    var attr = (EntityAttribute?)type.GetCustomAttributes(typeof(EntityAttribute), false).FirstOrDefault();
                    Console.WriteLine($"[Entity] {type.FullName} → Target: {attr?.TargetType}");
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in properties)
                    {
                        Console.WriteLine($"   ↳ {prop.Name} : {prop.PropertyType.Name}");
                    }
                }
            }
            Console.WriteLine("[Cruddy] Entity scan complete.");
        }


        private IEnumerable<Assembly> LoadAssembliesFromCurrentDirectory()
        {
            var dlls = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll", SearchOption.AllDirectories);
            var loadedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dll in dlls)
            {
                Assembly? asm = null;
                try
                {
                    asm = Assembly.LoadFrom(dll);
                }
                catch { }

                if (asm != null && loadedNames.Add(asm.FullName!))
                {
                    yield return asm;
                }
            }
        }

        private bool IsProjectDirectoryValid()
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.TopDirectoryOnly).Any();
        }
    }
}