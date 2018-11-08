using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CustomProtocol
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        
        static void tf_uriReceived(Uri fullURI)
        {
            string uriParams = fullURI.LocalPath.ToString();

            // execute merge command and close "/C" (/K Carries out the command specified by string but remains)
            Process.Start("cmd.exe", $"/C echo Merging Changes... & TF {uriParams}");
        }
        
        static void error(string message) {
            log.Fatal(message);
            Console.WriteLine(message);
        }

        static void Main(string[] args)
        {
            // custom uri handlers
            CustomProtocol tfCommand = new CustomProtocol("tfmerge", tf_uriReceived);
            
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

                Console.WriteLine("Install Done. Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}