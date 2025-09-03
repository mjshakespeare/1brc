Imports Generator.Extensions
Imports ShowsOnSale.World

Public Module StationNameGenerator
    public Function GenerateStationNames(count as Integer) As List(Of String)
        Dim stations = New HashSet(Of String)()

        Dim i = 0
        While i < count
            Dim cityName = SelectCityName()

            If stations.Add(cityName) Then
                i = i + 1
            End If
        End While

        Return stations.ToList()
    End Function

    Private Function SelectCityName() As String
        Dim country = WorldData.All.RandomElement()

        If country.States.Count = 0 Then
            return country.Capital
        End If

        Dim state = country.States.RandomElement()

        If state.Cities.Count = 0 Then
            Return country.Capital
        End If

        Return state.Cities.RandomElement().Name
    End Function
End Module
