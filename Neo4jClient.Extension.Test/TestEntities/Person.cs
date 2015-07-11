namespace Neo4jClient.Extension.Test.Cypher
{
    public class SampleDataFactory
    {
        public static Person GetWellKnownPerson(int n)
        {
            var archer = new Person
            {
                Id=7
                ,Name = "Sterling Archer"
                ,Address = new Address { Street="200 Isis Street", Suburb = "Fakeville"}
            };

            return archer;
        }
    }

    public class SomeClass
    {
        public string SomeString { get; set; }
        public int Foo { get; set; }
        public bool Bar { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Title { get; set; }
        public Address Address { get; set; }

        public Person()
        {
            Address = new Address();
        }

        public override string ToString()
        {
            return string.Format("Id={0}, Name={1}", Id, Name);
        }
    }
}