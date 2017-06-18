using System.IO;

namespace CLanWPFTest.Objects
{
    public class CLanFile
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string RelativePath { get; set; }

        public CLanFile(string name, long size = -1, string relpath = "")
        {
            this.Name = name;
            this.Size = size == -1 ? (new FileInfo(this.Name)).Length : size;
            this.RelativePath = relpath == "" ? Name : relpath;
        }
    }
}
