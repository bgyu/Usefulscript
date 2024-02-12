using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static bool CompareCsvFiles(string file1, string file2, double tolerance, Func<string, string, int>? sortFunction = null)
    {
        return CompareCsvFiles(file1, file2, tolerance, sortFunction, 0);
    }

    static bool CompareCsvFiles(string file1, string file2, double tolerance, Func<string, string, int>? sortFunction, int keyColumnIndex)
    {
        var lines1 = File.ReadAllLines(file1);
        var lines2 = File.ReadAllLines(file2);

        // Apply sorting if sortFunction is provided
        if (sortFunction != null)
        {
            Array.Sort(lines1, (a, b) =>
            {
                var columnA = a.Split(',')[keyColumnIndex].Trim();
                var columnB = b.Split(',')[keyColumnIndex].Trim();
                return sortFunction.Invoke(columnA, columnB);
            });

            Array.Sort(lines2, (a, b) =>
            {
                var columnA = a.Split(',')[keyColumnIndex].Trim();
                var columnB = b.Split(',')[keyColumnIndex].Trim();
                return sortFunction.Invoke(columnA, columnB);
            });
        }

        // Check if the number of lines in both files are equal
        if (lines1.Length != lines2.Length)
            return false;

        for (int i = 0; i < lines1.Length; i++)
        {
            var values1 = lines1[i].Split(',');
            var values2 = lines2[i].Split(',');

            // Check if the number of columns in both lines are equal
            if (values1.Length != values2.Length)
                return false;

            for (int j = 0; j < values1.Length; j++)
            {
                // If values are not equal
                if (values1[j].Trim() != values2[j].Trim())
                {
                    // If either value is not a number, consider them unequal
                    if (!IsNumeric(values1[j]) || !IsNumeric(values2[j]))
                        return false;

                    // If both values are numeric, compare with tolerance
                    double num1 = double.Parse(values1[j]);
                    double num2 = double.Parse(values2[j]);

                    if (Math.Abs(num1 - num2) > tolerance)
                        return false;
                }
            }
        }

        // If all comparisons pass, files are considered equal
        return true;
    }

    static bool IsNumeric(string value)
    {
        return double.TryParse(value, out _);
    }

    static void Main(string[] args)
    {
        // Example usage
        string file1 = "file1.csv";
        string file2 = "file2.csv";
        double tolerance = 0.0001; // Adjust tolerance as needed

        // Define custom sorting function
        Func<string, string, int> sortFunction = (a, b) =>
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase); // Sort alphabetically ignoring case
        };

        bool areEqual = CompareCsvFiles(file1, file2, tolerance, sortFunction);
        Console.WriteLine($"The two CSV files are {(areEqual ? "equal" : "not equal")}.");
    }
}
