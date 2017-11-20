using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CLanWPFTest.Objects
{
    public class CLanFile
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public long Size { get; set; }

        public CLanFile(string path, string name = "", long size = -1)
        {
            this.RelativePath = path;
            this.Size = size == -1 ? (new FileInfo(this.RelativePath)).Length : size;
            this.Name = name == "" ? Path.GetFileName(RelativePath) : name;
        }

        public static List<CLanFile> GetFiles(List<string> paths)
        {
            // Converts a list of paths in a sequence of CLanFiles
            List<CLanFile> files = new List<CLanFile>();

            // For each of those entries I must check if they are files 
            // or if they are folders, and in case i have to enumerate all possible
            // subfolders and files
            foreach (string p in paths)
            {
                FileAttributes attributes = File.GetAttributes(p);
                // now we will detect whether its a directory or file
                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // Directory
                    String folderName = new DirectoryInfo(Path.GetDirectoryName(p)).FullName + Path.DirectorySeparatorChar;
                    foreach (string f in Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories))
                    {
                        // Now i have all the possible subdirectories of that folder and all files included
                        String relativeName = f.Replace(folderName, "");
                        files.Add(new CLanFile(f, relativeName));
                    }
                }
                else
                {
                    // File
                    files.Add(new CLanFile(p));
                }
            }
            // files.ForEach((x) => Trace.WriteLine(x.Name));
            return files;
        }
    }
}
