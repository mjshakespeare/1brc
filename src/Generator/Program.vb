Imports System.CommandLine
Imports System.IO
Imports System.Text
Imports Microsoft.Extensions.Logging

Module Program
    Const MaxStations As Integer = 10_000

    Function Main(args As String()) As Integer
        Dim outputOption As New [Option](Of String)("--output", "-o") With {
            .DefaultValueFactory = Function(unused) "measurements.txt",
            .Description = "The output file name.",
            .Required = False
        }

        Dim rowsOption As New [Option](Of Long)("--rows", "-r") With {
            .DefaultValueFactory = Function(unused) 1_000_000_000L,
            .Description = "The number of rows to generate."
        }

        Dim rootCommand As New RootCommand("1BRC measurement file generator.")
        rootCommand.Options.Add(outputOption)
        rootCommand.Options.Add(rowsOption)
        rootCommand.SetAction(Sub(parseResult As ParseResult)
                                  Generate(parseResult.GetValue(outputOption), parseResult.GetValue(rowsOption))
                              End Sub)

        Return rootCommand.Parse(args).Invoke()
    End Function

    Private Sub Generate(outputFile As String, totalRows As Long)
        Dim logFactory = LoggerFactory.Create(Sub(builder) builder.AddSimpleConsole().AddConsole())

        Dim stations = GenerateStationNames(MaxStations)
        Dim weatherStationDataGenerator = New WeatherStationDataGenerator(stations, logFactory.CreateLogger(Of WeatherStationDataGenerator)())

        Using fileStream As New FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, 1_048_576)
            Using writer As New StreamWriter(fileStream, Encoding.UTF8, 1_048_576)
                weatherStationDataGenerator.Generate(writer, totalRows)
            End Using
        End Using
    End Sub
End Module
