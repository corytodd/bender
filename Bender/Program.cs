using System;
using System.IO;
using BenderLib;

namespace Bender
{
    class Program
    {
        static void Main(string[] args)
        {
            ArgParser parser = new ArgParser();
            var opts = parser.Parse(args);

            var ret = Run(opts);
            Environment.Exit(ret);  
        }

        private static int Run(Options opts)
        {
            if (!opts)
            {
                Console.WriteLine(opts.Message);
                return 1;
            }

            var file = DataFile.From(opts.SpecFile);
            if (file.Empty)
            {
                Console.WriteLine("Spec file is empty, cannot be read... etc. I can't read it.");
                return 1;
            }

            try
            {
                var spec = new SpecParser().Parse(file);

                if (opts.PrintSpec)
                {
                    Console.WriteLine("Specification File: {0}", opts.SpecFile);
                    Console.WriteLine(new string('=', 80));

                    Console.WriteLine(spec.ToString());

                    Console.WriteLine("-- Spec End");
                    Console.WriteLine(new string('=', 80));
                }

                var binary = DataFile.From(opts.BinaryFile);
                if (binary.Empty)
                {
                    Console.WriteLine("Binary file is empty, cannot be read... etc. I can't read it.");
                    return 1;
                }

                var bender = new BinaryParser(spec).Parse(binary);

                Console.WriteLine("Binary File - {0}{1}", spec.Name, Environment.NewLine);
                using (var stream = new MemoryStream())
                {

                    var printer = new BenderPrinter();
                    printer.WriteStream(bender, stream);


                    // Stream content to console
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            Console.WriteLine(reader.ReadLine());
                        }
                    }                
                }

                Console.WriteLine();
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }
    }
}
