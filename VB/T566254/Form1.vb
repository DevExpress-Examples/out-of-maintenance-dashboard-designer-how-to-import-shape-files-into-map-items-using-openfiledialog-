Imports DevExpress.DashboardCommon
Imports DevExpress.DashboardWin
Imports DevExpress.DashboardWin.Bars
Imports DevExpress.XtraBars
Imports System
Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms

Namespace T566254
	Partial Public Class Form1
		Inherits Form

		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
			dashboardDesigner1.CreateRibbon()
			dashboardDesigner1.Dashboard.Items.Add(New GeoPointMapDashboardItem())
		End Sub

		Private Sub dashboardDesigner1_PopupMenuShowing(ByVal sender As Object, ByVal e As DashboardPopupMenuShowingEventArgs) Handles dashboardDesigner1.PopupMenuShowing
			Dim designer = DirectCast(sender, DashboardDesigner)
			Dim mapItem = TryCast(designer.SelectedDashboardItem, MapDashboardItem)

			'Load shapes using File Path...
			If e.DashboardItemArea = DashboardItemArea.DashboardItem AndAlso mapItem IsNot Nothing Then
				For i As Integer = e.Menu.ItemLinks.Count - 1 To 0 Step -1
					If TypeOf e.Menu.ItemLinks(i).Item Is MapImportBarItem OrElse TypeOf e.Menu.ItemLinks(i).Item Is MapLoadBarItem Then
						e.Menu.ItemLinks.RemoveAt(i)
					End If
				Next i
				Dim bbi1 As New BarButtonItem() With {.Caption = "Load Custom Map", .Name = "mapCustomShapeFile"}
				AddHandler bbi1.ItemClick, Sub(s, args)
					Dim dialog As New OpenFileDialog()
					dialog.InitialDirectory = Path.Combine(Application.StartupPath, "ShapeFiles")
					dialog.Filter = "Shape files|*.shp"
					If dialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
						Try
							mapItem.CustomShapefile.Url = dialog.FileName
							mapItem.Area = ShapefileArea.Custom
						Catch ex As Exception
							MessageBox.Show("Error: Could not read file from disk. Original error: " & ex.Message)
						End Try
					End If
				End Sub

				'Load shapes using Stream...
				Dim bbi2 As New BarButtonItem() With {.Caption = "Load Custom Map (Stream approach)", .Name = "mapCustomShapeFile"}
				AddHandler bbi2.ItemClick, Sub(s, args)
					Dim dialog As New OpenFileDialog()
					dialog.InitialDirectory = Path.Combine(Application.StartupPath, "ShapeFiles")
					dialog.Filter = "Shape files|*.shp"
					If dialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
						Try
						   Dim fileName = Path.Combine(System.IO.Path.GetDirectoryName(dialog.FileName), Path.GetFileNameWithoutExtension(dialog.FileName))
							Dim shapeData() As Byte = File.ReadAllBytes(String.Format("{0}.{1}", fileName, "shp"))
							Dim attributeData() As Byte = File.ReadAllBytes(String.Format("{0}.{1}", fileName, "dbf"))
							mapItem.CustomShapefile.Data = New CustomShapefileData(shapeData, attributeData)
							mapItem.Area = ShapefileArea.Custom
						Catch ex As Exception
							MessageBox.Show("Error: Could not read file from disk. Original error: " & ex.Message)
						End Try
					End If
				End Sub

				If e.Menu.ItemLinks.OfType(Of BarButtonItemLink)().Where(Function(i) i.Item.Name = "mapCustomShapeFile").Count() = 0 Then
					Dim mapDefaultShapefile = e.Menu.ItemLinks.FirstOrDefault(Function(l) TypeOf l.Item Is MapFullExtentBarItem)
					e.Menu.ItemLinks.Insert(mapDefaultShapefile, bbi1)
					e.Menu.ItemLinks.Insert(mapDefaultShapefile, bbi2)
				End If
			End If
		End Sub
	End Class
End Namespace
