namespace DataHandlingBPlusTrees
{
    class Attributte
    {
        public string Name { get; set; }
        public bool PrimaryKey { get; set; }
        public bool NotNull { get; set; }
        public bool AutoIncrement { get; set; }

        public Attributte(string name)
        {
            this.Name = name;
            this.PrimaryKey = false;
            this.NotNull = false;
            this.AutoIncrement = false;
        }

        public Attributte(string name, bool primarykey, bool notnull, bool autoincrement)
        {
            this.Name = name;
            this.PrimaryKey = primarykey;
            this.NotNull = notnull;
            this.AutoIncrement = autoincrement;
        }
    }
}