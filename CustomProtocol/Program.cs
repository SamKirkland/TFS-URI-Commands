using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CustomProtocol
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        private static string getTFPath() {
            var possibleTFSPaths = new List<string> {
                "C:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\Common7\\IDE\\",
                "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer",
                "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer",
                "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer"
            };
            
            return getPath("VS140COMNTOOLS", possibleTFSPaths, "TF.exe");
        }
        private static string getTFPTPath()
        {
            var possibleTFSPTPaths = new List<string> {
                "C:\\Program Files (x86)\\Microsoft Team Foundation Server 2015 Power Tools\\"
            };

            return getPath("VS140COMNTOOLS", possibleTFSPTPaths, "TFPT.exe");
        }

        /// <summary>
        /// Returns a path based on the current users env vars (and system vars)
        /// </summary>
        /// <param name="envPath">env/system var to extract</param>
        /// <param name="backupPath">Path to use if the env/system var isn't set</param>
        /// <param name="fileName">The file name to look for</param>
        /// <returns>the path stored in the env/system var, or the backupPath</returns>
        private static string getPath(string envPath, List<string> backupPath, string fileName) {
            try {
                Environment.CurrentDirectory = Environment.GetEnvironmentVariable(envPath);
                DirectoryInfo info = new DirectoryInfo(".");
                return $"{info.FullName}\\..\\IDE\\{fileName}";
            }
            catch (ArgumentNullException e) {
                // user doesn't have the environment var, return the hard-coded path
                foreach (var path in backupPath) {
                    if (File.Exists(path + fileName))
                    {
                        return path + fileName;
                    }
                    else
                    {
                        log.Warn($"Could not find file envPath:{envPath}, backup:{backupPath}, exception:{e.ToString()}");
                        Console.WriteLine($"Could not find file: {path}{fileName}");
                    }
                }
                
            }
            catch (Exception e) {
                log.Fatal($"getPath exception! envPath:{envPath}, backup:{backupPath}, exception:{e.ToString()}");
                Console.WriteLine($"getPath exception! envPath:{envPath}, backup:{backupPath}, exception:{e.ToString()}");
            }

            log.Warn($"Failed to find {fileName}, to fix this error create a environment variable 'VS140COMNTOOLS' that points to the FOLDER with {fileName}");
            Console.WriteLine($"Failed to find {fileName}, to fix this error create a environment variable 'VS140COMNTOOLS' that points to the FOLDER with {fileName}");
            return null;
        }


        static void tf_uriReceived(Uri fullURI)
        {
            string uriParams = fullURI.LocalPath.ToString();
            log.Info($"Command tfmerge. Running:{getTFPath()} {uriParams}");
            Process.Start(getTFPath(), uriParams);
            // ToDo: add .StandardOutput.ReadToEnd(); and verify the process ran
        }


        static void tfpt_uriReceived(Uri fullURI)
        {
            string uriParams = fullURI.LocalPath.ToString();
            log.Info($"Command tfpt. Running:{getTFPath()} {uriParams}");
            Process.Start(getTFPTPath(), uriParams);
            // ToDo: add .StandardOutput.ReadToEnd(); and verify the process ran
        }

        static void error(string message) {
            log.Fatal(message);
            Console.WriteLine(message);
        }

        static void Main(string[] args)
        {
            // custom uri handlers
            CustomProtocol tfCommand = new CustomProtocol("tfmerge", tf_uriReceived);
            CustomProtocol tfptCommand = new CustomProtocol("tfpt", tfpt_uriReceived);
            
            if (args.Length > 0)
            { // a URI was passed and needs to be handled
                log.Info("App trigger from uri");

                try
                {
                    Console.WriteLine("Running URI");
                    Uri command = new Uri(args[0].Trim());

                    if (command.Scheme == "tfmerge")
                    {
                        tfCommand.uriHandler(command);
                    }
                    else
                    {
                        tfptCommand.uriHandler(command);
                    }

                }
                catch (UriFormatException e)
                {
                    log.Fatal($"Invalid uri {e}");
                    Console.WriteLine("Invalid URI.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            else
            {
                log.Info("Custom protocol started from the user");
                Console.WriteLine("Installing custom URI's");

                try
                {
                    if (tfCommand.isAttached())
                    {
                        Console.WriteLine("TF command already found, uninstalling first.");
                        tfCommand.detact();
                    }
                    tfCommand.attach();
                    Console.WriteLine("Successfully installed tfmerge uri.");
                }
                catch (Exception e) {
                    error($"Failed to install tfmerge protocol. Error:{e.Message}");
                }
                try
                {
                    if (tfptCommand.isAttached())
                    {
                        Console.WriteLine("TFPT command already found, uninstalling first.");
                        tfptCommand.detact();
                    }
                    tfptCommand.attach();
                    Console.WriteLine("Successfully installed tfpt uri.");
                }
                catch (Exception e)
                {
                    error($"Failed to install tfpt protocol. Error:{e.Message}");
                }

                Console.WriteLine("Install Done. Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}