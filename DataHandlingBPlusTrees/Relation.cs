using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class Relation
    {
        public List<Attributte> Attributes { get; set; }
        public List<string> AttributeNames { get; set; }
        private string Name { get; set; }
        public RelationFile File { get; set; }
        public int NextId { get; set; }

        public Relation(string name, List<Attributte> attributes)
        {
            this.Name = name;
            this.Attributes = new List<Attributte>(attributes);
            this.AttributeNames = new List<string>(attributes.Select(e => e.Name).ToList());
            this.File = new RelationFile(name, this.AttributeNames);
            this.NextId = 1;
        }
    }
}
