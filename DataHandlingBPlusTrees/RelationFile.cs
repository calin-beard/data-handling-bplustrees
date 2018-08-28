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

        public string FirstDeletedRecordLocation
        {
            get
            {
                return this.ReadHeader()["FirstDeletedRecord"];
            }
            set
            {
                this.WriteToFile("H," + value + ",1;", this.Block, 0, SeekOrigin.Begin);
            }
        }

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

            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Id", "H" },
                { "FirstDeletedRecord", "-0" },
                { "NextId", "1" }
            };

            this.WriteToFile(String.Join(Record.Separator, header.Values) + Record.Terminator, this.Block, 0, SeekOrigin.Begin);
        }
        public Dictionary<string, string> ReadHeader()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            using (FileStream fs = new FileStream(this.Path, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                string info = "";
                string[] temp = new string[3];
                Byte[] buffer = new Byte[4096];
                fs.Read(buffer, 0, this.Block);
                info = Encoding.UTF8.GetString(buffer).TrimEnd('\0').TrimEnd(';');
                temp = info.Split(',');
                results.Add("Id", temp[0]);
                results.Add("FirstDeletedRecord", temp[1]);
                results.Add("NextId", temp[2]);
            }

            return results;
        }

        //public void WriteHeader()
        //{
        //    this.WriteToFile(String.Join(Record.Separator, this.Header.Values) + Record.Terminator, this.Block, 0, SeekOrigin.Begin);
        //}

        /// <summary>
        /// Reads the record at the location and returns it as a string or returns "eof" if this was reached
        /// </summary>
        /// <param name="location">Location where to read the record from</param>
        /// <returns></returns>
        public string ReadRecord(int location)
        {
            string result = "";

            using (FileStream fs = new FileStream(this.Path, FileMode.Open))
            {
                fs.Seek(location, SeekOrigin.Begin);
                //string info = "";
                //string[] temp = new string[3];
                int eof = -1;
                Byte[] buffer = new Byte[4096];
                eof = fs.Read(buffer, 0, this.Block);
                if (eof > 0)
                {
                    result = Encoding.UTF8.GetString(buffer).TrimEnd('\0').TrimEnd(';');
                    //result = result.TrimStart('-');
                }
                else
                {
                    result = "eof";
                }
            }

            return result;
        }

        // Iterative
        /// <summary>
        /// Finds the next (or previous) location of a deleted record, starting at startLocation. Default is next.
        /// </summary>
        /// <param name="startLocation">Location where to start looking for a deleted record (not including the record at the location)</param>
        /// <param name="before">True if it needs to search before the startLocation. Default is false</param>
        /// <returns>The location of the previous/next location of a deleted record or -1 in case of an error.</returns>
        public int FindDeletedRecordLocation(int startLocation, bool before = false)
        {
            int result = -1;
            int current = startLocation;
            string info = "";

            info = this.ReadRecord(current);

            while (info != "eof" && current >= 4096)
            {
                if (info.StartsWith("-"))
                {
                    result = current;
                    break;
                }
                if (before)
                {
                    current -= this.RecordSize;
                }
                else
                {
                    current += this.RecordSize;
                }
                info = this.ReadRecord(current);
            }

            if (info == "eof")
            {
                result = startLocation;
            }

            return result;
        }

        // Revursive
        public int _FindDeletedRecordLocation(int location, bool before = false)
        {
            if (this.ReadRecord(location).StartsWith("-"))
            {
                return location;
            }
            if (before)
            {
                location -= this.RecordSize;
            }
            else
            {
                location += this.RecordSize;
            }
            return _FindDeletedRecordLocation(location, before);
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
                        if (!record.StartsWith("-"))
                        {
                            results.Add(new Record(this.RelationAttributes, record));
                        }
                    }
                    //read the buffer at the end in order to know when eof is reached
                    eof = fs.Read(buffer, 0, this.Block);
                }
            }

            return results;
        }

        public void WriteRecord(string record, int location = 0)
        {
            // most common scenario - insert new record at the end of the file
            SeekOrigin origin = SeekOrigin.End;
            int offset = 0;
            int firstDeletedRecord;
            Int32.TryParse(this.FirstDeletedRecordLocation.Substring(1), out firstDeletedRecord);
            // if there is a deleted record - insert there
            if (firstDeletedRecord != 0)
            {
                offset = firstDeletedRecord;
                origin = SeekOrigin.Begin;
                // specifies the location directly - when writing the location of the next deleted record
                // or when a record is deleted
                if (location > 0)
                {
                    offset = location;
                    origin = SeekOrigin.Begin;
                }
            }
            else if (location > 0)
            {
                offset = location;
                origin = SeekOrigin.Begin;
            }
            this.WriteToFile(record, this.RecordSize, offset, origin);
            if (!record.StartsWith("-") && offset > 0 && offset.ToString().CompareTo(this.FirstDeletedRecordLocation.Substring(1)) == 0)
            {
                string newFirstDeletedRecordLocation = "-" + this.FindDeletedRecordLocation(offset);
                if (this.FirstDeletedRecordLocation.CompareTo(newFirstDeletedRecordLocation) != 0)
                {
                    this.FirstDeletedRecordLocation = newFirstDeletedRecordLocation;
                }
                else
                {
                    this.FirstDeletedRecordLocation = "-0";
                }
            }
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

        public void DeleteRecord(int location)
        {
            int firstDeletedRecord;
            Int32.TryParse(this.FirstDeletedRecordLocation.Substring(1), out firstDeletedRecord);
            Console.WriteLine("----------firstDeletedRecord=" + firstDeletedRecord);

            // only occurs when the first record is deleted
            // or when a record is deleted after filling all the empty spots
            if (firstDeletedRecord == 0)
            {
                this.FirstDeletedRecordLocation = "-" + location.ToString();
                Console.WriteLine("----------NEWfirstDeletedRecord=" + firstDeletedRecord);
                //this.WriteHeader();
                this.WriteRecord("-" + location, location);
            }
            // one of the most common cases for a large file
            else if (location < firstDeletedRecord)
            {
                WriteRecord("-" + firstDeletedRecord.ToString(), location);
                this.FirstDeletedRecordLocation = "-" + location.ToString();
                //this.WriteHeader();
            }
            // one of the most common cases for a large file
            else if (location > firstDeletedRecord)
            {
                // TO DO
                // have to look for the previous location and add the new location to it
                int nextLocation = this.FindDeletedRecordLocation(location);
                int previousLocation = this.FindDeletedRecordLocation(location, true);
                this.WriteRecord("-" + nextLocation, location);
                this.WriteRecord("-" + location, previousLocation);
                //this.FirstDeletedRecordLocation = "-" + location.ToString();
                //this.WriteHeader();
            }
        }
    }
}
