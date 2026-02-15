Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography

Namespace Extensions
    Public Module ListExtensions

        <Extension>
        Public Function RandomElement(Of T)(source As T()) As T
            ArgumentNullException.ThrowIfNull(source)

            If source.Length = 0 Then
                Throw New InvalidOperationException("Cannot select a random element from an empty list")
            End If

            Dim randomIndex As Integer = RandomNumberGenerator.GetInt32(0, source.Length)

            Return source(randomIndex)
        End Function
    End Module
End Namespace
