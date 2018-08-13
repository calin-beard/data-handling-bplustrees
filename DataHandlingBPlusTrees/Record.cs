﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class Record
    {
        //public List<string> Attributes { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        private string Separator { get; set; } = ",";
        private string Terminator { get; set; } = ";";
        public Record() { }

        public Record(Dictionary<string, string> attributes)
        {
            this.Attributes = new Dictionary<string, string>(attributes);
        }

        public Record(List<string> relationattributes, string recordstring)
        {
            this.Attributes = new Dictionary<string, string>();
            string[] temp;
            List<string> attributeNames;

            recordstring = recordstring.Trim('\0').TrimEnd(';');
            foreach (string attribute in relationattributes)
            {
                this.Attributes.Add(attribute, "");
            }
            attributeNames = new List<string>(this.Attributes.Keys);
            temp = recordstring.Split(',');
            if(temp.Length != attributeNames.Count)
            {
                throw new Exception("--- The number of record attributes has to match the number of columns in the relation");
            }
            int i = 0;
            foreach (string attribute in attributeNames)
            {
                this.Attributes[attribute] = temp[i];
                i++;
            }
        }

        public Record(List<string> relationattributes, List<string> record)
        {
            this.Attributes = new Dictionary<string, string>();
            string[] temp = record.ToArray();
            List<string> attributeNames;

            foreach (string attribute in relationattributes)
            {
                this.Attributes.Add(attribute, "");
            }
            attributeNames = new List<string>(this.Attributes.Keys);
            int i = 0;
            foreach (string attribute in attributeNames)
            {
                this.Attributes[attribute] = temp[i];
                i++;
            }
        }

        public override string ToString()
        {
            //return string.Join(this.Separator, this.Attributes) + this.Terminator;
            string result = "";
            foreach (KeyValuePair<string, string> element in this.Attributes)
            {
                result += element.Value + this.Separator;
            }
            result = result.Substring(0, result.Length - 1);
            result += this.Terminator;

            return result;
        }
    }
}
