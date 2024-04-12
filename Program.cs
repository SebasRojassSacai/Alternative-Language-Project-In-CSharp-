using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Cell
{
    public string OEM { get; set; }
    public string Model { get; set; }
    public string LaunchAnnounced { get; set; }
    public string LaunchStatus { get; set; }
    public string BodyDimensions { get; set; }
    public float? BodyWeight { get; set; }
    public string BodySim { get; set; }
    public string DisplayType { get; set; }
    public float? DisplaySize { get; set; }
    public string DisplayResolution { get; set; }
    public string FeaturesSensors { get; set; }
    public string PlatformOS { get; set; }

    public Cell(string[] values)
    {
        if (values.Length < 12)
        {
            throw new ArgumentException("CSV row does not contain enough data.");
        }

        OEM = CleanData(values[0]);
        Model = CleanData(values[1]);
        LaunchAnnounced = CleanData(values[2]);
        LaunchStatus = CleanData(values[3]); // Keep as is for Discontinued or Cancelled
        BodyDimensions = CleanData(values[4]);
        BodyWeight = ParseBodyWeight(CleanData(values[5]));
        BodySim = CleanData(values[6]); // Assuming "No" and "Yes" are invalid
        DisplayType = CleanData(values[7]);
        DisplaySize = ParseNullableFloat(CleanData(values[8]));
        DisplayResolution = CleanData(values[9]);
        FeaturesSensors = CleanData(values[10]);
        PlatformOS = ParsePlatformOS(CleanData(values[11]));
    }

    public static bool VerifyValues(string[] values, int expectedCount)
    {
        if (values.Length < expectedCount)
        {
            Console.WriteLine("Row has insufficient columns. Data:");
            foreach (var value in values)
            {
                Console.WriteLine($"- {value}");
            }
            return false;
        }
        return true;
    }

    private string CleanData(string input)
    {
        return string.IsNullOrWhiteSpace(input) || input == "-" ? null : input.Trim();
    }

    private int? ParseNullableInt(string input)
    {
        if (int.TryParse(input, out int result))
            return result;
        return null;
    }

    private static float? ParseBodyWeight(string bodyWeight)
    {
        if (string.IsNullOrWhiteSpace(bodyWeight) || bodyWeight == "-")
            return null;

        // Expecting the format '190 g (6.70 oz)' and capturing the numeric value before the 'g'
        var match = Regex.Match(bodyWeight, @"(\d+)\s*g");
        if (match.Success && float.TryParse(match.Groups[1].Value, out float weight))
            return weight;
        return null;
    }

    private float? ParseNullableFloat(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input == "-")
            return null;

        var match = Regex.Match(input, @"\d+(\.\d+)?");
        if (match.Success && float.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            return result;

        return null;
    }

    private string ParsePlatformOS(string platformOS)
    {
        return platformOS?.Split(',')[0].Trim();
    }

    public override string ToString()
    {
        return $"OEM: {OEM ?? "Null"}, Model: {Model ?? "Null"}, Launch Announced: {LaunchAnnounced?.ToString() ?? "Null"}, " +
               $"Launch Status: {LaunchStatus ?? "Null"}, Body Dimensions: {BodyDimensions ?? "Null"}, " +
               $"Body Weight: {BodyWeight?.ToString(CultureInfo.InvariantCulture) ?? "Null"}, Body Sim: {BodySim ?? "Null"}, " +
               $"Display Type: {DisplayType ?? "Null"}, Display Size: {DisplaySize?.ToString(CultureInfo.InvariantCulture) ?? "Null"}, " +
               $"Display Resolution: {DisplayResolution ?? "Null"}, Features Sensors: {FeaturesSensors ?? "Null"}, " +
               $"Platform OS: {PlatformOS ?? "Null"}";
    }
}

class CellManager
{
    public List<Cell> Cells { get; private set; } = new List<Cell>();

    public void AddCell(Cell cell)
    {
        Cells.Add(cell);
    }

    public void LoadCellsFromCSV(string filePath)
    {
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine(); // Skip header
                int rowNumber = 1; // Start row number
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = ParseCsvRow(line).ToArray();
                    if (Cell.VerifyValues(values, 12)) // Verify the expected number of values
                    {
                        AddCell(new Cell(values));
                    }
                    else
                    {
                        Console.WriteLine($"Error on row {rowNumber}: Incorrect number of columns.");
                    }
                    rowNumber++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
        }
    }


    private List<string> ParseCsvRow(string line)
    {
        var values = new List<string>();
        var columns = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        foreach (var column in columns)
        {
            // Trim spaces only if the column is not wrapped in quotes
            var trimmedColumn = column.StartsWith("\"") && column.EndsWith("\"")
                ? column.Substring(1, column.Length - 2).Replace("\"\"", "\"")
                : column.Trim();
            values.Add(trimmedColumn);
        }

        return values;
    }

    public override string ToString()
    {
        return string.Join("\n", Cells.Select(cell => cell.ToString()));
    }
}

class Program
{
    static void Main(string[] args)
    {
        CellManager manager = new CellManager();
        manager.LoadCellsFromCSV("cells.csv");

        // Output the details of each cell
        Console.WriteLine(manager.ToString());
    }
}
