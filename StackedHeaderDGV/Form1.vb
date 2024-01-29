Imports StackedHeaderDGV.StackedHeader

Public Class Form1

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim objREnderer As New StackedHeaderDecorator(DataGridView1)

    End Sub

End Class
