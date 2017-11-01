using System.IO;

namespace CLanWPFTest.Objects
{
    public class CLanFile
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string RelativePath { get; set; }

        public CLanFile(string path, long size = -1, string name = "")
        {
            this.RelativePath = path;
            this.Name = name == "" ? Path.GetRandomFileName() : name;
            this.Size = size == -1 ? (new FileInfo(this.RelativePath)).Length : size;
        }
    }
}
