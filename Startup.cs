using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace simple.dbapp
{
    public class Startup
    {
        private const string LONG_LINE = "------------------------------------------------------------------------------------------------------------------------";
        private readonly ILogger<Startup> Logger;
        public readonly SimpleDbContext Database;
        public Startup(ILogger<Startup> logger,
            SimpleDbContext database)
        {
            Logger = logger;
            Database = database;
        }

        public async Task Run()
        {
            try
            {
                await DoWork();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task DoWork()
        {
            await PopulateDatabase();
            var builder = new ExpressionBuilder();

            // ********************************************************************************
            // *********************** Basic select of base properties ************************
            // ********************************************************************************

            //Correct as Georgia has an age, but we aren't selecting it
            //var workingSelect = "id,name";
            //{"Id":"dba3f6d4-3b31-434b-a7c8-e86c989e20f5","Name":"Georgia","Age":0,"VetId":null,"Vet":null,"Pets":null}

            //Correct as we can see age here
            var workingSelect = "id,name,age";

            var statement = builder.BuildSelector<Person, Person>(workingSelect);
            //{"Id":"dba3f6d4-3b31-434b-a7c8-e86c989e20f5","Name":"Georgia","Age":32,"VetId":null,"Vet":null,"Pets":null}
            var result = Database.People.Where(x => x.Name.Equals("Georgia")).Select(statement);

            Console.WriteLine("Result:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result.FirstOrDefault()));
            Console.WriteLine(LONG_LINE);

            // ********************************************************************************
            // *********************** Selecting collection properties ************************
            // ********************************************************************************

            var collectionSelect = "id,pets.name";
            var collectionStatement = builder.BuildSelector<Person, Person>(collectionSelect);

            //Correct as we are resolving the collection, and only the properties in it we asked for
            //{"Id":"dba3f6d4-3b31-434b-a7c8-e86c989e20f5","Name":null,"Age":0,"VetId":null,"Vet":null,"Pets":[
            // {"Id":"00000000-0000-0000-0000-000000000000","Name":"Evie","Species":0,"OwnerId":"00000000-0000-0000-0000-000000000000","Owner":null},
            // {"Id":"00000000-0000-0000-0000-000000000000","Name":"Buddy","Species":0,"OwnerId":"00000000-0000-0000-0000-000000000000","Owner":null}]}
            var collectionResult = Database.People.Where(x => x.Name.Equals("Georgia")).Select(collectionStatement);

            Console.WriteLine("Collection Result:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(collectionResult.FirstOrDefault()));
            Console.WriteLine(LONG_LINE);

            //Correct as Ethan has no pets 
            //{"Id":"e36be116-e342-4df5-b6ca-f9274efa3eed","Name":null,"Age":0,"VetId":null,"Vet":null,"Pets":[]}
            var emptyCollectionResult = Database.People.Where(x => x.Name.Equals("Ethan")).Select(collectionStatement);

            Console.WriteLine("Empty Collection Result:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(emptyCollectionResult.FirstOrDefault()));
            Console.WriteLine(LONG_LINE);

            // ********************************************************************************
            // ************** Selecting one to one relationship object ****************
            // ********************************************************************************

            var relationshipSelect = "id,vetid,vet.id";
            var relationshipStatement = builder.BuildSelector<Person, Person>(relationshipSelect);

            //Console.WriteLine(Database.People.Include(p => p.Vet).Where(x => x.Name.Equals("Mark")).FirstOrDefault().Vet.Name);

            var workingRelationshipResult = Database.People.Where(x => x.Name.Equals("Mark")).Select(relationshipStatement);

            Console.WriteLine("Working Relationship Result:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(workingRelationshipResult.FirstOrDefault()));
            Console.WriteLine(LONG_LINE);
            
            // ********************************************************************************
            // ************** Selecting missing one to one relationship object ****************
            // ********************************************************************************

            //Console.WriteLine(Database.People.Include(p => p.Vet).Where(x => x.Name.Equals("Ethan")).FirstOrDefault().Vet.Name);
            //var workingRelationshipResult = Database.People.Where(x => x.Name.Equals("Ethan")).Select(relationshipStatement);
            
            var brokenRelationshipResult = Database.People.Where(x => x.Name.Equals("Ethan")).Select(relationshipStatement);
            
            //Produces the same error (Manually building the lambda)
            //var brokenRelationshipResult = Database.People.Where(x => x.Name.Equals("Ethan")).Select(x => new Person() { Id = x.Id, Vet = new Vet() { Id = x.Vet.Id } });

            //Fixes the issue - Adding a null check
            //var brokenRelationshipResult = Database.People.Where(x => x.Name.Equals("Ethan")).Select(x => new Person() { Id = x.Id, Vet = x.VetId == null ? null : new Vet() { Id = x.Vet.Id } });

            Console.WriteLine("Broken Relationship Result:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(brokenRelationshipResult.FirstOrDefault()));
            Console.WriteLine(LONG_LINE);

        }

        private async Task PopulateDatabase()
        {
            var people = Database.People.ToList();

            foreach (var person in people)
            {
                Console.WriteLine($"Found {person.Name}");
            }

            if (people.Count == 0)
            {
                var georgia = new Person
                {
                    Name = "Georgia",
                    Age = 32,
                };
                Database.Add(georgia);

                var mark = new Person
                {
                    Name = "Mark",
                    Age = 37,
                };
                Database.Add(mark);

                var ethan = new Person
                {
                    Name = "Ethan",
                    Age = 1,
                };
                Database.Add(ethan);

                await Database.SaveChangesAsync();

                var evie = new Pet()
                {
                    Name = "Evie",
                    OwnerId = georgia.Id
                };
                Database.Add(evie);

                var buddy = new Pet()
                {
                    Name = "Buddy",
                    OwnerId = georgia.Id
                };
                Database.Add(buddy);

                var ceri = new Pet()
                {
                    Name = "Ceri",
                    OwnerId = mark.Id
                };
                Database.Add(ceri);

                await Database.SaveChangesAsync();

                var vet = new Vet
                {
                    Name = "Cedarmount",
                    Address = "Bryansburn Road"
                };
                Database.Add(vet);

                await Database.SaveChangesAsync();

                mark.VetId = vet.Id;

                await Database.SaveChangesAsync();

                var newPeople = Database.People.ToList();

                foreach (var person in newPeople)
                {
                    Console.WriteLine($"Added {person.Name}");
                }
            }
        }
    }
}