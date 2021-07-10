using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WoTClanIconDownloadConsole
{
    public static class Utils
    {
        public static void AttachAssemblyResolver()
        {
            //handle any assembly resolves
            //https://stackoverflow.com/a/19806004/3128017
            AppDomain.CurrentDomain.AssemblyResolve += (sender2, bargs) =>
            {
                string dllName = new AssemblyName(bargs.Name).Name + ".dll";
                Assembly assem = Assembly.GetExecutingAssembly();
                string resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName));
                using (Stream stream = assem.GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    Console.WriteLine("An assembly was loaded via AssemblyResolve: {0}", dllName);
                    return Assembly.Load(assemblyData);
                }
            };
        }

        public static void HandleException(Exception ex, bool debugMode, ApplicationExitCode exitCode)
        {
            Console.WriteLine(ex.ToString());

            HandleClose(debugMode, exitCode);  
        }

        public static void HandleError(string message, bool debugMode, ApplicationExitCode exitCode)
        {
            Console.WriteLine(message);

            HandleClose(debugMode, exitCode);
        }

        public static void HandleClose(bool debugMode, ApplicationExitCode exitCode)
        {
            if (debugMode)
            {
                Console.WriteLine("Press enter to exit, or the application will time out in 5 seconds");
                Task[] tasks = new Task[] { Task.Run(() => Console.ReadLine()) };
                Task.WaitAny(tasks, 5000);
            }

            Environment.Exit((int)exitCode);
        }
    }
}
