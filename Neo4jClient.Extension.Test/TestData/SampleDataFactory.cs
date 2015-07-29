using System;
using Neo4jClient.Extension.Test.TestData.Entities;
using UnitsNet;

namespace Neo4jClient.Extension.Test.Cypher
{
    public class SampleDataFactory
    {
        public static Person GetWellKnownPerson(int n)
        {
            var archer = new Person
            {
                Id=n
                ,Name = "Sterling Archer"
                ,Sex= Gender.Male
                ,HomeAddress = GetWellKnownAddress(200)
                ,WorkAddress = GetWellKnownAddress(59)
                ,IsOperative =true
                ,SerialNumber = 123456
                ,SpendingAuthorisation = 100.23m
                ,DateCreated = DateTimeOffset.Parse("2015-07-11T08:00:00+10:00")
            };

            return archer;
        }

        public static Address GetWellKnownAddress(int n)
        {
            var address = new Address {Street = n + " Isis Street", Suburb = "Fakeville"};
            return address;
        }

        public static Weapon GetWellKnownWeapon(int n)
        {
            var weapon = new Weapon();
            weapon.Id = n;
            weapon.Name = "Grenade Launcher";
            weapon.BlastRadius = Area.FromSquareMeters(20);
            return weapon;
        }
    }
}