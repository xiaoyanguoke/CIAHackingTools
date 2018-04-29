using System;

using System.IO;
using System.Xml;
using System.Text;

using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;


using System.Reflection;
using System.Security.Cryptography;


using Mono_Options;
using System.IO.Compression.ZipStorer;


using Word       = Microsoft.Office.Interop.Word;
using Excel      = Microsoft.Office.Interop.Excel;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;



namespace Scribbles
{
    static class Program
    {
        static string DEFAULT_WatermarkLog_FileName = "WatermarkLog.tsv";

        static string DEFAULT_ScribbesRcpt_FileName_Fmt = "ScribblesConfig_{0}.xml";

        static string Scribbles_XML_ReceiptFormat = 
@"<?xml version=""1.0"" encoding=""UTF-8""?>

<Scribble_WatermarkParameters>
    <URL_Scheme         Value=""{0}""/>
    <HostServerNameList Value=""{1}""/>
    <HostRootPathList   Value=""{2}""/>
    <HostSubDirsList    Value=""{3}""/>
    <HostFileNameList   Value=""{4}""/>
    <HostFileExtList    Value=""{5}""/>

    <Input__Directory    Value=""{6}""/>
    <Output_Directory    Value=""{7}""/>

    <Input__WatermarkLog Value=""{8}""/>
    <Output_WatermarkLog Value=""{9}""/>
</Scribble_WatermarkParameters>

";

        static string Scribbles_WatermarkLog_HeaderLines = @"
" + "File Number:\t\tFile Input Host:\t\tFile Input Path:\t\tFile Input Hash:\t\tWatermark Timestamp:\t\tWatermark Tag:\t\tFormatted Watermark Tag:\t\tWatermark URL:\t\tFile Output Hash:\t\tFile Output Path:" + @"
" + "------------\t\t----------------\t\t----------------\t\t----------------\t\t--------------------\t\t--------------\t\t------------------------\t\t--------------\t\t-----------------\t\t-----------------" + @"
";

        static string Scribbles_WatermarkLog_LineFormat = "{0,12}\t\t{1,16}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}\t\t{7}\t\t{8}\t\t{9}\n";

        static string WatermarkTag_URL_Format = "{0}://{1}/{2}/{3}{4}{5}{6}{7}{8}";
        //
        // NOTE -- The following is the key for the the format string above:
        //
        // "{0}" == URL scheme     (e.g. "http" or "https")
        // "{1}" == hostServerName (e.g. "test.example.com")
        // "{2}" == hostRootPath   (e.g. "hostRootDir")
        // "{3}" == hostSubDir     (e.g. "subDirectory")
        // "{4}" == "/"            (only if (hostSubDir.Length > 0) ...)
        // "{5}" == watermarkTag   (e.g. "012345/6789ab.cdefghi-jklmnopq_rstuvwxyz")
        // "{6}" == "/"            (only if (hostFileName.Length > 0) ...)
        // "{7}" == hostFileName   (e.g. "image")
        // "{8}" == hostFileExt    (e.g. ".jpg")
        //


        public struct Scribbles_WatermarkLog_Entry
        {
            public UInt16 fileNumber;
            public string fileInputHost;
            public string fileInputPath;
            public string fileInputHash;
            public string watermarkDateTime;
            public string watermarkTag;
            public string watermarkTag_Formatted;
            public string watermarkURL;
            public string fileOutputHash;
            public string fileOutputPath;
        } // END struct Scribbles_WatermarkLog_Entry { ... }

        public struct Scribbles_OptionsBlock
        {
            public string urlScheme;

            public string[] hostServerNameList;
            public string[] hostRootPathList;
            public string[] hostSubDirsList;
            public string[] hostFileNameList;
            public string[] hostFileExtList;

            public bool     bAddPathSeparators;

            public string input__WatermarkLog;
            public string output_WatermarkLog;

            public string input__Directory;
            public string output_Directory;

            public string input__ReceiptFile;
            public string output_ReceiptFile;

            //*
            public override bool Equals(object otherObj)
            {
                if ( (otherObj == null) || (otherObj.GetType() != this.GetType()) )
                {
                    return false;
                }

                Scribbles_OptionsBlock otherOptionsBlock = (Scribbles_OptionsBlock) otherObj;

                if ( this.urlScheme           != otherOptionsBlock.urlScheme           ) { return false; }
                if ( this.bAddPathSeparators  != otherOptionsBlock.bAddPathSeparators  ) { return false; }
                if ( this.input__WatermarkLog != otherOptionsBlock.input__WatermarkLog ) { return false; }
                if ( this.output_WatermarkLog != otherOptionsBlock.output_WatermarkLog ) { return false; }
                if ( this.input__Directory    != otherOptionsBlock.input__Directory    ) { return false; }
                if ( this.output_Directory    != otherOptionsBlock.output_Directory    ) { return false; }
                if ( this.input__ReceiptFile  != otherOptionsBlock.input__ReceiptFile  ) { return false; }
                if ( this.output_ReceiptFile  != otherOptionsBlock.output_ReceiptFile  ) { return false; }

                if (this.hostServerNameList.Length != otherOptionsBlock.hostServerNameList.Length) { return false; }
                for (int i = 0; i < hostServerNameList.Length; i++)
                {
                    if (this.hostServerNameList[i] != otherOptionsBlock.hostServerNameList[i]) { return false; }
                }

                if ( this.hostRootPathList.Length != otherOptionsBlock.hostRootPathList.Length) { return false; }
                for (int i = 0; i < hostRootPathList.Length; i++)
                {
                    if (this.hostRootPathList[i] != otherOptionsBlock.hostRootPathList[i]) { return false; }
                }

                if ( this.hostSubDirsList.Length != otherOptionsBlock.hostSubDirsList.Length) { return false; }
                for (int i = 0; i < hostSubDirsList.Length; i++)
                {
                    if (this.hostSubDirsList[i] != otherOptionsBlock.hostSubDirsList[i]) { return false; }
                }

                if ( this.hostFileNameList.Length != otherOptionsBlock.hostFileNameList.Length) { return false; }
                for (int i = 0; i < hostFileNameList.Length; i++)
                {
                    if (this.hostFileNameList[i] != otherOptionsBlock.hostFileNameList[i]) { return false; }
                }

                if ( this.hostFileExtList.Length != otherOptionsBlock.hostFileExtList.Length) { return false; }
                for (int i = 0; i < hostFileExtList.Length; i++)
                {
                    if (this.hostFileExtList[i] != otherOptionsBlock.hostFileExtList[i]) { return false; }
                }

                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            //*/

        } // END struct Scribbles_OptionsBlock { ... }


        public static void ShowHelp(string programName, OptionSet optionsList)
        {
            string usageStatement = @"
Watermarks a series of documents in 'inputDir' (using a format determined by
the [OPTIONS] specified) and writes the post-watermarked docs to 'outputDir'.
";
            Console.WriteLine();
            Console.WriteLine("Usage: '" + programName + " [PARAMS] --inputDir=.\\input --outputDir=.\\output'");
            Console.WriteLine( usageStatement );
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionsList.WriteOptionDescriptions(Console.Out);
        }

        public static void PrintParamErrorMsg(string parameterName, string commandArgument)
        {
            const string paramErrorMessage = @"
ERROR:  Parameter '{0}' must be specified!
[It can be specified either at the command line (""--{1}=XXXXXX""),
 or in an input XML config. file ('<{0} Value=""XXXXXX""/>').]
";
            Console.WriteLine();
            Console.WriteLine(String.Format(paramErrorMessage, parameterName, commandArgument));
            Console.WriteLine();
        }

        static string GetParamValueFromXmlNodeList(XmlNodeList parameterNodes, string parameterName, string cmdArgument, int exitVal)
        {
            if (parameterNodes.Count == 0)
            {
                if (exitVal != 0)
                {
                    PrintParamErrorMsg(parameterName, cmdArgument);
                    Environment.Exit(exitVal);
                }
                else
                {
                    return "";
                }
            }
            else if (parameterNodes.Count > 1)
            {
                string multipleNodesErrorMessage = @"
ERROR:  Receipt file contains multiple nodes for parameter '{0}'!
[Each parameter should be specified only once in the XML file.]
";
                Console.WriteLine();
                Console.WriteLine(String.Format(multipleNodesErrorMessage, parameterName));
                Console.WriteLine();
                Environment.Exit(exitVal);
            }

            XmlNode parameterNode = parameterNodes.Item(0);
            XmlAttributeCollection nodeAttribs = parameterNode.Attributes;

            XmlNode attribNode  = nodeAttribs.GetNamedItem("Value");
            string  attribValue = attribNode.Value;

            return  attribValue;
        }

 
        public static Scribbles_OptionsBlock LoadUndefinedOptionsFromReceiptFile(Scribbles_OptionsBlock cmdLineOptions,
                                                                                 string                 inputReceiptFile )
        {
            Scribbles_OptionsBlock returnOptions = cmdLineOptions;

            XmlDocument input_XML_data = new XmlDocument();

            using (XmlTextReader inputData = new XmlTextReader(inputReceiptFile))
            {
                inputData.WhitespaceHandling = WhitespaceHandling.None;
                inputData.MoveToContent();

                input_XML_data.Load(inputData);
            }

            XmlNodeList urlScheme______Nodes = input_XML_data.GetElementsByTagName( "URL_Scheme"         );
            XmlNodeList hostServerName_Nodes = input_XML_data.GetElementsByTagName( "HostServerNameList" );
            XmlNodeList hostRootPath___Nodes = input_XML_data.GetElementsByTagName( "HostRootPathList"   );
            XmlNodeList hostSubDirs____Nodes = input_XML_data.GetElementsByTagName( "HostSubDirsList"    );
            XmlNodeList hostFileName___Nodes = input_XML_data.GetElementsByTagName( "HostFileNameList"   );
            XmlNodeList hostFileExt____Nodes = input_XML_data.GetElementsByTagName( "HostFileExtList"    );

            XmlNodeList input__WatermarkLog_Nodes = input_XML_data.GetElementsByTagName("Input__WatermarkLog");
            XmlNodeList output_WatermarkLog_Nodes = input_XML_data.GetElementsByTagName("Output_WatermarkLog");

            XmlNodeList input__Directory_Nodes = input_XML_data.GetElementsByTagName("Input__Directory");
            XmlNodeList output_Directory_Nodes = input_XML_data.GetElementsByTagName("Output_Directory");

            if (returnOptions.urlScheme == null)
            {
                string  urlSchemeString = GetParamValueFromXmlNodeList(urlScheme______Nodes, "URL_Scheme", "urlScheme=", -10);
                returnOptions.urlScheme = urlSchemeString;
            }

            if ( (returnOptions.hostServerNameList == null) || (returnOptions.hostServerNameList.Length == 0) )
            {
                string hostServerNameString = GetParamValueFromXmlNodeList(hostServerName_Nodes, "HostServerNameList" , "hostServerName=", -11);
                returnOptions.hostServerNameList = hostServerNameString.Split(',');
            }

            if ( (returnOptions.hostRootPathList == null) || (returnOptions.hostRootPathList.Length == 0) )
            {
                string hostRootPathString = GetParamValueFromXmlNodeList(hostRootPath___Nodes, "HostRootPathList", "hostRootPath=", -12);
                returnOptions.hostRootPathList = hostRootPathString.Split(',');
            }

            if ( (returnOptions.hostSubDirsList == null) || (returnOptions.hostSubDirsList.Length == 0) )
            {
                string hostSubDirsString = GetParamValueFromXmlNodeList(hostSubDirs____Nodes, "HostSubDirsList", "hostSubDirs=", -13);
                returnOptions.hostSubDirsList = hostSubDirsString.Split(',');
            }

            if ( (returnOptions.hostFileNameList == null) || (returnOptions.hostFileNameList.Length == 0) )
            {
                string hostFileNameString = GetParamValueFromXmlNodeList(hostFileName___Nodes, "HostFileNameList", "hostFileName=", -14);
                returnOptions.hostFileNameList = hostFileNameString.Split(',');
            }

            if ( (returnOptions.hostFileExtList == null) || (returnOptions.hostFileExtList.Length == 0) )
            {
                string hostFileExtString = GetParamValueFromXmlNodeList(hostFileExt____Nodes, "HostFileExtList", "hostFileExt=", -15);
                returnOptions.hostFileExtList = hostFileExtString.Split(',');
            }

            if (returnOptions.input__WatermarkLog == null)
            {
                string inputWatermarkLogString = GetParamValueFromXmlNodeList(input__WatermarkLog_Nodes, "Input__WatermarkLog", "inputWatermarkLog=", 0);
                returnOptions.input__WatermarkLog = inputWatermarkLogString;
            }

            if (returnOptions.output_WatermarkLog == null)
            {
                string outputWatermarkLogString = GetParamValueFromXmlNodeList(output_WatermarkLog_Nodes, "Output_WatermarkLog", "outputWatermarkLog=", 0);
                if (outputWatermarkLogString == "")
                {
                    outputWatermarkLogString  = Path.Combine( Directory.GetCurrentDirectory(), DEFAULT_WatermarkLog_FileName );
                }
                returnOptions.output_WatermarkLog = outputWatermarkLogString;
            }

            if (returnOptions.input__Directory == null)
            {
                string inputDirectoryString = GetParamValueFromXmlNodeList(input__Directory_Nodes, "Input__Directory", "inputDir=", -18);
                returnOptions.input__Directory = inputDirectoryString;
            }

            if (returnOptions.output_Directory == null)
            {
                string outputDirectoryString = GetParamValueFromXmlNodeList(output_Directory_Nodes, "Output_Directory", "outputDir=", -19);
                returnOptions.output_Directory = outputDirectoryString;
            }

            return returnOptions;

        } // END LoadUndefinedOptionsFromReceiptFile(...) { ... }


