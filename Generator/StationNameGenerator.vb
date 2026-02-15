Imports System.IO
Imports System.Security.Cryptography

Public Module StationNameGenerator
    public Function GenerateStationNames(count as Integer) As String()
        dim worldCities = File.ReadLines("worldcities.csv").ToArray()
        dim stations = new String(count - 1) {}

        For i = 0 To stations.Length - 1
            stations(i) = SelectStation(worldCities)
        Next

        Return stations
    End Function

    Private Function SelectStation(lines As String()) As String
        Dim selectedStation = RandomNumberGenerator.GetInt32(0, lines.Length)

        Return lines(selectedStation).Split(","c)(0).Trim(""""c)
    End Function
End Module
