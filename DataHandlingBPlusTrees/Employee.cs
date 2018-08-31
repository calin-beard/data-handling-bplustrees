using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    public class Employee : SerializableRecord<Employee>
    {
        public int Id { get; set; }
        public char Gender { get; set; }
        public int Salary { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public static BlockCache Cache { get; set; }
        public static Employee Empty { get; set; } = new Employee();
        static Employee()
        {
            Cache = new BlockCache(PathName());
            Empty.Id = -1;
            Empty.Salary = 0;
            Empty.Gender = 'M';
            Empty.FirstName = Empty.LastName = "";
        }

        public override int RecordSize()
        {
            return sizeof(int) + sizeof(char) + sizeof(int) + 2 * 15;
        }

        public Employee() { }

        public Employee(int id, char gender, int salary, string firstname, string lastname)
        {
            this.Id = id;
            this.Gender = gender;
            this.Salary = salary;
            this.FirstName = firstname;
            this.LastName = lastname;
        }

        protected override Employee ReadRecord(Block b, int offset)
        {
            //int offs = RecordSize() * offset;
            Employee emp = new Employee();
            using (MemoryStream ms = new MemoryStream(b.Bytes, offset, RecordSize()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    emp.Id = br.ReadInt32();
                    emp.Gender = br.ReadChar();
                    emp.Salary = br.ReadInt32();
                    emp.FirstName = br.ReadString();
                    emp.LastName = br.ReadString();
                }
            }
            return emp;
        }

        protected override void WriteRecord(Employee record, Block b, int offset)
        {
            //int offs = RecordSize() * offset;
            using (MemoryStream ms = new MemoryStream(b.Bytes, offset, RecordSize()))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(record.Id);
                    bw.Write(record.Gender);
                    bw.Write(record.Salary);
                    bw.Write(record.FirstName);
                    bw.Write(record.LastName);
                }
            }
        }

        public override Employee GetEmptyRecord()
        {
            return Employee.Empty;
        }

        public override Block CreateEmptyBlock()
        {
            Block b = new Block();
            using (MemoryStream ms = new MemoryStream(b.Bytes, 0, this.RecordSize()))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    for (int i = 0; i < Block.Size() / this.RecordSize(); i++)
                    {
                        bw.Write(Employee.Empty.Id);
                        bw.Write(Employee.Empty.Gender);
                        bw.Write(Employee.Empty.Salary);
                        bw.Write(Employee.Empty.FirstName);
                        bw.Write(Employee.Empty.LastName);
                    }
                }
            }
            return b;
        }

        public override BlockCache GetCache()
        {
            return Employee.Cache;
        }

        public static string PathName()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "database");
        }

        public override string GetPathName()
        {
            return Employee.PathName();
        }

        public override string ToString()
        {
            return String.Join("-", new string[]{
                this.Id.ToString(),
                this.Gender.ToString(),
                this.Salary.ToString(),
                this.FirstName.ToString(),
                this.LastName.ToString(),
            });
        }
    }
}
