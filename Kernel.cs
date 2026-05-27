using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sys = Cosmos.System;

namespace bulbaShell
{
    public class Kernel : Sys.Kernel
    {
        Sys.FileSystem.CosmosVFS fs;
        String currentddir = @"0:\";
        List<string> commandHistory = new List<string>();

        protected override void BeforeRun()
        {
            fs = new Sys.FileSystem.CosmosVFS();
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
            Console.Clear();
            Console.WriteLine("Hi, lottop! :3");
            Console.WriteLine("------bulba-Shell------");
            Console.WriteLine("-----version-1.5------");
            Console.WriteLine("------Type help for all commands.-----");
        }

        protected override void Run()
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("BS>>");
            Console.ForegroundColor = ConsoleColor.White;
            Commands();
        }

        public void Commands()
        {
            string filename = "";
            string dirname = "";
            var input = Console.ReadLine();

        
            if (input != "history")
                commandHistory.Add(input);

            switch (input)
            {
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(input + ": Unknown command");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "dir":
                    try
                    {
                        var files = fs.GetDirectoryListing(currentddir);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(" Directory of " + currentddir);
                        Console.ForegroundColor = ConsoleColor.White;

                        foreach (var file in files)
                        {
                            if (file.mName.EndsWith("\\"))
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("  [DIR]  " + file.mName);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine("         " + file.mName);
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error reading directory: " + e.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;

                case "cd":
                    string newDir = Console.ReadLine();
                    string testDir = currentddir + newDir;

                    try
                    {
                        var test = fs.GetDirectoryListing(testDir);
                        currentddir = testDir;
                        if (!currentddir.EndsWith("\\"))
                            currentddir += "\\";
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Directory not found: " + testDir);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;

                case "mkfile":
                    filename = Console.ReadLine();
                    fs.CreateFile(currentddir + filename);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("File created: " + filename);
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "mkdir":
                    dirname = Console.ReadLine();
                    fs.CreateDirectory(currentddir + dirname);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Directory created: " + dirname);
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "rmfile":
                    filename = Console.ReadLine();
                    Sys.FileSystem.VFS.VFSManager.DeleteFile(currentddir + filename);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("File deleted: " + filename);
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "rmdir":
                    dirname = Console.ReadLine();
                    Sys.FileSystem.VFS.VFSManager.DeleteDirectory(currentddir + dirname, true);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Directory deleted: " + dirname);
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                // НОВЫЕ КОМАНДЫ:
                case "cp":
                    Console.Write("Source file: ");
                    string source = Console.ReadLine();
                    Console.Write("Destination file: ");
                    string dest = Console.ReadLine();
                    try
                    {
                        string sourcePath = currentddir + source;
                        string destPath = currentddir + dest;

                        if (Sys.FileSystem.VFS.VFSManager.FileExists(sourcePath))
                        {
                            string content = File.ReadAllText(sourcePath);
                            File.WriteAllText(destPath, content);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("File copied: " + source + " -> " + dest);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Source file not found: " + source);
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error copying file: " + e.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;

                case "calc":
                    Console.Write("Expression (e.g., 2+2): ");
                    string expr = Console.ReadLine();
                    try
                    {
                        // Простой парсер выражений
                        double result = EvaluateExpression(expr);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Result: " + result);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: " + e.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;

                case "history":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(" Command History:");
                    Console.ForegroundColor = ConsoleColor.White;
                    if (commandHistory.Count == 0)
                    {
                        Console.WriteLine(" No commands in history");
                    }
                    else
                    {
                        for (int i = 0; i < commandHistory.Count; i++)
                        {
                            Console.WriteLine(" " + (i + 1) + ". " + commandHistory[i]);
                        }
                    }
                    break;

                case "edit":
                    Console.Write("File to edit: ");
                    string editFile = Console.ReadLine();
                    string fullPath = currentddir + editFile;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("---bulba-Editor---");
                    Console.WriteLine("Enter text (type 'SAVE' on new line to save and exit)");
                    Console.WriteLine("Type 'CANCEL' to exit without saving");
                    Console.ForegroundColor = ConsoleColor.White;

                    StringBuilder content_builder = new StringBuilder();
                    string line;
                    bool save = false;

                    while (true)
                    {
                        line = Console.ReadLine();
                        if (line == "SAVE")
                        {
                            save = true;
                            break;
                        }
                        else if (line == "CANCEL")
                        {
                            break;
                        }
                        content_builder.AppendLine(line);
                    }

                    if (save)
                    {
                        try
                        {
                            // Если файл существует, перезаписываем
                            File.WriteAllText(fullPath, content_builder.ToString());
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("File saved: " + editFile);
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Error saving file: " + e.Message);
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Edit cancelled");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;

                case "help":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("help Commands:");
                    Console.WriteLine("shutdown - Turn off");
                    Console.WriteLine("reboot   - Restart");
                    Console.WriteLine("sysinfo  - System info");
                    Console.WriteLine("clear    - Clear screen");
                    Console.WriteLine("dir      - List directory");
                    Console.WriteLine("cd       - Change directory");
                    Console.WriteLine("mkfile   - Create file");
                    Console.WriteLine("mkdir    - Create directory");
                    Console.WriteLine("rmfile   - Delete file");
                    Console.WriteLine("rmdir    - Delete directory");
                    Console.WriteLine("cp       - Copy file");
                    Console.WriteLine("calc     - Simple calculator");
                    Console.WriteLine("history  - Show command history");
                    Console.WriteLine("edit     - Simple text editor");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "shutdown":
                    Console.WriteLine("Shutdown...");
                    Cosmos.System.Power.Shutdown();
                    break;

                case "reboot":
                    Console.WriteLine("Reboot...");
                    Cosmos.System.Power.Reboot();
                    break;

                case "sysinfo":
                    string CPUName = Cosmos.Core.CPU.GetCPUBrandString();
                    string CPUName_c = Cosmos.Core.CPU.GetCPUVendorName();
                    uint Amount_of_Ram = Cosmos.Core.CPU.GetAmountOfRAM();
                    uint UsedRam = Cosmos.Core.GCImplementation.GetUsedRAM();
                    ulong Available = Cosmos.Core.GCImplementation.GetAvailableRAM();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("CPU: {0}", CPUName);
                    Console.WriteLine("CPU Vendor: {0}", CPUName_c);
                    Console.WriteLine("Total RAM: {0} MB", Amount_of_Ram);
                    Console.WriteLine("Used RAM: {0} MB", UsedRam);
                    Console.WriteLine("Available: {0} MB", Available);
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "clear":
                    Console.Clear();
                    break;
            }
        }

        // Простой калькулятор
        private double EvaluateExpression(string expr)
        {
            expr = expr.Replace(" ", "");

            // Поддержка + - * /
            if (expr.Contains("+"))
            {
                string[] parts = expr.Split('+');
                return double.Parse(parts[0]) + double.Parse(parts[1]);
            }
            else if (expr.Contains("-"))
            {
                string[] parts = expr.Split('-');
                return double.Parse(parts[0]) - double.Parse(parts[1]);
            }
            else if (expr.Contains("*"))
            {
                string[] parts = expr.Split('*');
                return double.Parse(parts[0]) * double.Parse(parts[1]);
            }
            else if (expr.Contains("/"))
            {
                string[] parts = expr.Split('/');
                return double.Parse(parts[0]) / double.Parse(parts[1]);
            }

            return double.Parse(expr);
        }
    }
}