Imports System.IO
Imports System.Text
Imports System.Threading
Imports Generator.Extensions
Imports Microsoft.Extensions.Logging

Partial Public NotInheritable Class WeatherStationDataGenerator
    Const MaxBatchSize As Integer = 100_000

    Private ReadOnly _logger As ILogger(Of WeatherStationDataGenerator)
    Private ReadOnly _temperatureReadings As String()
    Private ReadOnly _stationNames As String()

    Public Sub New(stationNames As String(), logger As ILogger(Of WeatherStationDataGenerator))
        _logger = logger
        _temperatureReadings = Enumerable.Range(-999, 1999).Select(Function(t) (t / 10D).ToString("N1")).ToArray()
        _stationNames = stationNames
    End Sub

    Public Sub Generate(streamWriter As StreamWriter, rows As Long)
        Dim batches As Integer() = GenerateBatchSizes(rows, MaxBatchSize)
        Dim lock = New Lock

        _logger.LogInformation("Starting generation of {totalRows:N0} rows", rows)

        Dim tenPercent As Long = rows \ 10
        Dim totalRowsWritten As Long = 0
        Dim rowsWrittenAtLastReport As Long = 0

        Parallel.ForEach(
                    batches,
                    Function() New StringBuilder(2_000_000),
                    Function(batchSize, loopState, sb)
                        sb.Clear()

                        GenerateBatch(
                            sb,
                            batchSize)

                        WriteBatch(
                            streamWriter,
                            lock,
                            sb)

                        Dim rowsWrittenOnReport = Interlocked.Add(rowsWrittenAtLastReport, batchSize)
                        Dim totalRowsWrittenOnReport = Interlocked.Add(totalRowsWritten, batchSize)

                        If rowsWrittenOnReport >= tenPercent Then
                            LogProgress(totalRowsWrittenOnReport, rows)
                            Interlocked.Add(rowsWrittenAtLastReport, -rowsWrittenOnReport)
                        End If

                        Return sb
                    End Function,
                    Sub(sb)
                        sb.Clear()
                    End Sub)

        _logger.LogInformation("Generation Completed")
    End Sub

    Private Sub GenerateBatch(sb As StringBuilder, batchSize As Integer)
        For i = 1 To batchSize
            GenerateRow(sb)
        Next
    End Sub

    Private Shared Sub WriteBatch(streamWriter As StreamWriter, lock As Lock, sb As StringBuilder)
        lock.Enter()
        Try
            streamWriter.Write(sb)
        Finally
            lock.Exit()
        End Try
    End Sub

    Private Shared Function GenerateBatchSizes(totalRows As Long, batchSize As Integer) As Integer()
        Dim batchCount As Integer = Math.Ceiling(totalRows / batchSize)

        Dim batches As Integer() = Enumerable.Repeat(batchSize, batchCount).ToArray()

        batches(batches.Length - 1) += totalRows - (batchCount * batchSize)

        Return batches
    End Function

    Private Sub GenerateRow(sb As StringBuilder)
        Dim station = _stationNames.RandomElement()
        Dim temperature = _temperatureReadings.RandomElement()
        GenerateRow(sb, station, temperature)
    End Sub

    Private Shared Sub GenerateRow(sb As StringBuilder, station As String, temperature As String)
        sb.Append(station)
        sb.Append(";"c)
        sb.Append(temperature)
        sb.Append(ChrW(10))
    End Sub

    Private Sub LogProgress(rowsWritten As Long, totalRows As Long)
        _logger.LogInformation("Progress: {rowsWritten:N0}\{totalRows:N0} rows)", rowsWritten, totalRows)
    End Sub

End Class
