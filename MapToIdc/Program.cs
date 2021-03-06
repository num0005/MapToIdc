﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MapToIdc
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo mapFile = null;

            while (mapFile == null)
            {
                Console.WriteLine("Enter the path to the map file:");
                var mapPath = Console.ReadLine().Replace("\"", "");

                if (File.Exists(mapPath) && mapPath.EndsWith(".map"))
                    mapFile = new FileInfo(mapPath);
            }

            Console.WriteLine();
            
            int base_addr = 0;
            while (base_addr == 0)
            {
                Console.WriteLine("Enter base address:");
                int base;
                if(Int32.TryParse(Console.ReadLine(), out base))
                {
                    base_addr = base;
                }
            }
            
            Console.WriteLine();

            FileInfo idcFile = null;

            while (idcFile == null)
            {
                Console.WriteLine("Enter the path to the idc file:");
                var idcPath = Console.ReadLine().Replace("\"", "");

                if (idcPath.EndsWith(".idc"))
                    idcFile = new FileInfo(idcPath);
            }
            

            Console.WriteLine();
            Console.Write("Creating names...");

            var addresses = new HashSet<int>();

            using (var reader = mapFile.OpenText())
            using (var writer = idcFile.CreateText())
            {
                writer.WriteLine("#include <idc.idc>");
                writer.WriteLine();
                writer.WriteLine("static main()");
                writer.WriteLine("{");

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    while (line.Contains("\t\t"))
                        line = line.Replace("\t\t", "\t");
                    while (line.Contains("\t"))
                        line = line.Replace("\t", " ");
                    while (line.Contains("  "))
                        line = line.Replace("  ", " ");
                    while (line.StartsWith(" "))
                        line = line.Substring(1, line.Length - 1);

                    if (!line.StartsWith("00"))
                        continue;

                    var tokens = line.Split(' ').Where(token => token.Length > 0).ToList();

                    if (tokens.Count < 4 || !tokens.First().Contains(':') || !(tokens.Last().Contains(".obj") || tokens.Last().Contains(".exe") || tokens.Last().Contains(".xex")))
                        continue;

                    var address = base_addr + Convert.ToInt32($"0x{tokens[0].Split(':').Last()}", 16);
                    var name = tokens[1];
                    var comment = tokens.Count > 3 ? $" // {tokens[3]}" : "";

                    if (name.Contains("\\") || name.Contains("+") || addresses.Contains(address))
                        continue;

                    addresses.Add(address);

                    writer.WriteLine($"\tMakeName(0x{address:X}, \"{name}\");{comment}");
                }

                writer.WriteLine("}");
            }

            Console.WriteLine($"done. Created {addresses.Count} {(addresses.Count == 1 ? "name" : "names")}.");
        }
    }
}
