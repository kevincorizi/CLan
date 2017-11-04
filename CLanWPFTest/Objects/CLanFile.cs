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
    }
}
