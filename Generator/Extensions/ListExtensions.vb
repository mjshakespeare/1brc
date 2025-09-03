Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module ListExtensions

        <Extension()>
        Public Function RandomElement(Of T)(source As IList(Of T)) As T
            ArgumentNullException.ThrowIfNull(source)

            If source.Count = 0 Then
                Throw New InvalidOperationException("Cannot select a random element from an empty list")
            End If

            Dim randomIndex As Integer = Random.Shared.Next(source.Count)

            Return source(randomIndex)
        End Function
    End Module
End NameSpace
