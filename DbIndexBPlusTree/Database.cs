using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbIndexBPlusTree
{
    class Database
    {
        public static string CreateMock(int recordCount)
        {
            string firstNamesPath = Path.Combine(Directory.GetCurrentDirectory(), "first-names.txt");
            string namesPath = Path.Combine(Directory.GetCurrentDirectory(), "names.txt");
            string[] firstNames, lastNames;
            int block = 0;
            int offset = 0;

            firstNames = GetNamesFromFile(firstNamesPath);
            lastNames = GetNamesFromFile(namesPath);

            Random rs = new Random();
            Random rfn = new Random();
            Random rln = new Random();
            for (int i = 1; i <= recordCount; i++)
            {
                int fni = rfn.Next(1, firstNames.Length - 1);
                int lni = rln.Next(1, lastNames.Length - 1);
                char genre = '\0';
                if (fni % 2 == 0)
                {
                    genre = 'M';
                }
                else
                {
                    genre = 'F';
                }
                int salary = rs.Next(30, 60) * 100;
                string firstName = firstNames[fni];
                string lastName = lastNames[lni];
                Employee e = new Employee(i, genre, salary, firstName, lastName);
                e.SetRecord(e, block, offset);
                offset += e.RecordSize();
                if (Block.Size() - offset < e.RecordSize())
                {
                    offset = 0;
                    block++;
                }
            }

            return Employee.PathName();
        }

        private static string[] GetNamesFromFile(string path)
        {
            string[] result = new string[0];
            if (File.Exists(path))
            {
                result = File.ReadAllLines(path);
            }
            return result;
        }
    }
}
