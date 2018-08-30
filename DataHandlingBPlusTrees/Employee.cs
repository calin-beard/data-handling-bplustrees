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

        public static Employee empty { get; set; } = new Employee();
        static Employee()
        {
            empty.Id = empty.Salary = -1;
            empty.Gender = 'M';
            empty.FirstName = empty.LastName = "";
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

        public override Employee ReadRecord(Block b, int offset)
        {
            int offs = RecordSize() * offset;
            Employee emp = new Employee();
            using (MemoryStream ms = new MemoryStream(b.Bytes, offs, RecordSize()))
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

        public override void WriteRecord(Employee record, Block b, int offset)
        {
            int offs = RecordSize() * offset;
            using (MemoryStream ms = new MemoryStream(b.Bytes, offs, RecordSize()))
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

        public override string PathName()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "testing");
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
