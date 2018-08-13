using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class RelationFile
    {
        private string BasePath { get; set; } = Directory.GetCurrentDirectory() + @"\";
        public string Path { get; set; }
        private string Name { get; set; }
        private int RecordSize { get; set; } = 64;
        private int Block { get; set; } = 4096;
        public int FirstDeletedRecordLocation { get; set; }
        public List<string> RelationAttributes { get; set; }

        public RelationFile(string name, List<string> relationattributes)
        {
            this.Path = BasePath + name;
            this.Name = name;
            this.RelationAttributes = new List<string>(relationattributes);
            this.Create();
        }

        /// <summary>
        /// Creates file that stores a relation
        /// </summary>
        /// <param name="PointerLength">The fixed length of the Pointer stored within the file</param>
        public void Create()
        {
            if (this.Block % RecordSize != 0)
            {
                throw new Exception("The Pointer lenght must be a divisor of the block size: " + this.Block);
            }

            Record header = new Record();
            header.Attributes = new Dictionary<string, string>
            {
                { "Header", "H" },
                { "FirstDeletedRecordLocation", "0" },
                { "NextId", "1" }
            };
            FirstDeletedRecordLocation = 0;

            this.WriteToFile(header.ToString(), this.Block, 0, SeekOrigin.Begin);
        }

        public List<Record> Read()
        {
            List<Record> results = new List<Record>();

            using (FileStream fs = new FileStream(this.Path, FileMode.Open))
            {
                fs.Seek(this.Block, SeekOrigin.Begin);
                int eof = -1;
                Byte[] buffer = new Byte[this.Block];
                //load the buffer
                eof = fs.Read(buffer, 0, this.Block);
                while (eof != 0)
                {
                    string info = "";
                    string[] temp;
                    List<string> records = new List<string>();
                    info = Encoding.UTF8.GetString(buffer).TrimEnd('\0').TrimEnd(';');
                    temp = info.Split(';');
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = temp[i].TrimStart('\0');
                    }
                    records.AddRange(temp);
                    foreach (string record in records)
                    {
                        results.Add(new Record(this.RelationAttributes, record));
                    }
                    //read the buffer at the end in order to know when eof is reached
                    eof = fs.Read(buffer, 0, this.Block);
                }
            }

            return results;
        }

        public Record ReadHeader()
        {
            Record results = new Record();

            using (FileStream fs = new FileStream(this.Path, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                string info = "";
                Byte[] buffer = new Byte[4096];
                info = Encoding.UTF8.GetString(buffer).TrimEnd('\0').TrimEnd(';');
                results = new Record(this.RelationAttributes, info);
            }

            return results;
        }

        public void WriteRecord(string record)
        {
            SeekOrigin origin = SeekOrigin.End;
            int offset = 0;
            if (this.FirstDeletedRecordLocation != 0)
            {
                offset = this.FirstDeletedRecordLocation;
                origin = SeekOrigin.Begin;
            }
            this.WriteToFile(record, this.RecordSize, offset, origin);
        }

        /// <summary>
        /// Write a block of records to the file created by this class
        /// </summary>
        /// <param name="record">The information to be written</param>
        public void WriteToFile(string record, int length, long offset, SeekOrigin origin)
        {
            using (FileStream fs = new FileStream(this.Path, FileMode.OpenOrCreate))
            {
                fs.Seek(offset, origin);
                Byte[] buffer = new Byte[length];
                Byte[] info = new UTF8Encoding(true).GetBytes(record.ToString());
                Array.Copy(info, buffer, info.Length);
                for (int i = buffer.Length; i < length; i++)
                {
                    buffer[i] = new UTF8Encoding(true).GetBytes("\0")[0];
                }
                Console.WriteLine("-------------" + buffer.Length);
                if (buffer.Length <= this.Block)
                {
                    fs.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    throw new Exception("Record too large. Should be max " + this.Block + " bytes (chars)");
                }
            }
        }
    }
}
