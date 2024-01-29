Namespace StackedHeader
    Public Class StackedHeaderGenerator
        Inherits IStackedHeaderGenerator

        Private Shared objInstance As StackedHeaderGenerator

        ' Constructor is 'protected'
        Private Sub New()
        End Sub

        Public Shared Function Instance() As StackedHeaderGenerator
            ' Uses lazy initialization.
            ' Note: this is not thread safe.
            If objInstance Is Nothing Then
                objInstance = New StackedHeaderGenerator()
            End If
            Return objInstance
        End Function

        Public Function GenerateStackedHeader(objGridView As DataGridView) As Header
            Dim objParentHeader As Header = New Header()
            Dim objHeaderTree As Dictionary(Of String, Header) = New Dictionary(Of String, Header)
            Dim iX As Integer = 0
            For Each objColumn As DataGridViewColumn In objGridView.Columns
                Dim segments() As String = objColumn.HeaderText.Split(".")
                If segments.Count > 0 Then
                    Dim segment As String = segments(0)
                    Dim tempHeader As Header = Nothing
                    Dim lastTempHeader As Header = Nothing
                    If objHeaderTree.ContainsKey(segment) Then
                        tempHeader = objHeaderTree(segment)
                    Else
                        tempHeader = New Header With {.Name = segment, .X = iX}
                        objParentHeader.Children.Add(tempHeader)
                        objHeaderTree(segment) = tempHeader
                        tempHeader.ColumnId = objColumn.Index
                    End If
                    For i As Integer = 0 To UBound(segments)
                        segment = segments(i)
                        Dim found As Boolean = False
                        For Each child As Header In tempHeader.Children
                            If 0 = String.Compare(child.Name, segment, StringComparison.InvariantCultureIgnoreCase) Then
                                found = True
                                lastTempHeader = tempHeader
                                tempHeader = child
                                Exit For
                            End If
                        Next
                        If Not found OrElse i = segments.Length - 1 Then
                            Dim temp As Header = New Header With {.Name = segment, .X = iX}
                            temp.ColumnId = objColumn.Index
                            If found AndAlso i = segments.Length - 1 AndAlso Nothing IsNot lastTempHeader Then
                                lastTempHeader.Children.Add(temp)
                            Else
                                tempHeader.Children.Add(temp)
                            End If
                            tempHeader = temp
                        End If
                    Next
                End If
                iX += objColumn.Width
            Next
            Return objParentHeader
        End Function

    End Class

End Namespace
