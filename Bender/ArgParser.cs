namespace Bender
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Options
    {
        public Options()
        {
            Okay = true;
        }

        public bool Okay { get; set; }

        public string Message { get; set; }

        public string SpecFile { get; set; }

        public string BinaryFile { get; set; }

        /// <summary>
        /// YAML search path
        /// </summary>
        public string Root { get; set; }

        public bool PrintSpec { get; set; }

        public static implicit operator bool(Options o)
        {
            return o.Okay;
        }
        public override string ToString()
        {
            return Message;
        }
    }

    internal class ArgParser
    {       
        private readonly IList<string> _mRequired = new List<string> { "-b" };

        public Options Parse(string[] args)
        {
            var result = new Options();

            if (args.Length == 0)
            {
                result.Okay = false;
                result.Message = Usage();
                return result;
            }

            Action<string> nextCapture = null;
            var satisified = new List<string>();

            // Read input in pairs
            foreach(var str in args)
            {
                if (!result.Okay)
                {
                    return result;
                }

                if (nextCapture != null)
                {
                    nextCapture(str);
                    nextCapture = null;
                }
                else
                {
                    switch (str)
                    {
                        case "-s":
                        case "--spec":
                            nextCapture = (s) => result.SpecFile = s;           
                            break;
                        case "-b":
                        case "--binary":
                            nextCapture = (s) => result.BinaryFile = s;
                            satisified.Add("-b");
                            break;
                        case "-p":
                        case "--print-spec":
                            result.PrintSpec = true;
                            break;
                        case "-r":
                        case "--root":
                            nextCapture = (s) => result.Root = s;
                            break;
                        default:
                            result.Okay = false;
                            result.Message = string.Format("Unknown argument: {0}", str);
                            break;
                    }
                }
            }

            if (!satisified.Equals(_mRequired)) return result;
            result.Okay = false;
            result.Message = string.Format("Missing expected parameters: {0}",
                string.Join(",", _mRequired.Except(satisified)));

            return result;
        }

        public string Usage()
        {
            const string usage = "Bender.exe -f /path/to/spec.yaml -b /path/to/your.bin [--print-spec]\n" +
                                 "-s,--spec\t\tIndicates which specification to use. This is the file in YAML\n" +
                                 "-b,--binary\t\tIndicates which binary to parse. This is the binary described by your spec\n" +
                                 "-r,--root\t\tManually specify a YAML root for detecting binary specs (Optional)" +
                                 "-p,--print-spec\t\tPrints the spec file to stdout (optional)";
            return usage;
        }
    }
}
