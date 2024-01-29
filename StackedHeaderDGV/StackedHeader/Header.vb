Namespace StackedHeader
    Public Class Header

        Public Children As List(Of Header)

        Public Name As String

        Public X As Integer

        Public Y As Integer

        Public Width As Integer

        Public Height As Integer

        Public ColumnId As Integer

        Public Sub New()
            Name = String.Empty
            Children = New List(Of Header)
            ColumnId = -1
        End Sub

        Public Sub Measure(objGrid As DataGridView, iY As Integer, iHeight As Integer)
            Width = 0
            If Children.Count > 0 Then
                Dim tempY As Integer = If(String.IsNullOrEmpty(Name.Trim), iY, iY + iHeight)
                Dim columnWidthSet As Boolean = False
                For Each child As Header In Children
                    child.Measure(objGrid, tempY, iHeight)
                    Width += child.Width
                    If Not columnWidthSet AndAlso Width > 0 Then
                        ColumnId = child.ColumnId
                        columnWidthSet = True
                    End If
                Next
            ElseIf -1 <> ColumnId AndAlso objGrid.Columns(ColumnId).Visible Then
                Width = objGrid.Columns(ColumnId).Width
            End If

            Y = iY

            If Children.Count = 0 Then
                Height = objGrid.ColumnHeadersHeight - iY
            Else
                Height = iHeight
            End If
        End Sub

        Public Sub AcceptRenderer(objRenderer As StackedHeaderDecorator)
            For Each objChild As Header In Children
                objChild.AcceptRenderer(objRenderer)
            Next
            If -1 <> ColumnId AndAlso Not String.IsNullOrEmpty(Name.Trim()) Then
                objRenderer.Render(Me)
            End If
        End Sub

    End Class

End Namespace