Imports Ionic.Zip
Imports System.IO
Imports System.IO.Path

Public Class FRM1

    Dim sLine2 As Integer
    Dim indi As String

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        Dim ruta_archivo As String
        Dim ruta_archivo2 As String = GetTempPath()
        Dim nom_archivo As String
        Dim nom_archivo2 As String
        Dim cadena As String
        Dim validar As Boolean = True

        Try
            If guardar.ShowDialog() = DialogResult.OK Then
                'Encriptamos los archivos 
                For Each indi In Lista.Items
                    Dim encripta As EncriptCapufe.EncriptCapufe = New EncriptCapufe.EncriptCapufe()
                    encripta.EncriptarFile(indi)
                Next
                'Agregamos a zip
                Using zip As ZipFile = New ZipFile()

                    For Each indi In Lista.Items
                        zip.AddFile(indi, "")
                    Next

                    'creamos nombre de zip
                    ruta_archivo = guardar.SelectedPath
                    cadena = Lista.Items(1)
                    cadena = Strings.Right(cadena, 12)
                    nom_archivo = Mid(cadena, 1, 9) & "Z" & Mid(cadena, 10, 1) & "A"

                    'Guardamos el Zip
                    If Not System.IO.File.Exists(nom_archivo) Then
                        zip.Save(ruta_archivo2 & "\" & nom_archivo)

                    End If
                End Using

                'Creacion de Archivo "HASH"
                Dim Hastext As New HashClass.HashClass()
                Dim textoEncriptado As [String] = Hastext.EncryptString(ruta_archivo2 & "\" & nom_archivo)
                Dim PathF As String

                PathF = ruta_archivo & "\HASH.txt"

                'Escribimo sel archivo Hash
                Using sw As StreamWriter = File.CreateText(PathF)
                    sw.WriteLine("ValidaHASH:")
                    sw.WriteLine(textoEncriptado)
                    sw.Close()
                End Using

                'Agregamos a zip
                Using zip2 As ZipFile = New ZipFile()
                    zip2.AddFile(ruta_archivo2 & "\" & nom_archivo, "")
                    zip2.AddFile(PathF, "")

                    nom_archivo2 = Mid(nom_archivo, 1, 8) & sLine2 & Mid(nom_archivo, 9)

                    If Not System.IO.File.Exists(nom_archivo2) Then
                        zip2.Save(ruta_archivo & "\" & nom_archivo2)
                        My.Computer.FileSystem.DeleteFile(PathF)
                        My.Computer.FileSystem.DeleteFile(ruta_archivo2 & "\" & nom_archivo)
                    End If

                End Using

                MsgBox("Archivos encriptados con exito en:" & vbCrLf & vbCrLf & ruta_archivo, vbInformation, "Successful")

            End If
        Catch ex As Exception
            MsgBox("Error en la Encriptacion ", vbCritical, "¡¡¡¡Atención!!!!")
        End Try

        'Limpiamos nuestra Pantalla
        Button1.Visible = False
        Lista.Items.Clear()

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            Dim sLine As String = ""
            Dim validar As Boolean = True
            OpenFile.InitialDirectory = "C:\"
            OpenFile.Title = "Selcciona el Archivo para Encriptar"
            OpenFile.Multiselect = True

            'se abren visor de archivos y se enlistan los archivos
            While validar <> False
                If OpenFile.ShowDialog() = DialogResult.OK Then
                    Lista.Items.Clear()
                    For Each indi In OpenFile.FileNames
                        Lista.Items.Add(indi)
                    Next
                    'se valida que sean 5 o 4 archivos 
                    If Lista.Items.Count = 5 Or Lista.Items.Count = 4 Then
                        Dim url As String = Lista.Items(1)
                        'Se obtiene el año para nombre del archivo
                        Dim objReader As New StreamReader(url)
                        sLine = objReader.ReadLine()
                        sLine = Mid(sLine, 21, 4)
                        sLine2 = Integer.Parse(sLine)
                        objReader.Close()
                        validar = False
                        Button1.Visible = True
                    Else
                        MsgBox("Debes de seleccionar 4 a 5 archivos", vbCritical, "¡¡¡¡Atención!!!!")
                        validar = True
                        Lista.Items.Clear()
                    End If

                Else
                    validar = False
                End If
            End While
        Catch ex As Exception
            MsgBox("Archivo ya Encriptado o sin Encabezado", vbCritical, "¡¡¡¡Atención!!!!")
            Lista.Items.Clear()
        End Try

    End Sub

    Private Sub FRM1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MDIInicio.FileMenu.Enabled = False
        MDIInicio.ReportesMenu.Enabled = False
        MDIInicio.HelpMenu.Enabled = False
    End Sub

    Private Sub FRM1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        MDIInicio.FileMenu.Enabled = True
        MDIInicio.ReportesMenu.Enabled = True
        MDIInicio.HelpMenu.Enabled = True

    End Sub
End Class