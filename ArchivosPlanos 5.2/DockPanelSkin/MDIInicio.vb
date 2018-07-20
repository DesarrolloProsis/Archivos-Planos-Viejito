Imports System.Windows.Forms
Imports System.IO

Public Class MDIInicio

    'Dim str_FullPath As String = "D:\PROSIS\MEXICO-ACAPULCO\DESARROLLO_REPORTES\Leer archivo\R_21_COMMENT.txt"

    'Dim str_FullPath As String = "C:\GEAINT\TPC\TMP\R_21_COMMENT.txt"
    'C:\GEAINT\TPC\TMP

    Dim str_FullPath As String = objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "archivo")

    Dim dt_fecha_modificacion As Date
    Dim gb_segundos As Integer
    Dim ller_video As Boolean
    Dim fecha_enviada As Date
    Dim bl_pausa As Boolean = False

    Private Sub ShowNewForm(ByVal sender As Object, ByVal e As EventArgs)
        ' Cree una nueva instancia del formulario secundario.
        Dim ChildForm As New System.Windows.Forms.Form
        ' Conviértalo en un elemento secundario de este formulario MDI antes de mostrarlo.
        ChildForm.MdiParent = Me

        m_ChildFormNumber += 1
        ChildForm.Text = "Ventana " & m_ChildFormNumber

        ChildForm.Show()
    End Sub

    Private Sub OpenFile(ByVal sender As Object, ByVal e As EventArgs)
        Dim OpenFileDialog As New OpenFileDialog
        OpenFileDialog.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        OpenFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*"
        If (OpenFileDialog.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK) Then
            Dim FileName As String = OpenFileDialog.FileName
            ' TODO: agregue código aquí para abrir el archivo.
        End If
    End Sub

    Private Sub SaveAsToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim SaveFileDialog As New SaveFileDialog
        SaveFileDialog.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        SaveFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*"

        If (SaveFileDialog.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK) Then
            Dim FileName As String = SaveFileDialog.FileName
            ' TODO: agregue código aquí para guardar el contenido actual del formulario en un archivo.
        End If
    End Sub

    Private Sub ExitToolsStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.Close()
    End Sub

    Private Sub CutToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Utilice My.Computer.Clipboard para insertar el texto o las imágenes seleccionadas en el Portapapeles
    End Sub

    Private Sub CopyToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Utilice My.Computer.Clipboard para insertar el texto o las imágenes seleccionadas en el Portapapeles
    End Sub

    Private Sub PasteToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        'Utilice My.Computer.Clipboard.GetText() o My.Computer.Clipboard.GetData para recuperar la información del Portapapeles.
    End Sub

    Private Sub CascadeToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.Cascade)
    End Sub

    Private Sub TileVerticalToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.TileVertical)
    End Sub

    Private Sub TileHorizontalToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.TileHorizontal)
    End Sub

    Private Sub ArrangeIconsToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.ArrangeIcons)
    End Sub

    Private Sub CloseAllToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Cierre todos los formularios secundarios del principal.
        For Each ChildForm As Form In Me.MdiChildren
            ChildForm.Close()
        Next
    End Sub

    Private m_ChildFormNumber As Integer

    Private Sub MDIInicio_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        gl_DNS = "Data Source=(DESCRIPTION=" _
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.15.16)(PORT=1521)))" _
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
+ "User Id=GEADBA;Password=FGEUORJVNE;"


        gl_DNS = "Data Source=(DESCRIPTION=" _
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.1.20.100)(PORT=1521)))" _
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
+ "User Id=GEADBA;Password=FGEUORJVNE;"

        gl_DNS = "Data Source=(DESCRIPTION=" _
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.1.20.100)(PORT=1521)))" _
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
+ "User Id=GEADBA;Password=FGEUORJVNE;"

        'NUEVA VERSION GEA OF
        '        gl_DNS = "Data Source=(DESCRIPTION=" _
        '+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.218)(PORT=1521)))" _
        '+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
        '+ "User Id=GEADBA;Password=FGEUORJVNE;"

        'NUEVA VERSION GEA OF
        gl_DNS = "Data Source=(DESCRIPTION=" _
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=prosis.sytes.net)(PORT=1521)))" _
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
+ "User Id=GEADBA;Password=FGEUORJVNE;"


        '        '27 05 2013
        gl_DNS = "Data Source=(DESCRIPTION=" _
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=D)(PORT=1521)))" _
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
+ "User Id=GEAINT;Password=GEAINT;"

        '        gl_DNS = "Data Source=(DESCRIPTION=" _
        '+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=prosis.sytes.net)(PORT=1521)))" _
        '+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
        '+ "User Id=GEADBA;Password=FGEUORJVNE;"

        '        gl_DNS = "Data Source=(DESCRIPTION=" _
        '+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=189.249.90.181)(PORT=1521)))" _
        '+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
        '+ "User Id=GEADBA;Password=FGEUORJVNE;"

        '        gl_DNS = "Data Source=(DESCRIPTION=" _
        '+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=189.228.208.13)(PORT=1521)))" _
        '+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
        '+ "User Id=GEADBA;Password=FGEUORJVNE;"

        ''        gl_DNS = "Data Source=(DESCRIPTION=" _
        ''+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.213)(PORT=1521)))" _
        ''+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
        ''+ "User Id=GEAINT;Password=GEAINT;"

        '192.168.0.253

        'caseta()
        '        gl_DNS = "Data Source=(DESCRIPTION=" _
        '+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.1.184.200)(PORT=1521)))" _
        '+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
        '        + "User Id=GEAINT;Password=GEAINT;"


        gl_DNS = "Data Source=(DESCRIPTION=" _
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" & objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "ip") & ")(PORT=1521)))" _
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));" _
+ "User Id=GEAINT;Password=GEAINT;"

        '192.168.0.213

        Application.CurrentCulture = New System.Globalization.CultureInfo("en-US")


    End Sub

    Private Sub rpt4_Click(sender As Object, e As EventArgs) Handles rpt4.Click

        objControl.limpia_Catalogos()

        Dim frmR1 As New frmR1
        frmR1.Tag = rpt4.Tag
        strLinea = rpt4.Text
        frmR1.MdiParent = Me
        frmR1.Show()

    End Sub

    Private Sub FileMenu_Click(sender As Object, e As EventArgs) Handles FileMenu.Click
        End
    End Sub

    'Private Sub PreliquidacionesDeCajeroreceptorParaTránsitoVehicularToolStripMenuItem_Click(sender As Object, e As EventArgs)

    '    objControl.limpia_Catalogos()

    '    Dim frmR1 As New frmR1
    '    frmR1.Tag = rpt5.Tag
    '    strLinea = rpt5.Text
    '    frmR1.MdiParent = Me
    '    frmR1.Show()

    'End Sub

    'Private Sub rpt6_Click(sender As Object, e As EventArgs)


    '    objControl.limpia_Catalogos()

    '    Dim frmR1 As New frmR1
    '    frmR1.Tag = rpt6.Tag
    '    strLinea = rpt6.Text
    '    frmR1.MdiParent = Me
    '    frmR1.Show()

    'End Sub
    'click al control de versiones

    Private Sub V100ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles V100ToolStripMenuItem.Click
        MsgBox("Cambios version 4.0.2" & vbCrLf &
               "-Se habilito la encriptacion" & vbCrLf &
               "-Se acomodaron los archivos planos en orden de fecha" & vbCrLf &
               "-Se habilitan validaciones de bolsa, comentarios, carriles abiertos" & vbCrLf &
               "-Se agrega barra de progreso" & vbCrLf & "-Se cambia diseño de la interfaz" & vbCrLf &
               "Cambios version 4.0.3" & vbCrLf &
               "-Libreria HASH Y encriptacion CAPUFE" & vbCrLf &
               "Cambios version 4.0.5" & vbCrLf &
               "-Se abrieron todos los carriles de tlalpan" & vbCrLf &
               "Cambios version 4.0.6" & vbCrLf &
               "-Mensaje de basura de TAG" & vbCrLf &
               "-Correcion de Folios" & vbCrLf &
               "-Archivos comprimidos y de encriptación" & vbCrLf &
               "-Correcion en la validaciones de bolsa, comentario y carril cerrado" & vbCrLf &
               "-Correcion en la validacion del primer turno, primer dia del mes",
               "-Cambios version 5.1" & vbCrLf &
               "-Correcion de validaciones de carriles cerrados se agregan carriles de queretaro y cerro gordo" & vbCrLf &
               "-Contiene las cuatro plazas queretaro" &
                vbInformation, "Version 5.1")
    End Sub

    Private Sub rpt1_Click(sender As Object, e As EventArgs) Handles rpt1.Click
        FRM1.MdiParent = Me
        FRM1.Show()
    End Sub

    Private Sub HelpMenu_Click(sender As Object, e As EventArgs) Handles HelpMenu.Click

    End Sub
End Class
