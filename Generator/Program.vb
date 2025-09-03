Imports System
Imports System.IO
Imports System.Text
Imports Generator.Extensions

Module Program
    Const TotalRows As Long = 1_000_000_000
    Const MaxStations As Integer = 10_000
    Const OutputFile As String = "measurements.txt"

    Sub Main(args As String())
        Console.WriteLine($"Generating Station Names...")

        Dim stations = GenerateStationNames(MaxStations)

        Console.WriteLine($"Generated Stations: {MaxStations}")

        Console.WriteLine($"Generating {TotalRows} rows...")

        Using writer As New StreamWriter(OutputFile, False, Encoding.UTF8, 65536)
            For i As Long = 1 To TotalRows

                Dim row = GenerateRow(stations)

                writer.WriteLine(row)

                If i Mod 10000000 = 0 AndAlso i > 0 Then
                    Console.WriteLine($"Generated {i\1000000}M rows...")
                End If
            Next
        End Using

        Console.WriteLine("Done.")
    End Sub

    Private Function GenerateRow(stations As List(Of String)) As String
        Dim station = stations.RandomElement()
        Dim temp = Math.Round(Random.Shared.NextDouble()*199.8 - 99.9, 1)
        Return $"{station};{temp:F1}"
    End Function
End Module
