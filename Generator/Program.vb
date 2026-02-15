Imports System.CommandLine
Imports System.IO
Imports System.Text
Imports System.Threading
Imports Generator.Extensions

Module Program
    Const MaxStations As Integer = 10_000
    Const BatchSize As Integer = 100_000

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
        Console.WriteLine($"Output: {outputFile}, Rows: {totalRows}")
        Console.WriteLine($"Generating Station Names...")

        Dim stations = GenerateStationNames(MaxStations)

        Console.WriteLine($"Generated Stations: {MaxStations}")

        Dim batches As Integer() = GenerateBatches(totalRows, BatchSize)

        Console.WriteLine("Producing File")

        Dim temperatures = Enumerable.Range(-999, 1999).Select(Function(t) (t / 10D).ToString("N1")).ToArray()

        Dim lock = New Lock
        Dim rowsWritten As Long = 0
        Dim tenPercent As Long = totalRows \ 10

        Using fileStream As New FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, 1_048_576, FileOptions.SequentialScan)
            Using writer As New StreamWriter(fileStream, Encoding.UTF8, 1_048_576)
                Parallel.ForEach(Of Integer, StringBuilder)(
                    batches,
                    Function() New StringBuilder(2_000_000),
                    Function(batchSize, loopState, sb)
                        sb.Clear()
                        For i = 1 To batchSize
                            Dim station = stations.RandomElement()
                            Dim temp As Single = Random.Shared.Next(-999, 1000) \ 10.0F
                            sb.Append(station)
                            sb.Append(";"c)
                            sb.Append(temperatures.RandomElement)
                            sb.Append(ChrW(10))
                        Next

                        lock.Enter()
                        Try
                            writer.Write(sb)
                        Finally
                            lock.Exit()
                        End Try

                        Dim previous = Interlocked.Add(rowsWritten, batchSize) - batchSize
                        If tenPercent > 0 AndAlso previous \ tenPercent <> (previous + batchSize) \ tenPercent Then
                            Console.WriteLine($"Progress: {(previous + batchSize) * 100 \ totalRows}%")
                        End If

                        Return sb
                    End Function,
                    Sub(sb)
                        sb.Clear()
                    End Sub)
            End Using
        End Using
        Console.WriteLine($"Total Rows Written: {rowsWritten}")
        Console.WriteLine("Done.")
    End Sub

    Private Function GenerateBatches(TotalRows As Long, BatchSize As Integer) As Integer()
        Dim batchCount As Integer = Math.Ceiling(TotalRows \ BatchSize)

        Dim batches As Integer() = Enumerable.Repeat(BatchSize, batchCount).ToArray()

        batches(batches.Length - 1) += TotalRows Mod BatchSize

        Return batches
    End Function
End Module
