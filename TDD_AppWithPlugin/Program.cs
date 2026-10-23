using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using TDD_PluginBase;

namespace TDD_AppWithPlugin
{
    class AppContent : IAppContent
    {
        public required string var1 { get; set; }
        public int var2 { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 1 && args[0] == "/d")
                {
                    Console.WriteLine("Waiting for any key...");
                    Console.ReadLine();
                }

                // Load commands from plugins.
                string[] pluginPaths =
                [
                    // Path to the plugin.
                    @"TDD_HelloPlugin\bin\Debug\net10.0\TDD_HelloPlugin.dll"
                ];
                IEnumerable<TDD_PluginBase.ICommand> commands = [.. pluginPaths.SelectMany(path =>
                {
                    Assembly pluginAssembly = LoadPlugin(path);
                    return CreateCommands(pluginAssembly);
                })];

                if (args.Length == 0)
                {
                    Console.WriteLine("Commands: ");
                    // Output the loaded commands.
                    foreach (TDD_PluginBase.ICommand command in commands)
                    {
                        Console.WriteLine($"{command.Name}\t - {command.Description}");
                    }
                }
                else
                {
                    foreach (string commandName in args)
                    {
                        Console.WriteLine($"-- {commandName} --");

                        // Skip command-line switches/flags (arguments starting with '/' or '-')
                        if (commandName.StartsWith("/") || commandName.StartsWith("-"))
                        {
                            Console.WriteLine($"Skipping command-line flag: {commandName}");
                            continue;
                        }

                        // Execute the command with the name passed as an argument.
                        TDD_PluginBase.ICommand command = commands.FirstOrDefault(c => c.Name == commandName);
                        if (command == null)
                        {
                            Console.WriteLine($"No such command is known: {commandName}");
                            continue;
                        }

                        AppContent content = new()
                        {
                            var1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            var2 = 42,
                        };

                        command.Execute(content);

                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static Assembly LoadPlugin(string relativePath)
        {
            // Navigate up to the solution root
            string root = Path.GetFullPath(
                Path.Combine(typeof(Program).Assembly.Location, "..", "..", "..", "..", ".."));

            string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            Console.WriteLine($"Loading commands from: {pluginLocation}");
            PluginLoadContext loadContext = new(pluginLocation);
            return loadContext.LoadFromAssemblyName(new(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        static IEnumerable<TDD_PluginBase.ICommand> CreateCommands(Assembly assembly)
        {
            int count = 0;

            foreach (var type in assembly.GetTypes().Where(t => typeof(TDD_PluginBase.ICommand).IsAssignableFrom(t)))
            {
                if (Activator.CreateInstance(type) is TDD_PluginBase.ICommand result)
                {
                    count++;
                    yield return result;
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
