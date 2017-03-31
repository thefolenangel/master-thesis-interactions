using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetGenerator {
    public class AttemptRepository : DbContext {

        static AttemptRepository attemptRepository;
        public  static DatabaseSaveStatus SaveStatus { get; set; }

        static AttemptRepository Repository {
            get {
                if(attemptRepository == null) {
                    attemptRepository = new AttemptRepository();
                }
                return attemptRepository;
            }
        }

        public AttemptRepository() : base("SW9_Project") { }
        public DbSet<Attempt> Attempts { get; set; }

        public static void SaveOldTestToDatabase() {

            List<Test> tests = DataGenerator.GetTests(DataSource.Old);

            foreach (var test in tests) {
                SaveTest(test); 
            }
        }

        public static List<Attempt> GetAttempts(DataSource source) {
            List<Attempt> attempts = null;
            lock (Repository) {
                attempts = Repository.Attempts
                .Where(x => x.Source == source).ToList();
            }
            return attempts;
        }

        public static int GetTestCount(DataSource source) {

            int count = 0;
            lock (Repository) {
                count = (from attempt in Repository.Attempts
                         where attempt.Source == source
                         group attempt by attempt.ID into testsFound
                         select testsFound).Count();
            }
            return count;

        }

        public static Test GetTest(string id, DataSource source) {
            List<Attempt> attempts = null;
            lock(Repository) {
                attempts = Repository.Attempts
                    .Where(x => x.Source == source && x.ID == id)
                    .ToList();
            }

            return new Test(attempts);
        }

        public static List<Test> GetTests(DataSource source) {

            List<Test> tests = new List<Test>();

            lock (Repository) {
                var allTests = Repository.Attempts
                    .Where(attempt => attempt.Source == source)
                    .GroupBy(attempt => attempt.ID, attempt => attempt);

                foreach (var testgrouping in allTests) {
                    tests.Add(new Test(testgrouping.ToList()));
                }
            }
            
            return tests;
        }

        private static void SaveTest(Test test) {
            lock (Repository) {
                SaveStatus = DatabaseSaveStatus.Saving;
                DataSource source = test.Attempts.First().Value.First().Source;
                bool success = false;
                try {
                    Console.WriteLine($"Searching for {test.ID} in database...");
                    var testFound = Repository.Attempts.Where(z => z.ID == test.ID && z.Source == source).Count() > 0;
                    if (testFound) {
                        Console.WriteLine($"Test ID {test.ID} from {source} data source already exists in database");
                        SaveStatus = DatabaseSaveStatus.Failed;
                        return;
                    }
                    Console.WriteLine($"Test ID {test.ID} not found, saving...");
                    foreach (var technique in DataGenerator.AllTechniques) {
                        Repository.Attempts.AddRange(test.Attempts[technique]);
                    }

                    Repository.SaveChanges();
                    success = true;
                    Console.WriteLine($"Successfully saved test number {test.ID} to database");
                }
                catch (Exception e) {
                    Console.WriteLine("Failed saving to database");
                    Console.WriteLine("Message: " + e.Message);
                }
                SaveStatus = success ? DatabaseSaveStatus.Success : DatabaseSaveStatus.Failed;
            }
        }

        public static void SaveTestToDatabase(Test test) {
            Task.Factory.StartNew(() => {
                SaveTest(test);
            });
        }
    }
}
