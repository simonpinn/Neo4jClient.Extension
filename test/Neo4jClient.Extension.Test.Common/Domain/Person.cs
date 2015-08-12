using System;

namespace Neo4jClient.Extension.Test.Cypher
{
    public enum Gender
    {
        Unspecified = 0,
        Male,
        Female
    }

    /// <summary>
    /// Contains value types and one complex type
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Primary key seeded from else where
        /// </summary>
        public int Id { get; set; }
        
        public string Name { get; set; }

        public Gender Sex { get; set; }

        public string Title { get; set; }

        public Address HomeAddress { get; set; }

        public Address WorkAddress { get; set; }

        public bool IsOperative { get; set; }

        public int SerialNumber { get; set; }

        public Decimal SpendingAuthorisation { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public Person()
        {
            HomeAddress = new Address();
            WorkAddress = new Address();
        }

        public override string ToString()
        {
            return string.Format("Id={0}, Name={1}", Id, Name);
        }
    }
}