using System;
using System.Diagnostics;
using System.IO;

namespace SaveGameBackup
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                // Get the current user's home directory
                string userHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Eden Games", "Test Drive Unlimited 2");
                Console.WriteLine($"User Home Directory: {userHome}");

                // Set variables based on the user's home directory
                string sourceDir = Path.Combine(userHome, "savegame");
                string backupDir = Path.Combine(userHome, "backups");

                Console.WriteLine($"Source Directory: {sourceDir}");
                Console.WriteLine($"Backup Directory: {backupDir}");

                // Get current date and time
                string datetime = DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
                Console.WriteLine($"Datetime: {datetime}");

                // Specify the path to tar
                string tarPath = @"C:\Windows\System32\tar.exe";

                // Check if tar exists
                if (!File.Exists(tarPath))
                {
                    Console.WriteLine($"tar not found at {tarPath}. Exiting...");
                    return;
                }

                // Check if the source folder exists
                if (!Directory.Exists(sourceDir))
                {
                    Console.WriteLine($"Source folder {sourceDir} not found. Exiting...");
                    return;
                }
                else
                {
                    Console.WriteLine($"Source folder {sourceDir} found.");
                }

                // Create the backup directory if it doesn't exist
                if (!Directory.Exists(backupDir))
                {
                    try
                    {
                        Directory.CreateDirectory(backupDir);
                        Console.WriteLine($"Backup directory created at {backupDir}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create backup directory: {ex.Message}");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"Backup directory already exists at {backupDir}.");
                }

                // Create archive in the backup directory
                string archivePath = Path.Combine(backupDir, $"savegame_{datetime}.tar");
                Console.WriteLine($"Creating archive at {archivePath}");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = tarPath,
                    Arguments = $"-cf \"{archivePath}\" .",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = sourceDir // Set working directory to sourceDir
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("Archiving successful.");
                    }
                    else
                    {
                        Console.WriteLine("Archiving failed!");
                        Console.WriteLine($"Error: {process.StandardError.ReadToEnd()}");
                        return;
                    }
                }

                // Optionally ask if the user wants to delete the original savegame folder
                Console.Write("Do you want to delete the original savegame folder? (y/n) [n]: ");
                string deleteChoice = Console.ReadLine().Trim().ToLower();
                if (string.IsNullOrEmpty(deleteChoice))
                {
                    deleteChoice = "n";
                }

                if (deleteChoice == "y")
                {
                    Console.WriteLine("Deleting original folder...");
                    try
                    {
                        Directory.Delete(sourceDir, true);
                        Console.WriteLine("Folder deleted.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete folder: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Folder kept.");
                }

                // Inform the user that the backup process is complete and give a 5-second pause before closing
                Console.WriteLine("Backup process completed. Closing in 5 seconds...");
                System.Threading.Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