        private static Scribbles_OptionsBlock ParseCmdLineOptions(string[] args, out OptionSet optionsParser,
                                                                  out bool bShowHelp, string progName)
        {
            Scribbles_OptionsBlock cmdLineOptions = new Scribbles_OptionsBlock();
            bShowHelp = false;

            optionsParser = new OptionSet() {
                {"urlScheme=",      "identifies the protocol (e.g. HTTP or HTTPS).\r\n\r\n\r\n",
                    v => cmdLineOptions.urlScheme = v },

                {"hostServerName=", "the hostname  (or list of hostnames)  that will be used in the watermark tags.\r\n\r\n",
                    v => cmdLineOptions.hostServerNameList = v.Split(',') },
                {"hostRootPath=",   "the root path (or list of root paths) that will be used in the watermark tags.\r\n\r\n",
                    v => cmdLineOptions.hostRootPathList = v.Split(',') },
                {"hostSubDirs=",    "the subdirectory path (or list of...) that will be used in the watermark tags.\r\n\r\n",
                    v => cmdLineOptions.hostSubDirsList  = v.Split(',') },
                {"hostFileName=",   "the file name (or list of file names) that will be used in the watermark tags.\r\n\r\n",
                    v => cmdLineOptions.hostFileNameList = v.Split(',') },
                {"hostFileExt=",    "the extension (or list of extensions) that will be used in the watermark tags.\r\n\r\n\r\n",
                    v => cmdLineOptions.hostFileExtList  = v.Split(',') },

                {"inputReceiptFile=", "[OPTIONAL] a receipt file to load parameters from.\r\n",
                    v => cmdLineOptions.input__ReceiptFile  = v },
                {"inputWatermarkLog=", "[OPTIONAL] a log of existing watermarks  \r\n(default is '.\\WatermarkLog.tsv').",
                    v => cmdLineOptions.input__WatermarkLog = v },
                {"outputWatermarkLog=", "[OPTIONAL] the output log for watermarks\r\n(default is '.\\WatermarkLog.tsv').\r\n\r\n",
                    v => cmdLineOptions.output_WatermarkLog = v },

                {"inputDir=",  "the  input directory containing the files that will be watermarked.\r\n\r\n",
                    v => cmdLineOptions.input__Directory = v },
                {"outputDir=", "the output directory where the watermarked copies will be written.",
                    v => cmdLineOptions.output_Directory = v },
            };

            List<string> extraArgs;
            try
            {
                extraArgs = optionsParser.Parse(args);
                if (extraArgs.Count != 0) { bShowHelp = true; }
            }
            catch (OptionException e)
            {
                Console.Write(progName + ":  ");
                Console.Write(e.Message);
                Console.Write("Try '" + progName + " --help' for more information.");
                Environment.Exit(-5);
            }

            return cmdLineOptions;

        } // END ParseCmdLineOptions(...) { ... }


        public static void VerifyOptionsBlockAndSetDefaults(ref Scribbles_OptionsBlock scribblesOptions)
        {
            if (scribblesOptions.urlScheme == null)
            {
                PrintParamErrorMsg("URL_Scheme", "urlScheme");
                Environment.Exit(-30);
            }

            if ( (scribblesOptions.hostServerNameList == null) || (scribblesOptions.hostServerNameList.Length == 0) )
            {
                PrintParamErrorMsg("HostServerNameList", "hostServerName");
                Environment.Exit(-31);
            }

            if ( (scribblesOptions.hostRootPathList == null) || (scribblesOptions.hostRootPathList.Length == 0) )
            {
                PrintParamErrorMsg("HostRootPathList", "hostRootPath");
                Environment.Exit(-32);
            }

            if ( (scribblesOptions.hostSubDirsList == null) || (scribblesOptions.hostSubDirsList.Length == 0) )
            {
                PrintParamErrorMsg("HostSubDirsList", "hostSubDirs");
                Environment.Exit(-33);
            }

            if ( (scribblesOptions.hostFileNameList == null) || (scribblesOptions.hostFileNameList.Length == 0) )
            {
                PrintParamErrorMsg("HostFileNameList", "hostFileName");
                Environment.Exit(-34);
            }

            if ( (scribblesOptions.hostFileExtList == null) || (scribblesOptions.hostFileExtList.Length == 0) )
            {
                PrintParamErrorMsg("HostFileExtList", "hostFileExt");
                Environment.Exit(-35);
            }

            if (scribblesOptions.output_WatermarkLog == null)
            {
                string outputWatermarkLogString = Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_WatermarkLog_FileName);
                scribblesOptions.output_WatermarkLog = outputWatermarkLogString;
            }

            if (scribblesOptions.input__WatermarkLog == null)
            {
                scribblesOptions.input__WatermarkLog = scribblesOptions.output_WatermarkLog;
            }

            if ( (scribblesOptions.input__Directory == null) || !Directory.Exists(scribblesOptions.input__Directory) )
            {
                PrintParamErrorMsg("Input__Directory", "inputDir");
                Environment.Exit(-38);
            }

            if (scribblesOptions.output_Directory == null)
            {
                PrintParamErrorMsg("Output_Directory", "outputDir");
                Environment.Exit(-39);
            }

            if (scribblesOptions.output_ReceiptFile == null)
            {
                string timeDateStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string receiptFileName = String.Format(DEFAULT_ScribbesRcpt_FileName_Fmt, timeDateStamp);
                string output_ReceiptFile_path = Path.Combine(Directory.GetCurrentDirectory(), receiptFileName);

                scribblesOptions.output_ReceiptFile = output_ReceiptFile_path;
            }

            return;

        } // END VerifyOptionsBlockAndSetDefaults(...) { ... }


        public static void WriteOptionsBlockToReceiptFile(Scribbles_OptionsBlock scribblesOptions, string receiptOutputFilePath)
        {
            string hostServerNameList_String = String.Join( ",", scribblesOptions.hostServerNameList );
            string hostRootPathList___String = String.Join( ",", scribblesOptions.hostRootPathList   );
            string hostSubDirsList____String = String.Join( ",", scribblesOptions.hostSubDirsList    );
            string hostFileNameList___String = String.Join( ",", scribblesOptions.hostFileNameList   );
            string hostFileExtList____String = String.Join( ",", scribblesOptions.hostFileExtList    );

            string outputFileContent = String.Format(Scribbles_XML_ReceiptFormat,
                                                     scribblesOptions.urlScheme,
                                                     hostServerNameList_String,
                                                     hostRootPathList___String,
                                                     hostSubDirsList____String,
                                                     hostFileNameList___String,
                                                     hostFileExtList____String,
                                                     scribblesOptions.input__Directory,
                                                     scribblesOptions.output_Directory,
                                                     scribblesOptions.input__WatermarkLog,
                                                     scribblesOptions.output_WatermarkLog);

            File.WriteAllText(receiptOutputFilePath, outputFileContent, Encoding.UTF8);
            return;

        } // END WriteOptionsBlockToReceiptFile(...) { ... }


        public static Scribbles_OptionsBlock LoadFinalOptionsFromReceiptFile(string outputReceiptFile)
        {
            Scribbles_OptionsBlock returnOptions = new Scribbles_OptionsBlock();

            XmlDocument input_XML_data = new XmlDocument();

            using (XmlTextReader inputData = new XmlTextReader(outputReceiptFile))
            {
                inputData.WhitespaceHandling = WhitespaceHandling.None;
                inputData.MoveToContent();

                input_XML_data.Load(inputData);
            }

            XmlNodeList urlScheme______Nodes = input_XML_data.GetElementsByTagName("URL_Scheme");
            XmlNodeList hostServerName_Nodes = input_XML_data.GetElementsByTagName("HostServerNameList");
            XmlNodeList hostRootPath___Nodes = input_XML_data.GetElementsByTagName("HostRootPathList");
            XmlNodeList hostSubDirs____Nodes = input_XML_data.GetElementsByTagName("HostSubDirsList");
            XmlNodeList hostFileName___Nodes = input_XML_data.GetElementsByTagName("HostFileNameList");
            XmlNodeList hostFileExt____Nodes = input_XML_data.GetElementsByTagName("HostFileExtList");

            XmlNodeList input__WatermarkLog_Nodes = input_XML_data.GetElementsByTagName("Input__WatermarkLog");
            XmlNodeList output_WatermarkLog_Nodes = input_XML_data.GetElementsByTagName("Output_WatermarkLog");

            XmlNodeList input__Directory_Nodes = input_XML_data.GetElementsByTagName("Input__Directory");
            XmlNodeList output_Directory_Nodes = input_XML_data.GetElementsByTagName("Output_Directory");

            string urlSchemeString = GetParamValueFromXmlNodeList(urlScheme______Nodes, "URL_Scheme", "urlScheme=", 0);
            returnOptions.urlScheme = urlSchemeString;

            string hostServerNameString = GetParamValueFromXmlNodeList(hostServerName_Nodes, "HostServerNameList", "hostServerName=", 0);
            returnOptions.hostServerNameList = hostServerNameString.Split(',');

            string hostRootPathString = GetParamValueFromXmlNodeList(hostRootPath___Nodes, "HostRootPathList", "hostRootPath=", 0);
            returnOptions.hostRootPathList = hostRootPathString.Split(',');

            string hostSubDirsString = GetParamValueFromXmlNodeList(hostSubDirs____Nodes, "HostSubDirsList", "hostSubDirs=", 0);
            returnOptions.hostSubDirsList = hostSubDirsString.Split(',');

            string hostFileNameString = GetParamValueFromXmlNodeList(hostFileName___Nodes, "HostFileNameList", "hostFileName=", 0);
            returnOptions.hostFileNameList = hostFileNameString.Split(',');

            string hostFileExtString = GetParamValueFromXmlNodeList(hostFileExt____Nodes, "HostFileExtList", "hostFileExt=", 0);
            returnOptions.hostFileExtList = hostFileExtString.Split(',');

            // TODO: Implement option to add path separators to further obfuscate the watermark tags in our fake URLs:
            //
            returnOptions.bAddPathSeparators = false;

            string inputWatermarkLogString = GetParamValueFromXmlNodeList(input__WatermarkLog_Nodes, "Input__WatermarkLog", "inputWatermarkLog=", 0);
            returnOptions.input__WatermarkLog = inputWatermarkLogString;

            string outputWatermarkLogString = GetParamValueFromXmlNodeList(output_WatermarkLog_Nodes, "Output_WatermarkLog", "outputWatermarkLog=", 0);
            returnOptions.output_WatermarkLog = outputWatermarkLogString;

            string inputDirectoryString = GetParamValueFromXmlNodeList(input__Directory_Nodes, "Input__Directory", "inputDir=", 0);
            returnOptions.input__Directory = inputDirectoryString;

            string outputDirectoryString = GetParamValueFromXmlNodeList(output_Directory_Nodes, "Output_Directory", "outputDir=", 0);
            returnOptions.output_Directory = outputDirectoryString;

            return returnOptions;

        } // END LoadFinalOptionsFromReceiptFile(...) { ... }


