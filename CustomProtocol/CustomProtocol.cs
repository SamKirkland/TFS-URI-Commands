using Microsoft.Win32;
using System;
using System.Linq;

namespace CustomProtocol
{
    class CustomProtocol
    {
        public delegate void URI_Handle(Uri toHandle);
        public URI_Handle uriHandler;
        private string registryKey;
        private string customURI;
        
        /// <summary>
        /// Create a new CustomProtocol attached to the uri specified
        /// </summary>
        /// <param name="uri">The uri you with to attach to. Cannot contain spaces</param>
        /// <exception cref="InvalidOperationException">The custom uri cannot contain spaces</exception>
        public CustomProtocol(string protocolURI, URI_Handle uriCallback) // ToDo: add log location, and bind location as params
        {
            if (protocolURI.Contains(' '))
            {
                throw new InvalidOperationException("Custom protocol cannot contain spaces");
            }
            
            customURI = protocolURI;
            registryKey = $"URL:{customURI} Protocol";
            uriHandler = uriCallback;
        }

        public bool isAttached() {
            using (RegistryKey hkcrClass = Registry.ClassesRoot.OpenSubKey(customURI))
            {
                if (hkcrClass != null)
                {
                    string val = hkcrClass.GetValue("").ToString();
                    if (val == null)
                    {
                        Console.WriteLine("value not found");
                        return false;
                    }
                    else
                    {
                        // use the value
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("key not found");
                    return false;
                }
            }
        }

        /// <exception cref="Exception">attach was already called (and was successful) use .isAttached to see attach status</exception>
        public void attach()
        {
            if (isAttached())
            {
                return; // Custom protocol already attached
            }

            string appPath = System.Reflection.Assembly.GetEntryAssembly().Location;


            // HKEY_CLASSES_ROOT\myscheme
            using (RegistryKey hkcrClass = Registry.ClassesRoot.CreateSubKey(customURI))
            {
                hkcrClass.SetValue(null, registryKey);
                hkcrClass.SetValue("URL Protocol", String.Empty, RegistryValueKind.String);

                // use the application's icon as the URI scheme icon
                using (RegistryKey defaultIcon = hkcrClass.CreateSubKey("DefaultIcon"))
                {
                    string iconValue = String.Format("\"{0}\",0", appPath);
                    defaultIcon.SetValue(null, iconValue);
                }

                // open the application and pass the URI to the command-line
                using (RegistryKey shell = hkcrClass.CreateSubKey("shell"))
                {
                    using (RegistryKey open = shell.CreateSubKey("open"))
                    {
                        using (RegistryKey command = open.CreateSubKey("command"))
                        {
                            string cmdValue = String.Format("\"{0}\" \"%1\"", appPath);
                            command.SetValue(null, cmdValue);
                        }
                    }
                }
            }
        }

        public void detact()
        {
            if (!isAttached())
            {
                return; // Custom protocol isn't attached
            }

            Registry.ClassesRoot.DeleteSubKeyTree(customURI);
        }
    }
}
