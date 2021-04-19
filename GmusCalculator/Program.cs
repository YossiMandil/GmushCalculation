using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GmusCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Course> courses = new List<Course>
            {
                new Course("C++",212,CourseType.Qualification),
                new Course("C#",78,CourseType.Qualification),
                new Course("Evolutionary algorithm",14,CourseType.Qualification),
                new Course("Middle East",100,CourseType.Qualification),
                new Course("Project management",28,CourseType.Qualification),
                new Course("arrange and management",60,CourseType.Qualification),
                new Course("Negotiation",40,CourseType.Qualification),
                new Course("Genocide",100,CourseType.Enrichment),
                new Course("Grow with TV",100,CourseType.Enrichment),
            };
            var results = GetGmushOptions(courses, new Gmush("A", 200, 190), new Gmush("B", 200, 190));

            var ordersResults = results.OrderBy(x => x.Item3.Value);

            Console.WriteLine($"number of options: {results.Count}");

            int i = 0;
            foreach ((List<Course>, List<Course>, Course) item in ordersResults)
            {
                Console.WriteLine($"----------------------------------------------------{i++}---------------------------------");

                Console.WriteLine(CourseListToString("A",item.Item1));
                Console.WriteLine(CourseListToString("B",item.Item2));

                Console.WriteLine("Missing course:");
                Console.WriteLine(item.Item3);
            }
        }

        static string CourseListToString(string name, List<Course> courses)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{name}: ");
            foreach (var c in courses)
            {
                sb.Append($"{c.Name} | ");
            }

            sb.Append($"{Environment.NewLine}Sum Qualification: {courses.Sum(x => x.GetValue(CourseType.Qualification))} | " +
                  $"Sum Enrichment: {courses.Sum(x => x.GetValue(CourseType.Enrichment))}");

            return sb.ToString();
        }

        static List<(List<Course>, List<Course>, Course)> GetGmushOptions(List<Course> courses, Gmush gmushA, Gmush gmushB)
        {
            List<(List<Course>, List<Course>, Course)> results = new List<(List<Course>, List<Course>, Course)>();

            for (int i = 0; i < Math.Pow(2, courses.Count); i++)
            {
                List<Course> coursesA = new List<Course>();
                List<Course> coursesB = new List<Course>();
                for (int j = 0; j < courses.Count; j++)
                {
                    var bitI = (i >> j) & 1;

                    if (bitI == 0)
                        coursesB.Add(courses[j]);
                    else
                        coursesA.Add(courses[j]);
                }

                AddResult(results, coursesA, coursesB, gmushA, gmushB);
            }

            return results;
        }

        static void AddResult(List<(List<Course>, List<Course>, Course)> results, List<Course> coursesA, List<Course> coursesB, Gmush gmushA, Gmush gmushB)
        {
            var gmusARes = gmushA.GetMissing(coursesA);
            var gmusBRes = gmushB.GetMissing(coursesB);

            if (gmusARes.Value > 0 && gmusBRes.Value > 0)
                return;

            var missing = gmusARes.Value != 0 ? gmusARes : gmusBRes;
            results.Add((coursesA, coursesB, missing));
        }

        //static void AddResult(List<(HashSet<Course>, HashSet<Course>, Course)> results, HashSet<Course> coursesA, HashSet<Course> coursesB, Gmush gmushA, Gmush gmushB)
        //{
        //    results.Add((coursesA.Select(x => x).ToHashSet(), coursesB.Select(x => x).ToHashSet(), new Course("dd", 1, CourseType.Enrichment)));
        //}


    }

    internal class Course
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public CourseType Type { get; set; }

        public Course(string name, int value, CourseType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Value: {Value}, Type: {Type}";
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public int GetValue(CourseType type) => Type == type ? Value : 0;
    }

    internal enum CourseType
    {
        Qualification,
        Enrichment
    }

    internal class Gmush
    {
        public string Name { get; set; }
        public int MinQualification { get; set; }
        public int MinEnrichment { get; set; }

        public int TotalPoints { get; }
        public Gmush(string name, int minQualification, int minEnrichment)
        {
            Name = name;
            MinQualification = minQualification;
            MinEnrichment = minEnrichment;

            TotalPoints = minEnrichment + minQualification;
        }

        public Course GetMissing(List<Course> courses)
        {
            var parallelGmushes = courses.AsParallel();
            var missingQualification = MinQualification - parallelGmushes.Sum(x => x.Type == CourseType.Qualification ? x.Value : 0);
            var totalMissing = TotalPoints - parallelGmushes.Sum(x => x.Value);

            if (missingQualification <= 0 && totalMissing <= 0)
                return new Course("Non-Missing", 0, CourseType.Enrichment);

            var totalMissingWithoutQualification = totalMissing - missingQualification;

            if (totalMissing > 0)
            {
                return missingQualification > 0 ?
                    new Course("Missing", missingQualification + (totalMissingWithoutQualification > 0 ? totalMissingWithoutQualification : 0), CourseType.Qualification) :
                    new Course("Missing", totalMissing, CourseType.Enrichment);
            }

            return new Course("Missing", missingQualification, CourseType.Qualification);
        }

    }
}