        public static Scribbles_WatermarkLog_Entry[] LoadInputWatermarkLog(string inputWatermarkLogFilePath, string progName)
        {
            List<Scribbles_WatermarkLog_Entry> watermarkLogEntry_List = new List<Scribbles_WatermarkLog_Entry>();

            FileStream inputFileStream = null;
            try
            {
                inputFileStream = new FileStream(inputWatermarkLogFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(String.Format(@"{0}: Unable to open existing watermark log {1}.
(Documents will not be checked against existing watermarks.)", progName, inputWatermarkLogFilePath));
                return watermarkLogEntry_List.ToArray();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine(String.Format(@"{0}: Unable to open directory with existing watermark log ({1}).
(Documents will not be checked against existing watermarks.)", progName, Path.GetDirectoryName(inputWatermarkLogFilePath)));
                return watermarkLogEntry_List.ToArray();
            }


            List<string> inputLines = new List<string>();
            using (StreamReader inputFileReader = new StreamReader(inputFileStream, Encoding.UTF8, true))
            {
                bool bPassedHeader = false;
                string   inputLine = null;
                while ( (inputLine = inputFileReader.ReadLine()) != null)
                {
                    //
                    // The first few lines are 'header' lines, so we'll skip them:
                    //
                    if (inputLine.StartsWith("------------"))
                    {
                        bPassedHeader = true;
                        continue;
                    }
                    if (bPassedHeader)
                    {
                        inputLines.Add(inputLine);
                    }
                } // END while ( (inputLine = inputFileReader.ReadLine()) != null) { ... }

            } // END using (StreamReader inputFileReader = new StreamReader(...)) { ... }

            inputFileStream.Close();

            string[] inputSeparators = new string[] { "\t\t" };
 
            foreach (string inputString in inputLines)
            {
                if ( (inputString.Trim().Length == 0) ||
                     (inputString.StartsWith("File Number:")) ||
                     (inputString.StartsWith("------------")) )
                {
                    // This line was empty or blank, or it was a header line.  Skip it:
                    //
                    continue;
                }

                string[] inputFields = inputString.Split(inputSeparators, StringSplitOptions.None);

                Scribbles_WatermarkLog_Entry newEntry = new Scribbles_WatermarkLog_Entry();

                newEntry.fileNumber = UInt16.Parse(inputFields[0]);

                newEntry.fileInputHost = inputFields[1];
                newEntry.fileInputPath = inputFields[2];
                newEntry.fileInputHash = inputFields[3];

                newEntry.watermarkDateTime      = inputFields[4];
                newEntry.watermarkTag           = inputFields[5];
                newEntry.watermarkTag_Formatted = inputFields[6];
                newEntry.watermarkURL           = inputFields[7];

                newEntry.fileOutputHash = inputFields[8];
                newEntry.fileOutputPath = inputFields[9];

                watermarkLogEntry_List.Add(newEntry);

            } // END foreach (string inputString in inputLines) { ... }

            return watermarkLogEntry_List.ToArray();

        } // END LoadInputWatermarkLog(...) { ... }

        public static string convertByteArrayToString(byte[] inputArray)
        {
            return BitConverter.ToString(inputArray).Replace("-", String.Empty);
        }

        public static string ComputeFileHash(string inputFile)
        {
            SHA256 SHA256_object;
            if ( ((Environment.Version.Major > 3)                                     ) ||
                 ((Environment.Version.Major == 3) && (Environment.Version.Minor >= 5)) )
            {
                SHA256_object = new System.Security.Cryptography.SHA256CryptoServiceProvider();
            }
            else
            {
                SHA256_object = new System.Security.Cryptography.SHA256Managed();
            }

            using (FileStream inputStream = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] inputStreamHash = SHA256_object.ComputeHash(inputStream);
                return convertByteArrayToString(inputStreamHash);
            }
        }

        public static bool FindLogEntryByInputPath(string inputFile, ref Scribbles_WatermarkLog_Entry pLogEntry,
                                                   Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog)
        {
            foreach  (Scribbles_WatermarkLog_Entry logEntry in Scribbles_WatermarkLog)
            {
                if (inputFile == logEntry.fileInputPath)
                {
                    pLogEntry =  logEntry;
                    return true;
                }
            }

            return false;

        } // END FindLogEntryByInputPath(...) { ... }


        public static bool FindLogEntryByInputHash(string inputHash, ref Scribbles_WatermarkLog_Entry pLogEntry,
                                                   Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog)
        {
            foreach  (Scribbles_WatermarkLog_Entry logEntry in Scribbles_WatermarkLog)
            {
                if (inputHash == logEntry.fileInputHash)
                {
                    pLogEntry =  logEntry;
                    return true;
                }
            }

            return false;

        } // END FindLogEntryByInputPath(...) { ... }


        public enum WatermarkOperation
        {
            SkipExistingWatermarkedFile,
            WatermarkNewFile,
            RebuildSameWatermarkedFile,
            ReplaceFileWithNewWatermark,
            CopyMatchingWatermarkedFile,
            WatermarkThis_PowerPoint_file,
            Watermark_ALL_PowerPoint_files,
        };

        public struct Scribbles_Watermark_Disposition
        {
            public string hostName;
            public string fileInputPath;
            public WatermarkOperation           fileDisposition;
            public Scribbles_WatermarkLog_Entry originalWatermarkInfo;
        } // END struct Scribbles_Watermark_Disposition { ... }

        public static bool MoveExistingOutputFile(string outputFile)
        {
            string timeDateStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-FFF");
            string oldFileExtension = Path.GetExtension(outputFile);
            string newFileExtension = ".pre_" + timeDateStamp + oldFileExtension;

            int oldFilePathLength  = outputFile.Length;
            int oldExtensionLength = oldFileExtension.Length;

            string outputFile_oldPath = outputFile.Substring(0, (oldFilePathLength - oldExtensionLength) );
            string outputFile_newPath = outputFile + newFileExtension;

            File.Move(outputFile, outputFile_newPath);

            return true;
        }


        private static WatermarkOperation AskUser_RebuildReplaceSkip(string outputFilePath, string progName)
        {
            WatermarkOperation returnValue = new WatermarkOperation();

            string inputChar;
            bool   inputCharIsR = false, inputCharIsW = false, inputCharIsS = false;
            do
            {
                Console.Error.WriteLine(String.Format("{0}: Do you want to Rebuild it (R), replace it with a NeW watermark (W), or Skip it (S)?  (R/W/S)\n", progName));
                inputChar = Console.ReadLine();
                inputCharIsR      = (inputChar.StartsWith("R")) || (inputChar.StartsWith("r"));
                inputCharIsW      = (inputChar.StartsWith("W")) || (inputChar.StartsWith("w"));
                inputCharIsS      = (inputChar.StartsWith("S")) || (inputChar.StartsWith("s"));
                if (inputCharIsR)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: File {1} will be Rebuilt with same watermark.\n", progName, outputFilePath));
                    if (outputFilePath != null)
                    {
                        MoveExistingOutputFile(outputFilePath);
                    }
                    returnValue = WatermarkOperation.RebuildSameWatermarkedFile;
                }
                else if (inputCharIsW)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: File {1} will be replaced with NEW watermark.\n", progName, outputFilePath));
                    if (outputFilePath != null)
                    {
                        MoveExistingOutputFile(outputFilePath);
                    }
                    returnValue = WatermarkOperation.ReplaceFileWithNewWatermark;
                }
                else if (inputCharIsS)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: File {1} will be skipped.\n", progName, outputFilePath));
                    returnValue = WatermarkOperation.SkipExistingWatermarkedFile;
                }
                else
                {
                    Console.Error.WriteLine(String.Format("\n{0}: ERROR -- please enter 'R', 'W', or 'S'.  (Input was '{1}.)\n",
                                                    progName, inputChar));
                }

            } while (!(inputCharIsR || inputCharIsW || inputCharIsS));

            Console.WriteLine();
            return returnValue;

        } // END AskUser_RebuildReplaceSkip(string outputFilePath, string progName) { ... }


        private static WatermarkOperation AskUser_CopyOrReplaceWithNew(string inputFilePath, string existingOutputFile, string progName)
        {
            WatermarkOperation returnValue = new WatermarkOperation();

            string inputChar;
            bool   inputCharIsC = false, inputCharIsW = false;
            do
            {
                Console.Error.WriteLine(String.Format("{0}: Do you want to Copy the output file with the existing watermark (C)?", progName));
                Console.Error.WriteLine(String.Format("{0}: Or do you want to generate an output file with a NeW watermark (W)? (C/W)\n", progName));
                inputChar = Console.ReadLine();
                inputCharIsC      = (inputChar.StartsWith("C")) || (inputChar.StartsWith("c"));
                inputCharIsW      = (inputChar.StartsWith("W")) || (inputChar.StartsWith("w"));
                if (inputCharIsC)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: Output file {1} (and its watermark) will be copied.\n", progName, existingOutputFile));
                    returnValue = WatermarkOperation.CopyMatchingWatermarkedFile;
                }
                else if (inputCharIsW)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: A new watermark will be generated for input file {1}.\n", progName, inputFilePath));
                    returnValue = WatermarkOperation.ReplaceFileWithNewWatermark;
                }
                else
                {
                    Console.Error.WriteLine(String.Format("\n{0}: ERROR -- please enter 'C' or 'W'.  (Input was '{1}.)\n",
                                                    progName, inputChar));
                }

            } while (!(inputCharIsC || inputCharIsW));

            Console.WriteLine();
            return returnValue;

        } // END AskUser_CopyOrReplaceWithNew(string inputFilePath, string existingOutputFile, string progName) { ... }


