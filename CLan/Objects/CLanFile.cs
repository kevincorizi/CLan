using System;
using System.Collections.Generic;
using System.IO;

namespace CLan.Objects
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
                        String relativeName = f.Replace(folderName, "");    // This maintains folder structure from the root on
                        files.Add(new CLanFile(f, relativeName));
                    }
                }
                else
                {
                    // File
                    files.Add(new CLanFile(p));
                }
            }
            return files;
        }

        public static List<CLanFile> EnforceDuplicatePolicy(List<CLanFile> files, string root)
        {
            List<string> myDirectories = new List<string>();
            foreach (CLanFile f in files)
            {
                string directoryName = Path.GetDirectoryName(f.Name);   // f.Name maintains the folders from the root on
                // If incoming file is in folder
                if (directoryName.Length > 0)
                {
                    // If there is no current folder with the same name, create it
                    // Note that this will be always valid for subfolders
                    // If there is a folder with that name, apply the policy selected by the user

                    if (Directory.Exists(root + directoryName) && !myDirectories.Contains(root + directoryName))
                    {
                        // A folder with that name already exists
                        if (SettingsManager.DefaultRenameOnDuplicate)
                        {
                            // Apply renaming policy
                            // Need to change directoryName to something that does not exist
                            string newDirectoryName = directoryName;
                            for (int i = 1; ; i++)
                            {
                                newDirectoryName = directoryName + " (" + i + ")";
                                if (!Directory.Exists(root + newDirectoryName))
                                    break;
                            }
                            // If the name of the parent directory changes, this must be propagated to all CLanFiles starting from the same parent
                            foreach (CLanFile f2 in files)
                            {
                                if(Path.GetDirectoryName(f2.Name).Split(Path.DirectorySeparatorChar)[0].CompareTo(directoryName) == 0)
                                {
                                    files[files.IndexOf(f2)].Name = newDirectoryName + f2.Name.Substring(directoryName.Length);
                                }
                            }
                            directoryName = newDirectoryName;
                        }
                    }
                    Directory.CreateDirectory(root + directoryName);
                    myDirectories.Add(root + directoryName);
                }

                // Check if the file already exists and apply duplicate policy
                if (File.Exists(root + f.Name))
                {
                    if (SettingsManager.DefaultRenameOnDuplicate)
                    {
                        string newFileName = f.Name;
                        for (int i = 1; ; i++)
                        {
                            newFileName = Path.GetFileNameWithoutExtension(f.Name) + " (" + i + ")" + Path.GetExtension(f.Name);
                            if (!File.Exists(root + newFileName))
                                break;
                        }
                        f.Name = newFileName;
                    }
                }
            }
            return files;
        }
    }
}
