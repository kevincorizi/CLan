using System.IO;

namespace CLanWPFTest.Objects
{
    public class CLanFile
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string RelativePath { get; set; }

        public CLanFile(string name)
        {
            this.Name = name;
            this.RelativePath = name;
            this.Size = (new FileInfo(this.Name)).Length;
        }
    }
}
