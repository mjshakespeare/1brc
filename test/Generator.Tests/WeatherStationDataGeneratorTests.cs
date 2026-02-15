using System.Text;
using Microsoft.Extensions.Logging.Abstractions;

namespace Generator.Tests;

public static class WeatherStationDataGeneratorTests
{
    private static readonly string[] TestStations = ["London", "Paris", "Tokyo", "New York", "Sydney"];

    private static WeatherStationDataGenerator CreateGenerator(
        string[]? stations = null)
    {
        return new WeatherStationDataGenerator(
            stations ?? TestStations,
            NullLogger<WeatherStationDataGenerator>.Instance);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(500)]
    [InlineData(100_000)]
    [InlineData(500_000)]
    [InlineData(1_000_000)]
    public static void Generate_WritesExpectedRowCount_ForVariousSizes(
        long rows)
    {
        // Arrange
        var generator
            = CreateGenerator();

        // Act
        var lines
            = GenerateLines(
                generator,
                rows);

        // Assert
        Assert.Equal(rows, lines.Length);
    }

    [Fact]
    public static void Generate_EachRowHasCorrectFormat()
    {
        // Arrange
        var stations = new[] { "Berlin"};
        var generator = CreateGenerator(stations);

        // Act
        var lines = GenerateLines(generator);

        // Assert
        foreach (var line in lines)
        {
            var parts = line.Split(';');
            Assert.Equal(2, parts.Length);

            var stationName = parts[0];
            var temperature = parts[1];

            Assert.False(string.IsNullOrWhiteSpace(stationName), "Station name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(temperature), "Temperature should not be empty");

            Assert.Equal(stations[0], stationName);

            Assert.Contains('.', temperature);

            var decimalPlaces = temperature.Split('.')[1].Length;

            Assert.Equal(1, decimalPlaces);

            Assert.True(double.TryParse(temperature, out _), "Temperature should be a valid number");
            Assert.InRange(double.Parse(temperature), -99.9D, 99.9D);
        }
    }

    private static string[] GenerateLines(
        WeatherStationDataGenerator generator,
        long lineCount = 500)
    {
        using var stream
            = PopulateStream(
                generator,
                lineCount);

        return ReadLines(
            stream);
    }

    private static MemoryStream PopulateStream(
        WeatherStationDataGenerator generator,
        long lineCount)
    {
        var stream
            = new MemoryStream();

        using var writer
            = new StreamWriter(
                stream,
                Encoding.UTF8,
                leaveOpen: true);

        generator
            .Generate(
                writer,
                lineCount);

        writer.Flush();
        stream.Position = 0;

        return stream;
    }

    private static string[] ReadLines(
        MemoryStream stream)
    {
        using var reader
            = new StreamReader(
                stream);

        var content
            = reader.ReadToEnd();

        return content
            .Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries);
    }
}
