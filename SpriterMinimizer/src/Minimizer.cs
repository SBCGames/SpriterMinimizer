using System;
using System.IO;

namespace SpriterMinimizer {
    class Minimizer {
        // ----------------------------------------------------------
        static void Main(string[] aArgs) {
            // create options
            var options = new Options();
            if (!ParseArgs(aArgs, options)) {
                PrintUsage();
                Environment.Exit(1);
            }

            // read defs
            var defsReader = new DefsReader();
            var defs = defsReader.ReadDefs(options);

            // convert
            if (options.isInFileXml) {
                if (!options.outputBinary) {
                    new XmlMinimizer().Minimize(options, defs.rootDef);
                } else {
                    new Xml2Bin().Convert(options, defs.rootDef);
                }
            } else {

            }

            // to prevent closing console window
            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }
        
        // ----------------------------------------------------------
        private static void PrintUsage() {
            Console.WriteLine("Usage: SpriteMinimizer inputFile [outputFile] [-defs definitions] [-prettyPrint] [-binary] [-smallOffset]\n" +
                "\n" +
                "inputFile - file with Spriter .xml (.scml) animation to minimize\n" +
                "outputFile - file for minimized output (default = inputFile_out)\n" +
                "-defs definitions - .xml file with old and new names (default = definitions.xml)\n" +
                "-prettyPrint - make output nice and readable - good for checking if everything is OK or debug (default = false)\n" +
                "-binary - convert to binary output\n" +
                "-smallOffset - use short instead of int in binary format - can save around 10% of space\n"
                );
        }

        // ----------------------------------------------------------
        private static bool ParseArgs(string[] aArgs, Options aOptions) {
            int argIdx = 0;
            bool inFileProcessed = false;

            while (argIdx < aArgs.Length) {
                string arg = aArgs[argIdx++];

                switch (arg.ToLower()) {
                    case "-defs":
                        aOptions.defsFile = GetFileWithPath(aArgs[argIdx++]);
                        break;

                    case "-prettyprint":
                        aOptions.prettyPrint = true;
                        break;

                    case "-binary":
                        aOptions.outputBinary = true;
                        break;

                    case "-smalloffset":
                        aOptions.smallOffset = true;
                        break;

                    default:
                        if (!inFileProcessed) {
                            aOptions.inFile = GetFileWithPath(arg);
                            var extension = Path.GetExtension(aOptions.inFile).ToLower();
                            aOptions.isInFileXml = (extension == ".xml" || extension == ".scml");
                            inFileProcessed = true;
                        } else {
                            aOptions.outFile = GetFileWithPath(arg);
                        }
                        break;
                }
            }

            // check if output file is defined
            if (aOptions.outFile == null) {
                aOptions.outFile = Path.GetDirectoryName(aOptions.inFile) + Path.DirectorySeparatorChar +
                    Path.GetFileNameWithoutExtension(aOptions.inFile) +
                    "_out";

                if (aOptions.outputBinary) {
                    aOptions.outFile += ".bin";
                } else {
                    aOptions.outFile += Path.GetExtension(aOptions.inFile);
                }
            }

            // whether options are ok
            return aOptions.inFile != null && aOptions.outFile != null;
        }

        // ----------------------------------------------------------
        private static string GetFileWithPath(string aFile) {
            if (aFile.Contains("\\")) {
                return aFile;
            }

            return Path.Combine(Environment.CurrentDirectory, aFile);
        }
    }
}
