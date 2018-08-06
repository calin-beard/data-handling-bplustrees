using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class FileManager
    {
        public string Path { get; set; }
        //public string 

        public FileManager(string path)
        {
            this.Path = path;
        }

        public void Write(string record)
        {
            //string path = Directory.GetCurrentDirectory() + @"\test";
            //Console.WriteLine("------------" + path);
            int block = 4096;
            using (FileStream fs = new FileStream(this.Path, FileMode.OpenOrCreate))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(record);
                if (info.Length < block)
                {
                    fs.Write(info, 0, info.Length);
                }
                else
                {
                    throw new Exception("Record too large. Should be max 4096 bytes (chars)");
                }
            }
        }
    }
}
