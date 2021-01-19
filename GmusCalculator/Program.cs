using System;
using System.Collections.Generic;
using System.Linq;

namespace GmusCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Course> courses = new List<Course>
            {
                new Course("c++",212,CourseType.Qualification),
                new Course("c#",72,CourseType.Qualification),
                new Course("A.A",14,CourseType.Qualification),
                new Course("Middle East",100,CourseType.Qualification),
                new Course("arrange and mannage",60,CourseType.Enrichment),
                new Course("Negotiation",40,CourseType.Enrichment),
                new Course("Jenoside",100,CourseType.Enrichment),
                new Course("Born with TV",100,CourseType.Enrichment),
                new Course("Projects",40,CourseType.Enrichment),
            };
            var results = GetGmushOptions(courses, new Gmush("A", 200, 200), new Gmush("B", 200, 200));

            var ordersResults = results.OrderBy(x => x.Item3.Value);

            Console.WriteLine($"number of options: {results.Count}");

            int i = 0;
            foreach ((List<Course>, List<Course>, Course) item in ordersResults)
            {
                Console.WriteLine($"----------------------------------------------------{i++}---------------------------------");

                Console.Write("A: ");
                foreach (var c in item.Item1)
                {
                    Console.Write($"{c.Name} | ");
                }

                Console.Write($"{Environment.NewLine}B: ");
                foreach (var c in item.Item2)
                {
                    Console.Write($"{c.Name} | ");
                }

                Console.Write($"{Environment.NewLine}course:");
                Console.WriteLine(item.Item3);
            }
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

            if (totalMissing > 0)
            {
                return missingQualification > 0 ? 
                    new Course("Missing", totalMissing, CourseType.Qualification) : 
                    new Course("Missing", totalMissing, CourseType.Enrichment);
            }

            return new Course("Missing", missingQualification, CourseType.Qualification);
        }

    }
}