        private static WatermarkOperation AskUser_Skip_WatermarkThis_WatermarkALL_PPT_files(string inputFilePath, string progName)
        {
            WatermarkOperation returnValue = new WatermarkOperation();

            Console.Error.WriteLine(String.Format("{0}: WARNING -- Watermarking PowerPoint file {1} will alert 'end-users' about \"external images\".", progName, inputFilePath));

            string inputChar;
            bool inputCharIsS = false, inputCharIsW = false, inputCharIsA = false;
            do
            {
                Console.Error.WriteLine(String.Format("{0}: Do you want to Skip it (S), Watermark THIS file (W), or watermark ALL PowerPoint files (A)?  (S/C/A)\n", progName));
                inputChar = Console.ReadLine();
                inputCharIsS = (inputChar.StartsWith("S")) || (inputChar.StartsWith("s"));
                inputCharIsW = (inputChar.StartsWith("W")) || (inputChar.StartsWith("w"));
                inputCharIsA = (inputChar.StartsWith("A")) || (inputChar.StartsWith("a"));
                if (inputCharIsS)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: PowerPoint file {1} will be skipped.\n", progName, inputFilePath));
                    returnValue = WatermarkOperation.SkipExistingWatermarkedFile;
                }
                else if (inputCharIsW)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: PowerPoint file {1} will be watermarked.\n", progName, inputFilePath));
                    returnValue = WatermarkOperation.WatermarkThis_PowerPoint_file;
                }
                else if (inputCharIsA)
                {
                    Console.Error.WriteLine(String.Format("\n{0}: *ALL* PowerPoint files will be watermarked.\n", progName));
                    returnValue = WatermarkOperation.Watermark_ALL_PowerPoint_files;
                }
                else
                {
                    Console.Error.WriteLine(String.Format("\n{0}: ERROR -- please enter 'S', 'W', or 'A'.  (Input was '{1}.)\n",
                                                    progName, inputChar));
                }

            } while (!(inputCharIsS || inputCharIsW || inputCharIsA));

            Console.WriteLine();
            return returnValue;

        } // END AskUser_Skip_WatermarkThis_WatermarkALL_PPT_files(string inputFilePath, string progName) { ... }


        public static WatermarkOperation DecideWhetherToWatermarkInputFile(string inputFile, ref Scribbles_WatermarkLog_Entry pLogEntry,
                                                                           Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog, string progName)
        {
            string inputFileHash = ComputeFileHash(inputFile);

            Scribbles_WatermarkLog_Entry logEntry = new Scribbles_WatermarkLog_Entry();

            // First, check for previously-watermarked file in watermark log that matches input file path:
            //
            bool bPathHasBeenPreviouslyWatermarked = FindLogEntryByInputPath(inputFile, ref logEntry, Scribbles_WatermarkLog);
            if (bPathHasBeenPreviouslyWatermarked)
            {
                // Input file path has been previously watermarked.
                // Check for matching input file hash (to make sure the input file hasn't changed) :
                //
                if (inputFileHash == logEntry.fileInputHash)
                {
                    // Input file path has *NOT* changed.  Check for previously-generated output file:
                    //
                    string outputFilePath = logEntry.fileOutputPath;
                    if (File.Exists(outputFilePath))
                    {
                        // Output file *DOES* exist.  Verify output file hash:
                        //
                        string outputFileHash = ComputeFileHash(outputFilePath);
                        if (outputFileHash == logEntry.fileOutputHash)
                        {
                            // Output file hash matches!  Skip file:
                            //
                            return WatermarkOperation.SkipExistingWatermarkedFile;
                        }
                        else
                        {
                            // Output file hash does not match.  Warn user and ask whether to replace/rebuild/skip:
                            //
                            Console.Error.WriteLine(String.Format("{0}: WARNING -- Hash of previously watermarked output file {1} does not match.\n", progName,
                                                            outputFilePath));

                            WatermarkOperation returnValue = AskUser_RebuildReplaceSkip(outputFilePath, progName);
                            if (returnValue == WatermarkOperation.RebuildSameWatermarkedFile)
                            {
                                pLogEntry = logEntry;
                            }

                            return returnValue;

                        } // END if (outputFileHash == logEntry.fileOutputHash) { ... } else { ... }

                    } // END if if (File.Exists(outputFilePath)) { ... }
                    else
                    {
                        // Output file *DOES* *NOT* exist (anymore?).  Warn user and ask whether to replace/rebuild/skip:
                        //
                        Console.Error.WriteLine(String.Format("{0}: WARNING -- Previously watermarked output file {1} is no longer found.\n", progName,
                                                        outputFilePath));

                        WatermarkOperation returnValue = AskUser_RebuildReplaceSkip(null, progName);
                        if (returnValue == WatermarkOperation.RebuildSameWatermarkedFile)
                        {
                            pLogEntry = logEntry;
                        }

                        return returnValue;
                    }

                } // END if (inputFileHash == logEntry.fileInputHash) { ... }
                else
                {
                    // Input file still exists, but its contents (hash value) have changed!
                    //
                    Console.Error.WriteLine(String.Format("{0}: WARNING -- Previously watermarked input file {1} has changed!\n", progName,
                                                    inputFile));

                    WatermarkOperation returnValue;

                    string outputFilePath = logEntry.fileOutputPath;
                    if (File.Exists(outputFilePath))
                    {
                        Console.Error.WriteLine(String.Format("{0}:         -- Output file {1} still exists.\n", progName, outputFilePath));

                        returnValue = AskUser_RebuildReplaceSkip(outputFilePath, progName);

                        return returnValue;
                    }

                    returnValue = AskUser_RebuildReplaceSkip(null, progName);
                    if (returnValue == WatermarkOperation.RebuildSameWatermarkedFile)
                    {
                        pLogEntry = logEntry;
                    }

                    return returnValue;

                } // END else { ... } [i.e. if (inputFileHash != logEntry.fileInputHash) { ... }]

            } // END if  (bPathHasBeenPreviouslyWatermarked) { ... }


            // Next, check for previously-watermarked file in watermark log that matches inputFileHash:
            //
            bool bHashHasBeenPreviouslyWatermarked = FindLogEntryByInputHash(inputFileHash, ref logEntry, Scribbles_WatermarkLog);
            if (bHashHasBeenPreviouslyWatermarked)
            {
                string outputFilePath = logEntry.fileOutputPath;

                if (File.Exists(outputFilePath))
                {
                    // Output file *DOES* exist.  Verify its hash:
                    //
                    string outputFileHash = ComputeFileHash(outputFilePath);
                    if (outputFileHash == logEntry.fileOutputHash)
                    {
                        // The previously-watermarked file we found has a validated output file!
                        //
                        Console.Error.WriteLine(String.Format("{0}: Input file {1} -- An identical file has already been watermarked.", progName,
                                          inputFile));
                        Console.Error.WriteLine(String.Format("{0}: (The output file was <{1}>.)", progName, outputFilePath));

                        WatermarkOperation returnValue = AskUser_CopyOrReplaceWithNew(inputFile, outputFilePath, progName);
                        if (returnValue == WatermarkOperation.CopyMatchingWatermarkedFile)
                        {
                            pLogEntry = logEntry;
                        }
                        return returnValue;

                    } // END if (outputFileHash == logEntry.fileOutputHash) { ... }

                } // END if (File.Exists(outputFilePath)) { ... }

            } // END if (bHashHasBeenPreviouslyWatermarked) { ... }

            return WatermarkOperation.WatermarkNewFile;

        } // END DecideWhetherToWatermarkInputFile(string inputFile, ...) { ... }


        public static bool bDisplayPowerPointWarning = true;
        public static bool bWatermarkPowerPointFiles = false;

        public static bool bDisplay_MS_PowerPoint_Warning = true;
        public static bool bWatermarkAll_PPT_Files   = false;

        public static bool VerifyInputFileType(string inputFilePath, string progName)
        {
            string  fileExtension = Path.GetExtension(inputFilePath);
            switch (fileExtension)
            {
                case ".doc":
                case ".docx":
//              case ".docm":
                case ".xlsx":
//              case ".xlsm":
//              case ".xlam":
                    return true;

                case ".xls":
//              case ".xlt":
//              case ".xla":
                    return true;

                case ".ppt":
                case ".pptx":
                    if (bDisplay_MS_PowerPoint_Warning)
                    {
                        Console.Error.WriteLine("WARNING:  'End-users' will alerted about disabled \"external images\" when opening watermarked PowerPoint presentations.");
                        bDisplay_MS_PowerPoint_Warning = false;
                    }
                    if (bWatermarkAll_PPT_Files)
                    {
                        return true;
                    }

                    WatermarkOperation xlsOperation = AskUser_Skip_WatermarkThis_WatermarkALL_PPT_files(inputFilePath, progName);
                    switch (xlsOperation)
                    {
                        case WatermarkOperation.SkipExistingWatermarkedFile:
                            return false;
                        case WatermarkOperation.WatermarkThis_PowerPoint_file:
                            return true;
                        case WatermarkOperation.Watermark_ALL_PowerPoint_files:
                            bWatermarkAll_PPT_Files = true;
                            return true;
                        default:
                            Console.Error.WriteLine(String.Format("{0}: ERROR -- Unrecognized value [{1}] as disposition for XLS file {2}!",
                                                            progName, xlsOperation, progName));
                            break;
                    }

                    return false;

                default:
                    Console.WriteLine(String.Format("WARNING: Input file [{0}] does not have a recognized extension [{1}].  Skipping...",
                                                    inputFilePath, fileExtension));
                    break;
            }

            return false;
        }

        public static List<Scribbles_Watermark_Disposition> BuildListOfFilesForHost(string hostName, string hostDirectory,
                                                                                    Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog,
                                                                                    string progName)
        {
            List<Scribbles_Watermark_Disposition> fileDispositionList = new List<Scribbles_Watermark_Disposition>();

            string[] hostRootFiles = Directory.GetFiles(hostDirectory);
            foreach (string currentRootFile in hostRootFiles)
            {
                string currentFilePath = Path.GetFullPath(currentRootFile);

                Scribbles_WatermarkLog_Entry original_WM_Info = new Scribbles_WatermarkLog_Entry();
                WatermarkOperation currentRootFileDisposition = DecideWhetherToWatermarkInputFile(currentFilePath, ref original_WM_Info,
                                                                                                  Scribbles_WatermarkLog, progName);

                Scribbles_Watermark_Disposition fullFileDispInfo = new Scribbles_Watermark_Disposition();
                fullFileDispInfo.hostName        = hostName;
                fullFileDispInfo.fileInputPath   = currentFilePath;
                fullFileDispInfo.fileDisposition = currentRootFileDisposition;

                switch (currentRootFileDisposition)
                {
                    case WatermarkOperation.SkipExistingWatermarkedFile:
                        Console.WriteLine(String.Format("{0}: Skipping previously watermarked file {1}...", progName, currentRootFile));
                        continue;
                    case WatermarkOperation.WatermarkNewFile:
                        bool bFileTypeOk = VerifyInputFileType(currentRootFile, progName);
                        if (!bFileTypeOk)
                        {
                            currentRootFileDisposition = WatermarkOperation.SkipExistingWatermarkedFile;
                            continue;
                        }
                        break;
                    case WatermarkOperation.RebuildSameWatermarkedFile:
                        fullFileDispInfo.originalWatermarkInfo = original_WM_Info;
                        break;
                    case WatermarkOperation.ReplaceFileWithNewWatermark:
                        break;
                    case WatermarkOperation.CopyMatchingWatermarkedFile:
                        fullFileDispInfo.originalWatermarkInfo = original_WM_Info;
                        break;
                    default:
                        Console.WriteLine(String.Format("{0}: ERROR -- Unrecognized value [{1}] as disposition for host file {2}!",
                                                        progName, currentRootFileDisposition, currentRootFile));
                        continue;
                }

                fileDispositionList.Add( fullFileDispInfo );

            } // END foreach (string currentRootFile in hostRootFiles) { ... }

            string[] hostSubDirs = Directory.GetDirectories(hostDirectory);
            foreach (string hostSubDirectory in hostSubDirs)
            {
                List<Scribbles_Watermark_Disposition> filesFromSubDir = BuildListOfFilesForHost(hostName, hostSubDirectory,
                                                                                                Scribbles_WatermarkLog, progName);
                fileDispositionList.AddRange(filesFromSubDir);
            }

            return fileDispositionList;

        } // END BuildListOfFilesToWatermark(...) { ... }


        public static List<Scribbles_Watermark_Disposition> BuildListOfFilesToWatermark(string inputDirectory,
                                                                                        Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog,
                                                                                        string progName)
        {
            if (!Directory.Exists(inputDirectory))
            {
                Console.WriteLine(String.Format("{0}: ERROR -- input directory ({1}) does not exist!  Exiting...\n", progName, inputDirectory));
                Environment.Exit(-60);
            }

            string[] rootFiles = Directory.GetFiles(inputDirectory);
            if (rootFiles.Length > 0)
            {
                Console.WriteLine(String.Format("{0}: WARNING -- files in the root of input directory ({1}) will NOT be watermarked.\n", progName,
                                                String.Join(",", rootFiles) ));
                Console.WriteLine(String.Format("{0}: Do you want to continue?  (Y/n)\n", progName));
                int  inputChar    = Console.Read();
                bool inputCharIsY = ( (inputChar == 'Y') || (inputChar == 'y') );
                if (!inputCharIsY)
                {
                    Environment.Exit(-61);
                }
            }

            List<Scribbles_Watermark_Disposition> fullFileDispositionList = new List<Scribbles_Watermark_Disposition>();

            string[] hostDirectories = Directory.GetDirectories(inputDirectory);
            foreach (string hostDirectory in hostDirectories)
            {
                string hostName = hostDirectory;
                if (hostName.StartsWith(inputDirectory))
                {
                    hostName = hostName.Substring(inputDirectory.Length);
                }
                while (hostName.StartsWith(".") ||
                       hostName.StartsWith(Path.DirectorySeparatorChar.ToString()) ||
                       hostName.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    hostName = hostName.Substring(1);
                }


                List<Scribbles_Watermark_Disposition> filesFromHost = BuildListOfFilesForHost(hostName, hostDirectory,
                                                                                              Scribbles_WatermarkLog,
                                                                                              progName);
                fullFileDispositionList.AddRange(filesFromHost);
            }

            return fullFileDispositionList;

        } // END BuildListOfFilesToWatermark(...) { ... }

        struct WatermarkBytes
        {
            public UInt64 block1;
            public UInt32 block2;
            public UInt64 block3;
        }

        public static RNGCryptoServiceProvider RandomGenerator;

        private const  string Base36_CharMap_String = "0123456789abcdefghijklmnopqrstuvwxyz";
        private static char[] Base36_CharMap_Array  = null;

        public static string UInt64_to_Base36_string(UInt64 input)
        {
            if (Base36_CharMap_Array == null)
            {
                Base36_CharMap_Array = Base36_CharMap_String.ToCharArray();
            }

            Stack<char> resultStack = new Stack<char>();

            while (input > 0)
            {
                UInt64 inputRemainder = input % 36;
                char currentChar = Base36_CharMap_Array[inputRemainder];
                resultStack.Push(currentChar);

                input -= inputRemainder;
                input /= 36;
            }

            string outputString = new string(resultStack.ToArray());
            return outputString;

        } // END UInt64_to_Base36_string(UInt64 input) { ... }

        public static UInt16 ComputeNextWatermarkLogNumber(Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog)
        {
            UInt16 maxLogNumber = 0;

            foreach (Scribbles_WatermarkLog_Entry logEntry in Scribbles_WatermarkLog)
            {
                if (logEntry.fileNumber > maxLogNumber)
                {
                    maxLogNumber = logEntry.fileNumber;
                }
            }

            return (UInt16)(maxLogNumber + 1);

        } // END ComputeNextWatermarkLogNumber(Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog) { ... }

        public static string ComputeFileOutputPath(string fileInputHost, string inputFolderPath,
                                                   string fileInputPath, string fileOutputDirectory)
        {
            string strippedFileInputPath = fileInputPath;

            if (strippedFileInputPath.StartsWith(inputFolderPath))
            {
                strippedFileInputPath = strippedFileInputPath.Substring(inputFolderPath.Length);
            }

            string fullInputFolderPath = Path.GetFullPath(inputFolderPath);
//          string fullInputDirectory  = Path.GetDirectoryName(fullInputFolderPath);

            if (strippedFileInputPath.StartsWith(fullInputFolderPath))
            {
                strippedFileInputPath = strippedFileInputPath.Substring(fullInputFolderPath.Length);
            }

            while (strippedFileInputPath.StartsWith(".") ||
                   strippedFileInputPath.StartsWith(Path.DirectorySeparatorChar.ToString()) ||
                   strippedFileInputPath.StartsWith(Path.AltDirectorySeparatorChar.ToString()) )
            {
                strippedFileInputPath = strippedFileInputPath.Substring(1);
            }

            string fullOutputFolderPath = fileOutputDirectory;

            if (!(strippedFileInputPath.StartsWith(fileInputHost)))
            {
                fullOutputFolderPath = Path.Combine(fileOutputDirectory, fileInputHost);
            }

            string fileOutputPath = Path.Combine(fullOutputFolderPath, strippedFileInputPath);

            return fileOutputPath;
        }

        public static string GenerateWatermarkString()
        {
            WatermarkBytes newByteBlocks = new WatermarkBytes();

            byte[] newByteBlock1 = new byte[sizeof(UInt64)];
            byte[] newByteBlock2 = new byte[sizeof(UInt32)];
            byte[] newByteBlock3 = new byte[sizeof(UInt64)];

            RandomGenerator.GetBytes(newByteBlock1);
            RandomGenerator.GetBytes(newByteBlock2);
            RandomGenerator.GetBytes(newByteBlock3);

            newByteBlocks.block1 = BitConverter.ToUInt64(newByteBlock1, 0);
            newByteBlocks.block2 = BitConverter.ToUInt32(newByteBlock2, 0);
            newByteBlocks.block3 = BitConverter.ToUInt64(newByteBlock3, 0);

            string outputString = "";

            outputString = outputString + UInt64_to_Base36_string(newByteBlocks.block1);
            outputString = outputString + UInt64_to_Base36_string(newByteBlocks.block2);
            outputString = outputString + UInt64_to_Base36_string(newByteBlocks.block3);

            return outputString;

        } // END GenerateWatermarkString() { ... }

        public static uint GetRandomIndex(uint arrayLength)
        {
            uint maxValidValue = (uint.MaxValue / arrayLength) * arrayLength;

            bool bRandomValueInitialized = false;
            uint  randomValue = 0;

            while (!bRandomValueInitialized)
            {
                // To avoid bias, repeat until our random number generator returns something under our maxValidValue:
                //
                byte[] randomData = new byte[sizeof(uint)];
                RandomGenerator.GetBytes(randomData);

                UInt32 randomInt  = BitConverter.ToUInt32(randomData, 0);
                if (randomInt < maxValidValue)
                {
                    bRandomValueInitialized = true;
                    randomValue = randomInt % arrayLength;
                }

            } // END while (!bRandomValueInitialized) { ... }

            return randomValue;

        } // END public static uint GetRandomIndex(uint arrayLength) { ... }

        public static string PickRandomString(string[] stringArray)
        {
            if (stringArray.Length < 0)
            {
                Console.WriteLine("ERROR: Length of stringArray is negative!");
                Environment.Exit(-400);
            }

            uint arrayLength = (uint) stringArray.Length;
            uint randomIndex = GetRandomIndex(arrayLength);

            return stringArray[randomIndex];
        }

        public static int[] GenerateDividerPositions(uint numDividersToInsert, int watermarkStringLength)
        {
            int[] dividerPositions = new int[numDividersToInsert];
            for (int i = 0; i < numDividersToInsert; i++)
            {
                uint     newDivider = 0;
                bool    bNewDividerComputed = false;
                while (!bNewDividerComputed)
                {
                    newDivider = GetRandomIndex( (uint) watermarkStringLength);

                    uint dividerFloor   = newDivider - 1;
                    uint dividerCeiling = newDivider + 1;

                    bool bNewDividerTooClose = false;
                    for (int j = 0; j < i; j++)
                    {
                        if ( (dividerFloor <= dividerPositions[j]) && (dividerPositions[j] <= dividerCeiling) )
                        {
                            // New divider is too close to one of our existing dividers.  Discard it:
                            //
                            bNewDividerTooClose = true;
                            break;
                        }

                    } // END for (int j = 0; j < i; j++) { ... }

                    if (!bNewDividerTooClose)
                    {
                        bNewDividerComputed = true;
                        break;
                    }

                } // END while (!bNewDividerComputed) { ... }

                dividerPositions[i] = (int) newDivider;

            } // END for (int i = 0; i < numDividersToInsert; i++) { ... }

            return dividerPositions;

        } // END GenerateDividerPositions(uint numDividersToInsert, int watermarkStringLength) { ... }

        public static string FormatWatermarkString(string watermarkString, bool bAddPathSeparators)
        {
            const uint MAX_NUM_WATERMARK_DIVIDERS = 4;

            uint numDividersToInsert = GetRandomIndex(MAX_NUM_WATERMARK_DIVIDERS);

            int watermarkLength = watermarkString.Length;

            int[] dividerPositions = GenerateDividerPositions(numDividersToInsert, watermarkLength);

            List<string> dividerOptions = new List<string>();
            dividerOptions.Add(".");
            dividerOptions.Add("-");
            dividerOptions.Add("_");
            if (bAddPathSeparators)
            {
                dividerOptions.Add("/");
            }

            string formattedWatermark = watermarkString;
            for (int i = 0; i < numDividersToInsert; i++)
            {
                uint dividerIndex  = GetRandomIndex( (uint) dividerOptions.Count);
                string dividerStr  = dividerOptions[  (int) dividerIndex];

                formattedWatermark = (formattedWatermark.Substring(0, dividerPositions[i]) + dividerStr +
                                      formattedWatermark.Substring(dividerPositions[i] + 1) );
            }

            return formattedWatermark;

        } // END public static uint GetRandomIndex(uint arrayLength) { ... }


        // This kind of takes place of NULL when you don't care about things:
        //
        public static object objNULL = System.Reflection.Missing.Value;

        public static bool  bDummyFileInitialized = false;
        public static string dummyFilePath = "";

        public static bool Extract_ZIPped_Document_To_Folder(string inputDocumentPath, string outputFolderPath)
        {
            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }

            ZipStorer  zipFileHandle = null;
            bool      bZipFileOpenOk = false;
            do {
                try
                {
                    zipFileHandle  = ZipStorer.Open(inputDocumentPath, FileAccess.Read);
                    bZipFileOpenOk = true;
                }
                catch (System.IO.IOException)
                {
                    int retrySeconds = 1;
                    Console.WriteLine(String.Format("File {0} is currently unavailable.  Will try again in {1} second(s)...",
                                                    inputDocumentPath, retrySeconds));
                    Thread.Sleep(retrySeconds * 1000);
                }
            } while (!bZipFileOpenOk);

            using (zipFileHandle)
            {
                List<ZipStorer.ZipFileEntry> zipContentsList = zipFileHandle.ReadCentralDir();

                foreach (ZipStorer.ZipFileEntry fileEntry in zipContentsList)
                {
                    string fileOutputPath = Path.Combine(outputFolderPath, fileEntry.FilenameInZip);

                    zipFileHandle.ExtractFile(fileEntry, fileOutputPath);

                } // END foreach (ZipStorer.ZipFileEntry fileEntry in zipContentsList) { ... }
            }

            return true;

        } // END Extract_ZIPped_Document_To_Folder(string inputDocumentPath, string outputFolderPath) { ... }


        public static bool Replace_File_In_ZIPped_Document(string inputDocumentPath, string outputDirectory, string replaceFilePath)
        {
            ZipStorer zipFileHandle = ZipStorer.Open(inputDocumentPath, FileAccess.ReadWrite);

            List<ZipStorer.ZipFileEntry> zipContentsList = zipFileHandle.ReadCentralDir();
            List<ZipStorer.ZipFileEntry> removeFilesList = new List<ZipStorer.ZipFileEntry>();

            ZipStorer.Compression replaceFileCompression = ZipStorer.Compression.Store;
            string                replaceFileArchiveName = null;
            string                replaceFileCommentStr  = null;

            foreach (ZipStorer.ZipFileEntry fileEntry in zipContentsList)
            {
                string fileOutputPath  = Path.GetFullPath( Path.Combine(outputDirectory, fileEntry.FilenameInZip) );
                if    (fileOutputPath == replaceFilePath)
                {
                    removeFilesList.Add(fileEntry);

                    replaceFileCompression = fileEntry.Method;
                    replaceFileArchiveName = fileEntry.FilenameInZip;
                    replaceFileCommentStr  = fileEntry.Comment;

                } // END if (fileOutputPath == replaceFilePath) { ... }

            } // END foreach (ZipStorer.ZipFileEntry fileEntry in zipContentsList) { ... }

            if (removeFilesList.Count == 0)
            {
                Console.WriteLine(String.Format("WARNING -- Unable to find file '{0}' in '{1}' -- file will not be replaced!\n",
                                                replaceFilePath, inputDocumentPath) );
                return false;
            }

            ZipStorer.RemoveEntries(ref zipFileHandle, removeFilesList);

            zipFileHandle.AddFile( replaceFileCompression, replaceFilePath,
                                   replaceFileArchiveName, replaceFileCommentStr );
            zipFileHandle.Close();

            return true;

        } // END Replace_File_In_ZIPped_Document(string inputDocumentPath, string outputDirectory, string replaceFilePath) { ... }


        public static List<string> BuildXmlRelFilesList(string docFolderImagesPath)
        {
            List<string> xmlRelFilesList = new List<string>();

            string[] documentRootFiles = Directory.GetFiles(docFolderImagesPath);
            foreach (string documentRootFile in documentRootFiles)
            {
                if (documentRootFile.EndsWith(".xml.rels"))
                {
                    string docRootFileFullPath = Path.GetFullPath(documentRootFile);

                    xmlRelFilesList.Add(docRootFileFullPath);

                } // END if (documentRootFile.EndsWith(".xml.rels")) { ... }

            } // END foreach (string documentRootFile in documentRootFiles) { ... }

            string[] subDirectories = Directory.GetDirectories(docFolderImagesPath);
            foreach (string subDirectory in subDirectories)
            {
                List<string> filesFromSubDir = BuildXmlRelFilesList(subDirectory);

                xmlRelFilesList.AddRange(filesFromSubDir);
            }

            return xmlRelFilesList;

        } // END BuildXmlRelFilesList(string docFolderImagesPath) { ... }


        public static bool ReplaceDummyFilePathWithWatermark(string dummyPath, string watermarkURL,
                                                             string docFolderPath, string drawingObjects_relativePath,
                                                             out List<string> listOfUpdatedXmlFiles)
        {
            bool bDummyPathReplaced = false;

            string drawingObjectsPath = Path.Combine(docFolderPath, drawingObjects_relativePath);

            listOfUpdatedXmlFiles = new List<string>();

            List<string> imageRelationshipXmlFiles = BuildXmlRelFilesList(drawingObjectsPath);

            foreach (string xmlRelFile in imageRelationshipXmlFiles)
            {
                XmlDocument xmlRelData = new XmlDocument();
                xmlRelData.Load(xmlRelFile);

                bool bXmlDataModified = false;

                //              XmlNodeList relationshipNodes = xmlRelData.SelectNodes("/Relationships/Relationship");
                XmlNodeList relationshipNodes = xmlRelData.GetElementsByTagName("Relationship");

                foreach (XmlNode relationshipNode in relationshipNodes)
                {
                    XmlAttribute targetAttribute = relationshipNode.Attributes["Target"];

                    if ((targetAttribute != null) && targetAttribute.Value.Contains(dummyPath))
                    {
                        targetAttribute.Value = watermarkURL;
                        bXmlDataModified = true;
                        bDummyPathReplaced = true;

                    } // END if ( (targetAttribute != null) && targetAttribute.Value.Contains(dummyPath) ) { ... }

                } // END foreach (XmlNode relationshipNode in relationshipNodes) { ... }

                if (bXmlDataModified)
                {
                    xmlRelData.Save(xmlRelFile);
                    listOfUpdatedXmlFiles.Add(xmlRelFile);
                }

            } // END foreach (string xmlRelFile in imageRelationshipXmlFiles) { ... }

            return bDummyPathReplaced;

        } // END ReplaceDummyFilePathWithWatermark(...) { ... }


        public static bool WatermarkWordDocument(string watermark_URL, string outputFilePath)
        {
            if (!bDummyFileInitialized)
            {
                Console.WriteLine("Error -- 'DummyImage' file is not initialized!  Cannot watermark file.  Exiting...\n");
                Environment.Exit(-255);
            }

            bool bXmlFilesReplacedOk = true;
            try
            {
                Word._Application wordApp = new Word.Application(); //We will be opening Word to do our thing
                wordApp.Visible = true;    //Don't show it though

                string filename  = outputFilePath;
                object readOnly  = false;
                object isVisible = true;

                // wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;
                Console.WriteLine(String.Format("Opening document {0}...", filename));
                object filenameObj = (object) filename;

                Word._Document watermarkDoc;
                Word._Document original_Doc = wordApp.Documents.Open(ref filenameObj, ref objNULL, ref readOnly, ref objNULL,
                                                                     ref objNULL,     ref objNULL, ref objNULL,  ref objNULL,
                                                                     ref objNULL,     ref objNULL, ref objNULL,  ref isVisible,
                                                                     ref objNULL,     ref objNULL, ref objNULL,  ref objNULL);

                object saveDocument   = true;
                string newFilename    = filename;
                object newFilenameObj = (object) newFilename;

                string fileExtension  = Path.GetExtension(outputFilePath);
                if    (fileExtension == ".doc")
                {
                    Word.WdSaveFormat newDocumentFormat = Word.WdSaveFormat.wdFormatDocumentDefault;
                    Word.WdCompatibilityMode compatMode = Word.WdCompatibilityMode.wdCurrent;

                    newFilename = filename + "x";
                    Console.WriteLine(String.Format("Converting document {0} to DOCX format...", filename));
                    original_Doc.SaveAs2(newFilename, newDocumentFormat, ref objNULL, ref objNULL, ref objNULL, ref objNULL,
                                         ref objNULL, ref objNULL,       ref objNULL, ref objNULL, ref objNULL, ref objNULL,
                                         ref objNULL, ref objNULL,       ref objNULL, ref objNULL, compatMode);
                    original_Doc.Save();
                    wordApp.Quit(ref saveDocument, ref objNULL, ref objNULL);

                    wordApp = new Word.Application(); //We will be opening Word to do our thing
                    wordApp.Visible = true;    //Don't show it though

                    newFilenameObj = (object)newFilename;

                    Console.WriteLine(String.Format("Re-opening document {0} in DOCX format...", filename));
                    watermarkDoc = wordApp.Documents.Open(ref newFilenameObj, ref objNULL, ref readOnly, ref objNULL,
                                                          ref objNULL,        ref objNULL, ref objNULL,  ref objNULL,
                                                          ref objNULL,        ref objNULL, ref objNULL,  ref isVisible,
                                                          ref objNULL,        ref objNULL, ref objNULL,  ref objNULL);
                }
                else
                {
                    watermarkDoc = original_Doc;
                }

                //Word.CustomProperties myCustProps = myDoc.CustomDocumentProperties;
                //int x = myCustProps.Count;
                object linkToFile       = true;
                object saveWithDocument = false;
                object widthAndHeight   = 1;

                Word.WdHeaderFooterIndex hfIndex = Word.WdHeaderFooterIndex.wdHeaderFooterPrimary;

                Word.HeaderFooter headerFooter;

                for (int i = 1; i < watermarkDoc.Sections.Count + 1; i++)
                {
                    if (watermarkDoc.Sections[i].Headers != null)
                    {
                        headerFooter = watermarkDoc.Sections[i].Headers[hfIndex];
                    }
                    else if (watermarkDoc.Sections[i].Footers != null)
                    {
                        headerFooter = watermarkDoc.Sections[i].Footers[hfIndex];
                    }
                    else
                    {
                        headerFooter = null;
                    }

                    if (headerFooter != null)
                    {
                        Word.Shape watermarkShape;

                        Console.WriteLine(String.Format("Adding watermark image to section {0}...", i));
                        watermarkShape = headerFooter.Shapes.AddPicture(dummyFilePath, ref linkToFile, ref saveWithDocument,
                                                                        ref objNULL,   ref objNULL,    ref widthAndHeight,
                                                                        ref widthAndHeight, ref objNULL);
                    }

                } // END for (int i = 1; i < myDoc.Sections.Count + 1; i++) { ... }

                watermarkDoc.Save();
                wordApp.Quit(ref saveDocument, ref objNULL, ref objNULL);

                string documentFolderPath = filename + ".ZIPfolder";

                Console.WriteLine(String.Format("UnZIPping contents of document {0}...", newFilename));
                bool bDocumentExtractedOk = Extract_ZIPped_Document_To_Folder(newFilename, documentFolderPath);

                List<string> updatedFiles;
                Console.WriteLine(String.Format("Inserting watermark link(s) into document {0}...", newFilename));
                bool bXmlFileWatermarked = ReplaceDummyFilePathWithWatermark(dummyFilePath, watermark_URL,
                                                                             documentFolderPath, "word",
                                                                             out updatedFiles);
                if (!bXmlFileWatermarked)
                {
                    Console.WriteLine("WARNING -- Unable to replace 'dummy' path with watermark in [{0}].  Skipping...\n");
                    return false;
                }

                Console.WriteLine(String.Format("Re-packing Word document {0}...", newFilename));
                foreach (string updatedXmlFile in updatedFiles)
                {
                    bool bCurrentFileReplacedOk = Replace_File_In_ZIPped_Document(newFilename, documentFolderPath, updatedXmlFile);
                    if (!bCurrentFileReplacedOk)
                    {
                        bXmlFilesReplacedOk = false;
                    }
                }

                Directory.Delete(documentFolderPath, true);
                Thread.Sleep(1000);

                if (fileExtension == ".doc")
                {
                    wordApp = new Word.Application(); //We will be opening Word to do our thing
                    wordApp.Visible = true;    //Don't show it though

                    Console.WriteLine(String.Format("Re-opening document {0} in DOCX format...", newFilename));
                    Word._Document myDoc = wordApp.Documents.Open(ref newFilenameObj, ref objNULL, ref readOnly, ref objNULL,
                                                                  ref objNULL,        ref objNULL, ref objNULL,  ref objNULL,
                                                                  ref objNULL,        ref objNULL, ref objNULL,  ref isVisible,
                                                                  ref objNULL,        ref objNULL, ref objNULL,  ref objNULL);

                    Word.WdSaveFormat oldDocumentFormat = Word.WdSaveFormat.wdFormatDocument97;
                    Console.WriteLine(String.Format("Converting document {0} back to DOC format...", newFilename));

                    myDoc.SaveAs2(filename, oldDocumentFormat);
                    myDoc.Save();
                    wordApp.Quit(ref saveDocument, ref objNULL, ref objNULL);

                    Thread.Sleep(1000);
                    File.Delete( newFilename );
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error attempting to add MS Word watermark to output file {0}:", outputFilePath));
                Console.WriteLine("[\n" + e.Message + "\n]\n");
                return false;
            }

            return bXmlFilesReplacedOk;

        } // END WatermarkWordDocument(string inputFilePath, string watermark_URL, string outputFilePath) { ... }

        public static bool WatermarkExcelSpreadsheet(string watermark_URL, ref string outputFilePath)
        {
            if (!bDummyFileInitialized)
            {
                Console.WriteLine("Error -- 'DummyImage' file is not initialized!  Cannot watermark file.  Exiting...\n");
                Environment.Exit(-256);
            }

            object readOnly = false;
            string filename = outputFilePath;
            string fileExtension = Path.GetExtension(outputFilePath);

            try
            {
                Excel._Application excelApp = new Excel.Application(); //We will be opening Excel to do our thing
                excelApp.Visible = true;    //Don't show it though

                // wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;
                Console.WriteLine(String.Format("Opening spreadsheet {0}...", filename));
                Excel._Workbook myBook = excelApp.Workbooks.Open(filename, objNULL, readOnly, objNULL, objNULL,
                                                                 objNULL,  objNULL, objNULL,  objNULL, objNULL,
                                                                 objNULL,  objNULL, objNULL,  objNULL, objNULL);
                myBook.Activate();

                Microsoft.Office.Core.MsoTriState linkToFile       = Microsoft.Office.Core.MsoTriState.msoTrue;
                Microsoft.Office.Core.MsoTriState saveWithDocument = Microsoft.Office.Core.MsoTriState.msoFalse;

                float leftAndTopPosition = 0.0f;
                float widthAndHeight = 1;

                var activeSheet = myBook.ActiveSheet;

                Excel.Shapes activeSheetShapes = activeSheet.Shapes;       // dummyFilePath
                Excel.Shape  watermarkShape    = activeSheetShapes.AddPicture(dummyFilePath, linkToFile, saveWithDocument,
                                                                              leftAndTopPosition, leftAndTopPosition,
                                                                              widthAndHeight,     widthAndHeight);

                if    (fileExtension == ".xls")
                {
                    string newFileName = filename + "x";
                    Excel.XlFileFormat newExcelFileType = Excel.XlFileFormat.xlOpenXMLWorkbook;
                    Console.WriteLine(String.Format("Converting spreadsheet {0} to XLSX format...", filename));
                    myBook.SaveAs(newFileName, newExcelFileType);
                    excelApp.Quit();
                    Thread.Sleep(2000);
                    File.Delete(filename);
                    filename = newFileName;
                }
                else
                {
                    myBook.Save();
                    excelApp.Quit();
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error attempting to add MS Excel watermark to output file {0}:", outputFilePath));
                Console.WriteLine("[\n" + e.Message + "\n]\n");
                return false;
            }

            string documentFolderPath = filename + ".ZIPfolder";

            Console.WriteLine(String.Format("UnZIPping contents of spreadsheet {0}...", filename));
            bool bDocumentExtractedOk = Extract_ZIPped_Document_To_Folder(filename, documentFolderPath);

            List<string> updatedFiles;
            Console.WriteLine(String.Format("Inserting watermark link(s) into spreadsheet {0}...", filename));
            bool bXmlFileWatermarked = ReplaceDummyFilePathWithWatermark(dummyFilePath, watermark_URL,
                                                                         documentFolderPath, "xl\\drawings",
                                                                         out updatedFiles);
            if (!bXmlFileWatermarked)
            {
                Console.WriteLine("WARNING -- Unable to replace 'dummy' path with watermark in [{0}].  Skipping...\n");
                return false;
            }

            bool bXmlFilesReplacedOk = true;
            Console.WriteLine(String.Format("Re-packing XLSX spreadsheet {0}...", filename));
            foreach (string updatedXmlFile in updatedFiles)
            {
                bool bCurrentFileReplacedOk = Replace_File_In_ZIPped_Document(filename, documentFolderPath, updatedXmlFile);
                if (!bCurrentFileReplacedOk)
                {
                    bXmlFilesReplacedOk = false;
                }
            }

            Directory.Delete(documentFolderPath, true);

            if (fileExtension == ".xls")
            {
                try
                {
                    Excel._Application excelApp = new Excel.Application(); //We will be opening Excel to do our thing
                    excelApp.Visible = true;    //Don't show it though

                    // wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;
                    Excel._Workbook myBook = excelApp.Workbooks.Open(filename, objNULL, readOnly, objNULL, objNULL,
                                                                     objNULL, objNULL, objNULL, objNULL, objNULL,
                                                                     objNULL, objNULL, objNULL, objNULL, objNULL);
                    myBook.Activate();

                    string oldFileName = outputFilePath;
                    Excel.XlFileFormat oldExcelFileType = Excel.XlFileFormat.xlExcel8;
                    Console.WriteLine(String.Format("Converting spreadsheet {0} back to XLS format...", filename));
                    myBook.SaveAs(oldFileName, oldExcelFileType);
                    excelApp.Quit();
                    Thread.Sleep(2000);
                    File.Delete(filename);
                    filename = oldFileName;
                }
                catch (Exception e)
                {
                    Console.WriteLine(String.Format("Error attempting to convert watermarked XLSX document back to XLS file {0}:", outputFilePath));
                    Console.WriteLine("[\n" + e.Message + "\n]\n");
                    return false;
                }

            }

            return bXmlFilesReplacedOk;

        } // END WatermarkExcelSpreadsheet(string watermark_URL, ref string outputFilePath) { ... }


        public static bool WatermarkPowerPointPresentation(string watermark_URL, string outputFilePath)
        {
            bool bXmlFilesReplacedOk = false;

            if (!bDummyFileInitialized)
            {
                Console.WriteLine("Error -- 'DummyImage' file is not initialized!  Cannot watermark file.  Exiting...\n");
                Environment.Exit(-257);
            }

            try
            {
                PowerPoint._Application powerPointApp = new PowerPoint.Application(); //We will be opening PowerPoint to do our thing

                string filename = outputFilePath;
                string fileExtension = Path.GetExtension(outputFilePath);

                Microsoft.Office.Core.MsoTriState readOnly  = Microsoft.Office.Core.MsoTriState.msoFalse;
                Microsoft.Office.Core.MsoTriState objFalse  = Microsoft.Office.Core.MsoTriState.msoFalse;
                Microsoft.Office.Core.MsoTriState isVisible = Microsoft.Office.Core.MsoTriState.msoTrue;
                powerPointApp.Visible = isVisible;    //Don't show it though

                // wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;
                Console.WriteLine(String.Format("Opening presentation {0}...", filename));
                PowerPoint.Presentation myPresentation = powerPointApp.Presentations.Open(filename, readOnly, objFalse, objFalse);

                PowerPoint.SlideRange firstSlideRange  = myPresentation.Slides.Range(1);
                PowerPoint.Slide      firstSlide       = firstSlideRange[1];
                PowerPoint.Shapes     firstSlideShapes = firstSlide.Shapes;

                Microsoft.Office.Core.MsoTriState linkToFile       = Microsoft.Office.Core.MsoTriState.msoTrue;
                Microsoft.Office.Core.MsoTriState saveWithDocument = Microsoft.Office.Core.MsoTriState.msoFalse;
                float pictureLeftAndRight   = 0.0f;
                float pictureWidthAndHeight = 1.0f;

                PowerPoint.Shape watermarkShape = firstSlideShapes.AddPicture(dummyFilePath, linkToFile, saveWithDocument,
                                                                              pictureLeftAndRight,   pictureLeftAndRight,
                                                                              pictureWidthAndHeight, pictureWidthAndHeight);

                if (fileExtension == ".ppt")
                {
                    string newFileName = filename + "x";
                    PowerPoint.PpSaveAsFileType newPowerPointFileType = PowerPoint.PpSaveAsFileType.ppSaveAsOpenXMLPresentation;
                    Console.WriteLine(String.Format("Converting presentation {0} to PPTX format...", filename));
                    myPresentation.SaveAs(newFileName, newPowerPointFileType);
                    myPresentation.Close();
                    powerPointApp.Quit();
                    Thread.Sleep(2000);
                    File.Delete(filename);
                    filename = newFileName;
                }
                else
                {
                    myPresentation.Save();
                    myPresentation.Close();
                    powerPointApp.Quit();
                    Thread.Sleep(1000);
                }

                string documentFolderPath = filename + ".ZIPfolder";

                Console.WriteLine(String.Format("UnZIPping contents of presentation {0}...", filename));
                bool bDocumentExtractedOk = Extract_ZIPped_Document_To_Folder(filename, documentFolderPath);

                List<string> updatedFiles;
                Console.WriteLine(String.Format("Inserting watermark link(s) into presentation {0}...", filename));
                bool bXmlFileWatermarked = ReplaceDummyFilePathWithWatermark(dummyFilePath, watermark_URL,
                                                                             documentFolderPath, "ppt\\slides",
                                                                             out updatedFiles);
                if (!bXmlFileWatermarked)
                {
                    Console.WriteLine("WARNING -- Unable to replace 'dummy' path with watermark in [{0}].  Skipping...\n");
                    return false;
                }

                bXmlFilesReplacedOk = true;
                Console.WriteLine(String.Format("Re-packing PPTX presentation {0}...", filename));
                foreach (string updatedXmlFile in updatedFiles)
                {
                    bool bCurrentFileReplacedOk = Replace_File_In_ZIPped_Document(filename, documentFolderPath, updatedXmlFile);
                    if (!bCurrentFileReplacedOk)
                    {
                        bXmlFilesReplacedOk = false;
                    }
                }

                Directory.Delete(documentFolderPath, true);

                if (fileExtension == ".ppt")
                {
                    powerPointApp = new PowerPoint.Application(); 
                    PowerPoint.Presentation newPresentation = powerPointApp.Presentations.Open(filename, readOnly, objFalse, objFalse);
                    string oldFileName = outputFilePath;
                    PowerPoint.PpSaveAsFileType oldPowerPointFileType = PowerPoint.PpSaveAsFileType.ppSaveAsPresentation;
                    Console.WriteLine(String.Format("Converting presentation {0} back to PPT format...", filename));
                    newPresentation.SaveAs(oldFileName, oldPowerPointFileType);
                    newPresentation.Close();
                    powerPointApp.Quit();
                    Thread.Sleep(2000);
                    File.Delete(filename);
                    filename = oldFileName;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error attempting to watermark PowerPoint document {0}:", outputFilePath));
                Console.WriteLine("[\n" + e.Message + "\n]\n");
                return false;
            }

            return bXmlFilesReplacedOk;

        } // END WatermarkPowerPointPresentation(string inputFilePath, string watermark_URL, string outputFilePath) { ... }


        public static bool WatermarkFile(UInt16 newFileNumber, string fileInputHost, string fileInputPath,
                                         string fileOutputPath, Scribbles_OptionsBlock scribblesOptions,
                                         out Scribbles_WatermarkLog_Entry resultingLogEntry,
                                         bool bRebuildOldWatermarkedFile = false,
                                         Scribbles_WatermarkLog_Entry? oldLogEntry = null)
        {
            Scribbles_WatermarkLog_Entry watermarkResults = new Scribbles_WatermarkLog_Entry();
            string watermarkURL = null;

            if (bRebuildOldWatermarkedFile)
            {
                Scribbles_WatermarkLog_Entry oldLogEntryNonNull = (Scribbles_WatermarkLog_Entry) oldLogEntry;
                watermarkURL = oldLogEntryNonNull.watermarkURL;

                watermarkResults.fileNumber = newFileNumber;

                watermarkResults.fileInputHost = oldLogEntryNonNull.fileInputHost;
                watermarkResults.fileInputPath = oldLogEntryNonNull.fileInputPath;
                watermarkResults.fileInputHash = ComputeFileHash(fileInputPath);

                watermarkResults.watermarkTag           = oldLogEntryNonNull.watermarkTag;
                watermarkResults.watermarkTag_Formatted = oldLogEntryNonNull.watermarkTag_Formatted;
                watermarkResults.watermarkURL           = watermarkURL;

            }
            else {
                string newWatermarkString = GenerateWatermarkString();

                string formattedWatermark = FormatWatermarkString(newWatermarkString, scribblesOptions.bAddPathSeparators);

                string URL_scheme = scribblesOptions.urlScheme;

                string hostServerName = PickRandomString(scribblesOptions.hostServerNameList);
                string hostRootPath = PickRandomString(scribblesOptions.hostRootPathList);
                string hostSubDirs = PickRandomString(scribblesOptions.hostSubDirsList);
                string hostFileName = PickRandomString(scribblesOptions.hostFileNameList);
                string hostFileExt = PickRandomString(scribblesOptions.hostFileExtList);

                string subDirSeparator = (hostSubDirs.Length  > 0) ? "/" : "";
                string fileNmSeparator = (hostFileName.Length > 0) ? "/" : "";

                watermarkURL = String.Format(WatermarkTag_URL_Format, URL_scheme, hostServerName, hostRootPath, hostSubDirs,
                                             subDirSeparator, formattedWatermark, fileNmSeparator, hostFileName, hostFileExt);

                watermarkResults.fileNumber = newFileNumber;

                watermarkResults.fileInputHost = fileInputHost;
                watermarkResults.fileInputPath = fileInputPath;
                watermarkResults.fileInputHash = ComputeFileHash(fileInputPath);

                watermarkResults.watermarkTag           = newWatermarkString;
                watermarkResults.watermarkTag_Formatted = formattedWatermark;
                watermarkResults.watermarkURL           = watermarkURL;
            }

            bool   bWatermarkOkay = false;

            string fileOutputDirectory = Path.GetDirectoryName(fileOutputPath);
            if (!Directory.Exists(fileOutputDirectory))
            {
                Directory.CreateDirectory(fileOutputDirectory);
            }
            if (!File.Exists(fileOutputPath))
            {
                File.Copy(fileInputPath, fileOutputPath);
            }

            string  fileExtension = Path.GetExtension(fileInputPath);
            switch (fileExtension)
            {
                case ".doc":
                case ".docx":
                    bWatermarkOkay = WatermarkWordDocument(watermarkURL, fileOutputPath);
                    if (!bWatermarkOkay)
                    {
                        resultingLogEntry = new Scribbles_WatermarkLog_Entry();
                        return false;
                    }
                    break;
                case ".xls":
                case ".xlsx":
                    bWatermarkOkay = WatermarkExcelSpreadsheet(watermarkURL, ref fileOutputPath);
                    if (!bWatermarkOkay)
                    {
                        resultingLogEntry = new Scribbles_WatermarkLog_Entry();
                        return false;
                    }
                    break;
                case ".ppt":
                case ".pptx":
                    bWatermarkOkay = WatermarkPowerPointPresentation(watermarkURL, fileOutputPath);
                    if (!bWatermarkOkay)
                    {
                        resultingLogEntry = new Scribbles_WatermarkLog_Entry();
                        return false;
                    }
                    break;
                default:
                    Console.WriteLine(String.Format("WARNING: Input file [{0}] does not have a recognized extension [{1}].  Skipping...",
                                                    fileInputPath, fileExtension) );
                    resultingLogEntry = new Scribbles_WatermarkLog_Entry();
                    return false;
            }

            string watermarkDateTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");

            watermarkResults.watermarkDateTime = watermarkDateTimeStamp;
            watermarkResults.fileOutputHash    = ComputeFileHash(fileOutputPath);
            watermarkResults.fileOutputPath    = fileOutputPath;

            resultingLogEntry = watermarkResults;
            return true;

        } // END WatermarkFile(...) { ... }


        public static bool AddWatermarkLogEntryToFile(Scribbles_WatermarkLog_Entry LogEntry, string LogFilePath)
        {
            bool bCreateNewLogFile = false;
            if (!File.Exists(LogFilePath))
            {
                bCreateNewLogFile = true;
            }

            string logMessage = String.Format(Scribbles_WatermarkLog_LineFormat,
                                              LogEntry.fileNumber,
                                              LogEntry.fileInputHost,
                                              LogEntry.fileInputPath,
                                              LogEntry.fileInputHash,
                                              LogEntry.watermarkDateTime,
                                              LogEntry.watermarkTag,
                                              LogEntry.watermarkTag_Formatted,
                                              LogEntry.watermarkURL,
                                              LogEntry.fileOutputHash,
                                              LogEntry.fileOutputPath
                                              );
            if (bCreateNewLogFile)
            {
                Console.WriteLine(String.Format("Creating new watermark log file [{0}]...", LogFilePath));
                File.AppendAllText(LogFilePath, Scribbles_WatermarkLog_HeaderLines);
            }
            Console.WriteLine(String.Format("Appending watermark information to log file...\n"));
            File.AppendAllText(LogFilePath, logMessage);

            return true;

        } // END bool AddWatermarkLogEntryToFile(Scribbles_WatermarkLog_Entry LogEntry, string LogFilePath) { ... }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            string progName = System.AppDomain.CurrentDomain.FriendlyName;
            bool  bShowHelp = false;

            Console.WriteLine(String.Format("{0}: Starting...", progName));

            OptionSet optionsParser = null;

            // Loop until debugger is attached:
            //
            //Console.WriteLine(String.Format("{0}: Waiting for debugger to attach...", progName));
            //while (!Debugger.IsAttached) { Thread.Sleep(250); }
            //Console.WriteLine(String.Format("{0}: Debugger attached.  Proceeding...", progName));

            Scribbles_OptionsBlock cmdLineOptions = ParseCmdLineOptions(args, out optionsParser, out bShowHelp, progName);

            if (bShowHelp)
            {
                ShowHelp(progName, optionsParser);
                Environment.Exit(0);
            }

            RandomGenerator = new RNGCryptoServiceProvider();

            Scribbles_OptionsBlock totalOptions;
            if (cmdLineOptions.input__ReceiptFile != null)
            {
                string inputReceipt = cmdLineOptions.input__ReceiptFile;
                totalOptions = LoadUndefinedOptionsFromReceiptFile(cmdLineOptions, inputReceipt);
            }
            else
            {
                totalOptions = cmdLineOptions;
            }

            Console.WriteLine(String.Format("{0}: Verifying options and loading defaults where necessary...", progName) );
            VerifyOptionsBlockAndSetDefaults(ref totalOptions);

            string outputReceiptFile = totalOptions.output_ReceiptFile;
            Console.WriteLine(String.Format("{0}: Writing options out to receipt file ({1})...", progName, outputReceiptFile) );
            WriteOptionsBlockToReceiptFile(totalOptions, outputReceiptFile);

            totalOptions.input__ReceiptFile = null;
            totalOptions.output_ReceiptFile = null;

            Scribbles_OptionsBlock finalOptions;
            Console.WriteLine(String.Format("{0}: Re-loading options to verify receipt file...", progName) );
            finalOptions = LoadFinalOptionsFromReceiptFile(outputReceiptFile);

            if ( !( finalOptions.Equals(totalOptions) ) )
            {
                Console.WriteLine(String.Format("{0}: Error -- problem loading options from receipt file ({1}).  Exiting...\n", progName, outputReceiptFile) );
                Environment.Exit(-50);
            }

            Scribbles_WatermarkLog_Entry[] Scribbles_WatermarkLog = LoadInputWatermarkLog(finalOptions.input__WatermarkLog, progName);

            List<Scribbles_Watermark_Disposition> filesToWatermark = BuildListOfFilesToWatermark(finalOptions.input__Directory,
                                                                                                 Scribbles_WatermarkLog, progName);

            if (!bDummyFileInitialized)
            {
                string tempFilePath = Path.GetTempFileName();

                Resource1.DummyImage.Save(tempFilePath);

                dummyFilePath = tempFilePath;
                bDummyFileInitialized = true;
            } // END if (!bDummyFileInitialized) { ... }

            ushort nextWatermarkLogNumber = ComputeNextWatermarkLogNumber(Scribbles_WatermarkLog);

            foreach (Scribbles_Watermark_Disposition fileDisposition in filesToWatermark)
            {
                bool bWatermarkStatus = false;
                Scribbles_WatermarkLog_Entry newWatermarkLogEntry = new Scribbles_WatermarkLog_Entry();

                string  fileHostName   = fileDisposition.hostName;
                string  fileInputPath  = fileDisposition.fileInputPath;

                string  fileOutputPath = ComputeFileOutputPath(fileHostName,  finalOptions.input__Directory,
                                                               fileInputPath, finalOptions.output_Directory);

                fileInputPath  = Path.GetFullPath( fileInputPath  );
                fileOutputPath = Path.GetFullPath( fileOutputPath );

                switch (fileDisposition.fileDisposition)
                {
                case WatermarkOperation.WatermarkNewFile:
                    bWatermarkStatus = WatermarkFile(nextWatermarkLogNumber, fileHostName, fileInputPath, fileOutputPath,
                                                     finalOptions, out newWatermarkLogEntry);
                    if (bWatermarkStatus)
                    {
                        Console.WriteLine(String.Format("Created new watermark output file at [{0}]...",
                                                        newWatermarkLogEntry.fileOutputPath));
                    }
                    break;
                case WatermarkOperation.RebuildSameWatermarkedFile:
                    bool bRebuildSameWatermarkedFile = true;
                    string oldWatermarkOutputHash = fileDisposition.originalWatermarkInfo.fileOutputHash;
                    bWatermarkStatus = WatermarkFile(nextWatermarkLogNumber, fileHostName, fileInputPath, fileOutputPath,
                                                     finalOptions, out newWatermarkLogEntry, bRebuildSameWatermarkedFile,
                                                     fileDisposition.originalWatermarkInfo);
                    if (bWatermarkStatus)
                    {
                        Console.WriteLine(String.Format("Rebuilt watermarked output file at [{0}]...",
                                                        newWatermarkLogEntry.fileOutputPath));
                    }
                    break;
                case WatermarkOperation.ReplaceFileWithNewWatermark:
                    bWatermarkStatus = WatermarkFile(nextWatermarkLogNumber, fileHostName, fileInputPath, fileOutputPath,
                                                     finalOptions, out newWatermarkLogEntry);
                    if (bWatermarkStatus)
                    {
                        Console.WriteLine(String.Format("Replaced previously watermarked output file at [{0}]...",
                                                        newWatermarkLogEntry.fileOutputPath));
                    }
                    break;
                case WatermarkOperation.CopyMatchingWatermarkedFile:
                    string oldWatermarkOutputFile = fileDisposition.originalWatermarkInfo.fileOutputPath;

                    File.Copy(oldWatermarkOutputFile, fileOutputPath);
                    newWatermarkLogEntry.fileNumber             = nextWatermarkLogNumber;
                    newWatermarkLogEntry.fileInputHost          = fileHostName;
                    newWatermarkLogEntry.fileInputPath          = fileInputPath;
                    newWatermarkLogEntry.fileInputHash          = fileDisposition.originalWatermarkInfo.fileInputHash;
                    newWatermarkLogEntry.watermarkDateTime      = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                    newWatermarkLogEntry.watermarkTag           = fileDisposition.originalWatermarkInfo.watermarkTag;
                    newWatermarkLogEntry.watermarkTag_Formatted = fileDisposition.originalWatermarkInfo.watermarkTag_Formatted;
                    newWatermarkLogEntry.watermarkURL           = fileDisposition.originalWatermarkInfo.watermarkURL;
                    newWatermarkLogEntry.fileOutputHash         = ComputeFileHash(fileOutputPath);
                    newWatermarkLogEntry.fileOutputPath         = fileOutputPath;

                    bWatermarkStatus = true;
                    Console.WriteLine(String.Format("Copied  old watermark output file to [{0}] (from [{1}])...",
                                                    fileOutputPath, oldWatermarkOutputFile));
                    break;
                default:
                    Console.WriteLine(String.Format("ERROR:  Unrecognized file disposition ({1}).  Skipping...\n",
                                                    fileDisposition.fileDisposition));
                    break;
                }

                if (bWatermarkStatus)
                {
                    AddWatermarkLogEntryToFile(newWatermarkLogEntry, finalOptions.output_WatermarkLog);
                }

                nextWatermarkLogNumber++;

            } // END foreach (Scribbles_Watermark_Disposition currentFileDisposition in filesToWatermark) { ... }

            Console.WriteLine(String.Format("\nAll done!"));
            Console.WriteLine(String.Format("Receipt file with watermark parameters saved at [{0}].", outputReceiptFile));
            Console.WriteLine(String.Format("Watermark log file can be found at [{0}]...", finalOptions.output_WatermarkLog));
            Console.WriteLine(String.Format("Happy hunting!\n"));

            return;

        } // END public static void Main(string[] args) { ... }

    } // END static class Program { ... }

}
