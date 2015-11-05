using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace vdproj
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 3) throw new Exception("Wrong number of arguments");

                string filename = args[0];
                string productVersion = args[1];
                string guid = args[2];

                string data = ReadFile(filename);

                var node = Node.Parse(data);

                var query = from n in node.Children
                            where n.Name == "Deployable"
                            from c in n.Children
                            where c.Name == "Product"
                            select c;

                var product = query.First();

                product.Attributes.Where(a => a.Name == "ProductVersion").First().Value = "8:" + productVersion;
                product.Attributes.Where(a => a.Name == "ProductCode").First().Value = "8:{" + guid + "}";

                WriteFile(filename, node);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Usage: program_name vdproj_file ProductVersion GUID");
                Console.WriteLine("Example: program_name .\\Setup.vdproj 1.2.3 3D1A0087-FE47-481B-BB36-8EFA365EE942");
                return -1;
            }
            return 0;
        }

        static string ReadFile(string filename)
        {
            using (var file = new FileStream(filename, FileMode.Open))
            {
                return new StreamReader(file).ReadToEnd();
            }
        }

        static void WriteFile(string filename, Node node)
        {
            using (var writer = new StreamWriter(new FileStream(filename, FileMode.Create)))
            {
                Node.Write(writer, node);
            }
        }
    }
}
