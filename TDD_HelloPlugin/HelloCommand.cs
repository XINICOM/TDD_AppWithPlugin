using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using TDD_PluginBase;

namespace TDD_HelloPlugin
{
    public class HelloCommand : ICommand
    {
        public string Name { get => "hello"; }
        public string Description { get => "Displays hello message."; }

        public int Execute(IAppContent content)
        {
            Console.WriteLine("Hello, world !!!  <== from hello plugin");
            string jsonContent = JsonConvert.SerializeObject(content, Formatting.Indented);
            Console.WriteLine(jsonContent);
            //Console.WriteLine($"\tstring : {content.var1}");
            //Console.WriteLine($"\tint    : {content.var2}");
            return 0;
        }
    }
}
