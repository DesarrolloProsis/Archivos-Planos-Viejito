Imports Oracle.DataAccess.Client
Imports System.Data
Imports System.IO
Imports Ionic.Zip

Public Class frmR1

    Dim dtsPlasaCobro As New DataSet
    Dim archivo_1 As String
    Dim archivo_2 As String
    Dim archivo_3 As String
    Dim archivo_4 As String
    Dim archivo_5 As String

    Dim banValidaciones As Boolean = True
    Dim dir_archivo As String = objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "dircomp")
    Dim strIdentificador As String = objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "identificador")
    Dim strContraseña As String = objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "contraseña")

    Private Sub ValidaCarrilesCerrados()

        Dim consulta As String
        Dim consulta2 As String
        Dim cmd As OracleCommand = New OracleCommand()
        Dim conexion As OracleConnection = New OracleConnection()
        'conexion.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST= 192.168.0.90)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));User Id=GEADBA;Password=UORUORJVNE;"
        conexion.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" & objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "ip") & ")(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));User Id=GEADBA;Password=fgeuorjvne;"

        Dim fecInicioD As String = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy"))
        Dim fechaSelect As String = dtpFechaInicio.Value.ToString("MM/dd/yyyy")
        Dim TempTurno As String = cmbTurnoBlo.SelectedItem
        Dim turnoP As String = ""
        Dim FechaInicio As String = ""
        Dim FechaFinal As String = ""

        Dim mensaje As String = "Los Carriles "

        Dim carriles As List(Of Carril) = New List(Of Carril)
        Dim carriles_cerrados As List(Of Carril) = New List(Of Carril)

        'Se seleciona el turno
        Select Case TempTurno
            Case "22:00 - 06:00"
                turnoP = "1"
                FechaInicio = fecInicioD + " 22:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
            Case "06:00 - 14:00"
                turnoP = "2"
                FechaInicio = fechaSelect + " 06:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
            Case "14:00 - 22:00"
                turnoP = "3"
                FechaInicio = fechaSelect + " 14:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
        End Select

        consulta = "SELECT	LANE_ASSIGN.Id_plaza,
 		            LANE_ASSIGN.Id_lane,
		            TO_CHAR(LANE_ASSIGN.MSG_DHM,'MM/DD/YY HH24:MI:SS') AS FECHA_INICIO,
 		            LANE_ASSIGN.SHIFT_NUMBER,
 		            LANE_ASSIGN.OPERATION_ID,
		            TO_CHAR(LANE_ASSIGN.ASSIGN_DHM,'MM/DD/YY') AS FECHA,
		            LTRIM(TO_CHAR(LANE_ASSIGN.JOB_NUMBER,'09')) AS EMPLEADO,
		            LANE_ASSIGN.STAFF_NUMBER,
		            LANE_ASSIGN.IN_CHARGE_SHIFT_NUMBER
                     FROM 	LANE_ASSIGN
                     WHERE	 SHIFT_NUMBER = " & turnoP & "
                      AND LANE_ASSIGN.OPERATION_ID ='NA'
                     AND ((MSG_DHM >= TO_DATE('" & FechaInicio & "','MM-DD-YYYY HH24:MI:SS')) AND (MSG_DHM <= TO_DATE('" & FechaFinal & "','MM-DD-YYYY HH24:MI:SS')))
	                    ORDER BY LANE_ASSIGN.Id_PLAZA,
 			                     LANE_ASSIGN.Id_LANE,
 			                     LANE_ASSIGN.MSG_DHM "

        'Se llaman a todos los carriles con NA
        conexion.Open()
        cmd.CommandText = consulta
        cmd.Connection = conexion
        Dim dataReader As OracleDataReader = cmd.ExecuteReader()
        Dim carril As Carril
        While dataReader.Read
            carril = New Carril()
            carril.LANE = dataReader.Item("ID_LANE")
            carril.FECHA = dataReader.Item("FECHA_INICIO")
            carril.MATRICULE = dataReader.Item("STAFF_NUMBER")
            carriles.Add(carril)

        End While
        conexion.Close()

        'Se verifican que los carriles se encuentren cerrados en la tabla FIN_POSTE
        For Each tp As Carril In carriles
            consulta2 = "SELECT COUNT(*) FROM FIN_POSTE WHERE 
                    DATE_DEBUT_POSTE = TO_DATE('" & tp.FECHA & "','MM/DD/YY HH24:MI:SS')
                    AND VOIE = '" & tp.LANE & "' AND MATRICULE='" & tp.MATRICULE & "'"

            conexion.Open()
            cmd.CommandText = consulta2
            cmd.Connection = conexion

            If cmd.ExecuteScalar < 1 Then
                carril = New Carril()
                carril.LANE = tp.LANE
                carril.FECHA = tp.FECHA
                carril.MATRICULE = tp.MATRICULE
                carriles_cerrados.Add(carril)
                banValidaciones = False
            End If
            conexion.Close()
        Next


        For Each tp2 As Carril In carriles_cerrados
            mensaje = mensaje + tp2.LANE + ","
        Next

        If banValidaciones = False Then
            MsgBox(mensaje + " aun no estan Cerrados ", vbCritical, "¡¡¡¡Advertencia!!!!")
        End If

    End Sub

    Private Sub ValidaBolsas()
        Dim TempTurno As String = cmbTurnoBlo.SelectedItem
        Dim fechaSelect As String = dtpFechaInicio.Value.ToString("MM/dd/yyyy")
        'Dim fecInicioD As String = dtpFechaInicio.Value.Year.ToString() + "/" + dtpFechaInicio.Value.Month.ToString() + "/" + dtpFechaInicio.Value.AddDays(-1).ToString("dd")
        Dim fecInicioD As String = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy"))


        Dim turnoP As String = ""
        Dim FechaInicio As String = ""
        Dim FechaFinal As String = ""
        'Valida  el  turno selecionado
        Select Case TempTurno
            Case "22:00 - 06:00"
                turnoP = "1"
                FechaInicio = fecInicioD + " 22:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
            Case "06:00 - 14:00"
                turnoP = "2"
                FechaInicio = fechaSelect + " 06:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
            Case "14:00 - 22:00"
                turnoP = "3"
                FechaInicio = fechaSelect + " 14:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
        End Select


        'Verifica que todos los carriles cerrados tengan bolsa
        Dim consulta As String = "SELECT TO_CHAR( C.DATE_FIN_POSTE,'yyyy-mm-dd') AS FECHA, " +
                                  "C.MATRICULE AS cajero, " +
                                  "C.VOIE AS Carril, " +
                                  "C.NUMERO_POSTE AS Corte, " +
                                  "TO_CHAR(C.DATE_DEBUT_POSTE,'HH24:mi:SS') AS Inicio_Turno, " +
                                  "TO_CHAR(C.DATE_FIN_POSTE,'HH24:mi:SS') AS Fin_Turno, " +
                                "'Entrega no realizada de bolsa '||C.VOIE||' Inicio '||TO_CHAR(C.DATE_DEBUT_POSTE,'HH24:mi:SS')||',Fin '||TO_CHAR(C.DATE_FIN_POSTE,'HH24:mi:SS')||' '||A.MATRICULE||'/'|| A.NOM AS Aviso " +
                            "FROM FIN_POSTE C " +
                            "LEFT JOIN TABLE_PERSONNEL  A ON C.Matricule = A.Matricule " +
                            "WHERE C.DATE_DEBUT_POSTE " +
                            "BETWEEN to_date('" + FechaInicio + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                            "AND to_date('" + FechaFinal + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                            "AND SAC IS NULL AND FIN_POSTE_CPT22 = " + turnoP + "AND C.ID_MODE_VOIE in (1,7)"

        Dim cmd As OracleCommand = New OracleCommand()
        Dim conexion As OracleConnection = New OracleConnection
        conexion.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" & objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "ip") & ")(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));User Id=GEADBA;Password=fgeuorjvne;"
        conexion.Open()
        cmd.CommandText = consulta
        cmd.Connection = conexion
        Dim dataReader As OracleDataReader = cmd.ExecuteReader()
        While dataReader.Read
            banValidaciones = False
            MessageBox.Show(dataReader.Item("Aviso").ToString(), ErrorToString, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End While
        conexion.Close()
    End Sub

    Private Sub ValidaComentarios()
        Dim TempTurno As String = cmbTurnoBlo.SelectedItem
        Dim fechaSelect As String = dtpFechaInicio.Value.ToString("MM/dd/yyyy")
        'Dim fecInicioD As String = dtpFechaInicio.Value.Year.ToString() + "/" + dtpFechaInicio.Value.Month.ToString() + "/" + dtpFechaInicio.Value.AddDays(-1).ToString("dd")
        Dim fecInicioD As String = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy"))

        Dim turnoP As String = ""
        Dim FechaInicio As String = ""
        Dim FechaFinal As String = ""
        'Valida  el  turno selecionado
        Select Case TempTurno
            Case "22:00 - 06:00"
                turnoP = "1"
                FechaInicio = fecInicioD + " 22:00:00"
                FechaFinal = fechaSelect + " 21:59:59"
            Case "06:00 - 14:00"
                turnoP = "2"
                FechaInicio = fechaSelect + " 06:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
            Case "14:00 - 22:00"
                turnoP = "3"
                FechaInicio = fechaSelect + " 14:00:00"
                FechaFinal = fechaSelect + " 23:59:59"
        End Select
        'Valida que se  hayan capturado los  comentarios  en la  entrega de  Bolsa
        ' SE MODIFICIO DATE_FIN_POSTE POR C.DATE_DEBUT_POSTE [RODRIGO]
        Dim consulta As String = "SELECT " +
                                    "C.COMMENTAIRE AS COMENTARIOS, " +
                                    "C.SAC AS BOLSA, " +
                                    "C.OPERATING_SHIFT AS TURNO, " +
                                    "C.DATE_REDDITION AS FECHA, " +
                                    "C.RED_TXT1, " +
                                    "'Declaracion sin comentarios  carril '||C.RED_TXT1||' bolsa '||TO_CHAR(C.SAC)||' '||A.MATRICULE||'/'|| A.NOM AS Aviso " +
                                "FROM REDDITION  C " +
                                "LEFT JOIN TABLE_PERSONNEL  A ON C.Matricule = A.Matricule " +
                                "WHERE DATE_REDDITION " +
                                "BETWEEN to_date('" + FechaInicio + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                                    "AND to_date('" + FechaFinal + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                                 " AND COMMENTAIRE IS NULL AND C.OPERATING_SHIFT  = " + turnoP
        Dim cmd As OracleCommand = New OracleCommand()
        Dim conexion As OracleConnection = New OracleConnection
        conexion.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" & objControl.LeeINI(Application.StartupPath & "\conexion.ini", "conexion", "ip") & ")(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));User Id=GEADBA;Password=fgeuorjvne;"
        conexion.Open()
        cmd.CommandText = consulta
        cmd.Connection = conexion
        Dim dataReader As OracleDataReader = cmd.ExecuteReader()
        While dataReader.Read
            banValidaciones = False
            MessageBox.Show(dataReader.Item("Aviso").ToString(), ErrorToString, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End While
        conexion.Close()
    End Sub

    Private Sub validacionArchivos()

        Dim objReader As New StreamReader(dir_archivo & archivo_1)
        Dim objReader2 As New StreamReader(dir_archivo & archivo_2)
        Dim objReader3 As New StreamReader(dir_archivo & archivo_3)
        Dim nombre_archivo As String
        Dim sLine As String = ""
        Dim arrText As New ArrayList()
        Dim CountLins As Integer = 0
        Dim Countrows As Integer = 0
        Dim errores
        Dim mes = Format(dt_Fecha_Inicio, "MM")
        Dim año = Format(dt_Fecha_Inicio, "yyyy")
        If mes = 1 Then
            mes = "enero"
        ElseIf mes = 2 Then
            mes = "febrero"
        ElseIf mes = 3 Then
            mes = "marzo"
        ElseIf mes = 4 Then
            mes = "abril"
        ElseIf mes = 5 Then
            mes = "mayo"
        ElseIf mes = 6 Then
            mes = "junio"
        ElseIf mes = 7 Then
            mes = "julio"
        ElseIf mes = 8 Then
            mes = "agosto"
        ElseIf mes = 9 Then
            mes = "septiembre"
        ElseIf mes = 10 Then
            mes = "octubre"
        ElseIf mes = 11 Then
            mes = "noviembre"
        ElseIf mes = 12 Then
            mes = "diciembre"
        End If

        Dim dir_archivo_nuevos As String = dir_archivo & año & "\" & mes & "\" & Format(dt_Fecha_Inicio, "dd") & "\"
        Dim subString As String
        Dim turno As Integer
        If Not Directory.Exists(dir_archivo & año & "\" & mes & "\" & Format(dt_Fecha_Inicio, "dd")) Then
            Directory.CreateDirectory(dir_archivo & año & "\" & mes & "\" & Format(dt_Fecha_Inicio, "dd"))
        End If
        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            turno = 5
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            turno = 6
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
            turno = 4
        End If
        nombre_archivo = "ListaDeErrores-" & Format(dt_Fecha_Inicio, "dd") & "-Turno-" & turno & ".txt"
        Dim oSW As New StreamWriter(dir_archivo_nuevos & nombre_archivo)
        'Dim correo As New System.Net.Mail.MailMessage()
        'correo.From = New System.Net.Mail.MailAddress("errores_casetas@outlook.com") 'remitente
        'correo.Subject = "Validaciones Incorrectas del dia " & Format(dt_Fecha_Inicio, "MM") & "-" & Format(dt_Fecha_Inicio, "dd") & " Turno " & turno  'asunto
        'correo.To.Add("sistemas@grupo-prosis.com") 'destinatario
        'archivo 1A

        Do
            sLine = objReader.ReadLine()
            If Not sLine Is Nothing Then
                Dim result As String() = sLine.Split(",")
                If CountLins = 0 Then
                    subString = Microsoft.VisualBasic.Right(result(0), 2)
                End If
                If CountLins >= 1 Then
                    'Dim stringarray As String = result(3) & result(6) & result(7) & result(8)
                    arrText.Add(result(2) & "," & result(5) & "," & result(6) & "," & result(7))
                    'VERIFICAR ESTO
                    'If result(11).ToString() = "" AndAlso (result(7).ToString() = "NA" OrElse result(7).ToString() = "NB") Then
                    '    errores = "Error en el archivo de 1 en la linea: " & CountLins & ", el carril se encuentra abierto y no cuenta con un N°Preliquidación en la posición 12."
                    '    oSW.WriteLine(errores)
                    '    oSW.Flush()
                    'Else
                    '    If result(11).ToString() <> "" AndAlso (result(7).ToString() = "XA" OrElse result(7).ToString() = "XB") Then
                    '        errores = "Error en el archivo de 1 en la linea: " & CountLins & ", el carril se encuentra  cerrado con un N°Preliquidación en la posición 8."
                    '        oSW.WriteLine(errores)
                    '        oSW.Flush()
                    '    End If
                    'End If
                    Countrows = 0

                    'Valida que en todo el registro no se encuentre la  leyenda de "Pendiente"
                    For Each Temporal As String In result
                        If Countrows < 12 Then
                            If Temporal = "Pendiente" Then
                                errores = "Error en el archivo de 1 en la linea: " & CountLins & ",  contiene la leyenda de PENDIENTE en la posición" & Countrows
                                oSW.WriteLine(errores)
                                oSW.Flush()
                            End If
                            'Validas que ningun campo venga vacio a exepción 
                            If Temporal = "" AndAlso Countrows <> 11 Then
                                errores = "Error en el archivo 1A en la linea: " & CountLins & ",  valor Vacio o en blanco en la posición " & Countrows & vbCrLf
                                oSW.WriteLine(errores)
                                oSW.Flush()
                            End If
                            Countrows = Countrows + 1
                        End If
                    Next
                End If

                CountLins = CountLins + 1

            Else

                Dim i, j As Integer
                Dim Auxiliar As String
                For i = 0 To arrText.Count - 1
                    For j = 0 To arrText.Count - 2
                        If (arrText.Item(j) > arrText.Item(i)) Then
                            Auxiliar = arrText.Item(j)
                            arrText.Item(j) = arrText.Item(i)
                            arrText.Item(i) = Auxiliar
                        End If
                    Next
                Next

                Dim r As Integer = -2
                Dim e As Integer = -1
                For i = 0 To arrText.Count - 2
                    'Dim contador = contador + 1
                    r = 2 + r
                    e = 2 + e

                    If arrText.Count > e Then

                        Dim Hora1 = arrText(r).Substring(0, 13)
                        Dim Hora2 = arrText(e).Substring(0, 13)

                        If Hora1 = Hora2 Then
                            errores = "El archivo 1A el carril " & arrText(r).Substring(7, 4) & " abrio en la misma hora que cerro"
                            oSW.WriteLine(errores)
                            oSW.Flush()
                        End If
                    End If

                Next


                If CountLins - 1 <> Integer.Parse(subString) Then
                    errores = "Archivo con terminacion 1A conteo diferente en encabezado"
                    oSW.WriteLine(errores)
                    oSW.Flush()
                End If
            End If
        Loop Until sLine Is Nothing
        objReader.Close()

        CountLins = 0
        'ARCHIVO 2A

        Do
            sLine = objReader2.ReadLine()
            If Not sLine Is Nothing Then
                Dim result As String() = sLine.Split(",")
                If CountLins = 0 Then
                    subString = Microsoft.VisualBasic.Right(result(0), 2)
                End If
                If CountLins >= 1 Then
                    'Valida el PRIMER rollo de folios en las  posciciones (53,54)
                    If (result(52) & result(60) & result(53) & result(52)) > "0" Then
                        If (result(53) & result(52)) = "" Then
                            errores = "Error en el archivo 2A en la linea: " & CountLins & ", No existe folio en la linea 53 y 54."
                            oSW.WriteLine(errores)
                            oSW.Flush()
                        Else
                            If 0 > (Integer.Parse(result(53).ToString) - Integer.Parse(result(52).ToString())) Then
                                errores = "Error en el archivo 2A en la linea: " & CountLins & ", el folio final es menor que el inicial del primer rollo en la posición 53 y 54."
                                oSW.WriteLine(errores)
                                oSW.Flush()
                            Else
                                If 999999 < (Integer.Parse(result(53).ToString()) - Integer.Parse(result(52).ToString())) Then
                                    errores = "Error en el archivo 2A en la linea: " & CountLins & ", la diferencia entre los folios del primer rollo excede el limite de 999,999 en la posición 53 y 54."
                                    oSW.WriteLine(errores)
                                    oSW.Flush()
                                End If
                            End If
                        End If

                        'Valida el segundo rollos de folios en las  posciciones (56,57)
                        If (result(56) & result(60) & result(53) & result(52)) > 0 Then
                            If (result(56) & result(57)) = "" Then
                                errores = "Error en el archivo 2A en la linea: " & CountLins & ", No existe folio en la linea 56 y 57."
                                oSW.WriteLine(errores)
                                oSW.Flush()
                            Else
                                If 0 > (Integer.Parse(result(56).ToString()) - Integer.Parse(result(55).ToString())) Then
                                    errores = "Error en el archivo  2A en la linea: " & CountLins & ", el folio final es menor que el inicial del primer rollo en la posición 56 y 57."
                                    oSW.WriteLine(errores)
                                    oSW.Flush()
                                Else
                                    If 999999 < (Integer.Parse(result(56).ToString()) - Integer.Parse(result(55).ToString())) Then
                                        errores = "Error en el archivo 2A en la linea: " & CountLins & ", la diferencia entre los folios del primer rollo excede el limite de 999,999 en la posición 56 y 57."
                                        oSW.WriteLine(errores)
                                        oSW.Flush()
                                    End If
                                End If
                            End If

                            'Valida  folios en las  posciciones (59,60)


                        Else
                            If (result(56) & result(60) & result(53) & result(52)) > 0 Then
                                If (result(58) & result(59)) = "" Then
                                    errores = "Error en el archivo 2A en la linea: " & CountLins & ", No existe folio en la linea 58 y 59." & vbCrLf
                                    oSW.WriteLine(errores)
                                    oSW.Flush()
                                Else
                                    If 0 > (Integer.Parse(result(59).ToString()) - Integer.Parse(result(58).ToString())) Then
                                        errores = "Error en el archivo 2A en la linea: " & CountLins & ", el folio final es menor que el inicial del primer rollo en la posición 59 y 60."
                                        oSW.WriteLine(errores)
                                        oSW.Flush()
                                    Else
                                        If 999999 < (Integer.Parse(result(59).ToString()) - Integer.Parse(result(58).ToString())) Then
                                            errores = "Error en el archivo 2A en la linea: " & CountLins & ", la diferencia entre los folios del primer rollo excede el limite de 999,999 en la posición 59 y 60."
                                            oSW.WriteLine(errores)
                                            oSW.Flush()
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
                CountLins = CountLins + 1
            Else
                If CountLins - 1 <> Integer.Parse(subString) Then
                    errores = "Archivo con terminacion 2A con conteo diferente en encabezado"
                    oSW.WriteLine(errores)
                    oSW.Flush()
                End If
            End If
        Loop Until sLine Is Nothing
        objReader2.Close()
        'ARCHIVO 9A

        CountLins = 0
        Do
            sLine = objReader3.ReadLine()
            If Not sLine Is Nothing Then
                Dim result As String() = sLine.Split(",")

                If CountLins = 0 Then
                    subString = Microsoft.VisualBasic.Right(result(0), 5)
                End If
                If CountLins >= 1 Then
                    'Valida que la clase detectada sea diferente de cero
                    If result(8) = "0" Then
                        errores = "Error en el archivo 9A en la linea: " & CountLins & ", la clase detectada es cero en la posición 9."
                        oSW.WriteLine(errores)
                        oSW.Flush()
                    End If
                End If

                CountLins = CountLins + 1
            Else
                Dim i, j As Integer
                Dim Auxiliar As String
                For i = 0 To arrText.Count - 1
                    For j = 0 To arrText.Count - 2
                        If (arrText.Item(j) > arrText.Item(i)) Then
                            Auxiliar = arrText.Item(j)
                            arrText.Item(j) = arrText.Item(i)
                            arrText.Item(i) = Auxiliar
                        End If
                    Next
                Next
                'VALIDACION DE MISMA HORA DE CARRIL
                Dim r As Integer = -2
                Dim e As Integer = -1
                For i = 0 To arrText.Count - 2
                    'Dim contador = contador + 1
                    r = 2 + r
                    e = 2 + e

                    If arrText.Count > e Then

                        Dim Hora1 = arrText(r).Substring(0, 12)
                        Dim Hora2 = arrText(e).Substring(0, 12)

                        If Hora1 = Hora2 Then
                            errores = "En el archivo 9A el carril " & arrText(r).Substring(7, 4) & " abrio en la misma hora que cerro"
                            oSW.WriteLine(errores)
                            oSW.Flush()
                        End If
                    End If
                Next
                Dim Conteo
                If CountLins < 999 Then
                    Conteo = subString.Substring(2, 3)
                End If
                If CountLins < 9999 Then
                    Conteo = subString.Substring(1, 4)
                End If
                If CountLins < 99999 Then
                    Conteo = subString.Substring(0, 5)
                End If
                If CountLins - 1 <> Conteo Then
                    errores = "Archivo con terminacion 9A con conteo diferente en encabezado"
                    oSW.WriteLine(errores)
                    oSW.Flush()
                End If
            End If
        Loop Until sLine Is Nothing

        objReader3.Close()
        oSW.Close()

        'Dim objReader2 As New StreamReader(dir_archivo_errores & nombre_archivo)

        'sLine = objReader2.ReadToEnd()
        'If Not sLine Is Nothing Then
        '    arrText.Add(sLine)
        '    correo.Body = sLine
        '    errores = True
        'Else
        '    MsgBox("Archivos sin errores")
        '    errores = False
        '    My.Computer.FileSystem.DeleteFile(dir_archivo_errores & nombre_archivo)
        'End If
        'objReader2.Close()
        'enviar correo
        'Dim Servidor As New System.Net.Mail.SmtpClient
        'Servidor.Host = "smtp.live.com"
        'Servidor.Port = 587
        'Servidor.EnableSsl = True
        'Servidor.Credentials = New System.Net.NetworkCredential("errores_casetas@outlook.com", "k4puf32016")
        'If My.Computer.Network.IsAvailable() Then
        '    If My.Computer.Network.Ping("www.google.es", 1000) Then
        '        If errores = True Then
        '            Servidor.Send(correo)
        '        End If
        '    End If
        'End If
    End Sub

    Private Sub encriptar()
        Try
            Dim mes = Format(dt_Fecha_Inicio, "MM")
            Dim año = Format(dt_Fecha_Inicio, "yyyy")
            If mes = 1 Then
                mes = "enero"
            ElseIf mes = 2 Then
                mes = "febrero"
            ElseIf mes = 3 Then
                mes = "marzo"
            ElseIf mes = 4 Then
                mes = "abril"
            ElseIf mes = 5 Then
                mes = "mayo"
            ElseIf mes = 6 Then
                mes = "junio"
            ElseIf mes = 7 Then
                mes = "julio"
            ElseIf mes = 8 Then
                mes = "agosto"
            ElseIf mes = 9 Then
                mes = "septiembre"
            ElseIf mes = 10 Then
                mes = "octubre"
            ElseIf mes = 11 Then
                mes = "noviembre"
            ElseIf mes = 12 Then
                mes = "diciembre"
            End If
            Dim dir_archivo_sinEncriptar As String = dir_archivo & año & "\" & mes & "\" & Format(dt_Fecha_Inicio, "dd") & "\" & "SinEncriptar\"
            Dim nombre_archivo_de_errores = Format(dt_Fecha_Inicio, "dd") & "_errores.txt"
            Dim nombre_archivo As String

            Dim int_turno As Integer


            If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
                int_turno = 5
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
                int_turno = 6
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
                int_turno = 4
            End If

            Using zipOriginales As ZipFile = New ZipFile()



                zipOriginales.Password = strContraseña
                zipOriginales.Encryption = EncryptionAlgorithm.WinZipAes256



                zipOriginales.AddFile(dir_archivo & archivo_1, "")
                zipOriginales.AddFile(dir_archivo & archivo_2, "")
                zipOriginales.AddFile(dir_archivo & archivo_3, "")
                zipOriginales.AddFile(dir_archivo & archivo_4, "")
                zipOriginales.AddFile(dir_archivo & archivo_5, "")


                If Len(id_plaza_cobro) = 3 Then
                    If id_plaza_cobro = 108 Then
                        nombre_archivo = "0001"
                    ElseIf id_plaza_cobro = 109 Then
                        nombre_archivo = "001B"
                    ElseIf id_plaza_cobro = 107 Then
                        nombre_archivo = "0107"
                    ElseIf id_plaza_cobro = 106 Then
                        nombre_archivo = "0006"
                    Else
                        nombre_archivo = "0" & id_plaza_cobro

                    End If
                End If

                nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & ".Z" & int_turno & strIdentificador
                If Not Directory.Exists(dir_archivo_sinEncriptar) Then
                    Directory.CreateDirectory(dir_archivo_sinEncriptar)
                End If
                zipOriginales.Save(dir_archivo_sinEncriptar & nombre_archivo)
                ArchivoZip = dir_archivo_sinEncriptar & nombre_archivo

            End Using

            Dim encripta As EncriptCapufe.EncriptCapufe = New EncriptCapufe.EncriptCapufe()

            encripta.EncriptarFile(dir_archivo & archivo_1)
            encripta.EncriptarFile(dir_archivo & archivo_2)
            encripta.EncriptarFile(dir_archivo & archivo_3)
            encripta.EncriptarFile(dir_archivo & archivo_4)
            encripta.EncriptarFile(dir_archivo & archivo_5)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Comprimir()
        Dim PathF As String
        Try
            Dim mes = Format(dt_Fecha_Inicio, "MM")
            Dim año = Format(dt_Fecha_Inicio, "yyyy")
            If mes = 1 Then
                mes = "enero"
            ElseIf mes = 2 Then
                mes = "febrero"
            ElseIf mes = 3 Then
                mes = "marzo"
            ElseIf mes = 4 Then
                mes = "abril"
            ElseIf mes = 5 Then
                mes = "mayo"
            ElseIf mes = 6 Then
                mes = "junio"
            ElseIf mes = 7 Then
                mes = "julio"
            ElseIf mes = 8 Then
                mes = "agosto"
            ElseIf mes = 9 Then
                mes = "septiembre"
            ElseIf mes = 10 Then
                mes = "octubre"
            ElseIf mes = 11 Then
                mes = "noviembre"
            ElseIf mes = 12 Then
                mes = "diciembre"
            End If
            Dim dir_archivo_errores As String = dir_archivo & año & "\" & mes & "\" & Format(dt_Fecha_Inicio, "dd") & "\"
            Dim nombre_archivo_de_errores = Format(dt_Fecha_Inicio, "dd") & "_errores.txt"
            Dim nombre_archivo As String
            Dim nombre_archivo2 As String
            Dim int_turno As Integer

            If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
                int_turno = 5
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
                int_turno = 6
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
                int_turno = 4
            End If


            Using zip As ZipFile = New ZipFile()

                'zip.Password = strContraseña
                'zip.Encryption = EncryptionAlgorithm.WinZipAes256

                zip.AddFile(dir_archivo & archivo_1, "")
                zip.AddFile(dir_archivo & archivo_2, "")
                zip.AddFile(dir_archivo & archivo_3, "")
                zip.AddFile(dir_archivo & archivo_4, "")
                zip.AddFile(dir_archivo & archivo_5, "")


                If Len(id_plaza_cobro) = 3 Then
                    nombre_archivo = "0" & id_plaza_cobro
                End If

                If Len(id_plaza_cobro) = 3 Then
                    If id_plaza_cobro <> 108 Then
                        nombre_archivo = "0" & id_plaza_cobro
                    Else
                        nombre_archivo = "0001"
                    End If
                End If

                If Len(id_plaza_cobro) = 3 Then
                    If id_plaza_cobro = 108 Then
                        nombre_archivo = "0001"
                    ElseIf id_plaza_cobro = 109 Then
                        nombre_archivo = "001B"
                    ElseIf id_plaza_cobro = 107 Then
                        nombre_archivo = "0107"
                    ElseIf id_plaza_cobro = 106 Then
                        nombre_archivo = "0006"
                    Else
                        nombre_archivo = "0" & id_plaza_cobro

                    End If
                End If

                nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & ".Z" & int_turno & strIdentificador


                If Not System.IO.File.Exists(dir_archivo_errores & nombre_archivo_de_errores) Then
                    zip.Save(dir_archivo_errores & nombre_archivo)
                End If

                ''''''''''''' encriptacion'''''''''''''''''

                Dim Hastext As New HashClass.HashClass()

                Dim textoEncriptado As [String] = Hastext.EncryptString(dir_archivo_errores & nombre_archivo)


                PathF = dir_archivo_errores & "HASH.txt"


                'Creación y Escritura del Archivo validacion HASH

                Using sw As StreamWriter = File.CreateText(PathF)

                    sw.WriteLine("ValidaHASH:")
                    sw.WriteLine(textoEncriptado)
                    sw.Close()
                End Using
            End Using
            Using zip2 As ZipFile = New ZipFile()

                zip2.AddFile(PathF, "")
                zip2.AddFile(dir_archivo_errores & nombre_archivo, "")

                Dim NoPlaza
                If Len(id_plaza_cobro) = 3 Then
                    If id_plaza_cobro = 108 Then
                        NoPlaza = "0001"
                    ElseIf id_plaza_cobro = 109 Then
                        NoPlaza = "001B"
                    ElseIf id_plaza_cobro = 107 Then
                        NoPlaza = "0107"
                    ElseIf id_plaza_cobro = 106 Then
                        NoPlaza = "0006"
                    Else
                        NoPlaza = "0" & id_plaza_cobro

                    End If
                End If
                Dim archivo2
                archivo2 = NoPlaza & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & Format(dt_Fecha_Inicio, "yyyy") & ".Z" & int_turno & strIdentificador

                zip2.Save(dir_archivo_errores & archivo2)

                My.Computer.FileSystem.DeleteFile(PathF)
                My.Computer.FileSystem.DeleteFile(dir_archivo_errores & nombre_archivo)

            End Using
            elimina()
        Catch ex As Exception
            MsgBox(ex.Message.ToString, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub elimina()
        Try
            My.Computer.FileSystem.DeleteFile(dir_archivo & archivo_1)
            My.Computer.FileSystem.DeleteFile(dir_archivo & archivo_2)
            My.Computer.FileSystem.DeleteFile(dir_archivo & archivo_3)
            My.Computer.FileSystem.DeleteFile(dir_archivo & archivo_4)
            My.Computer.FileSystem.DeleteFile(dir_archivo & archivo_5)
        Catch ex As Exception
            Return
        End Try
    End Sub

    Private Sub frmR1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed


        MDIInicio.FileMenu.Enabled = True
        MDIInicio.ReportesMenu.Enabled = True
        MDIInicio.HelpMenu.Enabled = True


    End Sub

    Private Sub frmR1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        MDIInicio.FileMenu.Enabled = False
        MDIInicio.ReportesMenu.Enabled = False
        MDIInicio.HelpMenu.Enabled = False


        Dim strConsulta As String
        Me.Text = strLinea

        If oConexion.State = ConnectionState.Closed Then
            oConexion.ConnectionString = gl_DNS
            oConexion.Open()
        End If


        gl_DNS_SqlServer = "data source=(local);initial catalog=PROSIS; user id=SA; password=CAPUFE;"


        'delegacion
        strConsulta = "SELECT ID_RESEAU, NOM_RESEAU, NOM_RESEAU_L2 FROM TYPE_RESEAU"
        glstrTabla = "TYPE_RESEAU"

        If objQuerys.QueryDataSet(strConsulta, glstrTabla) = 1 Then

            If InStr(CStr(oDataRow("NOM_RESEAU")), "Acapulco") > 0 Then
                cmbDelegacion.Items.Add("Delegación IV Cuernavaca")
                cmbDelegacion.Tag = "04"
                cmbDelegacion.SelectedIndex = 0

            ElseIf InStr(CStr(oDataRow("NOM_RESEAU")), "Puebla") > 0 Then
                cmbDelegacion.Items.Add("Delegación V Puebla")
                cmbDelegacion.Tag = "05"
                cmbDelegacion.SelectedIndex = 0

            ElseIf InStr(CStr(oDataRow("NOM_RESEAU")), "Delegación") > 0 Then
                cmbDelegacion.Items.Add("Delegación I Noroeste Tijuana")
                cmbDelegacion.Tag = "05"
                cmbDelegacion.SelectedIndex = 0
            End If
        End If
        'fin delegacion


        strConsulta = "SELECT ID_SITE, NOM_SITE, NOM_SITE_L2, ID_SITE ||' '|| NOM_SITE as PlazaCobro " &
               "FROM TYPE_SITE " &
               "ORDER BY NOM_SITE"

        glstrTabla = "TYPE_SITE"

        Dim oDataAdapter = New OracleDataAdapter(strConsulta, oConexion)

        oDataAdapter.SelectCommand.CommandTimeout = 10000
        oDataAdapter.Fill(dtsPlasaCobro, glstrTabla)

        oDataAdapter = Nothing


        cmbPlazaCobro.DataSource = dtsPlasaCobro.Tables("TYPE_SITE")
        cmbPlazaCobro.DisplayMember = "PlazaCobro"
        cmbPlazaCobro.ValueMember = "ID_SITE"
        cmbPlazaCobro.SelectedValue = -1
        cmbPlazaCobro.SelectedIndex = 0

        'strConsulta = "Select JOB_NUMBER FROM GPOS_EOJ GROUP BY JOB_NUMBER ORDER BY JOB_NUMBER"

        strConsulta = "SELECT NUMERO_POSTE, COUNT(*) AS No_Turno " &
        "FROM FIN_POSTE " &
        "GROUP BY NUMERO_POSTE " &
        "ORDER BY NUMERO_POSTE"

        glstrTabla = "FIN_POSTE"



        oDataAdapter = New OracleDataAdapter(strConsulta, oConexion)

        oDataAdapter.SelectCommand.CommandTimeout = 10000
        oDataAdapter.Fill(dtsPlasaCobro, glstrTabla)

        oDataAdapter = Nothing


        'cmbTurno.DataSource = dtsPlasaCobro.Tables("FIN_POSTE")
        'cmbTurno.DisplayMember = "NUMERO_POSTE"
        'cmbTurno.ValueMember = "NUMERO_POSTE"
        'cmbTurno.SelectedValue = -1


        'carril
        strConsulta = "Select ID_LANE FROM GPOS_EOJ GROUP BY ID_LANE ORDER BY ID_LANE"
        glstrTabla = "ID_LANE"

        oDataAdapter = New OracleDataAdapter(strConsulta, oConexion)

        oDataAdapter.SelectCommand.CommandTimeout = 10000
        oDataAdapter.Fill(dtsPlasaCobro, glstrTabla)

        oDataAdapter = Nothing


        'cmbCarril.DataSource = dtsPlasaCobro.Tables("ID_LANE")
        'cmbCarril.DisplayMember = "ID_LANE"
        'cmbCarril.ValueMember = "ID_LANE"
        'cmbCarril.SelectedValue = -1



        ''controles
        'lblCajeroReceptor.Visible = False
        'txtCajeroReceptor.Visible = False

        lblTurno.Visible = False
        'cmbTurno.Visible = False
        cmbTurnoBlo.Visible = False

        lblEncargadoTurno.Visible = False
        txtEncargadoTurno.Visible = False

        lblDelegacion.Visible = False
        cmbDelegacion.Visible = False

        lblFecha2.Visible = False
        dtpFechaFin.Visible = False

        lblPlazaCobro.Visible = False
        cmbPlazaCobro.Visible = False

        lblFecha1.Visible = False
        dtpFechaInicio.Visible = False

        lblFecha2.Visible = False
        dtpFechaFin.Visible = False

        'lblCarril.Visible = False
        'cmbCarril.Visible = False

        Select Case Me.Tag

            Case 4, 5, 6 'bitacora de operacion
                lblDelegacion.Visible = True
                cmbDelegacion.Visible = True


                lblPlazaCobro.Visible = True
                cmbPlazaCobro.Visible = True

                lblFecha1.Visible = True
                dtpFechaInicio.Visible = True

                lblFecha1.Text = "Fecha Inicial"
                lblFecha2.Text = "Fecha Final"

                lblTurno.Visible = True
                cmbTurnoBlo.Visible = True

                dtpFechaInicio.Format = DateTimePickerFormat.Custom

                dtpFechaInicio.CustomFormat = "MM/dd/yyyy"





        End Select

    End Sub

    Private Sub btnGenerarReporte_Click(sender As Object, e As EventArgs) Handles btnGenerarReporte.Click
        banValidaciones = True
        Dim strQuerys As String
        'Dim frmReporte As New frmReporte
        'frmReporte.Tag = Me.Tag
        'frmReporte.MdiParent = MDIInicio
        'frmReporte.Show()

        objControl.limpia_Catalogos()

        If Not Directory.Exists(dir_archivo) Then
            Directory.CreateDirectory(dir_archivo)
        End If
        'Dim frmReporte As New frmReporteador
        'frmReporte.Tag = Me.Tag
        'frmReporte.MdiParent = MDIInicio

        Select Case Me.Tag

            Case 4 'bitacora de operacion
                If cmbDelegacion.Text = "" Then
                    MsgBox("Favor de seleccionar la delegación", MsgBoxStyle.Information, "Verificar")
                    Exit Sub
                End If

                If cmbPlazaCobro.SelectedValue = Nothing Then
                    MsgBox("Favor de seleccionar la plaza de cobro", MsgBoxStyle.Information, "Verificar")
                    Exit Sub
                End If

                If cmbTurnoBlo.Text = "" Then
                    MsgBox("Favor de seleccionar el turno", MsgBoxStyle.Information, "Verificar")
                    Exit Sub
                End If

                objControl.limpia_Catalogos()

                str_Plaza_Cobro = "1" & cmbPlazaCobro.Text
                id_plaza_cobro = CInt("1" & Trim(CStr(cmbPlazaCobro.SelectedValue)))

                dt_Fecha_Inicio = dtpFechaInicio.Value
                dt_Fecha_Fin = dtpFechaFin.Value

                str_delegacion = cmbDelegacion.Text

                str_Turno_block = Trim(cmbTurnoBlo.Text)

                archivo_1 = ""
                archivo_2 = ""
                archivo_3 = ""
                archivo_4 = ""
                archivo_5 = ""

                If banValidaciones Then
                    ValidaCarrilesCerrados()
                End If
                If banValidaciones Then
                    ValidaBolsas()
                End If
                If banValidaciones Then
                    ValidaComentarios()
                End If
                If banValidaciones Then
                    'ProgressBar1.Minimum = 0
                    'ProgressBar1.Maximum = 100
                    generar_bitacora_operacion()
                    Preliquidaciones_de_cajero_receptor_para_transito_vehicular()
                    eventos_detectados_y_marcados_en_el_ECT()
                    eventos_detectados_y_marcados_en_el_ECT_EAP()
                    registro_usuarios_telepeaje()
                    validacionArchivos()
                    encriptar()
                    Comprimir()
                    Dim resultado = MsgBox("Exportación Terminada", MsgBoxStyle.Information, "Exportación")
                    If resultado = vbOK Then
                        ProgressBar1.Value = 0
                    End If
                End If

                'Case 5
                '    'Preliquidación de Cajero-Receptor para Tránsito Vehicular
                '    If cmbDelegacion.Text = "" Then
                '        MsgBox("Favor de seleccionar la delegación", MsgBoxStyle.Information, "Verificar")
                '        Exit Sub
                '    End If

                '    If cmbPlazaCobro.SelectedValue = Nothing Then
                '        MsgBox("Favor de seleccionar la plaza de cobro", MsgBoxStyle.Information, "Verificar")
                '        Exit Sub
                '    End If

                '    If cmbTurnoBlo.Text = "" Then
                '        MsgBox("Favor de seleccionar el turno", MsgBoxStyle.Information, "Verificar")
                '        Exit Sub
                '    End If

                '    objControl.limpia_Catalogos()

                '    str_Plaza_Cobro = "1" & cmbPlazaCobro.Text
                '    id_plaza_cobro = CInt("1" & Trim(CStr(cmbPlazaCobro.SelectedValue)))

                '    dt_Fecha_Inicio = dtpFechaInicio.Value
                '    dt_Fecha_Fin = dtpFechaFin.Value

                '    str_delegacion = cmbPlazaCobro.Text

                '    str_Turno_block = Trim(cmbTurnoBlo.Text)

                '    Preliquidaciones_de_cajero_receptor_para_transito_vehicular()

                'Case 6
                '    'eventos_detectados_y_marcados_en_el_ECT 
                '    If cmbDelegacion.Text = "" Then
                '        MsgBox("Favor de seleccionar la delegación", MsgBoxStyle.Information, "Verificar")
                '        Exit Sub
                '    End If

                '    If cmbPlazaCobro.SelectedValue = Nothing Then
                '        MsgBox("Favor de seleccionar la plaza de cobro", MsgBoxStyle.Information, "Verificar")
                '        Exit Sub
                '    End If

                '    If cmbTurnoBlo.Text = "" Then
                '        MsgBox("Favor de seleccionar el turno", MsgBoxStyle.Information, "Verificar")
                '        Exit Sub
                '    End If

                '    objControl.limpia_Catalogos()

                '    str_Plaza_Cobro = "1" & cmbPlazaCobro.Text
                '    id_plaza_cobro = CInt("1" & Trim(CStr(cmbPlazaCobro.SelectedValue)))

                '    dt_Fecha_Inicio = dtpFechaInicio.Value
                '    dt_Fecha_Fin = dtpFechaFin.Value

                '    str_delegacion = cmbDelegacion.Text

                '    str_Turno_block = Trim(cmbTurnoBlo.Text)

                '    eventos_detectados_y_marcados_en_el_ECT()

        End Select

        'frmReporte.ShowDialog()
        'frmReporte.Show()


    End Sub
    'Archivo 1A
    Private Sub generar_bitacora_operacion()
        Dim strQuerys As String
        Dim Linea As String = ""
        Dim cabecera As String
        Dim pie As String
        Dim numero_archivo As String = ""
        Dim nombre_archivo As String
        Dim numero_registros As Double
        Dim cont As Integer
        Dim cont2 As Integer
        Dim int_turno As Integer
        Dim h_inicio_turno As Date
        Dim h_fin_turno As Date
        Dim no_registros As String
        Dim str_detalle_tc As String
        Dim str_encargado As String
        Dim dbl_registros As Double
        Dim strEncargadoTurno As String
        Dim cont_cerrado_todo_turno As Integer = 0


        Try
            If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
                int_turno = 5
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
                int_turno = 6
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
                int_turno = 4
            End If

            If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
                h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 06:00:00")
                h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 13:59:59")
                int_turno = 5
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
                h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
                h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 21:59:59")
                int_turno = 6
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
                'h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
                h_inicio_turno = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 22:00:00")
                'h_fin_turno = CDate(Format(DateAdd(DateInterval.Day, 1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 05:59:59")
                h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 05:59:59")
                int_turno = 4
            End If

            'If Len(id_plaza_cobro) = 3 Then
            '    If id_plaza_cobro <> 108 Then
            '        nombre_archivo = "0" & id_plaza_cobro
            '    Else
            '        nombre_archivo = "0001"
            '    End If
            'End If

            If Len(id_plaza_cobro) = 3 Then
                If id_plaza_cobro = 108 Then
                    nombre_archivo = "0001"
                ElseIf id_plaza_cobro = 109 Then
                    nombre_archivo = "001B"
                ElseIf id_plaza_cobro = 107 Then
                    nombre_archivo = "0107"
                ElseIf id_plaza_cobro = 106 Then
                    nombre_archivo = "0006"
                Else
                    nombre_archivo = "0" & id_plaza_cobro
                End If
            End If

            If Not Directory.Exists(dir_archivo) Then
                Directory.CreateDirectory(dir_archivo)
            End If

            nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "1" & strIdentificador

            Dim oSW As New StreamWriter(dir_archivo & nombre_archivo)
            archivo_1 = nombre_archivo
            'cabecera = "David Cabecera"
            'cabecera = "04"
            cabecera = cmbDelegacion.Tag

            'If Len(id_plaza_cobro) = 3 Then
            '    If id_plaza_cobro <> 108 Then
            '        cabecera = cabecera & "0" & id_plaza_cobro
            '    Else
            '        cabecera = cabecera & "0001"
            '    End If
            'End If

            If Len(id_plaza_cobro) = 3 Then
                If id_plaza_cobro = 108 Then
                    cabecera = cabecera & "0001"
                ElseIf id_plaza_cobro = 109 Then
                    cabecera = cabecera & "001B"
                ElseIf id_plaza_cobro = 107 Then
                    cabecera = cabecera & "0107"
                ElseIf id_plaza_cobro = 106 Then
                    cabecera = cabecera & "0006"
                Else
                    cabecera = cabecera & "0" & id_plaza_cobro

                End If
            End If



            cabecera = "03" & cabecera & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "1" & strIdentificador & Format(dt_Fecha_Inicio, "dd/MM/yyyy") & int_turno

            'CABECERA INICIO REGISTROS
            strQuerys = "SELECT	FIN_POSTE.Id_Gare, " &
  "TYPE_VOIE.libelle_court_voie_L2, " &
   "Voie, " &
 "'zzz', " &
   "TO_CHAR(Numero_Poste,'FM09'), " &
   "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " &
   "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " &
   "Matricule, " &
   "Sac, " &
   "FIN_POSTE.Id_Voie, " &
   "DATE_DEBUT_POSTE,Date_Fin_Poste, " &
   "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " &
   "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " &
  ",TYPE_VOIE.libelle_court_voie " &
  ",FIN_POSTE_CPT22, " &
  "ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " &
 "FROM 	TYPE_VOIE, " &
   "FIN_POSTE, " &
   "SITE_GARE " &
 "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " &
  "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " &
 "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " &
  "AND	SITE_GARE.id_reseau		= 	'01' " &
 "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
 "AND (Id_Mode_Voie IN (1,7,9)) " &
 "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
 "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
 "AND (FIN_POSTE.Id_Voie = '1' " &
  "OR FIN_POSTE.Id_Voie = '2' " &
  "OR FIN_POSTE.Id_Voie = '3' " &
  "OR FIN_POSTE.Id_Voie = '4' " &
  "OR FIN_POSTE.Id_Voie = 'X' " &
") " &
 "ORDER BY Id_Gare, " &
    "Id_Voie, " &
    "Voie, " &
    "Date_Debut_Poste," &
    "Date_Fin_Poste, " &
    "Numero_Poste, " &
    "Matricule " &
   ",Sac"

            '" & id_plaza_cobro - 100 & "'

            If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 1 Then

                dbl_registros = oDataSet.Tables("FIN_POSTE").Rows.Count
            Else
                dbl_registros = 0
            End If



            If Mid(id_plaza_cobro, 2, 2) = 84 Then



                'tramo corto
                strQuerys = "SELECT	FIN_POSTE.Id_Gare, " &
    "TYPE_VOIE.libelle_court_voie_L2, " &
    "Voie, " &
    "'zzz', " &
    "TO_CHAR(Numero_Poste,'FM09'), " &
    "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " &
    "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " &
    "Matricule, " &
    "Sac, " &
    "FIN_POSTE.Id_Voie, " &
    "DATE_DEBUT_POSTE,Date_Fin_Poste, " &
    "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " &
    "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " &
    ",TYPE_VOIE.libelle_court_voie " &
    ",FIN_POSTE_CPT22, " &
    "ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " &
    "FROM 	TYPE_VOIE, " &
    "FIN_POSTE, " &
    "SITE_GARE " &
    "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " &
    "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " &
    "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " &
    "AND	SITE_GARE.id_reseau		= 	'01' " &
    "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
    "AND (Id_Mode_Voie IN (1,7,9)) " &
    "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
    "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
    "AND (FIN_POSTE.Id_Voie = '1' " &
    "OR FIN_POSTE.Id_Voie = '2' " &
    "OR FIN_POSTE.Id_Voie = '3' " &
    "OR FIN_POSTE.Id_Voie = '4' " &
    "OR FIN_POSTE.Id_Voie = 'X' " &
    ")  and SUBSTR(Voie,1,1) = 'A'  " &
    "ORDER BY Id_Gare, " &
    "Id_Voie, " &
    "Voie, " &
    "Date_Debut_Poste," &
    "Date_Fin_Poste, " &
    "Numero_Poste, " &
    "Matricule " &
    ",Sac"

                '" & id_plaza_cobro - 100 & "'

                If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 1 Then

                    dbl_registros = dbl_registros + oDataSet.Tables("FIN_POSTE").Rows.Count
                Else
                    dbl_registros = dbl_registros + 0
                End If
                'fin tramo corto


            ElseIf Mid(id_plaza_cobro, 2, 2) = "02" Then


                'tramo corto
                strQuerys = "SELECT	FIN_POSTE.Id_Gare, " &
    "TYPE_VOIE.libelle_court_voie_L2, " &
    "Voie, " &
    "'zzz', " &
    "TO_CHAR(Numero_Poste,'FM09'), " &
    "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " &
    "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " &
    "Matricule, " &
    "Sac, " &
    "FIN_POSTE.Id_Voie, " &
    "DATE_DEBUT_POSTE,Date_Fin_Poste, " &
    "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " &
    "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " &
    ",TYPE_VOIE.libelle_court_voie " &
    ",FIN_POSTE_CPT22, " &
    "ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " &
    "FROM 	TYPE_VOIE, " &
    "FIN_POSTE, " &
    "SITE_GARE " &
    "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " &
    "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " &
    "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " &
    "AND	SITE_GARE.id_reseau		= 	'01' " &
    "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
    "AND (Id_Mode_Voie IN (1,7,9)) " &
    "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
    "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
    "AND (FIN_POSTE.Id_Voie = '1' " &
    "OR FIN_POSTE.Id_Voie = '2' " &
    "OR FIN_POSTE.Id_Voie = '3' " &
    "OR FIN_POSTE.Id_Voie = '4' " &
    "OR FIN_POSTE.Id_Voie = 'X' " &
    ")  and (Voie = 'A01' OR Voie = 'B08')  " &
    "ORDER BY Id_Gare, " &
    "Id_Voie, " &
    "Voie, " &
    "Date_Debut_Poste," &
    "Date_Fin_Poste, " &
    "Numero_Poste, " &
    "Matricule " &
    ",Sac"

                '" & id_plaza_cobro - 100 & "'

                If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 1 Then

                    dbl_registros = dbl_registros + oDataSet.Tables("FIN_POSTE").Rows.Count
                Else
                    dbl_registros = dbl_registros + 0
                End If
                'fin tramo corto


            End If

            strQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM, BAG_NUMBER, REPORT_FLAG, GENERATION_DHM " &
"FROM CLOSED_LANE_REPORT, SITE_GARE " &
"where " &
"CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"order by BEGIN_DHM"

            If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 1 Then
                dbl_registros = dbl_registros + oDataSet.Tables("CLOSED_LANE_REPORT").Rows.Count
            Else
                dbl_registros = dbl_registros + 0
            End If

            'carriles siempre cerrados
            'cont_cerrado_todo_turno

            strQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD "


            If id_plaza_cobro = 106 Then
                strQuerys = strQuerys & "where VOIE <> 'B04' and VOIE <> 'A03' "
            End If

            If objQuerys.QueryDataSetCuatro(strQuerys, "SEQ_VOIE_TOD") = 1 Then

                For cont2 = 0 To oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Count - 1

                    oDataRowCuatro = oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Item(cont2)

                    strQuerys = "SELECT	* FROM 	FIN_POSTE " &
                     "WHERE	 VOIE = '" & oDataRowCuatro("VOIE") & "' " &
                     "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
                     "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) "

                    If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 0 Then

                        strQuerys = "SELECT * " &
                        "FROM CLOSED_LANE_REPORT, SITE_GARE " &
                        "where " &
                        "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
                        "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
                        "AND	LANE		=	'" & oDataRowCuatro("VOIE") & "' " &
                        "AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
                        "AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
                        "order by BEGIN_DHM"

                        If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 0 Then
                            cont_cerrado_todo_turno = cont_cerrado_todo_turno + 1
                        End If

                    End If
                Next
            End If

            dbl_registros = dbl_registros + cont_cerrado_todo_turno

            'fin carriles siempre cerrados

            If Len(CStr(dbl_registros)) = 1 Then
                no_registros = "0000" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 2 Then
                no_registros = "000" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 3 Then
                no_registros = "00" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 4 Then
                no_registros = "0" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 5 Then
                no_registros = dbl_registros
            End If

            cabecera = cabecera & no_registros

            oSW.WriteLine(cabecera)
            'CABECERA FIN






            'inicio detalle
            strQuerys = "SELECT	FIN_POSTE.Id_Gare, " &
  "TYPE_VOIE.libelle_court_voie_L2, " &
   "Voie, " &
 "'zzz', " &
   "TO_CHAR(Numero_Poste,'FM09'), " &
   "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " &
   "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " &
   "Matricule, " &
   "Sac, " &
   "FIN_POSTE.Id_Voie, " &
   "DATE_DEBUT_POSTE,Date_Fin_Poste, " &
   "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " &
   "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " &
  ",TYPE_VOIE.libelle_court_voie " &
  ",FIN_POSTE_CPT22, " &
  "ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " &
 "FROM 	TYPE_VOIE, " &
   "FIN_POSTE, " &
   "SITE_GARE " &
 "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " &
  "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " &
 "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " &
  "AND	SITE_GARE.id_reseau		= 	'01' " &
 "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
 "AND (Id_Mode_Voie IN (1,7,9)) " &
 "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
 "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
 "AND (FIN_POSTE.Id_Voie = '1' " &
  "OR FIN_POSTE.Id_Voie = '2' " &
  "OR FIN_POSTE.Id_Voie = '3' " &
  "OR FIN_POSTE.Id_Voie = '4' " &
  "OR FIN_POSTE.Id_Voie = 'X' " &
") " &
 "ORDER BY Id_Gare, " &
    "Id_Voie, " &
    "Voie, " &
    "Date_Debut_Poste," &
    "Date_Fin_Poste, " &
    "Numero_Poste, " &
    "Matricule " &
   ",Sac"

            If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 1 Then

                'no_carriles = oDataSet.Tables("FIN_POSTE").Rows.Count

                'cabecera = cabecera & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "P" & Format(dt_Fecha_Inicio, "dd/MM/yyyy") & int_turno

                'If Len(CStr(oDataSet.Tables("FIN_POSTE").Rows.Count)) = 1 Then
                '    no_registros = "0000" & oDataSet.Tables("FIN_POSTE").Rows.Count
                'ElseIf Len(CStr(oDataSet.Tables("FIN_POSTE").Rows.Count)) = 2 Then
                '    no_registros = "000" & oDataSet.Tables("FIN_POSTE").Rows.Count
                'ElseIf Len(CStr(oDataSet.Tables("FIN_POSTE").Rows.Count)) = 3 Then
                '    no_registros = "00" & oDataSet.Tables("FIN_POSTE").Rows.Count
                'ElseIf Len(CStr(oDataSet.Tables("FIN_POSTE").Rows.Count)) = 4 Then
                '    no_registros = "0" & oDataSet.Tables("FIN_POSTE").Rows.Count
                'ElseIf Len(CStr(oDataSet.Tables("FIN_POSTE").Rows.Count)) = 5 Then
                '    no_registros = oDataSet.Tables("FIN_POSTE").Rows.Count
                'End If

                'cabecera = cabecera & no_registros

                'oSW.WriteLine(cabecera)


                For cont = 0 To oDataSet.Tables("FIN_POSTE").Rows.Count - 1

                    oDataRow = oDataSet.Tables("FIN_POSTE").Rows.Item(cont)

                    str_detalle = ""
                    str_detalle_tc = ""

                    'Fecha base de operación 	Fecha 	dd/mm/aaaa
                    'str_detalle = Format(oDataRow("DATE_DEBUT_POSTE"), "dd/MM/yyyy") & ","
                    'Format(dt_Fecha_Inicio, "MM/dd/yyyy")
                    str_detalle = Format(dt_Fecha_Inicio, "dd/MM/yyyy") & ","
                    'Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
                    str_detalle = str_detalle & int_turno & ","
                    'Hora inicial de operación 	Caracter 	hhmmss 	
                    str_detalle = str_detalle & Format(oDataRow("DATE_DEBUT_POSTE"), "HHmmss") & ","
                    'Hora final de operación 	Caracter 	hhmmss 	
                    str_detalle = str_detalle & Format(oDataRow("Date_Fin_Poste"), "HHmmss") & ","
                    'Clave de tramo	Entero 	>9	Valores posibles:  Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                    'Verificar 
                    'str_detalle = str_detalle & "247" & ","
                    'Número de carril 	Entero 	>>9	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                    'If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                    '    str_detalle = str_detalle & "2585" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                    '    str_detalle = str_detalle & "2586" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                    '    str_detalle = str_detalle & "2587" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                    '    str_detalle = str_detalle & "2588" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                    '    str_detalle = str_detalle & "2589" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                    '    str_detalle = str_detalle & "2590" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                    '    str_detalle = str_detalle & "2591" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                    '    str_detalle = str_detalle & "2592" & ","
                    'End If

                    'ConversionNoCarril()
                    str_detalle_tc = str_detalle

                    If id_plaza_cobro = 184 Then
                        str_detalle = str_detalle & "247" & ","

                        If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then
                            str_detalle_tc = str_detalle_tc & "340" & ","
                        End If

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2585" & ","
                            str_detalle_tc = str_detalle_tc & "2585" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2586" & ","
                            str_detalle_tc = str_detalle_tc & "2586" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "2587" & ","
                            str_detalle_tc = str_detalle_tc & "2587" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "2588" & ","
                            str_detalle_tc = str_detalle_tc & "2588" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "2589" & ","
                            str_detalle_tc = str_detalle_tc & "2589" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "2590" & ","
                            str_detalle_tc = str_detalle_tc & "2590" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "2591" & ","
                            str_detalle_tc = str_detalle_tc & "2591" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "2592" & ","
                            str_detalle_tc = str_detalle_tc & "2592" & ","
                        End If

                        'paso morelos
                    ElseIf id_plaza_cobro = 102 Then

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "249" & ","
                            str_detalle = str_detalle & "1803" & ","
                            str_detalle_tc = str_detalle_tc & "261" & ","
                            str_detalle_tc = str_detalle_tc & "1803" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1804" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1805" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1806" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1807" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1808" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1809" & ","

                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "249" & ","
                            str_detalle = str_detalle & "1810" & ","

                            str_detalle_tc = str_detalle_tc & "261" & ","
                            str_detalle_tc = str_detalle_tc & "1810" & ","

                            '--------------------------------------------
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1811" & ","

                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1812" & ","
                        End If


                        'la venta
                    ElseIf id_plaza_cobro = 104 Then
                        str_detalle = str_detalle & "252" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1830" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1831" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1832" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1833" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1834" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1835" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1836" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1837" & ","
                        End If
                        'Libramiento
                    ElseIf id_plaza_cobro = 161 Then


                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "364" & "," & "2681" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "364" & "," & "2682" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "363" & "," & "2683" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "363" & "," & "2684" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "364" & "," & "2685" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "364" & "," & "2686" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "363" & "," & "2687" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "363" & "," & "2688" & ","
                        End If
                        'PALO BLANCO
                    ElseIf id_plaza_cobro = 103 Then
                        str_detalle = str_detalle & "251" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1816" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1817" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1818" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1819" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1820" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1821" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1822" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1823" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1824" & ","
                        End If

                        'álpuyeca
                    ElseIf id_plaza_cobro = 101 Then
                        str_detalle = str_detalle & "246" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1794" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1795" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1796" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1797" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1798" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1799" & ","
                        End If

                        'álpuyeca
                        '101
                        '246
                        '1	1794
                        '2	1795
                        '3	1796
                        '4	1797

                        'aeropuerto
                        '106
                        '1		367	2734	B
                        '2		366	2735	A
                        '3		367	2736	B


                        'Tlalpan
                    ElseIf id_plaza_cobro = 108 Then

                        str_detalle = str_detalle & "118" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            str_detalle = str_detalle & "3076" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "3063" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "3064" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "3065" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "3066" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "3067" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "3068" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "3069" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "3070" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "3071" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "3072" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "3073" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            str_detalle = str_detalle & "3074" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            str_detalle = str_detalle & "3075" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                            str_detalle = str_detalle & "3077" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                            str_detalle = str_detalle & "3078" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                            str_detalle = str_detalle & "3079" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                            str_detalle = str_detalle & "3080" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                            str_detalle = str_detalle & "3081" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                            str_detalle = str_detalle & "3082" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                            str_detalle = str_detalle & "3083" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                            str_detalle = str_detalle & "3084" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                            str_detalle = str_detalle & "3085" & ","
                        End If

                        'xochitepec
                    ElseIf id_plaza_cobro = 105 Then

                        str_detalle = str_detalle & "365" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2727" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2728" & ","
                        End If
                        'CERRO GORDO
                    ElseIf id_plaza_cobro = 186 Then

                        str_detalle = str_detalle & "351" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                            str_detalle = str_detalle & "3199" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                            str_detalle = str_detalle & "3200" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                            str_detalle = str_detalle & "3201" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                            str_detalle = str_detalle & "3202" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                            str_detalle = str_detalle & "3203" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            'str_detalle = str_detalle & "3185" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            'str_detalle = str_detalle & "3186" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            'str_detalle = str_detalle & "3187" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            'str_detalle = str_detalle & "3188" & ","
                        End If

                        'Queretaro
                    ElseIf id_plaza_cobro = 106 Then
                        str_detalle = str_detalle & "112" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1079" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1080" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1081" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1082" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1083" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1084" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1085" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1086" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1087" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "1088" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "1089" & ","
                        End If

                        'VillaGrand
                    ElseIf id_plaza_cobro = 183 Then

                        str_detalle = str_detalle & "170" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2581" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2582" & ","
                        End If
                        'VillaGrand

                        'tres marias
                    ElseIf id_plaza_cobro = 109 Then

                        str_detalle = str_detalle & "102" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1020" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1021" & ","
                        End If
                        'Central de Abastos
                    ElseIf id_plaza_cobro = 107 Then
                        str_detalle = str_detalle & "368" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1843" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1844" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1845" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1846" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1847" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1848" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1849" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1850" & ","
                            'Segmento A
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1851" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "1852" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "1853" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            str_detalle = str_detalle & "1854" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            str_detalle = str_detalle & "2743" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            str_detalle = str_detalle & "2744" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                            str_detalle = str_detalle & "2745" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                            str_detalle = str_detalle & "2746" & ","
                        End If

                    ElseIf id_plaza_cobro = 189 Then
                        str_detalle = str_detalle & "189" & ","
                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1891" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1892" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1893" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1894" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1895" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1896" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1897" & ","
                        End If
                    Else
                        str_detalle = str_detalle & ","
                        str_detalle = str_detalle & ","




                    End If





                    'Cuerpo 	Caracter 	X(1)	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                    str_detalle = str_detalle & Mid(Trim(oDataRow("Voie")), 1, 1) & ","
                    str_detalle_tc = str_detalle_tc & Mid(Trim(oDataRow("Voie")), 1, 1) & ","
                    'Identificador de operación	Caracter 	X(2)	Valores posibles:  Tabla 17 - Códigos de Operación por Carril.
                    strQuerys = "SELECT	LANE_ASSIGN.Id_plaza,LANE_ASSIGN.Id_lane,TO_CHAR(LANE_ASSIGN.MSG_DHM,'MM/DD/YY HH24:MI:SS'),LANE_ASSIGN.SHIFT_NUMBER,LANE_ASSIGN.OPERATION_ID, " &
"LANE_ASSIGN.DELEGATION, TO_CHAR(LANE_ASSIGN.ASSIGN_DHM,'MM/DD/YY'),LTRIM(TO_CHAR(LANE_ASSIGN.JOB_NUMBER,'09')),	LANE_ASSIGN.STAFF_NUMBER,LANE_ASSIGN.IN_CHARGE_SHIFT_NUMBER " &
"FROM 	LANE_ASSIGN, SITE_GARE " &
"WHERE	LANE_ASSIGN.id_NETWORK = SITE_GARE.id_Reseau " &
"AND LANE_ASSIGN.id_plaza = SITE_GARE.id_Gare " &
"AND SITE_GARE.id_reseau = '01' " &
"AND	SITE_GARE.id_Site ='" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND LANE_ASSIGN.Id_lane = '" & Trim(oDataRow("Voie")) & "' " &
"AND ((MSG_DHM >= TO_DATE('" & Format(oDataRow("DATE_DEBUT_POSTE"), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (MSG_DHM <= TO_DATE('" & Format(oDataRow("DATE_DEBUT_POSTE"), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"ORDER BY LANE_ASSIGN.Id_PLAZA, LANE_ASSIGN.Id_LANE, LANE_ASSIGN.MSG_DHM"

                    If objQuerys.QueryDataSetDos(strQuerys, "Asig_Carril") = 1 Then
                        str_detalle = str_detalle & oDataRowDos("OPERATION_ID") & ","
                        str_detalle_tc = str_detalle_tc & oDataRowDos("OPERATION_ID") & ","

                        str_encargado = oDataRowDos("IN_CHARGE_SHIFT_NUMBER")
                        strEncargadoTurno = oDataRowDos("IN_CHARGE_SHIFT_NUMBER")
                    Else
                        str_detalle = str_detalle & "Pendiente,"
                        str_encargado = "Pendiente,"
                    End If



                    'No. empleado C-R 	Entero 	>>>>>9	
                    'str_detalle = str_detalle & oDataRow("Matricule") & ","




                    'If oDataRow("Matricule") = "570700" Then
                    '    str_detalle = str_detalle & "904166" & ","
                    '    'str_encargado = "904166"
                    'ElseIf oDataRow("Matricule") = "511118" Then
                    '    str_detalle = str_detalle & "904067" & ","
                    '    'str_encargado = "904067"
                    'ElseIf oDataRow("Matricule") = "555552" Then
                    '    str_detalle = str_detalle & "904094" & ","
                    '    'str_encargado = "904094"

                    '    '904628 - 444440
                    'ElseIf oDataRow("Matricule") = "444440" Then
                    '    str_detalle = str_detalle & "904628" & ","

                    '    'String: "513312"
                    'ElseIf oDataRow("Matricule") = "513312" Then
                    '    str_detalle = str_detalle & "904628" & ","

                    '    'String: "513288"
                    'ElseIf oDataRow("Matricule") = "513288" Then
                    '    str_detalle = str_detalle & "904628" & ","
                    'Else
                    '    str_detalle = str_detalle & "pendiente" & ","
                    'End If

                    If Trim(objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", oDataRow("Matricule"))) <> "" Then
                        str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", oDataRow("Matricule")) & ","
                        str_detalle_tc = str_detalle_tc & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", oDataRow("Matricule")) & ","
                    Else
                        str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado) & ","
                        str_detalle_tc = str_detalle_tc & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado) & ","
                    End If




                    'No. empleado encargado de turno 	Entero 	>>>>>9 	
                    'str_detalle = str_detalle & str_encargado & ","

                    'If str_encargado = "570700" Then
                    '    str_encargado = "904166"
                    'ElseIf str_encargado = "511118" Then
                    '    str_encargado = "904067"
                    'ElseIf str_encargado = "555552" Then
                    '    str_encargado = "904094"

                    '    '904628 - 444440
                    'ElseIf str_encargado = "444440" Then
                    '    str_encargado = "904628"

                    '    'String: "513312"
                    'ElseIf str_encargado = "513312" Then
                    '    str_encargado = "904628"

                    '    '   'String: "513288"
                    'ElseIf str_encargado = "513288" Then
                    '    str_encargado = "904628"

                    'Else
                    '    str_encargado = "pendiente"
                    'End If

                    'str_detalle = str_detalle & str_encargado & ","

                    str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado) & ","
                    str_detalle_tc = str_detalle_tc & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado) & ","

                    'If Trim(objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado)) <> "" Then
                    '    str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado) & ","
                    'Else
                    '    str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", "encargado_plaza") & ","
                    'End If



                    'No. empleado Admón. Gral. 	Entero 	>>>>>9 	
                    'str_detalle = str_detalle & "904067,"
                    'If id_plaza_cobro = 184 Then
                    '    str_detalle = str_detalle & "904628,"
                    'ElseIf id_plaza_cobro = 103 Then
                    '    str_detalle = str_detalle & "904326,"
                    'Else
                    '    str_detalle = str_detalle & "PENDIENTE,"
                    'End If

                    str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", "encargado_plaza") & ","
                    str_detalle_tc = str_detalle_tc & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", "encargado_plaza") & ","


                    'No. de control de preliquidación  	Entero 	>>>9 
                    Dim strSac As String
                    strSac = IIf(IsDBNull(oDataRow("Sac")), "", oDataRow("Sac"))
                    'str_detalle = str_detalle & oDataRow("Sac") & ","
                    strSac = Replace(strSac, "A", "")
                    strSac = Replace(strSac, "B", "")
                    str_detalle = str_detalle & strSac & ","
                    str_detalle_tc = str_detalle_tc & strSac & ","

                    str_detalle = str_detalle.Replace("X", "N")
                    oSW.WriteLine(str_detalle)

                    If Mid(id_plaza_cobro, 2, 2) = 84 Then


                        If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then
                            oSW.WriteLine(str_detalle_tc)

                        End If

                    ElseIf Mid(id_plaza_cobro, 2, 2) = "02" Then

                        If Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "B08" Then
                            oSW.WriteLine(str_detalle_tc)

                        End If

                    End If
                    '----------------------
                    'tramo corto
                    'tramo corto 17
                    '               strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " & _
                    '    "FROM GEADBA.TRANSACTION,SITE_GARE " & _
                    '     "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " & _
                    '    "AND (DATE_TRANSACTION >= TO_DATE('" & Format(oDataRow("DATE_DEBUT_POSTE"), "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " & _
                    '    "AND (DATE_TRANSACTION <= TO_DATE('" & Format(oDataRow("Date_Fin_Poste"), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                    '               strQuerys = strQuerys & "AND (MATRICULE = '" & oDataRowDos("OPERATION_ID") & "') " & _
                    '               "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " & _
                    '               "AND TRANSACTION.ID_GARE = " & int_id_gare & " " & _
                    '               "AND ID_VOIE = " & CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) & " " & _
                    '               "AND VOIE = '" & str_id_voie & "' "

                    '               strQuerys = strQuerys & "AND " & _
                    '    "ID_PAIEMENT = 2 " & _
                    '                        "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') "

                    '               If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    '                   VAR_17_tc = oDataRow("Cruces") 'IIf(IsDBNull(oDataRow("Cruces")), 0 + VAR_13, oDataRow("Cruces") + VAR_13)
                    '               End If

                    '               'fin tramo corto 17

                    '               'trmao cotro var 19
                    '               strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " & _
                    '"FROM GEADBA.TRANSACTION,SITE_GARE " & _
                    ' "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " & _
                    '"AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " & _
                    '"AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                    '               strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " & _
                    '               "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " & _
                    '               "AND TRANSACTION.ID_GARE = " & int_id_gare & " " & _
                    '               "AND ID_VOIE = " & int_id_voie & " " & _
                    '               "AND VOIE = '" & str_id_voie & "' "

                    '               strQuerys = strQuerys & "AND  " & _
                    '                    " ID_PAIEMENT = 2 " & _
                    '                                            "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') "

                    '               If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    '                   VAR_19_tc = IIf(IsDBNull(oDataRow("Cruces")), 0, oDataRow("Cruces"))
                    '               End If
                    'fin tramo corto var 19
                    'fin tramo corto

                Next

            Else
                'cabecera = cabecera & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "P" & Format(dt_Fecha_Inicio, "dd/MM/yyyy") & int_turno & "00000"


                'oSW.WriteLine(cabecera)
            End If
            'fin detalle

            'inicio carriles cerrados
            strQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM, BAG_NUMBER, REPORT_FLAG, GENERATION_DHM " &
     "FROM CLOSED_LANE_REPORT, SITE_GARE " &
     "where " &
     "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
      "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
      "AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
    "AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
    "order by BEGIN_DHM"

            If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 1 Then

                'no_carriles = oDataSet.Tables("FIN_POSTE").Rows.Count

                For cont = 0 To oDataSet.Tables("CLOSED_LANE_REPORT").Rows.Count - 1

                    oDataRow = oDataSet.Tables("CLOSED_LANE_REPORT").Rows.Item(cont)

                    str_detalle = ""

                    'Fecha base de operación 	Fecha 	dd/mm/aaaa
                    'str_detalle = Format(oDataRow("BEGIN_DHM"), "dd/MM/yyyy") & ","
                    'Format(dt_Fecha_Inicio, "dd/MM/yyyy") & ","
                    str_detalle = Format(dt_Fecha_Inicio, "dd/MM/yyyy") & ","

                    'Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
                    str_detalle = str_detalle & int_turno & ","
                    'Hora inicial de operación 	Caracter 	hhmmss 	
                    str_detalle = str_detalle & Format(oDataRow("BEGIN_DHM"), "HHmmss") & ","
                    'Hora final de operación 	Caracter 	hhmmss 	


                    'h_fin_turno
                    If oDataRow("END_DHM") > h_fin_turno Then
                        str_detalle = str_detalle & Format(h_fin_turno, "HHmmss") & ","
                    Else
                        str_detalle = str_detalle & Format(oDataRow("END_DHM"), "HHmmss") & ","
                    End If



                    'Clave de tramo	Entero 	>9	Valores posibles:  Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                    'Verificar 
                    'str_detalle = str_detalle & "247" & ","
                    ''Número de carril 	Entero 	>>9	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                    'If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                    '    str_detalle = str_detalle & "LANE" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                    '    str_detalle = str_detalle & "LANE" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                    '    str_detalle = str_detalle & "2587" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                    '    str_detalle = str_detalle & "2588" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                    '    str_detalle = str_detalle & "2589" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                    '    str_detalle = str_detalle & "2590" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                    '    str_detalle = str_detalle & "2591" & ","
                    'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                    '    str_detalle = str_detalle & "2592" & ","
                    'End If
                    If id_plaza_cobro = 184 Then
                        str_detalle = str_detalle & "247" & ","

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2585" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2586" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "2587" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "2588" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "2589" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "2590" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "2591" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "2592" & ","
                        End If


                        'paso morelos
                    ElseIf id_plaza_cobro = 102 Then

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "249" & ","
                            str_detalle = str_detalle & "1803" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1804" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1805" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1806" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1807" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1808" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1809" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "261" & ","
                            str_detalle = str_detalle & "1810" & ","
                            '-------------------------------------------------
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1811" & ","

                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1812" & ","
                        End If

                        'la venta
                    ElseIf id_plaza_cobro = 104 Then
                        str_detalle = str_detalle & "252" & ","
                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1830" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1831" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1832" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1833" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1834" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1835" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1836" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1837" & ","
                        End If
                        'la venta
                    ElseIf id_plaza_cobro = 161 Then

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "364" & "," & "2681" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "364" & "," & "2682" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "363" & "," & "2683" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "363" & "," & "2684" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "364" & "," & "2685" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "364" & "," & "2686" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "363" & "," & "2687" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "363" & "," & "2688" & ","
                        End If
                        'Central de Abastos
                    ElseIf id_plaza_cobro = 107 Then
                        str_detalle = str_detalle & "368" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1843" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1844" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1845" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1846" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1847" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1848" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1849" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1850" & ","
                            'Segmento A
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1851" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "1852" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "1853" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                            str_detalle = str_detalle & "1854" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                            str_detalle = str_detalle & "2743" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                            str_detalle = str_detalle & "2744" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "15" Then
                            str_detalle = str_detalle & "2745" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "16" Then
                            str_detalle = str_detalle & "2746" & ","
                        End If
                    ElseIf id_plaza_cobro = 103 Then
                        str_detalle = str_detalle & "251" & ","

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1816" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1817" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1818" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1819" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1820" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1821" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1822" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1823" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1824" & ","
                        End If

                        'álpuyeca
                        '101
                        '246
                        '1	1794
                        '2	1795
                        '3	1796
                        '4	1797
                    ElseIf id_plaza_cobro = 101 Then
                        str_detalle = str_detalle & "246" & ","

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1794" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1795" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1796" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1797" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1798" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1799" & ","
                        End If


                        'aeropuerto

                        'tlalpan
                    ElseIf id_plaza_cobro = 108 Then

                        str_detalle = str_detalle & "118" & ","


                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "3063" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "3064" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "3065" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "3066" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "3067" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "3068" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "3069" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "3070" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "3071" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "3072" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "3073" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                            str_detalle = str_detalle & "3074" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                            str_detalle = str_detalle & "3075" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                            str_detalle = str_detalle & "3076" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "15" Then
                            str_detalle = str_detalle & "3077" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "16" Then
                            str_detalle = str_detalle & "3078" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "17" Then
                            str_detalle = str_detalle & "3079" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "18" Then
                            str_detalle = str_detalle & "3080" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "19" Then
                            str_detalle = str_detalle & "3081" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "20" Then
                            str_detalle = str_detalle & "3082" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "21" Then
                            str_detalle = str_detalle & "3083" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "22" Then
                            str_detalle = str_detalle & "3084" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "23" Then
                            str_detalle = str_detalle & "3085" & ","

                        End If

                        'xochitepec
                    ElseIf id_plaza_cobro = 105 Then

                        str_detalle = str_detalle & "365" & ","

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2727" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2728" & ","
                        End If
                        'CERRO GORDO
                    ElseIf id_plaza_cobro = 186 Then

                        str_detalle = str_detalle & "351" & ","

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "21" Then
                            str_detalle = str_detalle & "3199" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "22" Then
                            str_detalle = str_detalle & "3200" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "23" Then
                            str_detalle = str_detalle & "3201" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "24" Then
                            str_detalle = str_detalle & "3202" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "25" Then
                            str_detalle = str_detalle & "3203" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            'str_detalle = str_detalle & "3185" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            'str_detalle = str_detalle & "3186" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            'str_detalle = str_detalle & "3187" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                            'str_detalle = str_detalle & "3188" & ","
                            'End If
                        End If
                        'QUERETARO
                    ElseIf id_plaza_cobro = 106 Then
                        str_detalle = str_detalle & "112" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1079" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1080" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1081" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1082" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1083" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1084" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1085" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1086" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1087" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "1088" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "1089" & ","
                        End If

                        'VillaGrand
                    ElseIf id_plaza_cobro = 183 Then

                        str_detalle = str_detalle & "170" & ","

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2581" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2582" & ","
                        End If

                        'tres marias
                    ElseIf id_plaza_cobro = 109 Then

                        str_detalle = str_detalle & "102" & ","

                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1020" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1021" & ","
                        End If

                        'Central de Abastos
                    ElseIf id_plaza_cobro = 107 Then
                        str_detalle = str_detalle & "368" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1843" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1844" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1845" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1846" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1847" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1848" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1849" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1850" & ","
                            'Segmento A
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1851" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "1852" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "1853" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                            str_detalle = str_detalle & "1854" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                            str_detalle = str_detalle & "2743" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                            str_detalle = str_detalle & "2744" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "15" Then
                            str_detalle = str_detalle & "2745" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "16" Then
                            str_detalle = str_detalle & "2746" & ","
                        End If



                    ElseIf id_plaza_cobro = 189 Then
                        str_detalle = str_detalle & "189" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1891" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1892" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1893" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1894" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1895" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1896" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1897" & ","
                        End If
                        'SAN MARCOS
                        'ElseIf id_plaza_cobro = 107 Then

                        '    str_detalle = str_detalle & "121" & ","

                        '    If Mid(Trim(oDataRow("LANE")), 1, 1) = "A" Then

                        '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        '            str_detalle = str_detalle & "1102" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        '            str_detalle = str_detalle & "1103" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        '            str_detalle = str_detalle & "1104" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        '            str_detalle = str_detalle & "1105" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                        '            str_detalle = str_detalle & "1106" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                        '            str_detalle = str_detalle & "1107" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                        '            str_detalle = str_detalle & "1108" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                        '            str_detalle = str_detalle & "1109" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                        '            str_detalle = str_detalle & "1110" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "15" Then
                        '            str_detalle = str_detalle & "1101" & ","
                        '        End If

                        '    ElseIf Mid(Trim(oDataRow("LANE")), 1, 1) = "B" Then

                        '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        '            str_detalle = str_detalle & "1097" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        '            str_detalle = str_detalle & "1098" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        '            str_detalle = str_detalle & "1099" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        '            str_detalle = str_detalle & "1100" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        '            str_detalle = str_detalle & "1101" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        '            str_detalle = str_detalle & "1102" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        '            str_detalle = str_detalle & "1103" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        '            str_detalle = str_detalle & "1104" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        '            str_detalle = str_detalle & "1105" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                        '            str_detalle = str_detalle & "1106" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                        '            str_detalle = str_detalle & "1107" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                        '            str_detalle = str_detalle & "1108" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                        '            str_detalle = str_detalle & "1109" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                        '            str_detalle = str_detalle & "1110" & ","
                        '        End If

                        '    End If





                    Else
                        str_detalle = str_detalle & ","
                        str_detalle = str_detalle & ","
                    End If


                    'Cuerpo 	Caracter 	X(1)	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                    str_detalle = str_detalle & Mid(Trim(oDataRow("LANE")), 1, 1) & ","
                    'Identificador de operación	Caracter 	X(2)	Valores posibles:  Tabla 17 - Códigos de Operación por Carril.
                    strQuerys = "SELECT	LANE_ASSIGN.Id_plaza,LANE_ASSIGN.Id_lane,TO_CHAR(LANE_ASSIGN.MSG_DHM,'MM/DD/YY HH24:MI:SS'),LANE_ASSIGN.SHIFT_NUMBER,LANE_ASSIGN.OPERATION_ID, " &
"LANE_ASSIGN.DELEGATION, TO_CHAR(LANE_ASSIGN.ASSIGN_DHM,'MM/DD/YY'),LTRIM(TO_CHAR(LANE_ASSIGN.JOB_NUMBER,'09')),	LANE_ASSIGN.STAFF_NUMBER,LANE_ASSIGN.IN_CHARGE_SHIFT_NUMBER " &
"FROM 	LANE_ASSIGN, SITE_GARE " &
 "WHERE	LANE_ASSIGN.id_NETWORK = SITE_GARE.id_Reseau " &
    "AND LANE_ASSIGN.id_plaza = SITE_GARE.id_Gare " &
    "AND SITE_GARE.id_reseau = '01' " &
    "AND	SITE_GARE.id_Site = '" & Mid(id_plaza_cobro, 2, 2) & "' " &
    "AND LANE_ASSIGN.Id_lane = '" & Trim(oDataRow("LANE")) & "' " &
 "AND ((MSG_DHM >= TO_DATE('" & Format(oDataRow("BEGIN_DHM"), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (MSG_DHM <= TO_DATE('" & Format(oDataRow("BEGIN_DHM"), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
    "ORDER BY LANE_ASSIGN.Id_PLAZA, LANE_ASSIGN.Id_LANE, LANE_ASSIGN.MSG_DHM"

                    If objQuerys.QueryDataSetDos(strQuerys, "Asig_Carril") = 1 Then
                        str_detalle = str_detalle & oDataRowDos("OPERATION_ID") & ","
                        str_encargado = oDataRowDos("IN_CHARGE_SHIFT_NUMBER")
                        strEncargadoTurno = oDataRowDos("IN_CHARGE_SHIFT_NUMBER")
                    Else
                        'str_detalle = str_detalle & "Pendiente,"
                        str_detalle = str_detalle & "X" & Mid(Trim(oDataRow("LANE")), 1, 1) & ","
                        'str_encargado = "Pendiente,"

                        'checar
                        str_encargado = "449902"
                        strEncargadoTurno = "449902"

                    End If

                    'No. empleado C-R 	Entero 	>>>>>9	


                    'Wendy 904166 - 570700
                    'inocente 904067 - 511118
                    'wilfrido 904094 - 555552

                    'If str_encargado = "570700" Then
                    '    str_encargado = "904166"
                    'ElseIf str_encargado = "511118" Then
                    '    str_encargado = "904067"
                    'ElseIf str_encargado = "555552" Then
                    '    str_encargado = "904094"

                    '    '904628 - 444440
                    'ElseIf str_encargado = "444440" Then
                    '    str_encargado = "904628"

                    '    ' 'String: "513312"
                    'ElseIf str_encargado = "513312" Then
                    '    str_encargado = "904628"

                    '    ' '   'String: "513288"
                    'ElseIf str_encargado = "513288" Then
                    '    str_encargado = "904628"
                    'Else
                    '    str_encargado = "pendiente"
                    'End If

                    'str_detalle = str_detalle & str_encargado & ","
                    str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado) & ","

                    'No. empleado encargado de turno 	Entero 	>>>>>9 	
                    'str_detalle = str_detalle & str_encargado & ","
                    str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", str_encargado) & ","
                    'No. empleado Admón. Gral. 	Entero 	>>>>>9 	
                    'str_detalle = str_detalle & "511118,"
                    'If id_plaza_cobro = 184 Then
                    '    str_detalle = str_detalle & "904628,"
                    'ElseIf id_plaza_cobro = 103 Then
                    '    str_detalle = str_detalle & "904326,"
                    'Else
                    '    str_detalle = str_detalle & "PENDIENTE,"
                    'End If
                    str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", "encargado_plaza") & ","

                    'No. de control de preliquidación  	Entero 	>>>9 	
                    str_detalle = str_detalle & ","

                    oSW.WriteLine(str_detalle)

                    '----------------------


                Next



            End If


            'CARRILES CERRADOS DOS
            'SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD;

            '************************************************
            '************************************************
            strQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD "

            If id_plaza_cobro = 106 Then
                strQuerys = strQuerys & "where VOIE <> 'B04' and VOIE <> 'A03' "
            End If

            If objQuerys.QueryDataSetCuatro(strQuerys, "SEQ_VOIE_TOD") = 1 Then

                For cont2 = 0 To oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Count - 1

                    oDataRowCuatro = oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Item(cont2)


                    strQuerys = "SELECT	* FROM 	FIN_POSTE " &
                        "WHERE	VOIE = '" & oDataRowCuatro("VOIE") & "' " &
                        "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
                        "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) "

                    If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 0 Then


                        strQuerys = "SELECT * " &
"FROM CLOSED_LANE_REPORT, SITE_GARE " &
"where " &
"CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
 "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
 "AND	LANE		=	'" & oDataRowCuatro("VOIE") & "' " &
 "AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"order by BEGIN_DHM"

                        If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 0 Then




                            str_detalle = ""
                            'Fecha base de operación 	Fecha 	dd/mm/aaaa
                            str_detalle = Format(dt_Fecha_Inicio, "dd/MM/yyyy") & ","
                            'Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
                            str_detalle = str_detalle & int_turno & ","
                            'Hora inicial de operación 	Caracter 	hhmmss 	
                            str_detalle = str_detalle & Format(h_inicio_turno, "HHmmss") & ","
                            'Hora final de operación 	Caracter 	hhmmss 	
                            'str_detalle = str_detalle & Format(h_fin_turno, "HHmmss") & ","
                            str_detalle = str_detalle & Format(DateAdd(DateInterval.Second, 1, h_fin_turno), "HHmmss") & ","
                            '                        ''Número de carril 	Entero 	>>9	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                            If id_plaza_cobro = 184 Then
                                str_detalle = str_detalle & "247" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2585" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2586" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "2587" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "2588" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "2589" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "2590" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "2591" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "2592" & ","
                                End If

                                'paso morelos
                            ElseIf id_plaza_cobro = 102 Then

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "249" & ","
                                    str_detalle = str_detalle & "1803" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1804" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1805" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1806" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1807" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1808" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1809" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "261" & ","
                                    str_detalle = str_detalle & "1810" & ","
                                    '-------------------------------------------------
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1811" & ","

                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1812" & ","
                                End If

                                'la venta
                            ElseIf id_plaza_cobro = 104 Then
                                str_detalle = str_detalle & "252" & ","
                                If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1830" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1831" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1832" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1833" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1834" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1835" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1836" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1837" & ","
                                End If

                                'la venta
                            ElseIf id_plaza_cobro = 161 Then

                                If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "364" & "," & "2681" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "364" & "," & "2682" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "363" & "," & "2683" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "363" & "," & "2684" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "364" & "," & "2685" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "364" & "," & "2686" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "363" & "," & "2687" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "363" & "," & "2688" & ","
                                End If

                            ElseIf id_plaza_cobro = 103 Then
                                str_detalle = str_detalle & "251" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1816" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1817" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1818" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1819" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1820" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1821" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1822" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1823" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1824" & ","
                                End If

                                'álpuyeca
                                '101
                                '246
                                '1	1794
                                '2	1795
                                '3	1796
                                '4	1797
                            ElseIf id_plaza_cobro = 101 Then
                                str_detalle = str_detalle & "246" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1794" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1795" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1796" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1797" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1798" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1799" & ","
                                End If


                                'tlalpan
                            ElseIf id_plaza_cobro = 108 Then

                                str_detalle = str_detalle & "118" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                                    str_detalle = str_detalle & "3076" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "3063" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "3064" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "3065" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "3066" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "3067" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "3068" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "3069" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "3070" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "3071" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "3072" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "3073" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                                    str_detalle = str_detalle & "3074" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                                    str_detalle = str_detalle & "3075" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "15" Then
                                    str_detalle = str_detalle & "3077" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "16" Then
                                    str_detalle = str_detalle & "3078" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "17" Then
                                    str_detalle = str_detalle & "3079" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "18" Then
                                    str_detalle = str_detalle & "3080" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "19" Then
                                    str_detalle = str_detalle & "3081" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "20" Then
                                    str_detalle = str_detalle & "3082" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "21" Then
                                    str_detalle = str_detalle & "3083" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "22" Then
                                    str_detalle = str_detalle & "3084" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "23" Then
                                    str_detalle = str_detalle & "3085" & ","
                                End If


                                'xochitepec
                            ElseIf id_plaza_cobro = 105 Then

                                str_detalle = str_detalle & "365" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2727" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2728" & ","
                                End If

                                'CERRO GORDO
                            ElseIf id_plaza_cobro = 186 Then

                                str_detalle = str_detalle & "351" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "21" Then
                                    str_detalle = str_detalle & "3199" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "22" Then
                                    str_detalle = str_detalle & "3200" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "23" Then
                                    str_detalle = str_detalle & "3201" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "24" Then
                                    str_detalle = str_detalle & "3202" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "25" Then
                                    str_detalle = str_detalle & "3203" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                    'str_detalle = str_detalle & "3185" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                    'str_detalle = str_detalle & "3186" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                    'str_detalle = str_detalle & "3187" & ","
                                    ' ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                    'str_detalle = str_detalle & "3188" & ","
                                End If
                                'Queretaro

                            ElseIf id_plaza_cobro = 106 Then

                                str_detalle = str_detalle & "112" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1079" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1080" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1081" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1082" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1083" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1084" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1085" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1086" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1087" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "1088" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "1089" & ","
                                End If


                            ElseIf id_plaza_cobro = 189 Then

                                str_detalle = str_detalle & "365" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1891" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1892" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1893" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1894" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1895" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1896" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1897" & ","
                                End If

                                'VillaGrand
                            ElseIf id_plaza_cobro = 183 Then

                                str_detalle = str_detalle & "170" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2581" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2582" & ","
                                End If

                                'tres marias
                            ElseIf id_plaza_cobro = 109 Then

                                str_detalle = str_detalle & "102" & ","

                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1020" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1021" & ","
                                End If


                                'Central de Abastos
                            ElseIf id_plaza_cobro = 107 Then
                                str_detalle = str_detalle & "368" & ","
                                'Segmento B
                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1843" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1844" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1845" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1846" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1847" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1848" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1849" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1850" & ","
                                    'Segmento A
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1851" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "1852" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "1853" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                                    str_detalle = str_detalle & "1854" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                                    str_detalle = str_detalle & "2743" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                                    str_detalle = str_detalle & "2744" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "15" Then
                                    str_detalle = str_detalle & "2745" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "16" Then
                                    str_detalle = str_detalle & "2746" & ","
                                End If

                            ElseIf id_plaza_cobro = 189 Then
                                str_detalle = str_detalle & "189" & ","
                                'Segmento B
                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1891" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1892" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1893" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1894" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1895" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1896" & ","
                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1897" & ","
                                End If


                                '    'SAN MARCOS
                                'ElseIf id_plaza_cobro = 107 Then

                                '    str_detalle = str_detalle & "121" & ","

                                '    If Mid(Trim(oDataRowCuatro("Voie")), 1, 1) = "A" Then

                                '        If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                '            str_detalle = str_detalle & "1102" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                '            str_detalle = str_detalle & "1103" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                '            str_detalle = str_detalle & "1104" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                '            str_detalle = str_detalle & "1105" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                '            str_detalle = str_detalle & "1106" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                '            str_detalle = str_detalle & "1107" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                                '            str_detalle = str_detalle & "1108" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                                '            str_detalle = str_detalle & "1109" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                                '            str_detalle = str_detalle & "1110" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "15" Then
                                '            str_detalle = str_detalle & "1101" & ","
                                '        End If

                                '    ElseIf Mid(Trim(oDataRowCuatro("Voie")), 1, 1) = "B" Then

                                '        If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                '            str_detalle = str_detalle & "1097" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                '            str_detalle = str_detalle & "1098" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                '            str_detalle = str_detalle & "1099" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                '            str_detalle = str_detalle & "1100" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                '            str_detalle = str_detalle & "1101" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                '            str_detalle = str_detalle & "1102" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                '            str_detalle = str_detalle & "1103" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                '            str_detalle = str_detalle & "1104" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                '            str_detalle = str_detalle & "1105" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                '            str_detalle = str_detalle & "1106" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                '            str_detalle = str_detalle & "1107" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                                '            str_detalle = str_detalle & "1108" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                                '            str_detalle = str_detalle & "1109" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                                '            str_detalle = str_detalle & "1110" & ","
                                '        End If

                                '    End If



                            Else
                                str_detalle = str_detalle & ","
                                str_detalle = str_detalle & ","
                            End If


                            'Cuerpo 	Caracter 	X(1)	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                            str_detalle = str_detalle & Mid(Trim(oDataRowCuatro("VOIE")), 1, 1) & ","

                            'Identificador de operación	Caracter 	X(2)	Valores posibles:  Tabla 17 - Códigos de Operación por Carril.
                            str_detalle = str_detalle & "X" & Mid(Trim(oDataRowCuatro("VOIE")), 1, 1) & ","


                            If Trim(strEncargadoTurno) = "" Then
                                strEncargadoTurno = "encargado_plaza"
                            End If
                            'No. empleado C-R 	Entero 	>>>>>9	
                            str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", strEncargadoTurno) & ","
                            'No. empleado encargado de turno 	Entero 	>>>>>9 	
                            str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", strEncargadoTurno) & ","
                            'No. empleado Admón. Gral. 	Entero 	>>>>>9 	
                            str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", "encargado_plaza") & ","
                            'No. de control de preliquidación  	Entero 	>>>9 	
                            str_detalle = str_detalle & ","

                            oSW.WriteLine(str_detalle)

                            '----------------------

                        End If


                    End If

                Next

            End If
            '************************************************
            '************************************************










            oSW.Flush()
            oSW.Close()
            ProgressBar1.Value = ProgressBar1.Value + 20



            'MsgBox("Exportación Terminada", MsgBoxStyle.Information, "Exportación")
        Catch ex As Exception
            MsgBox(ex.Message.ToString, MsgBoxStyle.Critical)
        End Try

    End Sub
    'Archivo 2A
    Private Sub Preliquidaciones_de_cajero_receptor_para_transito_vehicular()

        Dim strQuerys As String
        Dim Linea As String = ""
        Dim cabecera As String
        Dim pie As String
        Dim numero_archivo As String = ""
        Dim nombre_archivo As String
        Dim numero_registros As Double
        Dim cont As Integer
        Dim cont2 As Integer
        Dim int_turno As Integer

        Dim h_inicio_turno As Date
        Dim h_fin_turno As Date

        Dim no_registros As String

        Dim str_detalle As String
        Dim str_detalle_tc As String

        Dim str_encargado As String

        Dim dbl_registros As Double

        Dim strEncargadoTurno As String
        Dim cont_cerrado_todo_turno As Integer = 0

        'Try



        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            int_turno = 5
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            int_turno = 6
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
            int_turno = 4
        End If

        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 06:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 13:59:59")
            int_turno = 5
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 21:59:59")
            int_turno = 6
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
            'h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
            h_inicio_turno = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 22:00:00")
            'h_fin_turno = CDate(Format(DateAdd(DateInterval.Day, 1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 05:59:59")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 05:59:59")
            int_turno = 4
        End If

        'If Len(id_plaza_cobro) = 3 Then
        '    If id_plaza_cobro <> 108 Then
        '        nombre_archivo = "0" & id_plaza_cobro
        '    Else
        '        nombre_archivo = "0001"
        '    End If
        'End If

        If Len(id_plaza_cobro) = 3 Then
            If id_plaza_cobro = 108 Then
                nombre_archivo = "0001"
            ElseIf id_plaza_cobro = 109 Then
                nombre_archivo = "001B"
            ElseIf id_plaza_cobro = 107 Then
                nombre_archivo = "0107"
            ElseIf id_plaza_cobro = 106 Then
                nombre_archivo = "0006"
            Else
                nombre_archivo = "0" & id_plaza_cobro

            End If
        End If



        nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "2" & strIdentificador

        Dim oSW As New StreamWriter(dir_archivo & nombre_archivo)
        archivo_2 = nombre_archivo

        'cabecera = "David Cabecera"
        'cabecera = "04"
        cabecera = cmbDelegacion.Tag

        'If Len(id_plaza_cobro) = 3 Then
        '    cabecera = cabecera & "0" & id_plaza_cobro
        'End If
        'If Len(id_plaza_cobro) = 3 Then
        '    If id_plaza_cobro <> 108 Then
        '        cabecera = cabecera & "0" & id_plaza_cobro
        '    Else
        '        cabecera = cabecera & "0001"
        '    End If

        'End If

        If Len(id_plaza_cobro) = 3 Then
            If id_plaza_cobro = 108 Then
                cabecera = cabecera & "0001"
            ElseIf id_plaza_cobro = 109 Then
                cabecera = cabecera & "001B"
            ElseIf id_plaza_cobro = 107 Then
                cabecera = cabecera & "0107"
            ElseIf id_plaza_cobro = 106 Then
                cabecera = cabecera & "0006"
            Else
                cabecera = cabecera & "0" & id_plaza_cobro

            End If
        End If




        cabecera = "03" & cabecera & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "2" & strIdentificador & Format(dt_Fecha_Inicio, "dd/MM/yyyy") & int_turno

        'CABECERA INICIO REGISTROS
        strQuerys = "SELECT	FIN_POSTE.Id_Gare, " &
"TYPE_VOIE.libelle_court_voie_L2, " &
"Voie, " &
"'zzz', " &
"TO_CHAR(Numero_Poste,'FM09'), " &
"TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " &
"TO_CHAR(Date_Fin_Poste,'HH24:MI'), " &
"Matricule, " &
"Sac, " &
"FIN_POSTE.Id_Voie, " &
"DATE_DEBUT_POSTE,Date_Fin_Poste, " &
"TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " &
"TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " &
",TYPE_VOIE.libelle_court_voie " &
",FIN_POSTE_CPT22, " &
"ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " &
"FROM 	TYPE_VOIE, " &
"FIN_POSTE, " &
"SITE_GARE " &
"WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " &
"AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " &
"AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_reseau		= 	'01' " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND (Id_Mode_Voie IN (1,7,9)) " &
"AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"AND (FIN_POSTE.Id_Voie = '1' " &
"OR FIN_POSTE.Id_Voie = '2' " &
"OR FIN_POSTE.Id_Voie = '3' " &
"OR FIN_POSTE.Id_Voie = '4' " &
"OR FIN_POSTE.Id_Voie = 'X' " &
") " &
"ORDER BY Id_Gare, " &
"Id_Voie, " &
"Voie, " &
"Date_Debut_Poste," &
"Date_Fin_Poste, " &
"Numero_Poste, " &
"Matricule " &
",Sac"
        If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 1 Then

            dbl_registros = oDataSet.Tables("FIN_POSTE").Rows.Count
        Else
            dbl_registros = 0
        End If

        If Mid(id_plaza_cobro, 2, 2) = 84 Then
            'tramo corto
            strQuerys = "SELECT	FIN_POSTE.Id_Gare, " &
"TYPE_VOIE.libelle_court_voie_L2, " &
"Voie, " &
"'zzz', " &
"TO_CHAR(Numero_Poste,'FM09'), " &
"TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " &
"TO_CHAR(Date_Fin_Poste,'HH24:MI'), " &
"Matricule, " &
"Sac, " &
"FIN_POSTE.Id_Voie, " &
"DATE_DEBUT_POSTE,Date_Fin_Poste, " &
"TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " &
"TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " &
",TYPE_VOIE.libelle_court_voie " &
",FIN_POSTE_CPT22, " &
"ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " &
"FROM 	TYPE_VOIE, " &
"FIN_POSTE, " &
"SITE_GARE " &
"WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " &
"AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " &
"AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_reseau		= 	'01' " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND (Id_Mode_Voie IN (1,7,9)) " &
"AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"AND (FIN_POSTE.Id_Voie = '1' " &
"OR FIN_POSTE.Id_Voie = '2' " &
"OR FIN_POSTE.Id_Voie = '3' " &
"OR FIN_POSTE.Id_Voie = '4' " &
"OR FIN_POSTE.Id_Voie = 'X' " &
") and SUBSTR(Voie,1,1) = 'A' " &
"ORDER BY Id_Gare, " &
"Id_Voie, " &
"Voie, " &
"Date_Debut_Poste," &
"Date_Fin_Poste, " &
"Numero_Poste, " &
"Matricule " &
",Sac"
            If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 1 Then

                dbl_registros = dbl_registros + oDataSet.Tables("FIN_POSTE").Rows.Count
            Else
                dbl_registros = dbl_registros + 0
            End If
            'fin tramo corto

        ElseIf Mid(id_plaza_cobro, 2, 2) = "02" Then


            strQuerys = "SELECT	FIN_POSTE.Id_Gare, " &
"TYPE_VOIE.libelle_court_voie_L2, " &
"Voie, " &
"'zzz', " &
"TO_CHAR(Numero_Poste,'FM09'), " &
"TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " &
"TO_CHAR(Date_Fin_Poste,'HH24:MI'), " &
"Matricule, " &
"Sac, " &
"FIN_POSTE.Id_Voie, " &
"DATE_DEBUT_POSTE,Date_Fin_Poste, " &
"TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " &
"TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " &
",TYPE_VOIE.libelle_court_voie " &
",FIN_POSTE_CPT22, " &
"ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " &
"FROM 	TYPE_VOIE, " &
"FIN_POSTE, " &
"SITE_GARE " &
"WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " &
"AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " &
"AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_reseau		= 	'01' " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND (Id_Mode_Voie IN (1,7,9)) " &
"AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"AND (FIN_POSTE.Id_Voie = '1' " &
"OR FIN_POSTE.Id_Voie = '2' " &
"OR FIN_POSTE.Id_Voie = '3' " &
"OR FIN_POSTE.Id_Voie = '4' " &
"OR FIN_POSTE.Id_Voie = 'X' " &
") and (Voie = 'A01' or Voie = 'B08')  " &
"ORDER BY Id_Gare, " &
"Id_Voie, " &
"Voie, " &
"Date_Debut_Poste," &
"Date_Fin_Poste, " &
"Numero_Poste, " &
"Matricule " &
",Sac"
            If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 1 Then

                dbl_registros = dbl_registros + oDataSet.Tables("FIN_POSTE").Rows.Count
            Else
                dbl_registros = dbl_registros + 0
            End If

        End If





        strQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM, BAG_NUMBER, REPORT_FLAG, GENERATION_DHM " &
"FROM CLOSED_LANE_REPORT, SITE_GARE " &
"where " &
"CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"order by BEGIN_DHM"

        If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 1 Then

            dbl_registros = dbl_registros + oDataSet.Tables("CLOSED_LANE_REPORT").Rows.Count
        Else
            dbl_registros = dbl_registros + 0
        End If


        'carriles siempre cerrados
        'cont_cerrado_todo_turno


        strQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD "

        If id_plaza_cobro = 106 Then
            strQuerys = strQuerys & "where VOIE <> 'B04' and VOIE <> 'A03' "
        End If


        If objQuerys.QueryDataSetCuatro(strQuerys, "SEQ_VOIE_TOD") = 1 Then

            For cont2 = 0 To oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Count - 1

                oDataRowCuatro = oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Item(cont2)

                strQuerys = "SELECT	* FROM 	FIN_POSTE " &
                 "WHERE	VOIE = '" & oDataRowCuatro("VOIE") & "' " &
                 "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
                 "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) "

                If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 0 Then

                    strQuerys = "SELECT * " &
"FROM CLOSED_LANE_REPORT, SITE_GARE " &
"where " &
"CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND	LANE		=	'" & oDataRowCuatro("VOIE") & "' " &
"AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"order by BEGIN_DHM"
                    If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 0 Then
                        cont_cerrado_todo_turno = cont_cerrado_todo_turno + 1
                    End If

                End If
            Next
        End If

        dbl_registros = dbl_registros + cont_cerrado_todo_turno

        'fin carriles siempre cerrados




        If Len(CStr(dbl_registros)) = 1 Then
            no_registros = "0000" & dbl_registros
        ElseIf Len(CStr(dbl_registros)) = 2 Then
            no_registros = "000" & dbl_registros
        ElseIf Len(CStr(dbl_registros)) = 3 Then
            no_registros = "00" & dbl_registros
        ElseIf Len(CStr(dbl_registros)) = 4 Then
            no_registros = "0" & dbl_registros
        ElseIf Len(CStr(dbl_registros)) = 5 Then
            no_registros = dbl_registros
        End If



        cabecera = cabecera & no_registros


        oSW.WriteLine(cabecera)
        'CABECERA FIN


        'inicio detalle
        '**************************************
        '**************************************
        '**************************************

        Dim strQuerys_varios_cortes As String

        Dim hr_fecha_ini_varios_turnos As Date
        Dim hr_fecha_fin_varios_turnos As Date
        Dim bl_varios_turnos As Boolean = False

        Dim odataSetReporte As New DataSet
        Dim oDataTableReporte As DataTable = New DataTable("tabla_reporte")
        Dim oDataColumnaReporte As DataColumn
        Dim oDatarowReporte As DataRow

        Dim strGrupo As String
        Dim strConcepto As String
        Dim strTipoConcepto_A As String
        Dim strTipoConcepto_B As String
        Dim strTipoConcepto_C As String
        Dim strTipoConceptoGenero_A As String
        Dim strTipoConceptoGenero_B As String

        strQuerys = strLinea



        Dim str_id_RESEAU As String
        Dim int_id_gare As Integer
        Dim str_id_voie As String
        Dim int_id_voie As String 'Integer
        Dim int_numero_poste As Integer
        Dim dt_ini_poste As Date
        Dim dt_fin_poste As Date
        Dim str_MATRICULE As String

        Dim str_cajero As String = str_Cajero_Receptor
        Dim id_plaza_cobro_local As String = id_plaza_cobro

        'Dim h_inicio_turno As Date
        'Dim h_fin_turno As Date

        '        'VARIABLES DE CAMPO
        Dim VAR_01 As Date
        Dim VAR_08 As String = ""
        Dim VAR_02 As String = ""

        Dim VAR_11 As Double = 0
        Dim VAR_12 As Double = 0
        Dim VAR_13 As Double = 0
        Dim VAR_14 As Double = 0
        Dim VAR_15 As Double = 0
        Dim VAR_16 As Double = 0
        Dim VAR_17 As Double = 0
        'tramo corto
        Dim VAR_17_tc As Double = 0
        Dim VAR_18 As Double = 0
        Dim VAR_19 As Double = 0
        'tramo corto
        Dim VAR_19_tc As Double = 0

        Dim VAR_20 As Double = 0
        Dim VAR_21 As Double = 0
        Dim VAR_22 As Double = 0
        Dim VAR_23 As Double = 0

        Dim VAR_24 As Double = 0
        Dim VAR_25 As Double = 0
        Dim VAR_26 As Double = 0
        Dim VAR_27 As Double = 0

        Dim VAR_28 As Double = 0
        Dim VAR_29 As Double = 0
        Dim VAR_30 As Double = 0
        Dim VAR_31 As Double = 0
        Dim VAR_32 As Double = 0
        Dim VAR_33 As Double = 0
        Dim VAR_34 As Double = 0
        Dim VAR_35 As Double = 0
        Dim VAR_36 As Double = 0
        Dim VAR_37 As Double = 0
        Dim VAR_38 As Double = 0
        Dim VAR_39 As Double = 0

        Dim VAR_40 As Double = 0
        Dim VAR_41 As Double = 0
        Dim VAR_42 As Double = 0
        Dim VAR_43 As Double = 0
        Dim VAR_44 As Double = 0
        Dim VAR_45 As Double = 0
        Dim VAR_46 As Double = 0
        Dim VAR_47 As Double = 0
        Dim VAR_48 As Double = 0
        Dim VAR_49 As Double = 0

        Dim VAR_50 As Double = 0
        Dim VAR_51 As Double = 0
        Dim VAR_52 As Double = 0
        Dim VAR_53 As Double = 0
        Dim VAR_54 As Double = 0
        Dim VAR_55 As Double = 0
        Dim VAR_56 As Double = 0
        Dim VAR_57 As Double = 0
        Dim VAR_58 As Double = 0
        Dim VAR_59 As Double = 0

        Dim VAR_60 As Double = 0
        Dim VAR_61 As Double = 0
        Dim VAR_62 As Double = 0
        Dim VAR_63 As Double = 0
        Dim VAR_64 As Double = 0
        Dim VAR_65 As Double = 0
        Dim VAR_66 As Double = 0
        Dim VAR_67 As Double = 0
        Dim VAR_68 As Double = 0
        Dim VAR_69 As Double = 0

        Dim VAR_70 As Double = 0
        Dim VAR_71 As Double = 0
        Dim VAR_72 As Double = 0
        Dim VAR_73 As Double = 0
        Dim VAR_74 As Double = 0
        Dim VAR_75 As Double = 0
        Dim VAR_76 As Double = 0
        Dim VAR_77 As Double = 0
        Dim VAR_78 As Double = 0
        Dim VAR_79 As Double = 0

        Dim VAR_80 As Double = 0
        Dim VAR_81 As Double = 0
        Dim VAR_82 As Double = 0
        Dim VAR_83 As Double = 0
        Dim VAR_84 As Double = 0
        Dim VAR_85 As Double = 0
        Dim VAR_86 As Double = 0
        Dim VAR_87 As Double = 0
        Dim VAR_88 As Double = 0
        Dim VAR_89 As Double = 0

        Dim VAR_90 As Double = 0
        Dim VAR_91 As Double = 0
        Dim VAR_92 As Double = 0
        Dim VAR_93 As Double = 0
        Dim VAR_94 As Double = 0
        Dim VAR_95 As Double = 0
        Dim VAR_96 As Double = 0
        Dim VAR_97 As Double = 0
        Dim VAR_98 As Double = 0
        Dim VAR_99 As Double = 0

        Dim VAR_103 As Double = 0

        'Dim cont As Double

        Dim db_acom_63 As Double = 0
        Dim db_acom_94 As Double = 0
        Dim db_acom_65 As Double = 0
        Dim db_acom_67 As Double = 0
        Dim db_acom_folios As Double = 0
        Dim db_acom_folios2 As Double = 0
        Dim db_acom_folios3 As Double = 0
        Dim db_acom_eventos As Double = 0

        Dim No_Turno As String

        Dim saco As String

        Dim FOLIO_NUMBER_OPEN As Double
        Dim FOLIO_NUMBER_CLOSE As Double

        Dim FOLIO2_NUMBER_OPEN As Double
        Dim FOLIO2_NUMBER_CLOSE As Double

        Dim FOLIO3_NUMBER_OPEN As Double
        Dim FOLIO3_NUMBER_CLOSE As Double

        Dim bl_carril_automatico As Boolean

        'reporte cajero receptor / inicio turno


        'verifico cuantos cortes tiene en ese turno
        id_plaza_cobro_local = id_plaza_cobro_local - 100

        strQuerys = "SELECT GEADBA.FIN_POSTE.ID_RESEAU, GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE_L2, GEADBA.FIN_POSTE.VOIE, TO_CHAR(GEADBA.FIN_POSTE.NUMERO_POSTE, 'FM09') AS Expr1, " &
  "TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'MM/DD/YY') AS Expr2, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'HH24:MI') AS Expr3, " &
  "GEADBA.FIN_POSTE.MATRICULE, GEADBA.FIN_POSTE.SAC AS Expr4, GEADBA.FIN_POSTE.ID_VOIE, " &
  "TO_CHAR(GEADBA.FIN_POSTE.DATE_DEBUT_POSTE, 'YYYYMMDDHH24MISS') AS Expr5, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'YYYYMMDDHH24MISS') AS Expr6, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE, 0 AS Expr7, " &
  "FOLIO_NUMBER_CLOSE, INITIAL_EVENT_NUMBER, FINAL_EVENT_NUMBER " &
  "FROM GEADBA.TYPE_VOIE, GEADBA.FIN_POSTE, GEADBA.SITE_GARE " &
  "WHERE GEADBA.TYPE_VOIE.ID_VOIE = GEADBA.FIN_POSTE.ID_VOIE AND GEADBA.FIN_POSTE.ID_RESEAU = GEADBA.SITE_GARE.ID_RESEAU AND " &
  "GEADBA.FIN_POSTE.ID_GARE = GEADBA.SITE_GARE.ID_GARE AND (GEADBA.SITE_GARE.ID_RESEAU = '01') AND (GEADBA.SITE_GARE.ID_SITE = '" & Mid(id_plaza_cobro, 2, 2) & "') AND " &
  "(GEADBA.FIN_POSTE.ID_MODE_VOIE IN (1, 6, 7)) "


        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 06:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 13:59:59")
            No_Turno = "5 MATUTINO"
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 21:59:59")
            No_Turno = "6 VESPERTINO"
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then

            h_inicio_turno = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 22:00:00")

            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 05:59:59")
            No_Turno = "4 NOCTURNO"
        End If

        strQuerys = strQuerys & "AND (GEADBA.FIN_POSTE.DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) AND " &
   "(GEADBA.FIN_POSTE.DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) "




        strQuerys = strQuerys & "AND (GEADBA.FIN_POSTE.ID_VOIE = '1' OR " &
     "GEADBA.FIN_POSTE.ID_VOIE = '2' OR " &
     "GEADBA.FIN_POSTE.ID_VOIE = '3' OR " &
       "GEADBA.FIN_POSTE.ID_VOIE = '4' OR " &
     " GEADBA.FIN_POSTE.ID_VOIE = 'X') " &
     "AND GEADBA.FIN_POSTE.SAC is not null " &
     "ORDER BY GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.ID_VOIE, GEADBA.FIN_POSTE.VOIE, GEADBA.FIN_POSTE.DATE_FIN_POSTE, " &
     "GEADBA.FIN_POSTE.NUMERO_POSTE, GEADBA.FIN_POSTE.MATRICULE, Expr4"


        If objQuerys.QueryDataSetNueve(strQuerys, "TYPE_VOIE") = 1 Then



            ' dbl_registros = dbl_registros + oDataSetNueve.Tables("TYPE_VOIE").Rows.Count






            For iPosicionFilaActualNueve = 0 To oDataSetNueve.Tables("TYPE_VOIE").Rows.Count - 1




                oDataRowNueve = oDataSetNueve.Tables("TYPE_VOIE").Rows(iPosicionFilaActualNueve)

                hr_fecha_ini_varios_turnos = objControl.fecha(oDataRowNueve("Expr5"))
                hr_fecha_fin_varios_turnos = objControl.fecha(oDataRowNueve("Expr6"))
                str_cajero = oDataRowNueve("MATRICULE")


                'fin de verifico cuantos cortes tiene en ese turno


                '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''Inicio
                'DATOS GENERALES

                strQuerys = "SELECT GEADBA.FIN_POSTE.ID_RESEAU, GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE_L2, GEADBA.FIN_POSTE.VOIE, TO_CHAR(GEADBA.FIN_POSTE.NUMERO_POSTE, 'FM09') AS Expr1, " &
            "TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'MM/DD/YY') AS Expr2, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'HH24:MI') AS Expr3, " &
            "GEADBA.FIN_POSTE.MATRICULE, GEADBA.FIN_POSTE.SAC AS Expr4, GEADBA.FIN_POSTE.ID_VOIE, " &
            "TO_CHAR(GEADBA.FIN_POSTE.DATE_DEBUT_POSTE, 'YYYYMMDDHH24MISS') AS Expr5, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'YYYYMMDDHH24MISS') AS Expr6, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE, 0 AS Expr7, " &
            "FOLIO_NUMBER_OPEN, FOLIO_NUMBER_CLOSE, INITIAL_EVENT_NUMBER, FINAL_EVENT_NUMBER, FOLIO_ECT_NUMBER_INITIAL, FOLIO_ECT_NUMBER_FINAL, " &
            "FOLIO2_NUMBER_INITIAL,FOLIO2_NUMBER_FINAL,FOLIO3_NUMBER_INITIAL,FOLIO3_NUMBER_FINAL " &
            "FROM GEADBA.TYPE_VOIE, GEADBA.FIN_POSTE, GEADBA.SITE_GARE " &
            "WHERE GEADBA.TYPE_VOIE.ID_VOIE = GEADBA.FIN_POSTE.ID_VOIE AND GEADBA.FIN_POSTE.ID_RESEAU = GEADBA.SITE_GARE.ID_RESEAU AND " &
            "GEADBA.FIN_POSTE.ID_GARE = GEADBA.SITE_GARE.ID_GARE AND (GEADBA.SITE_GARE.ID_RESEAU = '01') AND (GEADBA.SITE_GARE.ID_SITE = '" & Mid(id_plaza_cobro, 2, 2) & "') AND " &
            "(GEADBA.FIN_POSTE.ID_MODE_VOIE IN (1, 6, 7)) "


                If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
                    h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 06:00:00")
                    h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 13:59:59")
                ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
                    h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
                    h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 21:59:59")
                ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then

                    h_inicio_turno = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 22:00:00")

                    h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 05:59:59")
                End If

                '     If bl_varios_turnos = False Then
                '13/11
                '         strQuerys = strQuerys & "AND (GEADBA.FIN_POSTE.DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) AND " & _
                '"(GEADBA.FIN_POSTE.DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) "

                '     Else


                '         strQuerys = strQuerys & "AND (GEADBA.FIN_POSTE.DATE_DEBUT_POSTE >= TO_DATE('" & Format(hr_fecha_ini_varios_turnos, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) AND " & _
                '"(GEADBA.FIN_POSTE.DATE_FIN_POSTE <= TO_DATE('" & Format(hr_fecha_fin_varios_turnos, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) "
                '20/06/2014
                strQuerys = strQuerys & "AND (GEADBA.FIN_POSTE.DATE_DEBUT_POSTE = TO_DATE('" & Format(hr_fecha_ini_varios_turnos, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS'))  "

                'End If


                strQuerys = strQuerys & "AND (GEADBA.FIN_POSTE.ID_VOIE = '1' OR " &
             "GEADBA.FIN_POSTE.ID_VOIE = '2' OR " &
             "GEADBA.FIN_POSTE.ID_VOIE = '3' OR " &
             "GEADBA.FIN_POSTE.ID_VOIE = '4' OR " &
             "GEADBA.FIN_POSTE.ID_VOIE = 'X') " &
             "AND (GEADBA.FIN_POSTE.MATRICULE = '" & str_cajero & "') " &
             "AND FIN_POSTE.SAC = '" & oDataRowNueve("Expr4") & "' " &
             "ORDER BY GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.ID_VOIE, GEADBA.FIN_POSTE.VOIE, GEADBA.FIN_POSTE.DATE_FIN_POSTE, " &
             "GEADBA.FIN_POSTE.NUMERO_POSTE, GEADBA.FIN_POSTE.MATRICULE, Expr4"



                If objQuerys.QueryDataSet(strQuerys, "TYPE_VOIE") = 1 Then
                    '18/10/2014
                    ' For PosicionFilaActual = 0 To oDataSet.Tables("TYPE_VOIE").Rows.Count - 1


                    '18/10/2014
                    'oDataRow = oDataSet.Tables("TYPE_VOIE").Rows(PosicionFilaActual)



                    'Dim par3 As New ReportParameter()
                    'par3.Name = "par3"
                    'par3.Values.Add(str_delegacion)

                    'Dim par8 As New ReportParameter()
                    'par8.Name = "par8"
                    'par8.Values.Add(oDataRow("VOIE"))
                    VAR_08 = oDataRow("VOIE")

                    str_id_voie = oDataRow("VOIE")
                    int_id_voie = oDataRow("ID_VOIE")
                    int_numero_poste = CInt(oDataRow("Expr1"))


                    'Dim par7 As New ReportParameter()
                    'par7.Name = "par7"


                    If bl_varios_turnos = False Then

                        'par7.Values.Add(No_Turno)
                    Else

                        If CInt(Format(objControl.fecha(oDataRow("Expr5")), "HH")) >= 6 And CInt(Format(objControl.fecha(oDataRow("Expr5")), "HH")) < 14 Then

                            'par7.Values.Add(No_Turno)
                        ElseIf CInt(Format(objControl.fecha(oDataRow("Expr5")), "HH")) >= 14 And CInt(Format(objControl.fecha(oDataRow("Expr5")), "HH")) < 22 Then
                            'par7.Values.Add(No_Turno)

                        Else
                            'par7.Values.Add(No_Turno)

                        End If

                    End If

                    'Dim par4 As New ReportParameter()
                    'par4.Name = "par4"
                    'par4.Values.Add(Format(objControl.fecha(oDataRow("Expr5")), "MM/dd/yyyy HH:mm:ss"))
                    dt_ini_poste = Format(objControl.fecha(oDataRow("Expr5")), "MM/dd/yyyy HH:mm:ss")

                    'Dim par1 As New ReportParameter()
                    'par1.Name = "par1"
                    'par1.Values.Add(Format(objControl.fecha(oDataRow("Expr5")), "MM/dd/yyyy"))
                    'VAR_01 = objControl.fecha(oDataRow("Expr5"))
                    'dtpFechaInicio
                    VAR_01 = Format(dt_Fecha_Inicio, "MM/dd/yyyy")

                    'Dim par6 As New ReportParameter()
                    'par6.Name = "par6"
                    'par6.Values.Add(Format(objControl.fecha(oDataRow("Expr6")), "MM/dd/yyyy HH:mm:ss"))
                    dt_fin_poste = Format(objControl.fecha(oDataRow("Expr6")), "MM/dd/yyyy HH:mm:ss")

                    'folios
                    'Dim par69 As New ReportParameter()
                    'par69.Name = "par69"
                    'par69.Values.Add(Format(oDataRow("FOLIO_NUMBER_OPEN"), "###,###,###,##0"))

                    'VAR_69 = oDataRow("FOLIO_NUMBER_OPEN")

                    If Not Len(CStr(oDataRow("FOLIO_NUMBER_OPEN"))) = 10 Then
                        FOLIO_NUMBER_OPEN = oDataRow("FOLIO_NUMBER_OPEN")
                    Else
                        FOLIO_NUMBER_OPEN = Mid(CStr(oDataRow("FOLIO_NUMBER_OPEN")), 1, 9)
                    End If

                    If Not Len(CStr(oDataRow("FOLIO_NUMBER_CLOSE"))) = 10 Then
                        FOLIO_NUMBER_CLOSE = oDataRow("FOLIO_NUMBER_CLOSE")
                    Else
                        FOLIO_NUMBER_CLOSE = Mid(CStr(oDataRow("FOLIO_NUMBER_CLOSE")), 1, 9)
                    End If

                    'If Not oDataRow("FOLIO_NUMBER_OPEN") > oDataRow("FOLIO_NUMBER_CLOSE") Then
                    '    VAR_77 = oDataRow("FOLIO_NUMBER_OPEN")
                    'Else
                    '    VAR_77 = oDataRow("FOLIO_NUMBER_CLOSE")
                    'End If

                    If Not FOLIO_NUMBER_OPEN > FOLIO_NUMBER_CLOSE Then
                        VAR_77 = FOLIO_NUMBER_OPEN
                    Else
                        VAR_77 = FOLIO_NUMBER_CLOSE
                    End If

                    'Dim par70 As New ReportParameter()
                    'par70.Name = "par70"
                    'par70.Values.Add(Format(oDataRow("FOLIO_NUMBER_CLOSE"), "###,###,###,##0"))
                    'VAR_78 = oDataRow("FOLIO_NUMBER_CLOSE")

                    VAR_78 = FOLIO_NUMBER_CLOSE

                    'Dim par71 As New ReportParameter()
                    'par71.Name = "par71"
                    'par71.Values.Add(Format(oDataRow("FOLIO_NUMBER_CLOSE") - oDataRow("FOLIO_NUMBER_OPEN") + 1, "###,###,###,##0"))

                    'Dim par73 As New ReportParameter()
                    'par73.Name = "par73"
                    'par73.Values.Add(Format(oDataRow("FOLIO_NUMBER_CLOSE") - oDataRow("FOLIO_NUMBER_OPEN") + 1, "###,###,###,##0"))

                    ''FOLIOS 2
                    'Dim par74 As New ReportParameter()
                    'par74.Name = "par74"
                    'par74.Values.Add(Format(oDataRow("FOLIO2_NUMBER_INITIAL"), "###,###,###,##0"))

                    If Not Len(CStr(oDataRow("FOLIO2_NUMBER_INITIAL"))) = 10 Then
                        FOLIO2_NUMBER_OPEN = oDataRow("FOLIO2_NUMBER_INITIAL")
                    Else
                        FOLIO2_NUMBER_OPEN = Mid(CStr(oDataRow("FOLIO2_NUMBER_INITIAL")), 1, 9)
                    End If

                    If Not Len(CStr(oDataRow("FOLIO2_NUMBER_FINAL"))) = 10 Then
                        FOLIO2_NUMBER_CLOSE = oDataRow("FOLIO2_NUMBER_FINAL")
                    Else
                        FOLIO2_NUMBER_CLOSE = Mid(CStr(oDataRow("FOLIO2_NUMBER_FINAL")), 1, 9)
                    End If

                    'VAR_82 = oDataRow("FOLIO2_NUMBER_INITIAL")
                    VAR_82 = FOLIO2_NUMBER_OPEN
                    'Dim par75 As New ReportParameter()
                    'par75.Name = "par75"
                    'par75.Values.Add(Format(oDataRow("FOLIO2_NUMBER_FINAL"), "###,###,###,##0"))
                    'VAR_83 = oDataRow("FOLIO2_NUMBER_FINAL")
                    VAR_83 = FOLIO2_NUMBER_CLOSE
                    'Dim par76 As New ReportParameter()
                    'par76.Name = "par76"
                    'If oDataRow("FOLIO2_NUMBER_FINAL") = 0 And oDataRow("FOLIO2_NUMBER_FINAL") = 0 Then
                    '    par76.Values.Add(0)
                    'Else
                    '    par76.Values.Add(Format(oDataRow("FOLIO2_NUMBER_FINAL") - oDataRow("FOLIO2_NUMBER_INITIAL") + 1, "###,###,###,##0"))
                    'End If


                    'Dim par78 As New ReportParameter()
                    'par78.Name = "par78"
                    'If oDataRow("FOLIO2_NUMBER_FINAL") = 0 And oDataRow("FOLIO2_NUMBER_FINAL") = 0 Then
                    '    par78.Values.Add(0)
                    'Else
                    '    par78.Values.Add(Format(oDataRow("FOLIO2_NUMBER_FINAL") - oDataRow("FOLIO2_NUMBER_INITIAL") + 1, "###,###,###,##0"))
                    'End If

                    ''FOLIOS 3
                    'Dim par79 As New ReportParameter()
                    'par79.Name = "par79"
                    'par79.Values.Add(Format(oDataRow("FOLIO3_NUMBER_INITIAL"), "###,###,###,##0"))

                    If Not Len(CStr(oDataRow("FOLIO3_NUMBER_INITIAL"))) = 10 Then
                        FOLIO3_NUMBER_OPEN = oDataRow("FOLIO3_NUMBER_INITIAL")
                    Else
                        FOLIO3_NUMBER_OPEN = Mid(CStr(oDataRow("FOLIO3_NUMBER_INITIAL")), 1, 9)
                    End If

                    If Not Len(CStr(oDataRow("FOLIO3_NUMBER_FINAL"))) = 10 Then
                        FOLIO3_NUMBER_CLOSE = oDataRow("FOLIO3_NUMBER_FINAL")
                    Else
                        FOLIO3_NUMBER_CLOSE = Mid(CStr(oDataRow("FOLIO3_NUMBER_FINAL")), 1, 9)
                    End If

                    'VAR_87 = oDataRow("FOLIO3_NUMBER_INITIAL")
                    VAR_87 = FOLIO3_NUMBER_OPEN
                    'Dim par80 As New ReportParameter()
                    'par80.Name = "par80"
                    'par80.Values.Add(Format(oDataRow("FOLIO3_NUMBER_FINAL"), "###,###,###,##0"))
                    'VAR_88 = oDataRow("FOLIO3_NUMBER_FINAL")
                    VAR_88 = FOLIO3_NUMBER_CLOSE
                    'Dim par81 As New ReportParameter()
                    'par81.Name = "par81"
                    'If oDataRow("FOLIO3_NUMBER_FINAL") = 0 And oDataRow("FOLIO3_NUMBER_FINAL") = 0 Then
                    '    par81.Values.Add(0)
                    'Else
                    'par81.Values.Add(Format(oDataRow("FOLIO3_NUMBER_FINAL") - oDataRow("FOLIO3_NUMBER_INITIAL") + 1, "###,###,###,##0"))
                    'End If


                    'Dim par83 As New ReportParameter()
                    'par83.Name = "par83"
                    'If oDataRow("FOLIO3_NUMBER_FINAL") = 0 And oDataRow("FOLIO3_NUMBER_FINAL") = 0 Then
                    '    par83.Values.Add(0)
                    'Else
                    '    par83.Values.Add(Format(oDataRow("FOLIO3_NUMBER_FINAL") - oDataRow("FOLIO3_NUMBER_INITIAL") + 1, "###,###,###,##0"))
                    'End If



                    '---------------------------------
                    'Dim par84 As New ReportParameter()
                    'par84.Name = "par84"

                    ''par84.Values.Add(Format(oDataRow("FOLIO_ECT_NUMBER_INITIAL"), "###,###,###,##0"))
                    'VAR_84 = oDataRow("FOLIO_ECT_NUMBER_INITIAL")

                    ''Dim par85 As New ReportParameter()
                    ''par85.Name = "par85"
                    ''par85.Values.Add(Format(oDataRow("FOLIO_ECT_NUMBER_FINAL"), "###,###,###,##0"))

                    'If oDataRow("FOLIO_ECT_NUMBER_FINAL") >= VAR_84 Then
                    '    VAR_85 = oDataRow("FOLIO_ECT_NUMBER_FINAL")
                    'Else
                    '    VAR_85 = VAR_84
                    'End If

                    'VERIFICO SI EL CARRIL ES AUTOMATICO
                    bl_carril_automatico = False

                    'TLALPAN
                    If id_plaza_cobro_local = "8" Then

                        If str_id_voie = "A08" Or str_id_voie = "A09" Or str_id_voie = "B01" Then
                            bl_carril_automatico = True
                        End If
                    End If



                    If oDataRow("FOLIO_ECT_NUMBER_INITIAL") < oDataRow("FOLIO_ECT_NUMBER_FINAL") Then


                        If bl_carril_automatico = False Then
                            VAR_92 = oDataRow("FOLIO_ECT_NUMBER_INITIAL")
                            VAR_93 = oDataRow("FOLIO_ECT_NUMBER_FINAL")

                        Else

                            strQuerys = "select  MIN(FOLIO_ECT) AS FOLIO_ECT_NUMBER_INITIAL_C, MAX(FOLIO_ECT) AS FOLIO_ECT_NUMBER_FINAL_C FROM TRANSACTION WHERE ID_GARE = '" & oDataRow("ID_GARE") & "' AND ID_VOIE  = '" & oDataRow("ID_VOIE") & "'  AND VOIE  = '" & oDataRow("VOIE") & "' " &
"AND  DATE_DEBUT_POSTE = TO_DATE('" & oDataRow("Expr5") & "', 'YYYYMMDDHH24MISS') and FOLIO_ECT <> 0   "
                            '= TO_DATE('" & Format(oDataRow("Expr5"), "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')


                            If objQuerys.QueryDataSetDos(strQuerys, "TRANSACTION") = 1 Then


                                If Not IsDBNull(oDataRowDos("FOLIO_ECT_NUMBER_INITIAL_C")) Then
                                    VAR_92 = oDataRowDos("FOLIO_ECT_NUMBER_INITIAL_C")
                                    VAR_93 = oDataRowDos("FOLIO_ECT_NUMBER_FINAL_C")
                                Else
                                    VAR_92 = 0
                                    VAR_93 = 0
                                End If

                                'VAR_84 = oDataRowDos("FOLIO_ECT_NUMBER_INITIAL_C")
                                'VAR_85 = oDataRowDos("FOLIO_ECT_NUMBER_FINAL_C")
                            Else
                                VAR_92 = 0
                                VAR_93 = 0
                            End If

                        End If


                    Else

                        strQuerys = "select  MIN(FOLIO_ECT) AS FOLIO_ECT_NUMBER_INITIAL_C, MAX(FOLIO_ECT) AS FOLIO_ECT_NUMBER_FINAL_C FROM TRANSACTION WHERE ID_GARE = '" & oDataRow("ID_GARE") & "' AND ID_VOIE  = '" & oDataRow("ID_VOIE") & "'  AND VOIE  = '" & oDataRow("VOIE") & "' " &
"AND  DATE_DEBUT_POSTE = TO_DATE('" & oDataRow("Expr5") & "', 'YYYYMMDDHH24MISS') and FOLIO_ECT <> 0   "
                        '= TO_DATE('" & Format(oDataRow("Expr5"), "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')


                        If objQuerys.QueryDataSetDos(strQuerys, "TRANSACTION") = 1 Then


                            If Not IsDBNull(oDataRowDos("FOLIO_ECT_NUMBER_INITIAL_C")) Then
                                VAR_92 = oDataRowDos("FOLIO_ECT_NUMBER_INITIAL_C")
                                VAR_93 = oDataRowDos("FOLIO_ECT_NUMBER_FINAL_C")
                            Else
                                VAR_92 = 0
                                VAR_93 = 0
                            End If

                            'VAR_84 = oDataRowDos("FOLIO_ECT_NUMBER_INITIAL_C")
                            'VAR_85 = oDataRowDos("FOLIO_ECT_NUMBER_FINAL_C")
                        Else
                            VAR_92 = 0
                            VAR_93 = 0
                        End If

                    End If





                    'Dim par86 As New ReportParameter()
                    'par86.Name = "par86"
                    'par86.Values.Add(Format(oDataRow("FOLIO_ECT_NUMBER_FINAL") - oDataRow("FOLIO_ECT_NUMBER_INITIAL") + 1, "###,###,###,##0"))


                    'Dim par88 As New ReportParameter()
                    'par88.Name = "par88"
                    'par88.Values.Add(Format(oDataRow("FOLIO_ECT_NUMBER_FINAL") - oDataRow("FOLIO_ECT_NUMBER_INITIAL") + 1, "###,###,###,##0"))



                    '-----------------------------------------------------------------------------
                    'Dim par89 As New ReportParameter()
                    'par89.Name = "par89"
                    'par89.Values.Add(Format(oDataRow("INITIAL_EVENT_NUMBER"), "###,###,###,##0"))
                    'VAR_89 = oDataRow("INITIAL_EVENT_NUMBER")

                    'Dim par90 As New ReportParameter()
                    'par90.Name = "par90"
                    'par90.Values.Add(Format(oDataRow("FINAL_EVENT_NUMBER"), "###,###,###,##0"))






                    If oDataRow("INITIAL_EVENT_NUMBER") < oDataRow("FINAL_EVENT_NUMBER") Then


                        If bl_carril_automatico = False Then
                            'VAR_97 = oDataRow("INITIAL_EVENT_NUMBER")
                            'VAR_98 = oDataRow("FINAL_EVENT_NUMBER")
                            strQuerys = "select  MIN(EVENT_NUMBER) AS INITIAL_EVENT_NUMBER_C, MAX(EVENT_NUMBER) AS FINAL_EVENT_NUMBER_C FROM TRANSACTION WHERE ID_GARE = '" & oDataRow("ID_GARE") & "' AND ID_VOIE  = '" & oDataRow("ID_VOIE") & "'  AND VOIE  = '" & oDataRow("VOIE") & "' " &
"AND  DATE_DEBUT_POSTE = TO_DATE('" & oDataRow("Expr5") & "', 'YYYYMMDDHH24MISS')  AND EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' "
                            '= TO_DATE('" & Format(oDataRow("Expr5"), "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')

                            If objQuerys.QueryDataSetDos(strQuerys, "TRANSACTION") = 1 Then

                                If Not IsDBNull(oDataRowDos("INITIAL_EVENT_NUMBER_C")) Then
                                    VAR_97 = oDataRowDos("INITIAL_EVENT_NUMBER_C")
                                    VAR_98 = oDataRowDos("FINAL_EVENT_NUMBER_C")
                                Else
                                    VAR_97 = 0
                                    VAR_98 = 0
                                End If


                            End If

                        Else

                            strQuerys = "select  MIN(EVENT_NUMBER) AS INITIAL_EVENT_NUMBER_C, MAX(EVENT_NUMBER) AS FINAL_EVENT_NUMBER_C FROM TRANSACTION WHERE ID_GARE = '" & oDataRow("ID_GARE") & "' AND ID_VOIE  = '" & oDataRow("ID_VOIE") & "'  AND VOIE  = '" & oDataRow("VOIE") & "' " &
      "AND  DATE_DEBUT_POSTE = TO_DATE('" & oDataRow("Expr5") & "', 'YYYYMMDDHH24MISS')  AND EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' "
                            '= TO_DATE('" & Format(oDataRow("Expr5"), "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')

                            If objQuerys.QueryDataSetDos(strQuerys, "TRANSACTION") = 1 Then

                                If Not IsDBNull(oDataRowDos("INITIAL_EVENT_NUMBER_C")) Then
                                    VAR_97 = oDataRowDos("INITIAL_EVENT_NUMBER_C")
                                    VAR_98 = oDataRowDos("FINAL_EVENT_NUMBER_C")
                                Else
                                    VAR_97 = 0
                                    VAR_98 = 0
                                End If


                            End If


                        End If


                    Else


                        strQuerys = "select  MIN(EVENT_NUMBER) AS INITIAL_EVENT_NUMBER_C, MAX(EVENT_NUMBER) AS FINAL_EVENT_NUMBER_C FROM TRANSACTION WHERE ID_GARE = '" & oDataRow("ID_GARE") & "' AND ID_VOIE  = '" & oDataRow("ID_VOIE") & "'  AND VOIE  = '" & oDataRow("VOIE") & "' " &
                              "AND  DATE_DEBUT_POSTE = TO_DATE('" & oDataRow("Expr5") & "', 'YYYYMMDDHH24MISS')  AND EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' "
                        '= TO_DATE('" & Format(oDataRow("Expr5"), "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')

                        If objQuerys.QueryDataSetDos(strQuerys, "TRANSACTION") = 1 Then

                            If Not IsDBNull(oDataRowDos("INITIAL_EVENT_NUMBER_C")) Then
                                VAR_97 = oDataRowDos("INITIAL_EVENT_NUMBER_C")
                                VAR_98 = oDataRowDos("FINAL_EVENT_NUMBER_C")
                            Else
                                VAR_97 = 0
                                VAR_98 = 0
                            End If


                        End If

                    End If

                Else

                    'ReportViewer1 = Nothing
                    'MsgBox("No existen datos", MsgBoxStyle.Exclamation, "PRELIQUIDACION DE CAJERO RECEPTOR")
                    'Me.Close()
                End If

                'OPERADOR
                strQuerys = "SELECT MATRICULE , rtrim(NOM)||' '||rtrim(PRENOM) as nombre " &
             "FROM TABLE_PERSONNEL " &
             "WHERE Matricule = '" & oDataRow("MATRICULE") & "'"
                If objQuerys.QueryDataSetTres(strQuerys, "TABLE_PERSONNEL") = 1 Then

                    'Dim par97 As New ReportParameter()
                    'par97.Name = "par97"
                    'par97.Values.Add(Trim(oDataRowTres("MATRICULE")) & "     " & Trim(oDataRowTres("nombre")))

                    str_MATRICULE = Trim(oDataRowTres("MATRICULE"))

                    'Dim parametersTitulos_2() As ReportParameter = {par97}
                    'ReportViewer1.LocalReport.SetParameters(parametersTitulos_2)

                End If

                strQuerys = "SELECT  ID_RESEAU, ID_GARE, NOM_GARE, NOM_GARE_L2 " &
             "FROM GEADBA.TYPE_GARE " &
             "WHERE ID_GARE = " & oDataRow("ID_GARE") & ""

                If objQuerys.QueryDataSetTres(strQuerys, "TYPE_GARE") = 1 Then

                    'Dim par5 As New ReportParameter()
                    'par5.Name = "par5"
                    'par5.Values.Add(oDataRowTres("NOM_GARE"))

                    int_id_gare = oDataRowTres("ID_GARE")

                    str_id_RESEAU = oDataRowTres("ID_RESEAU")

                    'Dim parametersTitulos_3() As ReportParameter = {par5}
                    'ReportViewer1.LocalReport.SetParameters(parametersTitulos_3)

                End If


                'pantalla:RESUMEN DE INGRESO CAJERO
                strQuerys = "SELECT MATRICULE,SAC AS Expr1, NVL(NB_POSTE, 0) + NVL(NB_POSTE_POS, 0) AS Expr2, NVL(RED_RCT_MONNAIE1, 0) AS Expr3, " &
                            "NVL(RED_RCT_CHQ, 0) AS Expr4, NVL(RED_NB_CHQ, 0) AS Expr5, NVL(RED_RCT_DEVISE, 0) AS Expr6, NVL(RED_RCT_MONNAIE3, 0) AS Expr7, " &
                            "NVL(RED_RCT_MONNAIE4, 0) AS Expr8, NVL(RED_CPT1, 0) AS Expr9, NVL(RED_RCT_MONNAIE1, 0) + NVL(RED_RCT_MONNAIE2, 0) + NVL(RED_RCT_MONNAIE3, 0) " &
                            "+ NVL(RED_RCT_MONNAIE4, 0) + NVL(RED_RCT_CHQ, 0) + NVL(RED_RCT_DEVISE, 0) AS Expr10, NVL(POSTE_RCT_MONNAIE1, 0) + NVL(POSTE_RCT_MONNAIE2, 0) " &
                            "+ NVL(POSTE_RCT_MONNAIE3, 0) + NVL(POSTE_RCT_MONNAIE4, 0) + NVL(POSTE_RCT_DEVISE, 0) + NVL(POSTE_RCT_CHQ, 0) + NVL(POSTE_POS_RCT_MONNAIE1, " &
                            "0) + NVL(POSTE_POS_RMB_MONNAIE1, 0) + NVL(POSTE_POS_RCT_MONNAIE2, 0) + NVL(POSTE_POS_RCT_MONNAIE3, 0) + NVL(POSTE_POS_RCT_MONNAIE4, 0) " &
                            "+ NVL(POSTE_POS_RCT_DEVISE, 0) + NVL(POSTE_POS_RCT_CHQ, 0) AS Expr11, NVL(RED_RCT_MONNAIE1, 0) + NVL(RED_RCT_MONNAIE2, 0) " &
                            "+ NVL(RED_RCT_MONNAIE3, 0) + NVL(RED_RCT_MONNAIE4, 0) + NVL(RED_RCT_DEVISE, 0) + NVL(RED_RCT_CHQ, 0) - NVL(POSTE_RCT_MONNAIE1, 0) " &
                            "- NVL(POSTE_RCT_MONNAIE2, 0) - NVL(POSTE_RCT_MONNAIE3, 0) - NVL(POSTE_RCT_MONNAIE4, 0) - NVL(POSTE_RCT_DEVISE, 0) - NVL(POSTE_RCT_CHQ, 0) " &
                            "- NVL(POSTE_POS_RCT_MONNAIE1, 0) - NVL(POSTE_POS_RMB_MONNAIE1, 0) - NVL(POSTE_POS_RCT_MONNAIE2, 0) - NVL(POSTE_POS_RCT_MONNAIE3, 0) " &
                            "- NVL(POSTE_POS_RCT_MONNAIE4, 0) - NVL(POSTE_POS_RCT_DEVISE, 0) - NVL(POSTE_POS_RCT_CHQ, 0) AS Expr12, NVL(RED_RCT_MONNAIE1, 0) " &
                            "- NVL(POSTE_RCT_MONNAIE1, 0) - NVL(POSTE_POS_RMB_MONNAIE1, 0) - NVL(POSTE_POS_RCT_MONNAIE1, 0) AS Expr13, NVL(RED_RCT_CHQ, 0) " &
                            "- NVL(POSTE_RCT_CHQ, 0) - NVL(POSTE_POS_RCT_CHQ, 0) AS Expr14, NVL(RED_RCT_DEVISE, 0) - NVL(POSTE_RCT_DEVISE, 0) - NVL(POSTE_POS_RCT_DEVISE, 0) " &
                            "AS Expr15, NVL(RED_CPT24, 0) AS Expr16, NVL(RED_CPT25, 0) AS Expr17, NVL(RED_JETON, 0) - NVL(POSTE_JETON, 0) AS Expr18, NVL(RED_RDD, 0) " &
                            "- NVL(POSTE_RDD, 0) AS Expr19, NVL(RED_GRATUIT, 0) + NVL(RED_CPT2, 0) - NVL(POSTE_GRATUIT, 0) AS Expr20, MATRICULE_COMMENTAIRE, COMMENTAIRE, " &
                            "0 AS Expr21, TO_CHAR(DATE_REDDITION, 'YYYYMMDDHH24MISS') AS Expr22, ID_SITE, RED_CPT21, NB_POSTE, ETAT_REDDITION, " &
                            "NVL(Red_Rct_Monnaie1,0)	+ NVL(Red_Rct_Devise,0)	+ NVL(Red_Rct_Chq,0) + NVL(Red_cpt21,0)	- NVL(Poste_Rct_Monnaie1,0) - NVL(Poste_Rct_Devise,0) - NVL(Poste_Rct_Chq,0) AS Expr23, RED_CPT22 " &
                            "FROM REDDITION " &
                            "WHERE  (ID_SITE = '" & Mid(id_plaza_cobro, 2, 2) & "') AND (MATRICULE = '" & oDataRow("MATRICULE") & "') AND (SAC = '" & oDataRow("Expr4") & "')"


                If objQuerys.QueryDataSetDos(strQuerys, "REDDITION") = 1 Then

                    VAR_11 = oDataRowDos("Expr3")
                    VAR_58 = oDataRowDos("Expr6")

                    If Not IsDBNull(oDataRowDos("RED_CPT21")) Then
                        VAR_71 = oDataRowDos("RED_CPT21")
                    Else
                        VAR_71 = 0
                    End If

                    If oDataRowDos("Expr23") > 0 Then

                        'VAR_67 = oDataRowDos("Expr23")
                        VAR_73 = 0

                    ElseIf oDataRowDos("Expr23") = 0 Then

                        'VAR_67 = 0
                        VAR_73 = 0

                    ElseIf oDataRowDos("Expr23") < 0 Then

                        VAR_73 = Math.Abs(oDataRowDos("Expr23"))
                        'VAR_67 = 0

                    End If

                    'Dim par9 As New ReportParameter()
                    'par9.Name = "par9"
                    'par9.Values.Add(Format(objControl.fecha(oDataRowDos("Expr22")), "MM/dd/yyyy"))

                    'Dim par10 As New ReportParameter()
                    'par10.Name = "par10"
                    'par10.Values.Add(Format(objControl.fecha(oDataRowDos("Expr22")), "HH:mm:ss"))

                    'Dim par2 As New ReportParameter()
                    'par2.Name = "par2"
                    'par2.Values.Add(oDataRowDos("Expr1"))
                    VAR_02 = oDataRowDos("Expr1")

                    VAR_02 = Replace(VAR_02, "A", "")
                    VAR_02 = Replace(VAR_02, "B", "")


                    'BOLETOS GENERADOS POR ERROR
                    VAR_13 = 0
                    VAR_12 = 0

                    'cancelados
                    'If Not IsDBNull(oDataRowDos("RED_CPT22")) Then
                    '    VAR_13 = oDataRowDos("RED_CPT22")
                    'End If



                End If


                'fin de SACAR TURNOS





                'par11.Name = "par11"
                'par11.Values.Add(Format(VAR_11, "###,###,##0.00"))

                'BOLETOS GENERADOS POR ERROR
                '            strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " & _
                '"FROM GEADBA.TRANSACTION,SITE_GARE " & _
                '"WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " & _
                '"AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " & _
                '"AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "



                '            strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " & _
                '            "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " & _
                '            "AND TRANSACTION.ID_GARE = " & int_id_gare & " " & _
                '            "AND ID_VOIE = '" & int_id_voie & "' " & _
                '            "AND VOIE = '" & str_id_voie & "' "



                '            strQuerys = strQuerys & "AND (ID_OBS_SEQUENCE = '5') "

                '14/07/2014
                'VAR_13 = 0

                ''VAR_12   importe de boletos generados por error (no se maneja)
                'VAR_12 = 0
                'If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                '    'par12.Name = "par12"
                '    'If IsDBNull(oDataRow("Monto_1")) Then
                '    '    par12.Values.Add(Format(0, "###,###,##0.00"))
                '    '    VAR_12 = 0
                '    'Else

                '    '    par12.Values.Add(Format(0, "###,###,##0.00"))
                '    '    VAR_12 = 0
                '    'End If


                '    'par13.Name = "par13"
                '    If IsDBNull(oDataRow("Cruces")) Then
                '        '    par13.Values.Add(Format(0, "###,###,##0.00"))
                '        VAR_13 = 0
                '    Else
                '        '    par13.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                '        VAR_13 = oDataRow("Cruces")
                '    End If


                'End If
                '---------------------------------------


                'VEHICULOS RESIDENTES PAGO INMEDIATO

                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1, SUM(PRIX_TOTAL) AS Monto_2, COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
          "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "



                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "



                strQuerys = strQuerys & "AND (ID_PAIEMENT = 71 " &
        "OR ID_PAIEMENT = 72 " &
        "OR ID_PAIEMENT = 73 " &
        "OR ID_PAIEMENT = 74  or ID_PAIEMENT = 10) AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') "

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    'par14.Name = "par14"
                    'If IsDBNull(oDataRow("Monto_2")) Then
                    '    par14.Values.Add(Format(0, "###,###,##0.00"))
                    'Else
                    '    par14.Values.Add(Format(oDataRow("Monto_2"), "###,###,##0.00"))
                    'End If

                    'par15.Name = "par15"
                    'If IsDBNull(oDataRow("Monto_2")) Then
                    '    par15.Values.Add(Format(0, "###,###,##0"))
                    'Else
                    '    par15.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                    'End If

                    VAR_14 = IIf(IsDBNull(oDataRow("Monto_2")), 0, oDataRow("Monto_2"))
                    VAR_15 = IIf(IsDBNull(oDataRow("Cruces")), 0, oDataRow("Cruces"))
                End If

                'SUBTOTAL MARCADO COMO PAGADO
                VAR_16 = VAR_11 + VAR_12


                'par16.Name = "par16"
                'par16.Values.Add(Format(VAR_16, "###,###,##0.00"))
                '-------------------
                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
         "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "



                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "



                strQuerys = strQuerys & "AND (ID_PAIEMENT = 1 " &
     "OR ID_PAIEMENT = 2 " &
    "OR ID_PAIEMENT = 71 " &
                    "OR ID_PAIEMENT = 72 " &
                    "OR ID_PAIEMENT = 73 " &
                    "OR ID_PAIEMENT = 74  or ID_PAIEMENT = 10) " &
                    "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') "



                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    'par17.Name = "par17"
                    'If IsDBNull(oDataRow("Cruces")) Then

                    '    par17.Values.Add(Format(0 + VAR_13, "###,###,##0"))
                    'Else

                    '    par17.Values.Add(Format(oDataRow("Cruces") + VAR_13, "###,###,##0"))
                    'End If



                    VAR_17 = IIf(IsDBNull(oDataRow("Cruces")), 0 + VAR_13, oDataRow("Cruces") + VAR_13)
                End If



                'tramo corto 17
                If Mid(id_plaza_cobro, 2, 2) = 84 Or Mid(id_plaza_cobro, 2, 2) = "02" Then
                    strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
         "FROM GEADBA.TRANSACTION,SITE_GARE " &
          "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
         "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
         "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                    strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                    "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                    "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                    "AND ID_VOIE = '" & int_id_voie & "' " &
                    "AND VOIE = '" & str_id_voie & "' "

                    strQuerys = strQuerys & "AND " &
         "ID_PAIEMENT = 2 " &
                             "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') "

                    If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                        VAR_17_tc = oDataRow("Cruces") 'IIf(IsDBNull(oDataRow("Cruces")), 0 + VAR_13, oDataRow("Cruces") + VAR_13)
                    End If
                End If
                'fin tramo corto 17




                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND (ID_PAIEMENT = 1 " &
                     "OR ID_PAIEMENT = 2 " &
                     "OR ID_PAIEMENT = 71 " &
                     "OR ID_PAIEMENT = 72 " &
                     "OR ID_PAIEMENT = 73 " &
                     "OR ID_PAIEMENT = 74  or ID_PAIEMENT = 10) " &
                     "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') "

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    'par18.Name = "par18"
                    'If IsDBNull(oDataRow("Monto_1")) Then
                    '    par18.Values.Add(Format(0, "###,###,##0.00"))
                    'Else
                    '    par18.Values.Add(Format(oDataRow("Monto_1"), "###,###,##0.00"))
                    'End If

                    'VAR_18 = IIf(IsDBNull(oDataRow("Monto_1")), 0, oDataRow("Monto_1"))


                    'par19.Name = "par19"
                    'If IsDBNull(oDataRow("Cruces")) Then
                    '    par19.Values.Add(Format(0, "###,###,##0"))
                    'Else
                    '    par19.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                    'End If

                    VAR_18 = IIf(IsDBNull(oDataRow("Monto_1")), 0, oDataRow("Monto_1"))
                    VAR_19 = IIf(IsDBNull(oDataRow("Cruces")), 0, oDataRow("Cruces"))
                End If


                'trmao cotro var 19
                If Mid(id_plaza_cobro, 2, 2) = 84 Or Mid(id_plaza_cobro, 2, 2) = "02" Then
                    strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                    strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                    "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                    "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                    "AND ID_VOIE = '" & int_id_voie & "' " &
                    "AND VOIE = '" & str_id_voie & "' "

                    strQuerys = strQuerys & "AND  " &
                         " ID_PAIEMENT = 2 " &
                                                 "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') "

                    If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                        VAR_19_tc = IIf(IsDBNull(oDataRow("Cruces")), 0, oDataRow("Cruces"))
                    End If
                End If
                'fin tramo corto var 19

                VAR_20 = VAR_16 - VAR_18
                'par20.Name = "par20"
                'par20.Values.Add(Format(VAR_20, "###,###,##0.00"))

                VAR_21 = VAR_17 - VAR_19
                'par21.Name = "par21"
                'par21.Values.Add(Format(VAR_21, "###,###,##0"))



                'BOLETOS RESIDENTES P.A.
                'par22.Name = "par22"
                'par22.Values.Add(Format(VAR_22, "###,###,##0.00"))

                'par23.Name = "par23"
                'par23.Values.Add(Format(VAR_23, "###,###,##0"))

                'par24.Name = "par24"
                'par24.Values.Add(Format(VAR_24, "###,###,##0.00"))

                'par25.Name = "par25"
                'par25.Values.Add(Format(VAR_25, "###,###,##0"))

                'par26.Name = "par26"
                'par26.Values.Add(Format(VAR_26, "###,###,##0.00"))

                'par27.Name = "par27"
                'par27.Values.Add(Format(VAR_27, "###,###,##0"))

                'SISTEMAS ELECTRONICOS
                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_PAIEMENT = 15 " '& _

                VAR_28 = 0
                VAR_29 = 0

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    'par28.Name = "par28"
                    If IsDBNull(oDataRow("Monto_1")) Then
                        'par28.Values.Add(Format(0, "###,###,##0.00"))
                        VAR_28 = 0
                    Else
                        'par28.Values.Add(Format(oDataRow("Monto_1"), "###,###,##0.00"))
                        VAR_28 = oDataRow("Monto_1")
                    End If

                    'par29.Name = "par29"
                    If IsDBNull(oDataRow("Cruces")) Then
                        'par29.Values.Add(Format(0, "###,###,##0"))
                        VAR_29 = 0
                    Else
                        'par29.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                        VAR_29 = oDataRow("Cruces")
                    End If

                End If

                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_PAIEMENT = 15 " '& _

                VAR_30 = 0
                VAR_31 = 0


                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    'par30.Name = "par30"
                    If IsDBNull(oDataRow("Monto_1")) Then
                        'par30.Values.Add(Format(0, "###,###,##0.00"))
                        VAR_30 = 0
                    Else
                        'par30.Values.Add(Format(oDataRow("Monto_1"), "###,###,##0.00"))
                        VAR_30 = oDataRow("Monto_1")
                    End If

                    'par31.Name = "par31"
                    If IsDBNull(oDataRow("Cruces")) Then
                        'par31.Values.Add(Format(0, "###,###,##0"))
                        VAR_31 = 0
                    Else
                        'par31.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                        VAR_31 = oDataRow("Cruces")
                    End If

                End If


                'VSC
                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                ' (ID_OBS_SEQUENCE = '0' or ID_OBS_SEQUENCE = 'F')
                'strQuerys = strQuerys & "AND (ID_PAIEMENT = 27 and ID_OBS_SEQUENCE = '0') "
                strQuerys = strQuerys & "AND (ID_PAIEMENT = 27 and (ID_OBS_SEQUENCE = '0' or ID_OBS_SEQUENCE = 'F')) "

                '14/07/2014
                VAR_40 = 0
                VAR_41 = 0
                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    'par32.Name = "par32"
                    If IsDBNull(oDataRow("Monto_1")) Then
                        '    par32.Values.Add(Format(0, "###,###,##0.00"))
                        VAR_40 = 0
                    Else
                        '    par32.Values.Add(Format(oDataRow("Monto_1"), "###,###,##0.00"))
                        VAR_40 = oDataRow("Monto_1")
                    End If

                    'par33.Name = "par33"
                    If IsDBNull(oDataRow("Cruces")) Then
                        '    par33.Values.Add(Format(0, "###,###,##0"))
                        VAR_41 = 0
                    Else
                        '    par33.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                        VAR_41 = oDataRow("Cruces")
                    End If

                End If

                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                '(ID_OBS_SEQUENCE = '0' or ID_OBS_SEQUENCE = 'F')
                'strQuerys = strQuerys & "AND (ID_PAIEMENT = 27 and ID_OBS_SEQUENCE = '0') "
                strQuerys = strQuerys & "AND (ID_PAIEMENT = 27 and (ID_OBS_SEQUENCE = '0' or ID_OBS_SEQUENCE = 'F')) "

                '14/07/2014
                VAR_42 = 0
                VAR_43 = 0

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    'par34.Name = "par34"
                    If IsDBNull(oDataRow("Monto_1")) Then
                        '    par34.Values.Add(Format(0, "###,###,##0.00"))
                        VAR_42 = 0
                    Else
                        '    par34.Values.Add(Format(oDataRow("Monto_1"), "###,###,##0.00"))
                        VAR_42 = oDataRow("Monto_1")
                    End If

                    'par35.Name = "par35"
                    If IsDBNull(oDataRow("Cruces")) Then
                        'par35.Values.Add(Format(0, "###,###,##0"))
                        VAR_43 = 0
                    Else
                        '    par35.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                        VAR_43 = oDataRow("Cruces")
                    End If

                End If





                'VEHICULOS ELUDIDOS
                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_PAIEMENT = 13 " &
                "AND ID_OBS_SEQUENCE = '7' "

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    'par40.Name = "par40"
                    'If IsDBNull(oDataRow("Monto_1")) Then
                    '    par40.Values.Add(Format(0, "###,###,##0.00"))
                    'Else
                    '    par40.Values.Add(Format(oDataRow("Monto_1"), "###,###,##0.00"))
                    '    VAR_40 = oDataRow("Monto_1")
                    'End If

                    'par41.Name = "par41"
                    'If IsDBNull(oDataRow("Cruces")) Then
                    '    par41.Values.Add(Format(0, "###,###,##0"))
                    'Else
                    '    par41.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                    '    VAR_41 = oDataRow("Cruces")
                    'End If

                End If



                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "



                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "



                strQuerys = strQuerys & "AND ID_PAIEMENT = 13 " &
                "AND ID_OBS_SEQUENCE = '7' "

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    'par42.Name = "par42"
                    'If IsDBNull(oDataRow("Monto_1")) Then
                    '    par42.Values.Add(Format(0, "###,###,##0.00"))
                    'Else
                    '    par42.Values.Add(Format(oDataRow("Monto_1"), "###,###,##0.00"))
                    '    VAR_42 = oDataRow("Monto_1")
                    'End If

                    'par43.Name = "par43"
                    'If IsDBNull(oDataRow("Cruces")) Then
                    '    par43.Values.Add(Format(0, "###,###,##0"))
                    'Else
                    '    par43.Values.Add(Format(oDataRow("Cruces"), "###,###,##0"))
                    '    VAR_43 = oDataRow("Cruces")
                    'End If

                End If

                'RECLASIFICADOS


                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "



                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "


                strQuerys = strQuerys & "AND ID_OBS_SEQUENCE = 'F'"

                '14/07/2014
                VAR_67 = 0
                VAR_68 = 0

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    If IsDBNull(oDataRow("Monto_1")) Then

                        VAR_67 = 0
                    Else

                        VAR_67 = oDataRow("Monto_1")
                    End If

                    If IsDBNull(oDataRow("Cruces")) Then

                        VAR_68 = 0
                    Else
                        VAR_68 = oDataRow("Cruces")
                    End If

                End If

                'DETECCIONES ERRONEAS
                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_OBS_PASSAGE = '6' "

                '14/07/2014
                VAR_69 = 0
                VAR_70 = 0

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    If IsDBNull(oDataRow("Monto_1")) Then
                        VAR_69 = 0
                    Else
                        VAR_69 = oDataRow("Monto_1")
                    End If

                    If IsDBNull(oDataRow("Cruces")) Then
                        VAR_70 = 0
                    Else
                        VAR_70 = oDataRow("Cruces")
                    End If

                End If

                'tarjera de credito
                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_PAIEMENT = 12 "

                VAR_32 = 0
                VAR_33 = 0
                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    If IsDBNull(oDataRow("Monto_1")) Then

                        VAR_32 = 0
                    Else

                        VAR_32 = oDataRow("Monto_1")
                    End If


                    If IsDBNull(oDataRow("Cruces")) Then

                        VAR_33 = 0
                    Else

                        VAR_33 = oDataRow("Cruces")
                    End If

                End If

                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_PAIEMENT = 12 "

                VAR_34 = 0
                VAR_35 = 0

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    If IsDBNull(oDataRow("Monto_1")) Then
                        VAR_34 = 0
                    Else
                        VAR_34 = oDataRow("Monto_1")
                    End If

                    If IsDBNull(oDataRow("Cruces")) Then
                        VAR_35 = 0
                    Else
                        VAR_35 = oDataRow("Cruces")
                    End If

                End If

                'tarjeta de debito

                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_PAIEMENT = 14 "

                VAR_36 = 0
                VAR_37 = 0
                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                    If IsDBNull(oDataRow("Monto_1")) Then

                        VAR_36 = 0
                    Else

                        VAR_36 = oDataRow("Monto_1")
                    End If


                    If IsDBNull(oDataRow("Cruces")) Then

                        VAR_37 = 0
                    Else

                        VAR_37 = oDataRow("Cruces")
                    End If

                End If

                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " &
        "FROM GEADBA.TRANSACTION,SITE_GARE " &
        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
        "AND (DATE_TRANSACTION >= TO_DATE('" & Format(dt_ini_poste, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
        "AND (DATE_TRANSACTION <= TO_DATE('" & Format(dt_fin_poste, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                strQuerys = strQuerys & "AND (MATRICULE = '" & str_MATRICULE & "') " &
                "AND TRANSACTION.ID_RESEAU = '" & str_id_RESEAU & "' " &
                "AND TRANSACTION.ID_GARE = " & int_id_gare & " " &
                "AND ID_VOIE = '" & int_id_voie & "' " &
                "AND VOIE = '" & str_id_voie & "' "

                strQuerys = strQuerys & "AND ID_PAIEMENT = 14 "

                VAR_38 = 0
                VAR_39 = 0

                If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then
                    If IsDBNull(oDataRow("Monto_1")) Then
                        VAR_38 = 0
                    Else
                        VAR_38 = oDataRow("Monto_1")
                    End If

                    If IsDBNull(oDataRow("Cruces")) Then
                        VAR_39 = 0
                    Else
                        VAR_39 = oDataRow("Cruces")
                    End If

                End If


                str_detalle = Format(VAR_01, "dd/MM/yyyy") & "," & int_turno & "," & Format(dt_ini_poste, "HHmmss") & "," & Format(dt_fin_poste, "HHmmss") & ","
                str_detalle_tc = Format(VAR_01, "dd/MM/yyyy") & "," & int_turno & "," & Format(dt_ini_poste, "HHmmss") & "," & Format(dt_fin_poste, "HHmmss") & ","
                'clave tramo verificar
                'Verificar 
                'str_detalle = str_detalle & "247" & ","

                'Francisco velasco
                If id_plaza_cobro_local = 84 Then

                    str_detalle = str_detalle & "247" & ","

                    str_detalle_tc = str_detalle_tc & "340" & ","

                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "2585" & ","
                        str_detalle_tc = str_detalle_tc & "2585" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "2586" & ","
                        str_detalle_tc = str_detalle_tc & "2586" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "2587" & ","
                        str_detalle_tc = str_detalle_tc & "2587" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "2588" & ","
                        str_detalle_tc = str_detalle_tc & "2588" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "2589" & ","
                        str_detalle_tc = str_detalle_tc & "2589" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "2590" & ","
                        str_detalle_tc = str_detalle_tc & "2590" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "2591" & ","
                        str_detalle_tc = str_detalle_tc & "2591" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "2592" & ","
                        str_detalle_tc = str_detalle_tc & "2592" & ","
                    End If

                    'Paso morelos
                ElseIf id_plaza_cobro_local = 2 Then

                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "249" & ","
                        str_detalle = str_detalle & "1803" & ","
                        str_detalle_tc = str_detalle_tc & "261" & ","
                        str_detalle_tc = str_detalle_tc & "1803" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1804" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1805" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1806" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1807" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1808" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1809" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "249" & ","
                        str_detalle = str_detalle & "1810" & ","
                        str_detalle_tc = str_detalle_tc & "261" & ","
                        str_detalle_tc = str_detalle_tc & "1810" & ","
                        '--------------------------------------------
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1811" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1812" & ","
                    End If

                    'la venta
                ElseIf id_plaza_cobro_local = 4 Then

                    str_detalle = str_detalle & "252" & ","

                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1830" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1831" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1832" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1833" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1834" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1835" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1836" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1837" & ","

                    End If

                    'la venta
                ElseIf id_plaza_cobro_local = 161 Then

                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "364" & "," & "2681" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "364" & "," & "2682" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "363" & "," & "2683" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "363" & "," & "2684" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "364" & "," & "2685" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "364" & "," & "2686" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "363" & "," & "2687" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "363" & "," & "2688" & ","

                    End If



                    'palo blanco
                ElseIf id_plaza_cobro_local = 3 Then

                    str_detalle = str_detalle & "251" & ","

                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1816" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1817" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1818" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1819" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1820" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1821" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1822" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1823" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "1824" & ","
                    End If

                    'álpuyeca
                    '101
                    '246
                    '1	1794
                    '2	1795
                    '3	1796
                    '4	1797
                ElseIf id_plaza_cobro_local = 1 Then

                    str_detalle = str_detalle & "246" & ","

                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1794" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1795" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1796" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1797" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1798" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1799" & ","

                    End If




                    'tlalpan
                ElseIf id_plaza_cobro_local = 8 Then

                    str_detalle = str_detalle & "118" & ","

                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "14" Then
                        str_detalle = str_detalle & "3076" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "3063" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "3064" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "3065" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "3066" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "3067" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "3068" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "3069" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "3070" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "3071" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "3072" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "11" Then
                        str_detalle = str_detalle & "3073" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "12" Then
                        str_detalle = str_detalle & "3074" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "13" Then
                        str_detalle = str_detalle & "3075" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "15" Then
                        str_detalle = str_detalle & "3077" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "16" Then
                        str_detalle = str_detalle & "3078" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "17" Then
                        str_detalle = str_detalle & "3079" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "18" Then
                        str_detalle = str_detalle & "3080" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "19" Then
                        str_detalle = str_detalle & "3081" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "20" Then
                        str_detalle = str_detalle & "3082" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "21" Then
                        str_detalle = str_detalle & "3083" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "22" Then
                        str_detalle = str_detalle & "3084" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "23" Then
                        str_detalle = str_detalle & "3085" & ","
                    End If

                    'xochitepec
                ElseIf id_plaza_cobro = 105 Then

                    str_detalle = str_detalle & "365" & ","

                    If CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "2727" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "2728" & ","
                    End If




                    'CERRO GORDO

                ElseIf id_plaza_cobro = 186 Then

                    str_detalle = str_detalle & "351" & ","

                    If CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "21" Then
                        str_detalle = str_detalle & "3199" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "22" Then
                        str_detalle = str_detalle & "3200" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "23" Then
                        str_detalle = str_detalle & "3201" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "24" Then
                        str_detalle = str_detalle & "3202" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "25" Then
                        str_detalle = str_detalle & "3203" & ","
                        'ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "6" Then
                        'str_detalle = str_detalle & "3185" & ","
                        'ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "7" Then
                        'str_detalle = str_detalle & "3186" & ","
                        'ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "8" Then
                        'str_detalle = str_detalle & "3187" & ","
                        'ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "9" Then
                        'str_detalle = str_detalle & "3188" & ","
                    End If

                    'QUERETARO
                ElseIf id_plaza_cobro = 106 Then
                    str_detalle = str_detalle & "112" & ","
                    'Segmento B
                    If CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1079" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1080" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1081" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1082" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1083" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1084" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1085" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1086" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "1087" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "1088" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "11" Then
                        str_detalle = str_detalle & "1089" & ","
                    End If
                ElseIf id_plaza_cobro = 183 Then

                    str_detalle = str_detalle & "179" & ","

                    If CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "2581" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "2582" & ","
                    End If


                ElseIf id_plaza_cobro = 189 Then

                    str_detalle = str_detalle & "365" & ","

                    If CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1891" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1892" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1893" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1894" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1895" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1896" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1897" & ","
                    End If
                    'VillaGrand
                ElseIf id_plaza_cobro = 183 Then

                    str_detalle = str_detalle & "170" & ","

                    If CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "2581" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "2582" & ","
                    End If


                    'tres marias
                ElseIf id_plaza_cobro = 109 Then

                    str_detalle = str_detalle & "102" & ","

                    If CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1020" & ","
                    ElseIf CInt(Mid(Trim(Trim(VAR_08)), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1021" & ","
                    End If

                    'central de abastos
                ElseIf id_plaza_cobro_local = 7 Then

                    str_detalle = str_detalle & "368" & ","





                    'Segmento B
                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1843" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1844" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1845" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1846" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1847" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1848" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1849" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1850" & ","

                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "1851" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "1852" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "11" Then
                        str_detalle = str_detalle & "1853" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "12" Then
                        str_detalle = str_detalle & "1854" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "13" Then
                        str_detalle = str_detalle & "2743" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "14" Then
                        str_detalle = str_detalle & "2744" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "15" Then
                        str_detalle = str_detalle & "2745" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "16" Then
                        str_detalle = str_detalle & "2746" & ","
                    End If

                ElseIf id_plaza_cobro_local = 189 Then

                    str_detalle = str_detalle & "189" & ","
                    'Segmento B
                    If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1891" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1892" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1893" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1894" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1895" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1896" & ","
                    ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1897" & ","
                    End If

                    '    'SAN MARCOS
                    'ElseIf id_plaza_cobro_local = 7 Then

                    '    str_detalle = str_detalle & "121" & ","

                    '    If Mid(Trim(VAR_08), 1, 1) = "A" Then

                    '        If CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                    '            str_detalle = str_detalle & "1102" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                    '            str_detalle = str_detalle & "1103" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                    '            str_detalle = str_detalle & "1104" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "9" Then
                    '            str_detalle = str_detalle & "1105" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "10" Then
                    '            str_detalle = str_detalle & "1106" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "11" Then
                    '            str_detalle = str_detalle & "1107" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "12" Then
                    '            str_detalle = str_detalle & "1108" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "13" Then
                    '            str_detalle = str_detalle & "1109" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "14" Then
                    '            str_detalle = str_detalle & "1110" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "15" Then
                    '            str_detalle = str_detalle & "1101" & ","
                    '        End If

                    '    ElseIf Mid(Trim(VAR_08), 1, 1) = "B" Then

                    '        If CInt(Mid(Trim(VAR_08), 2, 2)) = "1" Then
                    '            str_detalle = str_detalle & "1097" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "2" Then
                    '            str_detalle = str_detalle & "1098" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "3" Then
                    '            str_detalle = str_detalle & "1099" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "4" Then
                    '            str_detalle = str_detalle & "1100" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "5" Then
                    '            str_detalle = str_detalle & "1101" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "6" Then
                    '            str_detalle = str_detalle & "1102" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "7" Then
                    '            str_detalle = str_detalle & "1103" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "8" Then
                    '            str_detalle = str_detalle & "1104" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "9" Then
                    '            str_detalle = str_detalle & "1105" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "10" Then
                    '            str_detalle = str_detalle & "1106" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "11" Then
                    '            str_detalle = str_detalle & "1107" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "12" Then
                    '            str_detalle = str_detalle & "1108" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "13" Then
                    '            str_detalle = str_detalle & "1109" & ","
                    '        ElseIf CInt(Mid(Trim(VAR_08), 2, 2)) = "14" Then
                    '            str_detalle = str_detalle & "1110" & ","
                    '        End If

                    '    End If

                Else
                    str_detalle = str_detalle & ","
                    str_detalle = str_detalle & ","
                End If


                'cuerpo
                str_detalle = str_detalle & Mid(Trim(VAR_08), 1, 1) & ","
                str_detalle_tc = str_detalle_tc & Mid(Trim(VAR_08), 1, 1) & ","


                str_detalle = str_detalle & VAR_02 & ","
                str_detalle_tc = str_detalle_tc & VAR_02 & ","

                '97
                str_detalle = str_detalle & VAR_97 & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '98
                str_detalle = str_detalle & VAR_98 & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '100 (no se maneja)
                str_detalle = str_detalle & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '92
                str_detalle = str_detalle & VAR_92 & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '93
                str_detalle = str_detalle & VAR_93 & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '95
                str_detalle = str_detalle & ","
                str_detalle_tc = str_detalle_tc & ","
                '11
                str_detalle = str_detalle & VAR_11 & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '18
                str_detalle = str_detalle & VAR_18 & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '16
                str_detalle = str_detalle & VAR_16 & ","
                str_detalle_tc = str_detalle_tc & "0,"
                '17
                str_detalle = str_detalle & VAR_17 & ","

                '19
                str_detalle = str_detalle & VAR_19 & ","
                '58
                str_detalle = str_detalle & VAR_58 & ","
                '+
                str_detalle = str_detalle & VAR_11 & ","
                '71
                str_detalle = str_detalle & VAR_71 & ","
                '73
                str_detalle = str_detalle & VAR_73 & ","
                '13
                str_detalle = str_detalle & VAR_13 & ","
                '12
                str_detalle = str_detalle & VAR_12 & ","
                '23
                str_detalle = str_detalle & VAR_23 & ","
                '22
                str_detalle = str_detalle & VAR_22 & ","
                '25
                str_detalle = str_detalle & VAR_25 & ","
                '24
                str_detalle = str_detalle & VAR_24 & ","

                '---------------------------------------------------------------
                '29
                str_detalle = str_detalle & VAR_29 & ","

                '28
                str_detalle = str_detalle & VAR_28 & ","

                '31
                str_detalle = str_detalle & VAR_31 & ","

                '30
                str_detalle = str_detalle & VAR_30 & ","

                '41
                str_detalle = str_detalle & VAR_41 & ","

                '40
                str_detalle = str_detalle & VAR_40 & ","

                '43
                str_detalle = str_detalle & VAR_43 & ","

                '42
                str_detalle = str_detalle & VAR_42 & ","

                '45
                str_detalle = str_detalle & VAR_45 & ","

                '44
                str_detalle = str_detalle & VAR_44 & ","

                '47
                str_detalle = str_detalle & VAR_47 & ","

                '46
                str_detalle = str_detalle & VAR_46 & ","

                '49
                str_detalle = str_detalle & VAR_49 & ","

                '48
                str_detalle = str_detalle & VAR_48 & ","

                '51
                str_detalle = str_detalle & VAR_51 & ","

                '50
                str_detalle = str_detalle & VAR_50 & ","

                '68
                str_detalle = str_detalle & VAR_68 & ","

                '67
                str_detalle = str_detalle & VAR_67 & ","

                '70
                str_detalle = str_detalle & VAR_70 & ","

                '69
                str_detalle = str_detalle & VAR_69 & ","

                '102
                'str_detalle = str_detalle & VAR_94 & ","
                str_detalle = str_detalle & Format(CDec(VAR_11 + VAR_71), "########0.00") & ","

                '103
                str_detalle = str_detalle & VAR_103 & ","

                '+
                str_detalle = str_detalle & ","

                If (VAR_78 - VAR_77) >= 9999 Then
                    Dim h78 As Double
                    h78 = VAR_98 - VAR_97
                    VAR_78 = VAR_77 + h78
                    '77
                    str_detalle = str_detalle & VAR_77 & ","
                    '78
                    str_detalle = str_detalle & VAR_78 & ","
                Else
                    '77
                    str_detalle = str_detalle & VAR_77 & ","
                    '78
                    str_detalle = str_detalle & VAR_78 & ","
                End If
                '80
                str_detalle = str_detalle & VAR_80 & ","

                '82
                str_detalle = str_detalle & VAR_82 & ","

                '83
                str_detalle = str_detalle & VAR_83 & ","

                '85
                str_detalle = str_detalle & VAR_85 & ","

                '87
                str_detalle = str_detalle & VAR_87 & ","

                '88
                str_detalle = str_detalle & VAR_88 & ","

                '90
                str_detalle = str_detalle & VAR_90 & ","

                '60
                str_detalle = str_detalle & VAR_60 & ","

                '59
                str_detalle = str_detalle & VAR_59 & ","

                '62
                str_detalle = str_detalle & VAR_62 & ","

                '15
                str_detalle = str_detalle & VAR_15 & ","

                '14
                str_detalle = str_detalle & VAR_14 & ","

                '15
                str_detalle = str_detalle & VAR_15 & ","

                '14
                str_detalle = str_detalle & VAR_14 & ","

                '33
                str_detalle = str_detalle & VAR_33 & ","

                '32
                str_detalle = str_detalle & VAR_32 & ","

                '35
                str_detalle = str_detalle & VAR_35 & ","

                '34
                str_detalle = str_detalle & VAR_34 & ","


                '37
                str_detalle = str_detalle & VAR_37 & ","

                '36
                str_detalle = str_detalle & VAR_36 & ","


                '39
                str_detalle = str_detalle & VAR_39 & ","

                '38
                str_detalle = str_detalle & VAR_38 & ","


                oSW.WriteLine(str_detalle)


                'tramos cortos
                If id_plaza_cobro_local = 84 Then
                    'If VAR_17_tc > 0 Or VAR_19_tc > 0 Then
                    str_detalle_tc = str_detalle_tc & VAR_17_tc & "," & VAR_19_tc & "," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0,"

                    If Mid(Trim(VAR_08), 1, 1) = "A" Then
                        oSW.WriteLine(str_detalle_tc)
                    End If
                    'oSW.WriteLine(str_detalle_tc)
                    'End If

                ElseIf id_plaza_cobro_local = 2 Then

                    str_detalle_tc = str_detalle_tc & VAR_17_tc & "," & VAR_19_tc & "," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0," & "0,"

                    If Trim(VAR_08) = "A01" Or Trim(VAR_08) = "B08" Then
                        oSW.WriteLine(str_detalle_tc)
                    End If

                End If

                'fin tramos cortos

                '************************************
                '************************************
                '***********************************
                'fin detalle

            Next




        End If

        'cerrados inicio
        strQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM, BAG_NUMBER, REPORT_FLAG, GENERATION_DHM " &
"FROM CLOSED_LANE_REPORT, SITE_GARE " &
"where " &
"CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"order by BEGIN_DHM"

        If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 1 Then

            For cont = 0 To oDataSet.Tables("CLOSED_LANE_REPORT").Rows.Count - 1

                oDataRow = oDataSet.Tables("CLOSED_LANE_REPORT").Rows.Item(cont)

                str_detalle = ""

                'Fecha base de operación 	Fecha 	dd/mm/aaaa
                'str_detalle = Format(oDataRow("BEGIN_DHM"), "dd/MM/yyyy") & ","
                'Format(dt_Fecha_Inicio, "MM/dd/yyyy")
                str_detalle = Format(dt_Fecha_Inicio, "dd/MM/yyyy") & ","


                'Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
                str_detalle = str_detalle & int_turno & ","
                'Hora inicial de operación 	Caracter 	hhmmss 	
                str_detalle = str_detalle & Format(oDataRow("BEGIN_DHM"), "HHmmss") & ","
                'Hora final de operación 	Caracter 	hhmmss 	

                If oDataRow("END_DHM") > h_fin_turno Then
                    str_detalle = str_detalle & Format(h_fin_turno, "HHmmss") & ","
                Else
                    str_detalle = str_detalle & Format(oDataRow("END_DHM"), "HHmmss") & ","
                End If

                'Clave de tramo	Entero 	>9	Valores posibles:  Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                'Verificar 




                'str_detalle = str_detalle & "247" & ","
                ''Número de carril 	Entero 	>>9	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                'If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                '    str_detalle = str_detalle & "LANE" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                '    str_detalle = str_detalle & "LANE" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                '    str_detalle = str_detalle & "2587" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                '    str_detalle = str_detalle & "2588" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                '    str_detalle = str_detalle & "2589" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                '    str_detalle = str_detalle & "2590" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                '    str_detalle = str_detalle & "2591" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                '    str_detalle = str_detalle & "2592" & ","
                'End If
                If id_plaza_cobro = 184 Then
                    str_detalle = str_detalle & "247" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "2585" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "2586" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "2587" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "2588" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "2589" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "2590" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "2591" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "2592" & ","
                    End If

                    'paso morelos
                ElseIf id_plaza_cobro = 102 Then

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "249" & ","
                        str_detalle = str_detalle & "1803" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1804" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1805" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1806" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1807" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1808" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1809" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "261" & ","
                        str_detalle = str_detalle & "1810" & ","
                        '--------------------------------------------
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1811" & ","

                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "250" & ","
                        str_detalle = str_detalle & "1812" & ","
                    End If

                    'la venta
                ElseIf id_plaza_cobro = 104 Then
                    str_detalle = str_detalle & "252" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1830" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1831" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1832" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1833" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1834" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1835" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1836" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1837" & ","
                    End If


                ElseIf id_plaza_cobro = 161 Then


                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "364" & "," & "2681" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "364" & "," & "2682" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "363" & "," & "2683" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "363" & "," & "2684" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "364" & "," & "2685" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "364" & "," & "2686" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "363" & "," & "2687" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "363" & "," & "2688" & ","
                    End If

                ElseIf id_plaza_cobro = 103 Then
                    str_detalle = str_detalle & "251" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1816" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1817" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1818" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1819" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1820" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1821" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1822" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1823" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "1824" & ","
                    End If

                    'álpuyeca
                    '101
                    '246
                    '1	1794
                    '2	1795
                    '3	1796
                    '4	1797
                ElseIf id_plaza_cobro = 101 Then
                    str_detalle = str_detalle & "246" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1794" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1795" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1796" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1797" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1798" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1799" & ","
                    End If

                    'tlalpan
                ElseIf id_plaza_cobro = 108 Then

                    str_detalle = str_detalle & "118" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                        str_detalle = str_detalle & "3076" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "3063" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "3064" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "3065" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "3066" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "3067" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "3068" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "3069" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "3070" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "3071" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "3072" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                        str_detalle = str_detalle & "3073" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                        str_detalle = str_detalle & "3074" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                        str_detalle = str_detalle & "3075" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "15" Then
                        str_detalle = str_detalle & "3077" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "16" Then
                        str_detalle = str_detalle & "3078" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "17" Then
                        str_detalle = str_detalle & "3079" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "18" Then
                        str_detalle = str_detalle & "3080" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "19" Then
                        str_detalle = str_detalle & "3081" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "20" Then
                        str_detalle = str_detalle & "3082" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "21" Then
                        str_detalle = str_detalle & "3083" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "22" Then
                        str_detalle = str_detalle & "3084" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "23" Then
                        str_detalle = str_detalle & "3085" & ","
                    End If

                    'xochitepec
                ElseIf id_plaza_cobro = 105 Then

                    str_detalle = str_detalle & "365" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "2727" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "2728" & ","
                    End If

                    'CERRO GORDO
                ElseIf id_plaza_cobro = 186 Then

                    str_detalle = str_detalle & "351" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "21" Then
                        str_detalle = str_detalle & "3199" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "22" Then
                        str_detalle = str_detalle & "3200" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "23" Then
                        str_detalle = str_detalle & "3201" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "24" Then
                        str_detalle = str_detalle & "3202" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "25" Then
                        str_detalle = str_detalle & "3203" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "3185" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "3186" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "3187" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "3188" & ","


                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "22" Then
                        str_detalle = str_detalle & "3200" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "23" Then
                        str_detalle = str_detalle & "3201" & ","

                    End If

                    '   QUERETARO
                ElseIf id_plaza_cobro = 106 Then
                    str_detalle = str_detalle & "112" & ","
                    'Segmento B
                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1079" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1080" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1081" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1082" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1083" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1084" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1085" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1086" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "1087" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "1088" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                        str_detalle = str_detalle & "1089" & ","
                    End If
                    'Villagrand
                ElseIf id_plaza_cobro = 183 Then

                    str_detalle = str_detalle & "170" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "2581" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "2582" & ","
                    End If

                    'tres marias
                ElseIf id_plaza_cobro = 109 Then

                    str_detalle = str_detalle & "102" & ","

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1020" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1021" & ","
                    End If

                    'Central de Abastos
                ElseIf id_plaza_cobro = 107 Then
                    str_detalle = str_detalle & "368" & ","
                    'Segmento B

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1843" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1844" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1845" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1846" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1847" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1848" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1849" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                        str_detalle = str_detalle & "1850" & ","
                        'Segmento A

                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                        str_detalle = str_detalle & "1851" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                        str_detalle = str_detalle & "1852" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                        str_detalle = str_detalle & "1853" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                        str_detalle = str_detalle & "1854" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                        str_detalle = str_detalle & "2743" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                        str_detalle = str_detalle & "2744" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "15" Then
                        str_detalle = str_detalle & "2745" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "16" Then
                        str_detalle = str_detalle & "2746" & ","
                    End If

                ElseIf id_plaza_cobro = 189 Then
                    str_detalle = str_detalle & "189" & ","
                    'Segmento B

                    If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        str_detalle = str_detalle & "1891" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        str_detalle = str_detalle & "1892" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        str_detalle = str_detalle & "1893" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        str_detalle = str_detalle & "1894" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                        str_detalle = str_detalle & "1895" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                        str_detalle = str_detalle & "1896" & ","
                    ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                        str_detalle = str_detalle & "1897" & ","
                    End If
                    '    'SAN MARCOS
                    'ElseIf id_plaza_cobro = 107 Then

                    '    str_detalle = str_detalle & "121" & ","

                    '    If Mid(Trim(oDataRow("LANE")), 1, 1) = "A" Then

                    '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                    '            str_detalle = str_detalle & "1102" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                    '            str_detalle = str_detalle & "1103" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                    '            str_detalle = str_detalle & "1104" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                    '            str_detalle = str_detalle & "1105" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                    '            str_detalle = str_detalle & "1106" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                    '            str_detalle = str_detalle & "1107" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                    '            str_detalle = str_detalle & "1108" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                    '            str_detalle = str_detalle & "1109" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                    '            str_detalle = str_detalle & "1110" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "15" Then
                    '            str_detalle = str_detalle & "1101" & ","
                    '        End If

                    '    ElseIf Mid(Trim(oDataRow("LANE")), 1, 1) = "B" Then

                    '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                    '            str_detalle = str_detalle & "1097" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                    '            str_detalle = str_detalle & "1098" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                    '            str_detalle = str_detalle & "1099" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                    '            str_detalle = str_detalle & "1100" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "5" Then
                    '            str_detalle = str_detalle & "1101" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "6" Then
                    '            str_detalle = str_detalle & "1102" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "7" Then
                    '            str_detalle = str_detalle & "1103" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "8" Then
                    '            str_detalle = str_detalle & "1104" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "9" Then
                    '            str_detalle = str_detalle & "1105" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "10" Then
                    '            str_detalle = str_detalle & "1106" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "11" Then
                    '            str_detalle = str_detalle & "1107" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "12" Then
                    '            str_detalle = str_detalle & "1108" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "13" Then
                    '            str_detalle = str_detalle & "1109" & ","
                    '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "14" Then
                    '            str_detalle = str_detalle & "1110" & ","
                    '        End If

                    '    End If



                Else
                    str_detalle = str_detalle & ","
                    str_detalle = str_detalle & ","
                End If




                'Cuerpo 	Caracter 	X(1)	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                '& ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"
                str_detalle = str_detalle & Mid(Trim(oDataRow("LANE")), 1, 1) & ","

                'Format((hr_fecha_fin_varios_turnos), "yyyy") & Format((hr_fecha_fin_varios_turnos), "MM") & Format((hr_fecha_fin_varios_turnos), "dd") & Format((hr_fecha_fin_varios_turnos), "HH") & Format((hr_fecha_fin_varios_turnos), "mm") & Format((hr_fecha_fin_varios_turnos), "ss")

                'str_detalle = str_detalle & Format((oDataRow("END_DHM")), "yyyy") & Format((oDataRow("END_DHM")), "MM") & Format((oDataRow("END_DHM")), "dd") & Format((oDataRow("END_DHM")), "HH") & Format((oDataRow("END_DHM")), "mm") & Format((oDataRow("END_DHM")), "ss") & ","
                str_detalle = str_detalle & Format((oDataRow("END_DHM")), "mm") & Format((oDataRow("END_DHM")), "ss") & ","

                strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces, MIN(EVENT_NUMBER) AS minimo, MAX(EVENT_NUMBER) as maximo " &
   "FROM GEADBA.TRANSACTION,SITE_GARE " &
   "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
   "AND (DATE_TRANSACTION >= TO_DATE('" & Format(oDataRow("BEGIN_DHM"), "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) AND ID_OBS_PASSAGE <> '1' " '& _
                '"AND (DATE_TRANSACTION <= TO_DATE('" & Format(oDataRow("END_DHM"), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                If oDataRow("END_DHM") > h_fin_turno Then
                    strQuerys = strQuerys & "AND (DATE_TRANSACTION <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "
                Else
                    strQuerys = strQuerys & "AND (DATE_TRANSACTION <= TO_DATE('" & Format(oDataRow("END_DHM"), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "
                End If


                strQuerys = strQuerys & " " &
                                              "AND VOIE = '" & oDataRow("LANE") & "'  and EVENT_NUMBER <> 0  "


                If objQuerys.QueryDataSetTres(strQuerys, "TRANSACTION") = 1 Then

                    If oDataRowTres("Cruces") > 0 Then
                        'par89.Values.Add(oDataRow("minimo"))
                        str_detalle = str_detalle & oDataRowTres("minimo") & ","
                    Else
                        'par89.Values.Add(0)
                        str_detalle = str_detalle & "0,"
                    End If

                    If oDataRowTres("Cruces") > 0 Then
                        'par90.Values.Add(oDataRow("maximo"))
                        str_detalle = str_detalle & oDataRowTres("maximo") & ","
                    Else
                        'par90.Values.Add(0)
                        str_detalle = str_detalle & "0,"
                    End If

                End If


                str_detalle = str_detalle & ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"


                oSW.WriteLine(str_detalle)

            Next
        End If
        'cerrados fin

        'CARRILES CERRADOS DOS
        'SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD;

        '************************************************
        '************************************************
        strQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD "

        If id_plaza_cobro = 106 Then
            strQuerys = strQuerys & "where VOIE <> 'B04' and VOIE <> 'A03' "
        End If


        If objQuerys.QueryDataSetCuatro(strQuerys, "SEQ_VOIE_TOD") = 1 Then

            For cont2 = 0 To oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Count - 1

                oDataRowCuatro = oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Item(cont2)


                strQuerys = "SELECT	* FROM 	FIN_POSTE " &
                    "WHERE VOIE = '" & oDataRowCuatro("VOIE") & "' " &
                    "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
                    "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) "

                If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 0 Then

                    strQuerys = "SELECT * " &
"FROM CLOSED_LANE_REPORT, SITE_GARE " &
"where " &
"CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
"AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
"AND	LANE		=	'" & oDataRowCuatro("VOIE") & "' " &
"AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
"order by BEGIN_DHM"

                    If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 0 Then

                        'End If


                        str_detalle = ""
                        'Fecha base de operación 	Fecha 	dd/mm/aaaa
                        str_detalle = Format(dt_Fecha_Inicio, "dd/MM/yyyy") & ","
                        'Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
                        str_detalle = str_detalle & int_turno & ","
                        'Hora inicial de operación 	Caracter 	hhmmss 	
                        str_detalle = str_detalle & Format(h_inicio_turno, "HHmmss") & ","
                        'Hora final de operación 	Caracter 	hhmmss 	
                        'str_detalle = str_detalle & Format(h_fin_turno, "HHmmss") & ","
                        str_detalle = str_detalle & Format(DateAdd(DateInterval.Second, 1, h_fin_turno), "HHmmss") & ","
                        ''Número de carril 	Entero 	>>9	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                        If id_plaza_cobro = 184 Then
                            str_detalle = str_detalle & "247" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2585" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2586" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "2587" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "2588" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "2589" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "2590" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "2591" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "2592" & ","
                            End If

                            'paso morelos
                        ElseIf id_plaza_cobro = 102 Then

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "249" & ","
                                str_detalle = str_detalle & "1803" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1804" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1805" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1806" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1807" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1808" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1809" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "261" & ","
                                str_detalle = str_detalle & "1810" & ","
                                '--------------------------------------------
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1811" & ","

                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1812" & ","
                            End If

                            'la venta
                        ElseIf id_plaza_cobro = 104 Then
                            str_detalle = str_detalle & "252" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1830" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1831" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1832" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1833" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1834" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1835" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1836" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1837" & ","

                            End If

                        ElseIf id_plaza_cobro = 161 Then


                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "364" & "," & "2681" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "364" & "," & "2682" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "363" & "," & "2683" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "363" & "," & "2684" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "364" & "," & "2685" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "364" & "," & "2686" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "363" & "," & "2687" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "363" & "," & "2688" & ","

                            End If
                        ElseIf id_plaza_cobro = 103 Then
                            str_detalle = str_detalle & "251" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1816" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1817" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1818" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1819" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1820" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1821" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1822" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1823" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1824" & ","
                            End If

                            'álpuyeca
                            '101
                            '246
                            '1	1794
                            '2	1795
                            '3	1796
                            '4	1797
                        ElseIf id_plaza_cobro = 101 Then
                            str_detalle = str_detalle & "246" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1794" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1795" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1796" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1797" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1798" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1799" & ","
                            End If



                            'tlalpan
                        ElseIf id_plaza_cobro = 108 Then

                            str_detalle = str_detalle & "118" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "3076" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "3063" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "3064" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "3065" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "3066" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "3067" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "3068" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "3069" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "3070" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "3071" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "3072" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "3073" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "3074" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "3075" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "3077" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "3078" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "17" Then
                                str_detalle = str_detalle & "3079" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "18" Then
                                str_detalle = str_detalle & "3080" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "19" Then
                                str_detalle = str_detalle & "3081" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "20" Then
                                str_detalle = str_detalle & "3082" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3083" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3084" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3085" & ","
                            End If

                            'xochitepec
                        ElseIf id_plaza_cobro = 105 Then

                            str_detalle = str_detalle & "365" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2727" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2728" & ","
                            End If
                            'CERRO gordo

                        ElseIf id_plaza_cobro = 186 Then

                            str_detalle = str_detalle & "351" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3199" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3200" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3201" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "24" Then
                                str_detalle = str_detalle & "3202" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "25" Then
                                str_detalle = str_detalle & "3203" & ","
                                'ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                'str_detalle = str_detalle & "3185" & ","
                                'ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                'str_detalle = str_detalle & "3186" & ","
                                'ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                'str_detalle = str_detalle & "3187" & ","
                                'ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                'str_detalle = str_detalle & "3188" & ","
                            End If
                            'QUERETARO
                        ElseIf id_plaza_cobro = 106 Then
                            str_detalle = str_detalle & "112" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1079" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1080" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1081" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1082" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1083" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1084" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1085" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1086" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1087" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1088" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1089" & ","
                            End If

                            'VillaGrand

                        ElseIf id_plaza_cobro = 186 Then

                            str_detalle = str_detalle & "170" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2581" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2582" & ","
                            End If


                            'tres marias
                        ElseIf id_plaza_cobro = 109 Then

                            str_detalle = str_detalle & "102" & ","

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1020" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1021" & ","
                            End If
                            'Central de Abastos
                        ElseIf id_plaza_cobro = 107 Then
                            str_detalle = str_detalle & "368" & ","
                            'Segmento B

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1843" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1844" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1845" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1846" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1847" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1848" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1849" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1850" & ","
                                'Segmento A

                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1851" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1852" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1853" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "1854" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "2743" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "2744" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "2745" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "2746" & ","
                            End If


                        ElseIf id_plaza_cobro = 189 Then
                            str_detalle = str_detalle & "189" & ","
                            'Segmento B

                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1891" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1892" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1893" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1894" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1895" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1896" & ","
                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1897" & ","
                            End If

                            '    'SAN MARCOS
                            'ElseIf id_plaza_cobro = 107 Then

                            '    str_detalle = str_detalle & "121" & ","

                            '    If Mid(Trim(oDataRowCuatro("Voie")), 1, 1) = "A" Then

                            '        If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "15" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        End If

                            '    ElseIf Mid(Trim(oDataRowCuatro("Voie")), 1, 1) = "B" Then

                            '        If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
                            '            str_detalle = str_detalle & "1097" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
                            '            str_detalle = str_detalle & "1098" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
                            '            str_detalle = str_detalle & "1099" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
                            '            str_detalle = str_detalle & "1100" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        End If

                            '    End If


                        Else
                            str_detalle = str_detalle & ","
                            str_detalle = str_detalle & ","
                        End If


                        'Cuerpo 	Caracter 	X(1)	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                        str_detalle = str_detalle & Mid(Trim(oDataRowCuatro("VOIE")), 1, 1) & "," '& ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"


                        '*****************************************
                        str_detalle = str_detalle & Format((h_fin_turno), "mm") & Format((h_fin_turno), "ss") & ","

                        strQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces, MIN(EVENT_NUMBER) AS minimo, MAX(EVENT_NUMBER) as maximo " &
           "FROM GEADBA.TRANSACTION,SITE_GARE " &
           "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " &
           "AND (DATE_TRANSACTION >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
           "AND (DATE_TRANSACTION <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
           "AND (DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "', 'YYYYMMDDHH24MISS')) " &
           "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) "

                        strQuerys = strQuerys & " " &
                                                      "AND VOIE = '" & oDataRowCuatro("VOIE") & "' and EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' "


                        If objQuerys.QueryDataSetTres(strQuerys, "TRANSACTION") = 1 Then

                            If oDataRowTres("Cruces") > 0 Then
                                'par89.Values.Add(oDataRow("minimo"))
                                str_detalle = str_detalle & oDataRowTres("minimo") & ","
                            Else
                                'par89.Values.Add(0)
                                str_detalle = str_detalle & "0,"
                            End If

                            If oDataRowTres("Cruces") > 0 Then
                                'par90.Values.Add(oDataRow("maximo"))
                                str_detalle = str_detalle & oDataRowTres("maximo") & ","
                            Else
                                'par90.Values.Add(0)
                                str_detalle = str_detalle & "0,"
                            End If

                        End If


                        str_detalle = str_detalle & ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"


                        '*****************************************







                        oSW.WriteLine(str_detalle)
                        '----------------------

                    End If
                End If
            Next

        End If
        '************************************************
        '************************************************

        'FIN CARRILES CERRADO DOS

        oSW.Flush()
        oSW.Close()
        ProgressBar1.Value = ProgressBar1.Value + 20
        '    'MsgBox("Exportación Terminada", MsgBoxStyle.Information, "Exportación")
        'Catch ex As Exception
        '    MsgBox(ex.Message.ToString, MsgBoxStyle.Critical)
        'End Try

    End Sub
    'Archivo 9A
    Private Sub eventos_detectados_y_marcados_en_el_ECT()
        Dim strQuerys As String
        Dim Linea As String = ""
        Dim cabecera As String
        Dim pie As String
        Dim numero_archivo As String = ""
        Dim nombre_archivo As String
        Dim numero_registros As Double
        Dim cont As Integer
        Dim int_turno As Integer

        Dim h_inicio_turno As Date
        Dim h_fin_turno As Date

        Dim no_registros As String

        Dim str_detalle As String
        Dim str_encargado As String

        Dim dbl_registros As Double

        Dim strClaseExcedente As String
        Dim strCodigoVhMarcado As String
        Dim strCodigoVhPagoMarcado As String

        Dim tag_iag As String
        Dim tarjeta As String

        Dim strQuerysTag As String
        Dim val As New ArrayList()
        Dim contador As Integer
        Dim sLine As String

        Try



            If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
                int_turno = 5
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
                int_turno = 6
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
                int_turno = 4
            End If

            If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
                h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 06:00:00")
                h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 13:59:59")
                int_turno = 5
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
                h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
                h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 21:59:59")
                int_turno = 6
            ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
                'h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
                h_inicio_turno = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 22:00:00")
                'h_fin_turno = CDate(Format(DateAdd(DateInterval.Day, 1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 05:59:59")
                h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 05:59:59")
                int_turno = 4
            End If



            If Len(id_plaza_cobro) = 3 Then
                If id_plaza_cobro = 108 Then
                    nombre_archivo = "0001"
                ElseIf id_plaza_cobro = 109 Then
                    nombre_archivo = "001B"
                ElseIf id_plaza_cobro = 107 Then
                    nombre_archivo = "0107"
                ElseIf id_plaza_cobro = 106 Then
                    nombre_archivo = "0006"
                Else
                    nombre_archivo = "0" & id_plaza_cobro

                End If
            End If



            nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "9" & strIdentificador

            Dim oSW As New StreamWriter(dir_archivo & nombre_archivo)
            archivo_3 = nombre_archivo

            'cabecera = "David Cabecera"
            'cabecera = "04"
            cabecera = cmbDelegacion.Tag

            If Len(id_plaza_cobro) = 3 Then
                If id_plaza_cobro = 108 Then
                    cabecera = cabecera & "0001"
                ElseIf id_plaza_cobro = 109 Then
                    cabecera = cabecera & "001B"
                ElseIf id_plaza_cobro = 107 Then
                    cabecera = cabecera & "0107"
                ElseIf id_plaza_cobro = 106 Then
                    cabecera = cabecera & "0006"
                Else
                    cabecera = cabecera & "0" & id_plaza_cobro

                End If
            End If


            cabecera = "03" & cabecera & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "9" & strIdentificador & Format(dt_Fecha_Inicio, "dd/MM/yyyy") & int_turno

            'CABECERA INICIO REGISTROS

            'CABECERA FIN

2:
            'inicio detalle

            '            "AND	SITE_GARE.id_reseau		= 	'01' " & _
            '"AND	SITE_GARE.id_Site		=	'" & id_plaza_cobro - 100 & "' " & _

            'DATE_DEBUT_POSTE
            strQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " &
"TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " &
"ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID " &
"FROM TRANSACTION " &
"JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " &
"LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " &
"WHERE " &
"(DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
" AND  ID_PAIEMENT  <> 0 " &
"AND (TRANSACTION.Id_Voie = '1' " &
"OR TRANSACTION.Id_Voie = '2' " &
"OR TRANSACTION.Id_Voie = '3' " &
"OR TRANSACTION.Id_Voie = '4' " &
"OR TRANSACTION.Id_Voie = 'X') " &
"ORDER BY DATE_TRANSACTION"

            If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                'dbl_registros = oDataSet.Tables("TRANSACTION").Rows.Count

                'If Len(CStr(dbl_registros)) = 1 Then
                '    no_registros = "0000" & dbl_registros
                'ElseIf Len(CStr(dbl_registros)) = 2 Then
                '    no_registros = "000" & dbl_registros
                'ElseIf Len(CStr(dbl_registros)) = 3 Then
                '    no_registros = "00" & dbl_registros
                'ElseIf Len(CStr(dbl_registros)) = 4 Then
                '    no_registros = "0" & dbl_registros
                'ElseIf Len(CStr(dbl_registros)) = 5 Then
                '    no_registros = dbl_registros
                'End If

                'cabecera = cabecera & no_registros

                'oSW.WriteLine(cabecera)

                dbl_registros = 0

                For cont = 0 To oDataSet.Tables("TRANSACTION").Rows.Count - 1

                    oDataRow = oDataSet.Tables("TRANSACTION").Rows.Item(cont)

                    str_detalle = ""

                    If Not IsDBNull(oDataRow("CLASE_DETECTADA")) Then
                        'Else
                        'End If


                        'Fecha del evento 	Fecha 	dd/mm/aaaa 
                        str_detalle = Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                        'Número de turno	Entero 	9
                        str_detalle = str_detalle & int_turno & ","
                        'Hora de evento 	Caracter 	hhmmss 
                        str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "HHmmss") & ","
                        'Clave de tramo	Entero 	>9
                        'Verificar 
                        'str_detalle = str_detalle & "247" & ","
                        'Número de carril	Entero 	>>9
                        If id_plaza_cobro = 184 Then

                            If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                str_detalle = str_detalle & "340" & ","
                            Else
                                str_detalle = str_detalle & "247" & ","
                            End If

                            '340

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2585" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2586" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "2587" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "2588" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "2589" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "2590" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "2591" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "2592" & ","
                            End If

                            'paso morelos
                        ElseIf id_plaza_cobro = 102 Then

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then

                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "261" & ","
                                Else
                                    str_detalle = str_detalle & "249" & ","
                                End If
                                str_detalle = str_detalle & "1803" & ","

                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1804" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1805" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1806" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1807" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1808" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1809" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "261" & ","
                                Else
                                    str_detalle = str_detalle & "249" & ","
                                End If
                                str_detalle = str_detalle & "1810" & ","
                                '--------------------------------------------
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1811" & ","

                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1812" & ","
                            End If

                            'la venta
                        ElseIf id_plaza_cobro = 104 Then

                            str_detalle = str_detalle & "252" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1830" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1831" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1832" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1833" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1834" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1835" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1836" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1837" & ","

                            End If

                        ElseIf id_plaza_cobro = 161 Then

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "364" & "," & "2681" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "364" & "," & "2682" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "363" & "," & "2683" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "363" & "," & "2684" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "364" & "," & "2685" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "364" & "," & "2686" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "363" & "," & "2687" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "363" & "," & "2688" & ","

                            End If
                        ElseIf id_plaza_cobro = 103 Then

                            str_detalle = str_detalle & "251" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1816" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1817" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1818" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1819" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1820" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1821" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1822" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1823" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1824" & ","
                            End If

                            'álpuyeca

                        ElseIf id_plaza_cobro = 101 Then

                            str_detalle = str_detalle & "246" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1794" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1795" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1796" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1797" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1798" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1799" & ","
                            End If



                            'tlalpan
                        ElseIf id_plaza_cobro = 108 Then

                            str_detalle = str_detalle & "118" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "3076" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "3063" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "3064" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "3065" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "3066" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "3067" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "3068" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "3069" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "3070" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "3071" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "3072" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "3073" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "3074" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "3075" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "3077" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "3078" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                                str_detalle = str_detalle & "3079" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                                str_detalle = str_detalle & "3080" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                                str_detalle = str_detalle & "3081" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                                str_detalle = str_detalle & "3082" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3083" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3084" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3085" & ","
                            End If

                            'xochitepec
                        ElseIf id_plaza_cobro = 105 Then

                            str_detalle = str_detalle & "365" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2727" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2728" & ","
                            End If

                            'CERRO GORDO

                        ElseIf id_plaza_cobro = 186 Then

                            str_detalle = str_detalle & "351" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3199" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3200" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3201" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                                str_detalle = str_detalle & "3202" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                                str_detalle = str_detalle & "3203" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                'str_detalle = str_detalle & "3185" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                'str_detalle = str_detalle & "3186" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                'str_detalle = str_detalle & "3187" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                'str_detalle = str_detalle & "3188" & ","
                            End If
                            'QUERETARO
                        ElseIf id_plaza_cobro = 106 Then
                            str_detalle = str_detalle & "112" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1079" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1080" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1081" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1082" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1083" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1084" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1085" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1086" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1087" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1088" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1089" & ","
                            End If


                            'VillaGrand
                        ElseIf id_plaza_cobro = 183 Then

                            str_detalle = str_detalle & "170" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2581" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2582" & ","
                            End If
                            'tres marias
                        ElseIf id_plaza_cobro = 109 Then

                            str_detalle = str_detalle & "102" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1020" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1021" & ","
                            End If

                            'Central de Abastos
                        ElseIf id_plaza_cobro = 107 Then
                            str_detalle = str_detalle & "368" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1843" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1844" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1845" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1846" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1847" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1848" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1849" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1850" & ","
                                'Segmento A
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1851" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1852" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1853" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "1854" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "2743" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "2744" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "2745" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "2746" & ","
                            End If

                        ElseIf id_plaza_cobro = 189 Then
                            str_detalle = str_detalle & "189" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1891" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1892" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1893" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1894" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1895" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1896" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1897" & ","
                            End If
                            '    'SAN MARCOS
                            'ElseIf id_plaza_cobro = 107 Then

                            '    str_detalle = str_detalle & "121" & ","

                            '    If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

                            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        End If

                            '    ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

                            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            '            str_detalle = str_detalle & "1097" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            '            str_detalle = str_detalle & "1098" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            '            str_detalle = str_detalle & "1099" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            '            str_detalle = str_detalle & "1100" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        End If

                            '    End If


                        Else
                            str_detalle = str_detalle & ","
                            str_detalle = str_detalle & ","
                        End If


                        'Cuerpo	Caracter 	X(1)
                        str_detalle = str_detalle & Mid(Trim(oDataRow("Voie")), 1, 1) & ","
                        'Número de evento 	Entero 	>>>>>>9



                        str_detalle = str_detalle & oDataRow("EVENT_NUMBER") & ","
                        'Número de folio 	Entero 	>>>>>>9 
                        str_detalle = str_detalle & oDataRow("FOLIO_ECT") & ","
                        'Código de vehículo detectado ECT 	Caracter 	X(6)

                        If Not IsDBNull(oDataRow("CLASE_DETECTADA")) Then

                            strClaseExcedente = ""
                            If Trim(oDataRow("CLASE_DETECTADA")) = "T01A" Then
                                str_detalle = str_detalle & "T01" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01M" Then
                                str_detalle = str_detalle & "T01" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01T" Then
                                'str_detalle = str_detalle & "T01" & ","
                                'T01,T => T09P01,C
                                str_detalle = str_detalle & "T09P01" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T02B" Then
                                str_detalle = str_detalle & "T02" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T03B" Then
                                str_detalle = str_detalle & "T03" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T04B" Then
                                str_detalle = str_detalle & "T04" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T02C" Then
                                str_detalle = str_detalle & "T02" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T03C" Then
                                str_detalle = str_detalle & "T03" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T04C" Then
                                str_detalle = str_detalle & "T04" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T05C" Then
                                str_detalle = str_detalle & "T05" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T06C" Then
                                str_detalle = str_detalle & "T06" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T07C" Then
                                str_detalle = str_detalle & "T07" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T08C" Then
                                str_detalle = str_detalle & "T08" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T09C" Then
                                str_detalle = str_detalle & "T09" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TL01A" Then
                                str_detalle = str_detalle & "T01L01" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TL02A" Then
                                str_detalle = str_detalle & "T01L02" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TLnnA" Then
                                '04/12/2013
                                'str_detalle = str_detalle & "T01Lnn" & ","
                                'strClaseExcedente = "T01L"
                                str_detalle = str_detalle & "T01L" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT")) & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01P" Then
                                str_detalle = str_detalle & "T01P" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TP01C" Then
                                str_detalle = str_detalle & "T09P01" & ","
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TPnnC" Then
                                '04/12/2013
                                'str_detalle = str_detalle & "T09Pnn" & ","
                                'strClaseExcedente = "T09P" & ","
                                'str_detalle = str_detalle & "T09P" & ","
                                str_detalle = str_detalle & "T09P" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT")) & ","
                            Else
                                str_detalle = str_detalle & "No detectada" & ",0,"
                            End If

                        Else
                            str_detalle = str_detalle & "0,"
                        End If

                        'Importe vehículo detectado ECT 	Decimal 	>>9.99 


                        strQuerys = "SELECT " &
                 "TYPE_PAIEMENT.libelle_paiement_L2 " &
                 ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
                 ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
                 ",Prix_Cl19, Prix_Cl20 " &
                 ",TYPE_PAIEMENT.libelle_paiement " &
                 ",TABLE_TARIF.CODE " &
                 "FROM TABLE_TARIF, " &
                 "TYPE_PAIEMENT " &
                 "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                        'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "


                        'borrar
                        strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRow("Version_Tarif") & " " &
                            "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                       "ORDER BY TABLE_TARIF.CODE "

                        '        strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5 " & _
                        '     "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                        '"ORDER BY TABLE_TARIF.CODE "

                        If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                            If oDataRow("ACD_CLASS") > 0 And oDataRow("ACD_CLASS") <= 9 Then
                                str_detalle = str_detalle & oDataRow("MONTO_DETECTADO") & ",,"
                            ElseIf oDataRow("ACD_CLASS") >= 12 And oDataRow("ACD_CLASS") <= 15 Then
                                str_detalle = str_detalle & oDataRow("MONTO_DETECTADO") & ",,"
                                'EXCEDENTES
                            ElseIf oDataRow("ACD_CLASS") >= 10 And oDataRow("ACD_CLASS") <= 11 Then
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                            ElseIf oDataRow("ACD_CLASS") = 16 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                            ElseIf oDataRow("ACD_CLASS") = 17 Then
                                'strClaseExcedente = "T01Lnn"
                                'str_detalle = str_detalle & strClaseExcedente & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                            ElseIf oDataRow("ACD_CLASS") = 18 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                'la tomamos como la 16
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                            Else
                                str_detalle = str_detalle & ",,"
                            End If

                        Else
                            str_detalle = str_detalle & ",,"
                        End If


                        'Importe eje excedente detectado ECT 	Decimal 	>9.99 
                        'Código de vehículo marcado C-R	Caracter 	X(6)
                        If Not IsDBNull(oDataRow("CLASE_MARCADA")) Then

                            strClaseExcedente = ""
                            strCodigoVhMarcado = ""
                            If Trim(oDataRow("CLASE_MARCADA")) = "T01A" Then
                                str_detalle = str_detalle & "T01" & ",A,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T01M" Then
                                str_detalle = str_detalle & "T01" & ",M,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T01T" Then
                                'str_detalle = str_detalle & "T01" & ",T,"
                                'T01,T => T09P01,C
                                str_detalle = str_detalle & "T09P01" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T02B" Then
                                str_detalle = str_detalle & "T02" & ",B,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T03B" Then
                                str_detalle = str_detalle & "T03" & ",B,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T04B" Then
                                str_detalle = str_detalle & "T04" & ",B,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T02C" Then
                                str_detalle = str_detalle & "T02" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T03C" Then
                                str_detalle = str_detalle & "T03" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T04C" Then
                                str_detalle = str_detalle & "T04" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T05C" Then
                                str_detalle = str_detalle & "T05" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T06C" Then
                                str_detalle = str_detalle & "T06" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T07C" Then
                                str_detalle = str_detalle & "T07" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T08C" Then
                                str_detalle = str_detalle & "T08" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T09C" Then
                                str_detalle = str_detalle & "T09" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TL01A" Then
                                str_detalle = str_detalle & "T01L01" & ",A,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TL02A" Then
                                str_detalle = str_detalle & "T01L02" & ",A,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TLnnA" Then
                                'str_detalle = str_detalle & "T01Lnn" & ",A,"
                                'strClaseExcedente = "T01L"
                                'strCodigoVhMarcado = "A,"
                                str_detalle = str_detalle & "T01L" & IIf(Len(oDataRow("CODE_GRILLE_TARIF")) = 1, "0" & oDataRow("CODE_GRILLE_TARIF"), oDataRow("CODE_GRILLE_TARIF")) & ",A,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T01P" Then
                                str_detalle = str_detalle & "T01P" & ",A,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TP01C" Then
                                str_detalle = str_detalle & "T09P01" & ",C,"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TPnnC" Then
                                'str_detalle = str_detalle & "T09Pnn" & ",C,"
                                'strClaseExcedente = "T09P"
                                'strCodigoVhMarcado = "C,"

                                'strClaseExcedente = "T09P,"
                                'strCodigoVhMarcado = strClaseExcedente & "C,"
                                str_detalle = str_detalle & "T09P" & IIf(Len(oDataRow("CODE_GRILLE_TARIF")) = 1, "0" & oDataRow("CODE_GRILLE_TARIF"), oDataRow("CODE_GRILLE_TARIF")) & ",C,"
                            Else
                                str_detalle = str_detalle & "No detectada" & ",0,"
                            End If

                        Else
                            str_detalle = str_detalle & ",0,"
                        End If

                        'Tipo de vehículo marcado C-R	Caracter 	X(1)
                        'Código de usuario pago marcado C-R	Caracter 	X(3)

                        If Trim(oDataRow("ID_PAIEMENT")) = 1 Then
                            'str_detalle = str_detalle & "NOR" & ","
                            strCodigoVhPagoMarcado = "NOR" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                            'str_detalle = str_detalle & "CRE" & ","
                            'strCodigoVhPagoMarcado = "CRE" & ","
                            'TRAMO CORTO
                            strCodigoVhPagoMarcado = "NOR" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 27 Then
                            'str_detalle = str_detalle & "VSC" & ","
                            strCodigoVhPagoMarcado = "VSC" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 9 Then
                            'str_detalle = str_detalle & "FCUR" & ","
                            strCodigoVhPagoMarcado = "FCUR" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 10 Then
                            'str_detalle = str_detalle & "RPI" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Then
                            'str_detalle = str_detalle & "Tag" & ","
                            strCodigoVhPagoMarcado = "TDC" & ","

                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                            'str_detalle = str_detalle & "Tag" & ","
                            strCodigoVhPagoMarcado = "TDD" & ","

                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                            'str_detalle = str_detalle & "IAV" & ","
                            strCodigoVhPagoMarcado = "IAV" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 13 Then
                            'str_detalle = str_detalle & "ELU" & ","
                            strCodigoVhPagoMarcado = "ELU" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 71 Then
                            'str_detalle = str_detalle & "RP1" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 72 Then
                            'str_detalle = str_detalle & "RP2" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 73 Then
                            'str_detalle = str_detalle & "RP3" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 74 Then
                            'str_detalle = str_detalle & "RP4" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 75 Then
                            'str_detalle = str_detalle & "RP4" & ","
                            strCodigoVhPagoMarcado = "RSP" & ","
                        Else
                            'str_detalle = str_detalle & ","
                            strCodigoVhPagoMarcado = ","
                        End If



                        'Importe vehículo marcado C-R[1]	Decimal 	>>9.99
                        strQuerys = "SELECT " &
       "TYPE_PAIEMENT.libelle_paiement_L2 " &
       ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
       ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
       ",Prix_Cl19, Prix_Cl20 " &
       ",TYPE_PAIEMENT.libelle_paiement " &
       ",TABLE_TARIF.CODE " &
       "FROM TABLE_TARIF, " &
       "TYPE_PAIEMENT " &
       "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                        'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "


                        'borrar
                        strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRow("Version_Tarif") & " " &
                            "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                       "ORDER BY TABLE_TARIF.CODE "

                        ' strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5 " & _
                        '     "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                        '"ORDER BY TABLE_TARIF.CODE "

                        If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                            If oDataRow("TAB_ID_CLASSE") > 0 And oDataRow("TAB_ID_CLASSE") <= 9 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRow("MONTO_MARCADO") & ",,"
                            ElseIf oDataRow("TAB_ID_CLASSE") >= 12 And oDataRow("TAB_ID_CLASSE") <= 15 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRow("MONTO_MARCADO") & ",,"
                                'EXCEDENTES
                            ElseIf oDataRow("TAB_ID_CLASSE") >= 10 And oDataRow("TAB_ID_CLASSE") <= 11 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                            ElseIf oDataRow("TAB_ID_CLASSE") = 16 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                            ElseIf oDataRow("TAB_ID_CLASSE") = 17 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                            ElseIf oDataRow("TAB_ID_CLASSE") = 18 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                'la tomamos como la 16
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                            Else
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & ",,"
                            End If

                        Else
                            str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                            str_detalle = str_detalle & ",,"
                        End If


                        'Importe eje excedente marcado C-R 	Decimal 	>9.99 
                        'Número de tarjeta Pagos Electrónicos[2]	Caracter 	X(20)
                        'str_detalle = str_detalle & Trim(oDataRow("CONTENU_ISO")) & ","
                        'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","


                        'Situación de la tarjeta Pagos Electrónicos	Caracter 	X(1)
                        'If Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                        '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","
                        '    str_detalle = str_detalle & "V" & ","

                        'Else
                        '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                        '    str_detalle = str_detalle & ","
                        'End If

                        tag_iag = ""
                        tarjeta = ""

                        If Trim(oDataRow("ID_PAIEMENT")) = 15 Then

                            'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","

                            tag_iag = IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO")))

                            tag_iag = Trim(Mid(tag_iag, 1, 16))

                            If Len(Trim(tag_iag)) = 13 And Mid(Trim(tag_iag), 1, 3) = "009" Then
                                tag_iag = Mid(Trim(tag_iag), 1, 3) & Mid(Trim(tag_iag), 6, 8)
                            End If

                            str_detalle = str_detalle & tag_iag & ","
                            'If IsNumeric(tag_iag) Then

                            '    tarjeta = Trim(tag_iag)

                            '    If Len(tarjeta) <> 11 Then

                            '        'si es menor a 4000 verifico si existe 003
                            '        If CDbl(tarjeta) <= 4000 Then

                            '            strQuerysTag = "SELECT roll FROM roll WHERE roll = " & CDbl(tarjeta)

                            '            If objQuerys_SqlServer.QueryDataSet_SqlServerDos(strQuerysTag, "roll") = 1 Then

                            '                'si esta en la lista le pongo el 099
                            '                tarjeta = "099" & tarjeta.PadLeft(8, "0")

                            '            Else
                            '                'si no esta en la lista le pongo el 003
                            '                tarjeta = "003" & tarjeta.PadLeft(8, "0")

                            '            End If

                            '        ElseIf IsNumeric(tarjeta) >= 16000000 Then
                            '            tarjeta = "003" & tarjeta.PadLeft(8, "0")

                            '        Else
                            '            tarjeta = "099" & tarjeta.PadLeft(8, "0")
                            '            'no es menor a 4000 meto 099
                            '        End If

                            '    End If

                            '    str_detalle = str_detalle & tarjeta & ","
                            'Else
                            '    str_detalle = str_detalle & tag_iag & ","
                            'End If


                            str_detalle = str_detalle & "V" & ","
                            str_detalle = str_detalle & ","
                            str_detalle = str_detalle & ","

                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then


                            'str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID").PadLeft(16, "*")) & ","



                            str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID")) & ","
                            str_detalle = str_detalle & "V" & ","
                            'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) & ","

                            If IsNumeric(Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) Then


                                If InStr(Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6), "E") = 0 Then
                                    str_detalle = str_detalle & Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6) & ","

                                Else
                                    str_detalle = str_detalle & "0,"
                                End If

                            Else
                                str_detalle = str_detalle & "0,"
                            End If


                            str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","

                        Else
                            str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                            str_detalle = str_detalle & ","

                            str_detalle = str_detalle & ","
                            str_detalle = str_detalle & ","
                        End If


                        'If Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                        '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("ISSUER_ID").PadLeft(16, "*"))) & ","
                        '    str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                        'Else
                        '    str_detalle = str_detalle & ","
                        '    str_detalle = str_detalle & ","
                        'End If

                        'FIN UNO
                        'str_detalle = Replace(str_detalle, "T01,T", "T09P01,C")
                    Else

                        'inicio clase detectada 

                        strQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " &
"TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " &
"ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID " &
"FROM TRANSACTION " &
"JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " &
"LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " &
"WHERE " &
"(DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND VOIE = '" & oDataRow("VOIE") & "' " &
"AND  ID_OBS_SEQUENCE = '7' " &
"AND EVENT_NUMBER = " & oDataRow("EVENT_NUMBER") & " " &
"AND (TRANSACTION.Id_Voie = '1' " &
"OR TRANSACTION.Id_Voie = '2' " &
"OR TRANSACTION.Id_Voie = '3' " &
"OR TRANSACTION.Id_Voie = '4' " &
"OR TRANSACTION.Id_Voie = 'X') " &
"ORDER BY DATE_TRANSACTION"


                        strQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " &
"TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " &
"ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID " &
"FROM TRANSACTION " &
"JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " &
"LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " &
"WHERE " &
"(DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND VOIE = '" & oDataRow("VOIE") & "' " &
"AND  ID_OBS_SEQUENCE <> '7777' " &
"AND EVENT_NUMBER = " & oDataRow("EVENT_NUMBER") & " " &
"AND (TRANSACTION.Id_Voie = '1' " &
"OR TRANSACTION.Id_Voie = '2' " &
"OR TRANSACTION.Id_Voie = '3' " &
"OR TRANSACTION.Id_Voie = '4' " &
"OR TRANSACTION.Id_Voie = 'X') " &
"ORDER BY DATE_TRANSACTION desc"


                        If objQuerys.QueryDataSetTres(strQuerys, "TRANSACTION") = 1 Then

                            str_detalle = Format(oDataRowTres("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                            'Número de turno	Entero 	9
                            str_detalle = str_detalle & int_turno & ","
                            'Hora de evento 	Caracter 	hhmmss 
                            str_detalle = str_detalle & Format(oDataRowTres("DATE_TRANSACTION"), "HHmmss") & ","
                            'Clave de tramo	Entero 	>9
                            'Verificar 
                            'str_detalle = str_detalle & "247" & ","
                            'Número de carril	Entero 	>>9
                            If id_plaza_cobro = 184 Then

                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "340" & ","
                                Else
                                    str_detalle = str_detalle & "247" & ","
                                End If


                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2585" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2586" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "2587" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "2588" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "2589" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "2590" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "2591" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "2592" & ","
                                End If

                                'paso morelos
                            ElseIf id_plaza_cobro = 102 Then

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                        str_detalle = str_detalle & "261" & ","
                                    Else
                                        str_detalle = str_detalle & "249" & ","
                                    End If
                                    str_detalle = str_detalle & "1803" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1804" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1805" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1806" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1807" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1808" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1809" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                        str_detalle = str_detalle & "261" & ","
                                    Else
                                        str_detalle = str_detalle & "249" & ","
                                    End If
                                    str_detalle = str_detalle & "1810" & ","
                                    '--------------------------------------------
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1811" & ","

                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1812" & ","
                                End If

                                'la venta
                            ElseIf id_plaza_cobro = 104 Then

                                str_detalle = str_detalle & "252" & ","

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1830" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1831" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1832" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1833" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1834" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1835" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1836" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1837" & ","

                                End If

                            ElseIf id_plaza_cobro = 161 Then

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "364" & "," & "2681" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "364" & "," & "2682" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "363" & "," & "2683" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "363" & "," & "2684" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "364" & "," & "2685" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "364" & "," & "2686" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "363" & "," & "2687" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "363" & "," & "2688" & ","

                                End If


                            ElseIf id_plaza_cobro = 103 Then

                                str_detalle = str_detalle & "251" & ","

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1816" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1817" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1818" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1819" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1820" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1821" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1822" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1823" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1824" & ","
                                End If

                                'álpuyeca
                                '101
                                '246
                                '1	1794
                                '2	1795
                                '3	1796
                                '4	1797

                            ElseIf id_plaza_cobro = 101 Then

                                str_detalle = str_detalle & "246" & ","

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1794" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1795" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1796" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1797" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1798" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1799" & ","

                                End If
                                '++++++++++++++++++++++++++++++++++++++++++++


                                'aeropuerto
                                '106
                                'A 366
                                'B 367
                                '1		367	2734	B
                                '2		366	2735	A
                                '3		367	2736	B
                                '4		366	2737	A
                                'ElseIf id_plaza_cobro_local = 1 Then
                                '    If Mid(Trim(oDataRow("LANE")), 1, 1) = "A" Then

                                'str_detalle = str_detalle & "366" & ","

                                '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                                '            str_detalle = str_detalle & "2735" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                                '            str_detalle = str_detalle & "2737" & ","
                                '        End If

                                '    ElseIf Mid(Trim(oDataRow("LANE")), 1, 1) = "B" Then

                                'str_detalle = str_detalle & "367" & ","

                                '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                                '            str_detalle = str_detalle & "2734" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                                '            str_detalle = str_detalle & "2736" & ","
                                '        End If

                                '    End If
                                '+++++++++++++++++++++++++++
                                'tlalpan
                            ElseIf id_plaza_cobro = 108 Then

                                str_detalle = str_detalle & "118" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                    str_detalle = str_detalle & "3076" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "3063" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "3064" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "3065" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "3066" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "3067" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "3068" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "3069" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "3070" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "3071" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "3072" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "3073" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                    str_detalle = str_detalle & "3074" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                    str_detalle = str_detalle & "3075" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                    str_detalle = str_detalle & "3077" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                    str_detalle = str_detalle & "3078" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                                    str_detalle = str_detalle & "3079" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                                    str_detalle = str_detalle & "3080" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                                    str_detalle = str_detalle & "3081" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                                    str_detalle = str_detalle & "3082" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                    str_detalle = str_detalle & "3083" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                    str_detalle = str_detalle & "3084" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                    str_detalle = str_detalle & "3085" & ","
                                End If

                                'xochitepec
                            ElseIf id_plaza_cobro = 105 Then

                                str_detalle = str_detalle & "365" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2727" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2728" & ","
                                End If


                                'CERRO GORDO
                            ElseIf id_plaza_cobro = 186 Then

                                str_detalle = str_detalle & "351" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                    str_detalle = str_detalle & "3199" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                    str_detalle = str_detalle & "3200" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                    str_detalle = str_detalle & "3201" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                                    str_detalle = str_detalle & "3202" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                                    str_detalle = str_detalle & "3203" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    'str_detalle = str_detalle & "3185" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    'str_detalle = str_detalle & "3186" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    'str_detalle = str_detalle & "3187" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    'str_detalle = str_detalle & "3188" & ","
                                End If

                                'QUERETARO
                            ElseIf id_plaza_cobro = 106 Then
                                str_detalle = str_detalle & "112" & ","
                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1079" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1080" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1081" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1082" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1083" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1084" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1085" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1086" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1087" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "1088" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "1089" & ","
                                End If



                                'VillaGrand
                            ElseIf id_plaza_cobro = 183 Then

                                str_detalle = str_detalle & "170" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2581" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2582" & ","
                                End If

                                'tres marias
                            ElseIf id_plaza_cobro = 109 Then

                                str_detalle = str_detalle & "102" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1020" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1021" & ","
                                End If

                                'Central de Abastos
                            ElseIf id_plaza_cobro = 107 Then
                                str_detalle = str_detalle & "368" & ","
                                'Segmento B
                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1843" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1844" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1845" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1846" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1847" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1848" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1849" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1850" & ","
                                    'Segmento A
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1851" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "1852" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "1853" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                    str_detalle = str_detalle & "1854" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                    str_detalle = str_detalle & "2743" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                    str_detalle = str_detalle & "2744" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                    str_detalle = str_detalle & "2745" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                    str_detalle = str_detalle & "2746" & ","
                                End If
                            ElseIf id_plaza_cobro = 189 Then
                                str_detalle = str_detalle & "189" & ","
                                'Segmento B
                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1891" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1892" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1893" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1894" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1895" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1896" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1897" & ","

                                End If


                                '    'SAN MARCOS
                                'ElseIf id_plaza_cobro = 107 Then

                                '    str_detalle = str_detalle & "121" & ","

                                '    If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

                                '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                '            str_detalle = str_detalle & "1102" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                '            str_detalle = str_detalle & "1103" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                '            str_detalle = str_detalle & "1104" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                '            str_detalle = str_detalle & "1105" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                '            str_detalle = str_detalle & "1106" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                '            str_detalle = str_detalle & "1107" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                '            str_detalle = str_detalle & "1108" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                '            str_detalle = str_detalle & "1109" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                '            str_detalle = str_detalle & "1110" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                '            str_detalle = str_detalle & "1101" & ","
                                '        End If

                                '    ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

                                '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                '            str_detalle = str_detalle & "1097" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                '            str_detalle = str_detalle & "1098" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                '            str_detalle = str_detalle & "1099" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                '            str_detalle = str_detalle & "1100" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                '            str_detalle = str_detalle & "1101" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                '            str_detalle = str_detalle & "1102" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                '            str_detalle = str_detalle & "1103" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                '            str_detalle = str_detalle & "1104" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                '            str_detalle = str_detalle & "1105" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                '            str_detalle = str_detalle & "1106" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                '            str_detalle = str_detalle & "1107" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                '            str_detalle = str_detalle & "1108" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                '            str_detalle = str_detalle & "1109" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                '            str_detalle = str_detalle & "1110" & ","
                                '        End If

                                '    End If


                            Else
                                str_detalle = str_detalle & ","
                                str_detalle = str_detalle & ","
                            End If


                            'Cuerpo	Caracter 	X(1)
                            str_detalle = str_detalle & Mid(Trim(oDataRowTres("Voie")), 1, 1) & ","
                            'Número de evento 	Entero 	>>>>>>9


                            str_detalle = str_detalle & oDataRowTres("EVENT_NUMBER") & ","
                            'Número de folio 	Entero 	>>>>>>9 
                            '27_04
                            'str_detalle = str_detalle & oDataRow("FOLIO_ECT") & ","
                            'str_detalle = str_detalle & oDataRowTres("FOLIO_ECT") & ","
                            If oDataRowTres("FOLIO_ECT") = 0 Then
                                str_detalle = str_detalle & oDataRow("FOLIO_ECT") & ","
                            Else
                                str_detalle = str_detalle & oDataRowTres("FOLIO_ECT") & ","
                            End If


                            'Código de vehículo detectado ECT 	Caracter 	X(6)

                            If Not IsDBNull(oDataRowTres("CLASE_DETECTADA")) Then

                                strClaseExcedente = ""
                                If Trim(oDataRowTres("CLASE_DETECTADA")) = "T01A" Then
                                    str_detalle = str_detalle & "T01" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01M" Then
                                    str_detalle = str_detalle & "T01" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01T" Then
                                    'str_detalle = str_detalle & "T01" & ","
                                    'T01,T => T09P01,C
                                    str_detalle = str_detalle & "T09P01" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T02B" Then
                                    str_detalle = str_detalle & "T02" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T03B" Then
                                    str_detalle = str_detalle & "T03" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T04B" Then
                                    str_detalle = str_detalle & "T04" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T02C" Then
                                    str_detalle = str_detalle & "T02" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T03C" Then
                                    str_detalle = str_detalle & "T03" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T04C" Then
                                    str_detalle = str_detalle & "T04" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T05C" Then
                                    str_detalle = str_detalle & "T05" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T06C" Then
                                    str_detalle = str_detalle & "T06" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T07C" Then
                                    str_detalle = str_detalle & "T07" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T08C" Then
                                    str_detalle = str_detalle & "T08" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T09C" Then
                                    str_detalle = str_detalle & "T09" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TL01A" Then
                                    str_detalle = str_detalle & "T01L01" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TL02A" Then
                                    str_detalle = str_detalle & "T01L02" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TLnnA" Then
                                    '04/12/2013
                                    'str_detalle = str_detalle & "T01Lnn" & ","
                                    'strClaseExcedente = "T01L"
                                    str_detalle = str_detalle & "T01L" & IIf(Len(oDataRowTres("ID_OBS_TT")) = 1, "0" & oDataRowTres("ID_OBS_TT"), oDataRowTres("ID_OBS_TT")) & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01P" Then
                                    str_detalle = str_detalle & "T01P" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TP01C" Then
                                    str_detalle = str_detalle & "T09P01" & ","
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TPnnC" Then
                                    '04/12/2013
                                    'str_detalle = str_detalle & "T09Pnn" & ","
                                    'strClaseExcedente = "T09P" & ","
                                    ' str_detalle = str_detalle & "T09P" & ","
                                    str_detalle = str_detalle & "T09P" & IIf(Len(oDataRowTres("ID_OBS_TT")) = 1, "0" & oDataRowTres("ID_OBS_TT"), oDataRowTres("ID_OBS_TT")) & ","
                                Else
                                    str_detalle = str_detalle & "No detectada" & ",0,"
                                End If

                            Else
                                str_detalle = str_detalle & "0,"
                            End If

                            'Importe vehículo detectado ECT 	Decimal 	>>9.99 


                            strQuerys = "SELECT " &
                     "TYPE_PAIEMENT.libelle_paiement_L2 " &
                     ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
                     ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
                     ",Prix_Cl19, Prix_Cl20 " &
                     ",TYPE_PAIEMENT.libelle_paiement " &
                     ",TABLE_TARIF.CODE " &
                     "FROM TABLE_TARIF, " &
                     "TYPE_PAIEMENT " &
                     "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                            'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "

                            'borrar
                            strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRowTres("Version_Tarif") & " " &
                                "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                           "ORDER BY TABLE_TARIF.CODE "

                            ' strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5 " & _
                            '     "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                            '"ORDER BY TABLE_TARIF.CODE "



                            If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                                If oDataRowTres("ACD_CLASS") > 0 And oDataRowTres("ACD_CLASS") <= 9 Then
                                    str_detalle = str_detalle & oDataRowTres("MONTO_DETECTADO") & ",,"
                                ElseIf oDataRowTres("ACD_CLASS") >= 12 And oDataRowTres("ACD_CLASS") <= 15 Then
                                    str_detalle = str_detalle & oDataRowTres("MONTO_DETECTADO") & ",,"
                                    'EXCEDENTES
                                ElseIf oDataRowTres("ACD_CLASS") >= 10 And oDataRowTres("ACD_CLASS") <= 11 Then
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                ElseIf oDataRowTres("ACD_CLASS") = 16 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                ElseIf oDataRowTres("ACD_CLASS") = 17 Then
                                    'strClaseExcedente = "T01Lnn"
                                    'str_detalle = str_detalle & strClaseExcedente & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                ElseIf oDataRowTres("ACD_CLASS") = 18 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                    'la tomamos como la 16
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                Else
                                    str_detalle = str_detalle & ",,"
                                End If

                            Else
                                str_detalle = str_detalle & ",,"
                            End If


                            'Importe eje excedente detectado ECT 	Decimal 	>9.99 
                            'Código de vehículo marcado C-R	Caracter 	X(6)
                            If Not IsDBNull(oDataRowTres("CLASE_MARCADA")) Then

                                strClaseExcedente = ""
                                strCodigoVhMarcado = ""
                                If Trim(oDataRowTres("CLASE_MARCADA")) = "T01A" Then
                                    str_detalle = str_detalle & "T01" & ",A,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T01M" Then
                                    str_detalle = str_detalle & "T01" & ",M,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T01T" Then
                                    'str_detalle = str_detalle & "T01" & ",T,"
                                    'T01,T => T09P01,C
                                    str_detalle = str_detalle & "T09P01" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T02B" Then
                                    str_detalle = str_detalle & "T02" & ",B,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T03B" Then
                                    str_detalle = str_detalle & "T03" & ",B,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T04B" Then
                                    str_detalle = str_detalle & "T04" & ",B,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T02C" Then
                                    str_detalle = str_detalle & "T02" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T03C" Then
                                    str_detalle = str_detalle & "T03" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T04C" Then
                                    str_detalle = str_detalle & "T04" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T05C" Then
                                    str_detalle = str_detalle & "T05" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T06C" Then
                                    str_detalle = str_detalle & "T06" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T07C" Then
                                    str_detalle = str_detalle & "T07" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T08C" Then
                                    str_detalle = str_detalle & "T08" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T09C" Then
                                    str_detalle = str_detalle & "T09" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TL01A" Then
                                    str_detalle = str_detalle & "T01L01" & ",A,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TL02A" Then
                                    str_detalle = str_detalle & "T01L02" & ",A,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TLnnA" Then
                                    'str_detalle = str_detalle & "T01Lnn" & ",A,"
                                    'strClaseExcedente = "T01L"
                                    'strCodigoVhMarcado = "A,"
                                    str_detalle = str_detalle & "T01L" & IIf(Len(oDataRowTres("CODE_GRILLE_TARIF")) = 1, "0" & oDataRowTres("CODE_GRILLE_TARIF"), oDataRowTres("CODE_GRILLE_TARIF")) & ",A,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T01P" Then
                                    str_detalle = str_detalle & "T01P" & ",A,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TP01C" Then
                                    str_detalle = str_detalle & "T09P01" & ",C,"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TPnnC" Then
                                    'str_detalle = str_detalle & "T09Pnn" & ",C,"
                                    'strClaseExcedente = "T09P"
                                    'strCodigoVhMarcado = "C,"

                                    ' strClaseExcedente = "T09P,"
                                    ' strCodigoVhMarcado = strClaseExcedente & "C,"
                                    str_detalle = str_detalle & "T09P" & IIf(Len(oDataRowTres("CODE_GRILLE_TARIF")) = 1, "0" & oDataRowTres("CODE_GRILLE_TARIF"), oDataRowTres("CODE_GRILLE_TARIF")) & ",C,"
                                Else
                                    str_detalle = str_detalle & "No detectada" & ",0,"
                                End If

                            Else
                                str_detalle = str_detalle & ",0,"
                            End If

                            'Tipo de vehículo marcado C-R	Caracter 	X(1)
                            'Código de usuario pago marcado C-R	Caracter 	X(3)

                            If Trim(oDataRow("ID_PAIEMENT")) = 1 Then
                                'str_detalle = str_detalle & "NOR" & ","
                                strCodigoVhPagoMarcado = "NOR" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                'str_detalle = str_detalle & "CRE" & ","
                                'strCodigoVhPagoMarcado = "CRE" & ","
                                'TRAMO CORTO
                                strCodigoVhPagoMarcado = "NOR" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 27 Then
                                'str_detalle = str_detalle & "VSC" & ","
                                strCodigoVhPagoMarcado = "VSC" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 9 Then
                                'str_detalle = str_detalle & "FCUR" & ","
                                strCodigoVhPagoMarcado = "FCUR" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 10 Then
                                'str_detalle = str_detalle & "RPI" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Then
                                'str_detalle = str_detalle & "Tag" & ","
                                strCodigoVhPagoMarcado = "TDC" & ","

                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                                'str_detalle = str_detalle & "Tag" & ","
                                strCodigoVhPagoMarcado = "TDD" & ","

                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                                'str_detalle = str_detalle & "IAV" & ","
                                strCodigoVhPagoMarcado = "IAV" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 13 Then
                                'str_detalle = str_detalle & "ELU" & ","
                                strCodigoVhPagoMarcado = "ELU" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 71 Then
                                'str_detalle = str_detalle & "RP1" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 72 Then
                                'str_detalle = str_detalle & "RP2" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 73 Then
                                'str_detalle = str_detalle & "RP3" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 74 Then
                                'str_detalle = str_detalle & "RP4" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 75 Then
                                'str_detalle = str_detalle & "RP4" & ","
                                strCodigoVhPagoMarcado = "RSP" & ","
                            Else
                                'str_detalle = str_detalle & ","
                                strCodigoVhPagoMarcado = ","
                            End If



                            'Importe vehículo marcado C-R[1]	Decimal 	>>9.99
                            strQuerys = "SELECT " &
           "TYPE_PAIEMENT.libelle_paiement_L2 " &
           ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
           ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
           ",Prix_Cl19, Prix_Cl20 " &
           ",TYPE_PAIEMENT.libelle_paiement " &
           ",TABLE_TARIF.CODE " &
           "FROM TABLE_TARIF, " &
           "TYPE_PAIEMENT " &
           "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                            'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "


                            'borrar
                            strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRowTres("Version_Tarif") & " " &
                                "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                           "ORDER BY TABLE_TARIF.CODE "

                            '                            strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5" & _
                            '    "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                            '"ORDER BY TABLE_TARIF.CODE "


                            If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                                If oDataRowTres("TAB_ID_CLASSE") > 0 And oDataRowTres("TAB_ID_CLASSE") <= 9 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowTres("MONTO_MARCADO") & ",,"
                                ElseIf oDataRowTres("TAB_ID_CLASSE") >= 12 And oDataRowTres("TAB_ID_CLASSE") <= 15 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowTres("MONTO_MARCADO") & ",,"
                                    'EXCEDENTES
                                ElseIf oDataRowTres("TAB_ID_CLASSE") >= 10 And oDataRowTres("TAB_ID_CLASSE") <= 11 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                                ElseIf oDataRowTres("TAB_ID_CLASSE") = 16 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                                ElseIf oDataRowTres("TAB_ID_CLASSE") = 17 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                                ElseIf oDataRowTres("TAB_ID_CLASSE") = 18 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","

                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                    'la tomamos como la 16
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    '
                                Else
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & ",,"
                                End If

                            Else
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & ",,"
                            End If


                            'Importe eje excedente marcado C-R 	Decimal 	>9.99 
                            'Número de tarjeta Pagos Electrónicos[2]	Caracter 	X(20)
                            ' str_detalle = str_detalle & Trim(oDataRowTres("CONTENU_ISO")) & ","
                            'lo elimino para solo madar el campo cuando es tag
                            'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & "," & ","

                            'Situación de la tarjeta Pagos Electrónicos	Caracter 	X(1)
                            If Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                                'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","

                                '27_04
                                'tag_iag = IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO")))
                                tag_iag = IIf(Trim(oDataRowTres("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRowTres("CONTENU_ISO")))

                                tag_iag = Trim(Mid(tag_iag, 1, 16))

                                If Len(Trim(tag_iag)) = 13 And Mid(Trim(tag_iag), 1, 3) = "009" Then
                                    tag_iag = Mid(Trim(tag_iag), 1, 3) & Mid(Trim(tag_iag), 6, 8)
                                End If

                                str_detalle = str_detalle & tag_iag & ","

                                'If IsNumeric(tag_iag) Then

                                '    tarjeta = Trim(tag_iag)

                                '    If Len(tarjeta) <> 11 Then

                                '        'si es menor a 4000 verifico si existe 003
                                '        If CDbl(tarjeta) <= 4000 Then

                                '            strQuerysTag = "SELECT roll FROM roll WHERE roll = " & CDbl(tarjeta)

                                '            If objQuerys_SqlServer.QueryDataSet_SqlServerDos(strQuerysTag, "roll") = 1 Then

                                '                'si esta en la lista le pongo el 099
                                '                tarjeta = "099" & tarjeta.PadLeft(8, "0")

                                '            Else
                                '                'si no esta en la lista le pongo el 003
                                '                tarjeta = "003" & tarjeta.PadLeft(8, "0")

                                '            End If

                                '        ElseIf IsNumeric(tarjeta) >= 16000000 Then
                                '            tarjeta = "003" & tarjeta.PadLeft(8, "0")

                                '        Else
                                '            tarjeta = "099" & tarjeta.PadLeft(8, "0")
                                '            'no es menor a 4000 meto 099
                                '        End If

                                '    End If

                                '    str_detalle = str_detalle & tarjeta & ","
                                'Else
                                '    str_detalle = str_detalle & tag_iag & ","
                                'End If







                                str_detalle = str_detalle & "V" & ","

                                str_detalle = str_detalle & ","
                                str_detalle = str_detalle & ","

                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then





                                'str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID").PadLeft(16, "*")) & ","
                                '27_04
                                'str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID")) & ","
                                str_detalle = str_detalle & Trim(oDataRowTres("ISSUER_ID")) & ","
                                str_detalle = str_detalle & "V" & ","
                                'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) & ","

                                '27_04
                                'If IsNumeric(Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) Then
                                '    str_detalle = str_detalle & Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6) & ","
                                'Else
                                '    str_detalle = str_detalle & "0,"
                                'End If

                                If IsNumeric(Mid(Trim(oDataRowTres("CONTENU_ISO")), 1, 6)) Then


                                    If InStr(Mid(Trim(oDataRowTres("CONTENU_ISO")), 1, 6), "E") = 0 Then
                                        str_detalle = str_detalle & Mid(Trim(oDataRowTres("CONTENU_ISO")), 1, 6) & ","
                                    Else
                                        str_detalle = str_detalle & "0,"
                                    End If

                                Else
                                    str_detalle = str_detalle & "0,"
                                End If

                                '27_04
                                'str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                                str_detalle = str_detalle & Format(oDataRowTres("DATE_TRANSACTION"), "dd/MM/yyyy") & ","

                            Else
                                '27_04
                                'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                                str_detalle = str_detalle & IIf(Trim(oDataRowTres("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                                str_detalle = str_detalle & ","

                                str_detalle = str_detalle & ","
                                str_detalle = str_detalle & ","
                            End If



                            'If Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                            '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("ISSUER_ID").PadLeft(16, "*"))) & ","
                            '    str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                            'Else
                            '    str_detalle = str_detalle & ","
                            '    str_detalle = str_detalle & ","
                            'End If



                        End If
                        'fin clase detectada


                    End If

                    If InStr(str_detalle, "-") <= 1 Then
                        dbl_registros = dbl_registros + 1
                        val.Add(str_detalle)
                        'oSW.WriteLine(str_detalle)
                    End If

                    '----------------------
                Next



                If Len(CStr(dbl_registros)) = 1 Then
                    no_registros = "0000" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 2 Then
                    no_registros = "000" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 3 Then
                    no_registros = "00" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 4 Then
                    no_registros = "0" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 5 Then
                    no_registros = dbl_registros
                End If

                cabecera = cabecera & no_registros

                oSW.WriteLine(cabecera)

            Else

                cabecera = cabecera & "00000"
                oSW.WriteLine(cabecera)


            End If
            'fin detalle

            For Each sLine In val
                oSW.WriteLine(sLine)
            Next

            ProgressBar1.Value = ProgressBar1.Value + 20
            oSW.Flush()
            oSW.Close()




            'MsgBox("Exportación Terminada", MsgBoxStyle.Information, "Exportación")
        Catch ex As Exception
            MsgBox(ex.Message.ToString, MsgBoxStyle.Critical)
        End Try

    End Sub
    'Archivo II
    Private Sub registro_usuarios_telepeaje()

        Dim strQuerys As String
        Dim Linea As String = ""
        Dim cabecera As String
        Dim pie As String
        Dim numero_archivo As String = ""
        Dim nombre_archivo As String
        Dim numero_registros As Double
        Dim cont As Integer
        Dim int_turno As Integer

        Dim h_inicio_turno As Date
        Dim h_fin_turno As Date

        Dim no_registros As String

        Dim str_detalle As String
        Dim str_encargado As String

        Dim dbl_registros As Double

        Dim strClaseExcedente As String
        Dim strCodigoVhMarcado As String
        Dim strCodigoVhPagoMarcado As String

        Dim tag_iag As String
        Dim tarjeta As String

        Dim strQuerysTag As String
        Dim lenText As Integer
        Dim KeyAscii As String
        Dim validar As Boolean
        Dim i As Integer
        Dim val As New ArrayList()
        Dim sLine As String = ""


        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            int_turno = 5
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            int_turno = 6
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
            int_turno = 4
        End If

        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 06:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 13:59:59")
            int_turno = 5
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 21:59:59")
            int_turno = 6
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
            'h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
            h_inicio_turno = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 22:00:00")
            'h_fin_turno = CDate(Format(DateAdd(DateInterval.Day, 1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 05:59:59")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 05:59:59")
            int_turno = 4
        End If



        If Len(id_plaza_cobro) = 3 Then
            If id_plaza_cobro = 108 Then
                nombre_archivo = "0001"
            ElseIf id_plaza_cobro = 109 Then
                nombre_archivo = "001B"
            ElseIf id_plaza_cobro = 107 Then
                nombre_archivo = "0107"
            ElseIf id_plaza_cobro = 106 Then
                nombre_archivo = "0006"
            Else
                nombre_archivo = "0" & id_plaza_cobro

            End If
        End If



        'nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "9" & strIdentificador
        nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "II"



        Dim oSW As New StreamWriter(dir_archivo & nombre_archivo)
        archivo_5 = nombre_archivo

        'cabecera = "David Cabecera"
        'cabecera = "04"
        cabecera = cmbDelegacion.Tag

        If Len(id_plaza_cobro) = 3 Then
            If id_plaza_cobro = 108 Then
                cabecera = cabecera & "0001"
            ElseIf id_plaza_cobro = 109 Then
                cabecera = cabecera & "001B"
            ElseIf id_plaza_cobro = 107 Then
                cabecera = cabecera & "0107"
            ElseIf id_plaza_cobro = 106 Then
                cabecera = cabecera & "0006"
            Else
                cabecera = cabecera & "0" & id_plaza_cobro

            End If
        End If


        cabecera = "03" & cabecera & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "II" & Format(dt_Fecha_Inicio, "dd/MM/yyyy") & int_turno

        'CABECERA INICIO REGISTROS

        'CABECERA FIN

2:
        'inicio detalle

        '            "AND	SITE_GARE.id_reseau		= 	'01' " & _
        '"AND	SITE_GARE.id_Site		=	'" & id_plaza_cobro - 100 & "' " & _

        'DATE_DEBUT_POSTE

        strQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " &
"TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " &
"ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID " &
"FROM TRANSACTION " &
"JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " &
"LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " &
"WHERE " &
"(DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
" AND  (ID_PAIEMENT  = 15 or ID_OBS_MP = 30) " &
"AND (TRANSACTION.Id_Voie = '1' " &
"OR TRANSACTION.Id_Voie = '2' " &
"OR TRANSACTION.Id_Voie = '3' " &
"OR TRANSACTION.Id_Voie = '4' " &
"OR TRANSACTION.Id_Voie = 'X')  AND (MODE_REGLEMENT = 'IAV ')  " &
"ORDER BY DATE_TRANSACTION"
        '


        If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then



            'dbl_registros = oDataSet.Tables("TRANSACTION").Rows.Count

            'If Len(CStr(dbl_registros)) = 1 Then
            '    no_registros = "0000" & dbl_registros
            'ElseIf Len(CStr(dbl_registros)) = 2 Then
            '    no_registros = "000" & dbl_registros
            'ElseIf Len(CStr(dbl_registros)) = 3 Then
            '    no_registros = "00" & dbl_registros
            'ElseIf Len(CStr(dbl_registros)) = 4 Then
            '    no_registros = "0" & dbl_registros
            'ElseIf Len(CStr(dbl_registros)) = 5 Then
            '    no_registros = dbl_registros
            'End If

            'cabecera = cabecera & no_registros

            'oSW.WriteLine(cabecera)

            dbl_registros = 0

            For cont = 0 To oDataSet.Tables("TRANSACTION").Rows.Count - 1


                oDataRow = oDataSet.Tables("TRANSACTION").Rows.Item(cont)

                str_detalle = ""

                If Not IsDBNull(oDataRow("CLASE_DETECTADA")) Then
                    'Else
                    'End If

OBS_TREINTA:

                    'Fecha del evento 	Fecha 	dd/mm/aaaa 
                    str_detalle = Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","

                    'Número de carril	Entero 	>>9
                    If id_plaza_cobro = 184 Then

                        If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                            str_detalle = str_detalle & "340" & ","
                        Else
                            str_detalle = str_detalle & "247" & ","
                        End If

                        '340

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2585" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2586" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "2587" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "2588" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "2589" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "2590" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "2591" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "2592" & ","
                        End If

                        'paso morelos
                    ElseIf id_plaza_cobro = 102 Then

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then

                            If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                str_detalle = str_detalle & "261" & ","
                            Else
                                str_detalle = str_detalle & "249" & ","
                            End If
                            str_detalle = str_detalle & "1803" & ","

                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1804" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1805" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1806" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1807" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1808" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1809" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                str_detalle = str_detalle & "261" & ","
                            Else
                                str_detalle = str_detalle & "249" & ","
                            End If
                            str_detalle = str_detalle & "1810" & ","
                            '--------------------------------------------
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1811" & ","

                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "250" & ","
                            str_detalle = str_detalle & "1812" & ","
                        End If

                        'la venta
                    ElseIf id_plaza_cobro = 104 Then

                        str_detalle = str_detalle & "252" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1830" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1831" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1832" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1833" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1834" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1835" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1836" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1837" & ","

                        End If

                        'la venta
                    ElseIf id_plaza_cobro = 161 Then

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "364" & "," & "2681" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "364" & "," & "2682" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "363" & "," & "2683" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "363" & "," & "2684" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "364" & "," & "2685" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "364" & "," & "2686" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "363" & "," & "2687" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "363" & "," & "2688" & ","
                        End If

                    ElseIf id_plaza_cobro = 103 Then

                        str_detalle = str_detalle & "251" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1816" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1817" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1818" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1819" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1820" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1821" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1822" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1823" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1824" & ","
                        End If

                        'álpuyeca

                    ElseIf id_plaza_cobro = 101 Then

                        str_detalle = str_detalle & "246" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1794" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1795" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1796" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1797" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1798" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1799" & ","
                        End If


                        '+++++++++++++++++++++++++++

                        'aeropuerto
                        '106
                        'A 366
                        'B 367
                        '1		367	2734	B
                        '2		366	2735	A
                        '3		367	2736	B
                        '4		366	2737	A
                        'ElseIf id_plaza_cobro_local = 1 Then
                        '    If Mid(Trim(oDataRow("LANE")), 1, 1) = "A" Then

                        'str_detalle = str_detalle & "366" & ","

                        '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                        '            str_detalle = str_detalle & "2735" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                        '            str_detalle = str_detalle & "2737" & ","
                        '        End If

                        '    ElseIf Mid(Trim(oDataRow("LANE")), 1, 1) = "B" Then

                        'str_detalle = str_detalle & "367" & ","

                        '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                        '            str_detalle = str_detalle & "2734" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                        '            str_detalle = str_detalle & "2736" & ","
                        '        End If

                        '    End If
                        '+++++++++++++++++++++++++++
                        'tlalpan
                    ElseIf id_plaza_cobro = 108 Then

                        str_detalle = str_detalle & "118" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            str_detalle = str_detalle & "3076" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "3063" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "3064" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "3065" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "3066" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "3067" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "3068" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "3069" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "3070" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "3071" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "3072" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "3073" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            str_detalle = str_detalle & "3074" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            str_detalle = str_detalle & "3075" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                            str_detalle = str_detalle & "3077" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                            str_detalle = str_detalle & "3078" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                            str_detalle = str_detalle & "3079" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                            str_detalle = str_detalle & "3080" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                            str_detalle = str_detalle & "3081" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                            str_detalle = str_detalle & "3082" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                            str_detalle = str_detalle & "3083" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                            str_detalle = str_detalle & "3084" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                            str_detalle = str_detalle & "3085" & ","
                        End If

                        'xochitepec
                    ElseIf id_plaza_cobro = 105 Then

                        str_detalle = str_detalle & "365" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2727" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2728" & ","
                        End If
                        'CERRO GORDO
                    ElseIf id_plaza_cobro = 186 Then

                        str_detalle = str_detalle & "351" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                            str_detalle = str_detalle & "3199" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                            str_detalle = str_detalle & "3200" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                            str_detalle = str_detalle & "3201" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                            str_detalle = str_detalle & "3202" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                            str_detalle = str_detalle & "3203" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            'str_detalle = str_detalle & "3185" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            'str_detalle = str_detalle & "3186" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            'str_detalle = str_detalle & "3187" & ","
                            'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            'str_detalle = str_detalle & "3188" & ","
                        End If
                        'QUERETARO

                    ElseIf id_plaza_cobro = 106 Then
                        str_detalle = str_detalle & "112" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1079" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1080" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1081" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1082" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1083" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1084" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1085" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1086" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1087" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "1088" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "1089" & ","
                        End If


                    ElseIf id_plaza_cobro = 183 Then

                        str_detalle = str_detalle & "170" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "2581" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "2582" & ","
                        End If



                    ElseIf id_plaza_cobro = 109 Then

                        str_detalle = str_detalle & "102" & ","

                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1020" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1021" & ","
                        End If

                        'EMILIANO ZAPATA
                    ElseIf id_plaza_cobro = 107 Then
                        str_detalle = str_detalle & "368" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1843" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1844" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1845" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1846" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1847" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1848" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1849" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            str_detalle = str_detalle & "1850" & ","
                            'Segmento A
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            str_detalle = str_detalle & "1851" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            str_detalle = str_detalle & "1852" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            str_detalle = str_detalle & "1853" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            str_detalle = str_detalle & "1854" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            str_detalle = str_detalle & "2743" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            str_detalle = str_detalle & "2744" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                            str_detalle = str_detalle & "2745" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                            str_detalle = str_detalle & "2746" & ","
                        End If

                    ElseIf id_plaza_cobro = 189 Then
                        str_detalle = str_detalle & "189" & ","
                        'Segmento B
                        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            str_detalle = str_detalle & "1891" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            str_detalle = str_detalle & "1892" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            str_detalle = str_detalle & "1893" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            str_detalle = str_detalle & "1894" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            str_detalle = str_detalle & "1895" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            str_detalle = str_detalle & "1896" & ","
                        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            str_detalle = str_detalle & "1897" & ","
                        End If

                        '    'SAN MARCOS
                        'ElseIf id_plaza_cobro = 107 Then

                        '    str_detalle = str_detalle & "121" & ","

                        '    If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

                        '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                        '            str_detalle = str_detalle & "1102" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                        '            str_detalle = str_detalle & "1103" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                        '            str_detalle = str_detalle & "1104" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                        '            str_detalle = str_detalle & "1105" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                        '            str_detalle = str_detalle & "1106" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                        '            str_detalle = str_detalle & "1107" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                        '            str_detalle = str_detalle & "1108" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                        '            str_detalle = str_detalle & "1109" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                        '            str_detalle = str_detalle & "1110" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                        '            str_detalle = str_detalle & "1101" & ","
                        '        End If

                        '    ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

                        '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                        '            str_detalle = str_detalle & "1097" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                        '            str_detalle = str_detalle & "1098" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                        '            str_detalle = str_detalle & "1099" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                        '            str_detalle = str_detalle & "1100" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                        '            str_detalle = str_detalle & "1101" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                        '            str_detalle = str_detalle & "1102" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                        '            str_detalle = str_detalle & "1103" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                        '            str_detalle = str_detalle & "1104" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                        '            str_detalle = str_detalle & "1105" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                        '            str_detalle = str_detalle & "1106" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                        '            str_detalle = str_detalle & "1107" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                        '            str_detalle = str_detalle & "1108" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                        '            str_detalle = str_detalle & "1109" & ","
                        '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                        '            str_detalle = str_detalle & "1110" & ","
                        '        End If

                        '    End If


                    Else
                        str_detalle = str_detalle & ","
                        str_detalle = str_detalle & ","
                    End If


                    'Cuerpo	Caracter 	X(1)
                    str_detalle = str_detalle & Mid(Trim(oDataRow("Voie")), 1, 1) & ","


                    'Hora de evento 	Caracter 	hhmmss 
                    str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "HHmmss") & ","



                    'numero tarjeta iave
                    validar = True
                    tag_iag = IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO")))

                    tag_iag = Trim(Mid(tag_iag, 1, 16))

                    If Len(Trim(tag_iag)) = 13 And Mid(Trim(tag_iag), 1, 3) = "009" Then
                        tag_iag = Mid(Trim(tag_iag), 1, 3) & Mid(Trim(tag_iag), 6, 8)
                    End If


                    lenText = Len(tag_iag)

                    For i = 1 To lenText
                        'KeyAscii = KeyAscii & CStr(Asc(Mid$(cadena, i, 1)))
                        KeyAscii = CStr(Asc(Mid$(tag_iag, i, 1)))
                        If (KeyAscii >= 33) And (KeyAscii <= 47) Or (KeyAscii >= 58) And (KeyAscii <= 64) Or
                (KeyAscii >= 91) And (KeyAscii <= 96) Or (KeyAscii >= 123) And (KeyAscii <= 126) Then
                            validar = False
                            KeyAscii = 8
                        End If
                    Next

                    str_detalle = str_detalle & tag_iag & ","


                    'situacion tarjeta iave
                    If Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                        str_detalle = str_detalle & "V" & ","
                    Else
                        str_detalle = str_detalle & "I" & ","
                    End If

                    'clave transportsta iave
                    str_detalle = str_detalle & ","

                    'clase vehiculo iave
                    str_detalle = str_detalle & ","

                    'importe usuario iave
                    str_detalle = str_detalle & oDataRow("MONTO_DETECTADO") & ","

                    'numero de evento ect
                    str_detalle = str_detalle & oDataRow("EVENT_NUMBER") & ","

                    'Número de turno	Entero 	9
                    str_detalle = str_detalle & int_turno & ","


                    'numero de ejes segun ect
                    If Not IsDBNull(oDataRow("CLASE_DETECTADA")) Then

                        strClaseExcedente = ""
                        If Trim(oDataRow("CLASE_DETECTADA")) = "T01A" Then
                            str_detalle = str_detalle & "2" & ","
                            str_detalle = str_detalle & "L"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01M" Then
                            str_detalle = str_detalle & "2" & ","
                            str_detalle = str_detalle & "L"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01T" Then
                            'str_detalle = str_detalle & "T01" & ","
                            'T01,T => T09P01,C
                            str_detalle = str_detalle & "2" & ","
                            str_detalle = str_detalle & "L"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T02B" Then
                            str_detalle = str_detalle & "2" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T03B" Then
                            str_detalle = str_detalle & "3" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T04B" Then
                            str_detalle = str_detalle & "4" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T02C" Then
                            str_detalle = str_detalle & "2" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T03C" Then
                            str_detalle = str_detalle & "3" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T04C" Then
                            str_detalle = str_detalle & "4" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T05C" Then
                            str_detalle = str_detalle & "5" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T06C" Then
                            str_detalle = str_detalle & "6" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T07C" Then
                            str_detalle = str_detalle & "7" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T08C" Then
                            str_detalle = str_detalle & "8" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T09C" Then
                            str_detalle = str_detalle & "9" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TL01A" Then
                            str_detalle = str_detalle & "2" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TL02A" Then
                            str_detalle = str_detalle & "2" & ","
                            str_detalle = str_detalle & "P"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TLnnA" Then
                            '04/12/2013
                            'str_detalle = str_detalle & "T01Lnn" & ","
                            'strClaseExcedente = "T01L"
                            'str_detalle = str_detalle & "T01L" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT")) & ","
                            str_detalle = str_detalle & oDataRow("ID_OBS_TT") & ","
                            str_detalle = str_detalle & "L"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01P" Then
                            str_detalle = str_detalle & "1" & ","
                            str_detalle = str_detalle & "L"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TP01C" Then
                            str_detalle = str_detalle & "9" & ","
                            str_detalle = str_detalle & "L"
                        ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TPnnC" Then
                            '04/12/2013
                            'str_detalle = str_detalle & "T09Pnn" & ","
                            'strClaseExcedente = "T09P" & ","
                            'str_detalle = str_detalle & "T09P" & ","
                            'str_detalle = str_detalle & "T09P" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT")) & ","
                            str_detalle = str_detalle & oDataRow("ID_OBS_TT") & ","
                            str_detalle = str_detalle & "P"
                        Else
                            str_detalle = str_detalle & "No detectada" & ",0,"
                        End If

                    Else
                        str_detalle = str_detalle & "0,"
                    End If

                    'II Fin



                Else


                    If oDataRow("ID_OBS_MP") = 30 Then
                        GoTo OBS_TREINTA
                    End If

                    'inicio clase detectada 




                    strQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " &
"TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " &
"ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID " &
"FROM TRANSACTION " &
"JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " &
"LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " &
"WHERE " &
"(DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
"AND VOIE = '" & oDataRow("VOIE") & "' " &
"AND  ID_OBS_SEQUENCE <> '7777' " &
"AND EVENT_NUMBER = " & oDataRow("EVENT_NUMBER") & " " &
"AND (TRANSACTION.Id_Voie = '1' " &
"OR TRANSACTION.Id_Voie = '2' " &
"OR TRANSACTION.Id_Voie = '3' " &
"OR TRANSACTION.Id_Voie = '4' " &
"OR TRANSACTION.Id_Voie = 'X') " &
"ORDER BY DATE_TRANSACTION desc"


                    If objQuerys.QueryDataSetTres(strQuerys, "TRANSACTION") = 1 Then

                        str_detalle = Format(oDataRowTres("DATE_TRANSACTION"), "dd/MM/yyyy") & ","

                        'Número de carril	Entero 	>>9
                        If id_plaza_cobro = 184 Then

                            If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                str_detalle = str_detalle & "340" & ","
                            Else
                                str_detalle = str_detalle & "247" & ","
                            End If


                            If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2585" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2586" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "2587" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "2588" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "2589" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "2590" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "2591" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "2592" & ","
                            End If

                            'paso morelos
                        ElseIf id_plaza_cobro = 102 Then

                            If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "261" & ","
                                Else
                                    str_detalle = str_detalle & "249" & ","
                                End If
                                str_detalle = str_detalle & "1803" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1804" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1805" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1806" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1807" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1808" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1809" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "261" & ","
                                Else
                                    str_detalle = str_detalle & "249" & ","
                                End If
                                str_detalle = str_detalle & "1810" & ","
                                '--------------------------------------------
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1811" & ","

                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1812" & ","
                            End If

                            'la venta
                        ElseIf id_plaza_cobro = 104 Then

                            str_detalle = str_detalle & "252" & ","

                            If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1830" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1831" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1832" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1833" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1834" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1835" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1836" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1837" & ","
                            End If


                            'la venta
                        ElseIf id_plaza_cobro = 161 Then

                            If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "364" & "," & "2681" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "364" & "," & "2682" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "363" & "," & "2683" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "363" & "," & "2684" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "364" & "," & "2685" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "364" & "," & "2686" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "363" & "," & "2687" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "363" & "," & "2688" & ","
                            End If

                        ElseIf id_plaza_cobro = 103 Then

                            str_detalle = str_detalle & "251" & ","

                            If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1816" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1817" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1818" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1819" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1820" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1821" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1822" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1823" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1824" & ","
                            End If

                            'álpuyeca
                            '101
                            '246
                            '1	1794
                            '2	1795
                            '3	1796
                            '4	1797

                        ElseIf id_plaza_cobro = 101 Then

                            str_detalle = str_detalle & "246" & ","

                            If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1794" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1795" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1796" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1797" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1798" & ","
                            ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1799" & ","

                            End If
                            '++++++++++++++++++++++++++++++++++++++++++++


                            'aeropuerto
                            '106
                            'A 366
                            'B 367
                            '1		367	2734	B
                            '2		366	2735	A
                            '3		367	2736	B
                            '4		366	2737	A
                            'ElseIf id_plaza_cobro_local = 1 Then
                            '    If Mid(Trim(oDataRow("LANE")), 1, 1) = "A" Then

                            'str_detalle = str_detalle & "366" & ","

                            '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            '            str_detalle = str_detalle & "2735" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            '            str_detalle = str_detalle & "2737" & ","
                            '        End If

                            '    ElseIf Mid(Trim(oDataRow("LANE")), 1, 1) = "B" Then

                            'str_detalle = str_detalle & "367" & ","

                            '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            '            str_detalle = str_detalle & "2734" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            '            str_detalle = str_detalle & "2736" & ","
                            '        End If

                            '    End If
                            '+++++++++++++++++++++++++++
                            'tlalpan
                        ElseIf id_plaza_cobro = 108 Then

                            str_detalle = str_detalle & "118" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "3076" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "3063" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "3064" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "3065" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "3066" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "3067" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "3068" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "3069" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "3070" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "3071" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "3072" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "3073" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "3074" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "3075" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "3077" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "3078" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                                str_detalle = str_detalle & "3079" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                                str_detalle = str_detalle & "3080" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                                str_detalle = str_detalle & "3081" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                                str_detalle = str_detalle & "3082" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3083" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3084" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3085" & ","
                            End If

                            'xochitepec
                        ElseIf id_plaza_cobro = 105 Then

                            str_detalle = str_detalle & "365" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2727" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2728" & ","
                            End If
                            'CERRO GORDO
                        ElseIf id_plaza_cobro = 186 Then

                            str_detalle = str_detalle & "351" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3199" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3200" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3201" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                                str_detalle = str_detalle & "3202" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                                str_detalle = str_detalle & "3203" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                'str_detalle = str_detalle & "3185" & ","
                                ' ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                'str_detalle = str_detalle & "3186" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                ' str_detalle = str_detalle & "3187" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                'str_detalle = str_detalle & "3188" & ","
                            End If

                            'QUERETARO

                        ElseIf id_plaza_cobro = 106 Then
                            str_detalle = str_detalle & "112" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1079" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1080" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1081" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1082" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1083" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1084" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1085" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1086" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1087" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1088" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1089" & ","
                            End If

                        ElseIf id_plaza_cobro = 183 Then

                            str_detalle = str_detalle & "170" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2581" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2582" & ","
                            End If


                            'tres marias
                        ElseIf id_plaza_cobro = 109 Then

                            str_detalle = str_detalle & "102" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1020" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1021" & ","
                            End If




                            'Central de Abastos
                        ElseIf id_plaza_cobro = 107 Then
                            str_detalle = str_detalle & "368" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1843" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1844" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1845" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1846" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1847" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1848" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1849" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1850" & ","
                                'Segmento A
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1851" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1852" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1853" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "1854" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "2743" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "2744" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "2745" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "2746" & ","
                            End If

                            'Central de Abastos
                        ElseIf id_plaza_cobro = 189 Then
                            str_detalle = str_detalle & "189" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1891" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1892" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1893" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1894" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1895" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1896" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1897" & ","

                            End If

                            '    'SAN MARCOS
                            'ElseIf id_plaza_cobro = 107 Then

                            '    str_detalle = str_detalle & "121" & ","

                            '    If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

                            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        End If

                            '    ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

                            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            '            str_detalle = str_detalle & "1097" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            '            str_detalle = str_detalle & "1098" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            '            str_detalle = str_detalle & "1099" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            '            str_detalle = str_detalle & "1100" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        End If

                            '    End If

                        Else
                            str_detalle = str_detalle & ","
                            str_detalle = str_detalle & ","
                        End If

                        'Cuerpo	Caracter 	X(1)
                        str_detalle = str_detalle & Mid(Trim(oDataRowTres("Voie")), 1, 1) & ","

                        'Hora de evento 	Caracter 	hhmmss 
                        str_detalle = str_detalle & Format(oDataRowTres("DATE_TRANSACTION"), "HHmmss") & ","


                        validar = True
                        tag_iag = IIf(Trim(oDataRowTres("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRowTres("CONTENU_ISO")))

                        tag_iag = Trim(Mid(tag_iag, 1, 16))

                        If Len(Trim(tag_iag)) = 13 And Mid(Trim(tag_iag), 1, 3) = "009" Then
                            tag_iag = Mid(Trim(tag_iag), 1, 3) & Mid(Trim(tag_iag), 6, 8)
                        End If

                        lenText = Len(tag_iag)

                        For i = 1 To lenText
                            'KeyAscii = KeyAscii & CStr(Asc(Mid$(cadena, i, 1)))
                            KeyAscii = CStr(Asc(Mid$(tag_iag, i, 1)))
                            If (KeyAscii >= 33) And (KeyAscii <= 47) Or (KeyAscii >= 58) And (KeyAscii <= 64) Or
                (KeyAscii >= 91) And (KeyAscii <= 96) Or (KeyAscii >= 123) And (KeyAscii <= 126) Then
                                validar = False
                                KeyAscii = 8
                            End If
                        Next


                        str_detalle = str_detalle & tag_iag & ","

                        'situacion tarjeta iave
                        If Trim(oDataRowTres("ID_PAIEMENT")) = 15 Then
                            str_detalle = str_detalle & "V" & ","
                        Else
                            str_detalle = str_detalle & "I" & ","
                        End If

                        'clave transportsta iave
                        str_detalle = str_detalle & ","

                        'clase vehiculo iave
                        str_detalle = str_detalle & ","

                        'importe usuario iave
                        str_detalle = str_detalle & oDataRow("MONTO_DETECTADO") & ","

                        'numero de evento ect
                        str_detalle = str_detalle & oDataRowTres("EVENT_NUMBER") & ","

                        'Número de turno	Entero 	9
                        str_detalle = str_detalle & int_turno & ","

                        'numero de ejes segun ect
                        If Not IsDBNull(oDataRowTres("CLASE_DETECTADA")) Then

                            strClaseExcedente = ""
                            If Trim(oDataRowTres("CLASE_DETECTADA")) = "T01A" Then
                                str_detalle = str_detalle & "2" & ","
                                str_detalle = str_detalle & "L"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01M" Then
                                str_detalle = str_detalle & "2" & ","
                                str_detalle = str_detalle & "L"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01T" Then
                                'str_detalle = str_detalle & "T01" & ","
                                'T01,T => T09P01,C
                                str_detalle = str_detalle & "2" & ","
                                str_detalle = str_detalle & "L"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T02B" Then
                                str_detalle = str_detalle & "2" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T03B" Then
                                str_detalle = str_detalle & "3" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T04B" Then
                                str_detalle = str_detalle & "4" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T02C" Then
                                str_detalle = str_detalle & "2" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T03C" Then
                                str_detalle = str_detalle & "3" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T04C" Then
                                str_detalle = str_detalle & "4" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T05C" Then
                                str_detalle = str_detalle & "5" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T06C" Then
                                str_detalle = str_detalle & "6" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T07C" Then
                                str_detalle = str_detalle & "7" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T08C" Then
                                str_detalle = str_detalle & "8" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T09C" Then
                                str_detalle = str_detalle & "9" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TL01A" Then
                                str_detalle = str_detalle & "2" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TL02A" Then
                                str_detalle = str_detalle & "2" & ","
                                str_detalle = str_detalle & "P"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TLnnA" Then
                                '04/12/2013
                                'str_detalle = str_detalle & "T01Lnn" & ","
                                'strClaseExcedente = "T01L"
                                'str_detalle = str_detalle & "T01L" & IIf(Len(oDataRowtReS("ID_OBS_TT")) = 1, "0" & oDataRowtReS("ID_OBS_TT"), oDataRowtReS("ID_OBS_TT")) & ","
                                str_detalle = str_detalle & oDataRowTres("ID_OBS_TT") & ","
                                str_detalle = str_detalle & "L"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01P" Then
                                str_detalle = str_detalle & "1" & ","
                                str_detalle = str_detalle & "L"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TP01C" Then
                                str_detalle = str_detalle & "9" & ","
                                str_detalle = str_detalle & "L"
                            ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TPnnC" Then
                                '04/12/2013
                                'str_detalle = str_detalle & "T09Pnn" & ","
                                'strClaseExcedente = "T09P" & ","
                                'str_detalle = str_detalle & "T09P" & ","
                                'str_detalle = str_detalle & "T09P" & IIf(Len(oDataRowtReS("ID_OBS_TT")) = 1, "0" & oDataRowtReS("ID_OBS_TT"), oDataRowtReS("ID_OBS_TT")) & ","
                                str_detalle = str_detalle & oDataRowTres("ID_OBS_TT") & ","
                                str_detalle = str_detalle & "P"
                            Else
                                str_detalle = str_detalle & "No detectada" & ",0,"
                            End If

                        Else
                            str_detalle = str_detalle & "0,"
                        End If
                        '--------------------
                        'FIN II







                    End If
                    'fin clase detectada


                End If

                If validar = True Then
                    dbl_registros = dbl_registros + 1
                    val.Add(str_detalle)

                    'oSW.WriteLine(str_detalle)
                    '----------------------
                End If



            Next

            If Len(CStr(dbl_registros)) = 1 Then
                no_registros = "0000" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 2 Then
                no_registros = "000" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 3 Then
                no_registros = "00" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 4 Then
                no_registros = "0" & dbl_registros
            ElseIf Len(CStr(dbl_registros)) = 5 Then
                no_registros = dbl_registros
            End If

            cabecera = cabecera & no_registros

            oSW.WriteLine(cabecera)

        Else

            cabecera = cabecera & "00000"
            oSW.WriteLine(cabecera)


        End If
        'fin detalle

        For Each sLine In val
            oSW.WriteLine(sLine)
        Next


        oSW.Flush()
        oSW.Close()
        ProgressBar1.Value = ProgressBar1.Value + 20



        'MsgBox("Exportación Terminada", MsgBoxStyle.Information, "Exportación")
        'Catch ex As Exception
        '    MsgBox(ex.Message.ToString, MsgBoxStyle.Critical)
        'End Try

    End Sub
    'REGISTROS DE USUARIOS TELEPEAJE   Archivo PA
    Private Sub eventos_detectados_y_marcados_en_el_ECT_EAP()


        Dim strQuerys As String
        Dim Linea As String = ""
        Dim cabecera As String
        Dim pie As String
        Dim numero_archivo As String = ""
        Dim nombre_archivo As String
        Dim numero_registros As Double
        Dim cont As Integer
        Dim int_turno As Integer

        Dim h_inicio_turno As Date
        Dim h_fin_turno As Date

        Dim no_registros As String

        Dim str_detalle As String
        Dim str_encargado As String

        Dim dbl_registros As Double

        Dim strClaseExcedente As String
        Dim strCodigoVhMarcado As String
        Dim strCodigoVhPagoMarcado As String

        Dim tag_iag As String
        Dim tarjeta As String

        Dim strQuerysTag As String

        Dim str_pre As String
        Dim str_det As String
        Dim str_marc As String

        Dim db_pre As String
        Dim db_det As String
        Dim db_det_ejes As String
        Dim db_marc As String

        Dim info() As String
        Dim cont_info As Double = 0

        Dim bl_det_a_pos As Boolean
        Dim bl_sin_pre As Boolean





        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            int_turno = 5
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            int_turno = 6
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
            int_turno = 4
        End If

        If Mid(Trim(str_Turno_block), 1, 2) = "06" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 06:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 13:59:59")
            int_turno = 5
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "14" Then
            h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 21:59:59")
            int_turno = 6
        ElseIf Mid(Trim(str_Turno_block), 1, 2) = "22" Then
            'h_inicio_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 14:00:00")
            h_inicio_turno = CDate(Format(DateAdd(DateInterval.Day, -1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 22:00:00")
            'h_fin_turno = CDate(Format(DateAdd(DateInterval.Day, 1, dt_Fecha_Inicio), "MM/dd/yyyy") & " 05:59:59")
            h_fin_turno = CDate(Format(dt_Fecha_Inicio, "MM/dd/yyyy") & " 05:59:59")
            int_turno = 4
        End If


        Try

            If Len(id_plaza_cobro) = 3 Then
                If id_plaza_cobro = 108 Then
                    nombre_archivo = "0001"
                ElseIf id_plaza_cobro = 109 Then
                    nombre_archivo = "001B"
                ElseIf id_plaza_cobro = 107 Then
                    nombre_archivo = "0107"
                ElseIf id_plaza_cobro = 106 Then
                    nombre_archivo = "0006"
                Else
                    nombre_archivo = "0" & id_plaza_cobro

                End If
            End If



            nombre_archivo = nombre_archivo & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "P" & strIdentificador

            Dim oSW As New StreamWriter(dir_archivo & nombre_archivo)
            archivo_4 = nombre_archivo


            cabecera = cmbDelegacion.Tag

            If Len(id_plaza_cobro) = 3 Then
                If id_plaza_cobro = 108 Then
                    cabecera = cabecera & "0001"
                ElseIf id_plaza_cobro = 109 Then
                    cabecera = cabecera & "001B"
                ElseIf id_plaza_cobro = 107 Then
                    cabecera = cabecera & "0107"
                ElseIf id_plaza_cobro = 106 Then
                    cabecera = cabecera & "0006"
                Else
                    cabecera = cabecera & "0" & id_plaza_cobro

                End If
            End If


            cabecera = "03" & cabecera & Format(dt_Fecha_Inicio, "MM") & Format(dt_Fecha_Inicio, "dd") & "." & int_turno & "P" & strIdentificador & Format(dt_Fecha_Inicio, "dd/MM/yyyy") & int_turno

            'CABECERA INICIO REGISTROS

            'CABECERA FIN

2:
            'inicio detalle

            '            "AND	SITE_GARE.id_reseau		= 	'01' " & _
            '"AND	SITE_GARE.id_Site		=	'" & id_plaza_cobro - 100 & "' " & _

            'DATE_DEBUT_POSTE
            strQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " &
"TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " &
"ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID, " &
"TYPE_CLASSE_PRE.LIBELLE_COURT1 AS CLASE_PRE, TRANSACTION.ID_CLASSE AS ID_CLASE_PRE, CODE1 AS PRE_EJES_EX " &
"FROM TRANSACTION " &
"JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " &
"LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " &
"LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_PRE  ON TRANSACTION.ID_CLASSE = TYPE_CLASSE_PRE.ID_CLASSE " &
"WHERE " &
"(DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
" AND  ID_PAIEMENT  <> 0 " &
"AND (TRANSACTION.Id_Voie = '1' " &
"OR TRANSACTION.Id_Voie = '2' " &
"OR TRANSACTION.Id_Voie = '3' " &
"OR TRANSACTION.Id_Voie = '4' " &
"OR TRANSACTION.Id_Voie = 'X') " &
"ORDER BY DATE_TRANSACTION"



            If objQuerys.QueryDataSet(strQuerys, "TRANSACTION") = 1 Then

                dbl_registros = oDataSet.Tables("TRANSACTION").Rows.Count

                If Len(CStr(dbl_registros)) = 1 Then
                    no_registros = "0000" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 2 Then
                    no_registros = "000" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 3 Then
                    no_registros = "00" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 4 Then
                    no_registros = "0" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 5 Then
                    no_registros = dbl_registros
                End If

                '29_04
                'cabecera = cabecera & no_registros

                'oSW.WriteLine(cabecera)




                For cont = 0 To oDataSet.Tables("TRANSACTION").Rows.Count - 1

                    oDataRow = oDataSet.Tables("TRANSACTION").Rows.Item(cont)

                    str_detalle = ""

                    If Not IsDBNull(oDataRow("CLASE_DETECTADA")) Then
                        'Else
                        'End If


                        'Fecha del evento 	Fecha 	dd/mm/aaaa 
                        str_detalle = Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                        'Número de turno	Entero 	9
                        str_detalle = str_detalle & int_turno & ","
                        'Hora de evento 	Caracter 	hhmmss 
                        str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "HHmmss") & ","
                        'Clave de tramo	Entero 	>9
                        'Verificar 
                        'str_detalle = str_detalle & "247" & ","
                        'Número de carril	Entero 	>>9


                        If id_plaza_cobro = 184 Then

                            If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                str_detalle = str_detalle & "340" & ","
                            Else
                                str_detalle = str_detalle & "247" & ","
                            End If

                            '340

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2585" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2586" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "2587" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "2588" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "2589" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "2590" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "2591" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "2592" & ","
                            End If

                            'paso morelos
                        ElseIf id_plaza_cobro = 102 Then

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "261" & ","
                                Else
                                    str_detalle = str_detalle & "249" & ","
                                End If
                                str_detalle = str_detalle & "1803" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1804" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1805" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1806" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1807" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1808" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1809" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "261" & ","
                                Else
                                    str_detalle = str_detalle & "249" & ","
                                End If
                                str_detalle = str_detalle & "1810" & ","
                                '--------------------------------------------
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1811" & ","

                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "250" & ","
                                str_detalle = str_detalle & "1812" & ","
                            End If

                            'la venta
                        ElseIf id_plaza_cobro = 104 Then

                            str_detalle = str_detalle & "252" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1830" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1831" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1832" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1833" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1834" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1835" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1836" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1837" & ","

                            End If

                            'la venta
                        ElseIf id_plaza_cobro = 161 Then

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "364" & "," & "2681" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "364" & "," & "2682" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "363" & "," & "2683" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "363" & "," & "2684" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "364" & "," & "2685" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "364" & "," & "2686" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "363" & "," & "2687" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "363" & "," & "2688" & ","

                            End If

                        ElseIf id_plaza_cobro = 103 Then

                            str_detalle = str_detalle & "251" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1816" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1817" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1818" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1819" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1820" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1821" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1822" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1823" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1824" & ","
                            End If

                            'álpuyeca
                            '101
                            '246
                            '1	1794
                            '2	1795
                            '3	1796
                            '4	1797
                        ElseIf id_plaza_cobro = 101 Then

                            str_detalle = str_detalle & "246" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1794" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1795" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1796" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1797" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1798" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1799" & ","
                            End If



                            '106
                            'A 366
                            'B 367
                            '1		367	2734	B
                            '2		366	2735	A
                            '3		367	2736	B
                            '4		366	2737	A
                            'ElseIf id_plaza_cobro_local = 1 Then
                            '    If Mid(Trim(oDataRow("LANE")), 1, 1) = "A" Then

                            'str_detalle = str_detalle & "366" & ","

                            '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                            '            str_detalle = str_detalle & "2735" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                            '            str_detalle = str_detalle & "2737" & ","
                            '        End If

                            '    ElseIf Mid(Trim(oDataRow("LANE")), 1, 1) = "B" Then

                            'str_detalle = str_detalle & "367" & ","

                            '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                            '            str_detalle = str_detalle & "2734" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                            '            str_detalle = str_detalle & "2736" & ","
                            '        End If

                            '    End If
                            '+++++++++++++++++++++++++++
                            'tlalpan
                        ElseIf id_plaza_cobro = 108 Then

                            str_detalle = str_detalle & "118" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "3076" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "3063" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "3064" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "3065" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "3066" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "3067" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "3068" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "3069" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "3070" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "3071" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "3072" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "3073" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "3074" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "3075" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "3077" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "3078" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                                str_detalle = str_detalle & "3079" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                                str_detalle = str_detalle & "3080" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                                str_detalle = str_detalle & "3081" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                                str_detalle = str_detalle & "3082" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3083" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3084" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3085" & ","
                            End If

                            'xochitepec
                        ElseIf id_plaza_cobro = 105 Then

                            str_detalle = str_detalle & "365" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2727" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2728" & ","
                            End If



                            'CERRO GORDO

                        ElseIf id_plaza_cobro = 186 Then

                            str_detalle = str_detalle & "351" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                str_detalle = str_detalle & "3199" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                str_detalle = str_detalle & "3200" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                str_detalle = str_detalle & "3201" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                                str_detalle = str_detalle & "3202" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                                str_detalle = str_detalle & "3203" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                'str_detalle = str_detalle & "3186" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                'str_detalle = str_detalle & "3187" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                'str_detalle = str_detalle & "3188" & ","
                                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                'str_detalle = str_detalle & "3189" & ","
                            End If

                            'QUERETARO

                        ElseIf id_plaza_cobro = 106 Then
                            str_detalle = str_detalle & "112" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1079" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1080" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1081" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1082" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1083" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1084" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1085" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1086" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1087" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1088" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1089" & ","
                            End If


                            'VillaGrand
                        ElseIf id_plaza_cobro = 183 Then

                            str_detalle = str_detalle & "170" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "2581" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "2582" & ","
                            End If

                            'tres marias
                        ElseIf id_plaza_cobro = 109 Then

                            str_detalle = str_detalle & "102" & ","

                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1020" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1021" & ","
                            End If


                            'Central de Abastos
                        ElseIf id_plaza_cobro = 107 Then
                            str_detalle = str_detalle & "368" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1843" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1844" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1845" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1846" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1847" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1848" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1849" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                str_detalle = str_detalle & "1850" & ","
                                'Segmento A
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                str_detalle = str_detalle & "1851" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                str_detalle = str_detalle & "1852" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                str_detalle = str_detalle & "1853" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                str_detalle = str_detalle & "1854" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                str_detalle = str_detalle & "2743" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                str_detalle = str_detalle & "2744" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                str_detalle = str_detalle & "2745" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                str_detalle = str_detalle & "2746" & ","
                            End If
                        ElseIf id_plaza_cobro = 189 Then
                            str_detalle = str_detalle & "189" & ","
                            'Segmento B
                            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                str_detalle = str_detalle & "1891" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                str_detalle = str_detalle & "1892" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                str_detalle = str_detalle & "1893" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                str_detalle = str_detalle & "1894" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                str_detalle = str_detalle & "1895" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                str_detalle = str_detalle & "1896" & ","
                            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                str_detalle = str_detalle & "1897" & ","
                            End If

                            '    'SAN MARCOS
                            'ElseIf id_plaza_cobro = 107 Then

                            '    str_detalle = str_detalle & "121" & ","

                            '    If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

                            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        End If

                            '    ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

                            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                            '            str_detalle = str_detalle & "1097" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                            '            str_detalle = str_detalle & "1098" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                            '            str_detalle = str_detalle & "1099" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                            '            str_detalle = str_detalle & "1100" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                            '            str_detalle = str_detalle & "1101" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                            '            str_detalle = str_detalle & "1102" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                            '            str_detalle = str_detalle & "1103" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                            '            str_detalle = str_detalle & "1104" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                            '            str_detalle = str_detalle & "1105" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                            '            str_detalle = str_detalle & "1106" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                            '            str_detalle = str_detalle & "1107" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                            '            str_detalle = str_detalle & "1108" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                            '            str_detalle = str_detalle & "1109" & ","
                            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                            '            str_detalle = str_detalle & "1110" & ","
                            '        End If

                            '    End If


                        Else
                            str_detalle = str_detalle & ","
                            str_detalle = str_detalle & ","
                        End If


                        'Cuerpo	Caracter 	X(1)
                        str_detalle = str_detalle & Mid(Trim(oDataRow("Voie")), 1, 1) & ","
                        'Número de evento 	Entero 	>>>>>>9
                        str_detalle = str_detalle & oDataRow("EVENT_NUMBER") & ","
                        'Número de folio 	Entero 	>>>>>>9 
                        str_detalle = str_detalle & oDataRow("FOLIO_ECT") & ","
                        'Código de vehículo detectado ECT 	Caracter 	X(6)

                        str_pre = ""
                        str_det = ""
                        str_marc = ""
                        db_det = 0
                        db_det_ejes = 0

                        If Not IsDBNull(oDataRow("CLASE_DETECTADA")) Then

                            strClaseExcedente = ""
                            If Trim(oDataRow("CLASE_DETECTADA")) = "T01A" Then
                                str_detalle = str_detalle & "T01" & ","
                                str_det = "T01"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01M" Then
                                str_detalle = str_detalle & "T01" & ","
                                str_det = "T01"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01T" Then
                                'str_detalle = str_detalle & "T01" & ","
                                'T01,T => T09P01,C
                                str_detalle = str_detalle & "T09P01" & ","
                                str_det = "T09P01"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T02B" Then
                                str_detalle = str_detalle & "T02" & ","
                                str_det = "T02"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T03B" Then
                                str_detalle = str_detalle & "T03" & ","
                                str_det = "T03"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T04B" Then
                                str_detalle = str_detalle & "T04" & ","
                                str_det = "T04"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T02C" Then
                                str_detalle = str_detalle & "T02" & ","
                                str_det = "T02"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T03C" Then
                                str_detalle = str_detalle & "T03" & ","
                                str_det = "T03"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T04C" Then
                                str_detalle = str_detalle & "T04" & ","
                                str_det = "T04"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T05C" Then
                                str_detalle = str_detalle & "T05" & ","
                                str_det = "T05"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T06C" Then
                                str_detalle = str_detalle & "T06" & ","
                                str_det = "T06"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T07C" Then
                                str_detalle = str_detalle & "T07" & ","
                                str_det = "T07"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T08C" Then
                                str_detalle = str_detalle & "T08" & ","
                                str_det = "T08"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T09C" Then
                                str_detalle = str_detalle & "T09" & ","
                                str_det = "T09"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TL01A" Then
                                str_detalle = str_detalle & "T01L01" & ","
                                str_det = "T01L01"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TL02A" Then
                                str_detalle = str_detalle & "T01L02" & ","
                                str_det = "T01L02"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TLnnA" Then
                                '04/12/2013
                                'str_detalle = str_detalle & "T01Lnn" & ","
                                'strClaseExcedente = "T01L"
                                str_detalle = str_detalle & "T01L" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT")) & ","
                                str_det = "T01L" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT"))
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "T01P" Then
                                str_detalle = str_detalle & "T01P" & ","
                                str_det = "T01P"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TP01C" Then
                                str_detalle = str_detalle & "T09P01" & ","
                                str_det = "T09P01"
                            ElseIf Trim(oDataRow("CLASE_DETECTADA")) = "TPnnC" Then
                                '04/12/2013
                                'str_detalle = str_detalle & "T09Pnn" & ","
                                'strClaseExcedente = "T09P" & ","
                                'str_detalle = str_detalle & "T09P" & ","
                                str_detalle = str_detalle & "T09P" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT")) & ","
                                str_det = "T09P" & IIf(Len(oDataRow("ID_OBS_TT")) = 1, "0" & oDataRow("ID_OBS_TT"), oDataRow("ID_OBS_TT"))
                            Else
                                str_detalle = str_detalle & "No detectada" & ",0,"
                                'str_det =
                            End If

                        Else
                            str_detalle = str_detalle & ",0,"
                            'str_det =
                        End If

                        'Importe vehículo detectado ECT 	Decimal 	>>9.99 


                        strQuerys = "SELECT " &
                 "TYPE_PAIEMENT.libelle_paiement_L2 " &
                 ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
                 ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
                 ",Prix_Cl19, Prix_Cl20 " &
                 ",TYPE_PAIEMENT.libelle_paiement " &
                 ",TABLE_TARIF.CODE " &
                 "FROM TABLE_TARIF, " &
                 "TYPE_PAIEMENT " &
                 "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                        'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "


                        'borrar
                        strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRow("Version_Tarif") & " " &
                            "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                       "ORDER BY TABLE_TARIF.CODE "

                        '        strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5 " & _
                        '     "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                        '"ORDER BY TABLE_TARIF.CODE "

                        If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                            If oDataRow("ACD_CLASS") > 0 And oDataRow("ACD_CLASS") <= 9 Then
                                str_detalle = str_detalle & oDataRow("MONTO_DETECTADO") & ",,"
                                db_det = oDataRow("MONTO_DETECTADO")
                                db_det_ejes = 0


                            ElseIf oDataRow("ACD_CLASS") >= 12 And oDataRow("ACD_CLASS") <= 15 Then
                                str_detalle = str_detalle & oDataRow("MONTO_DETECTADO") & ",,"
                                db_det = oDataRow("MONTO_DETECTADO")
                                db_det_ejes = 0
                                'EXCEDENTES
                            ElseIf oDataRow("ACD_CLASS") >= 10 And oDataRow("ACD_CLASS") <= 11 Then
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                db_det = oDataRowCuatro("Prix_Cl01")
                                db_det_ejes = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                            ElseIf oDataRow("ACD_CLASS") = 16 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                db_det = oDataRowCuatro("Prix_Cl09")
                                db_det_ejes = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")
                            ElseIf oDataRow("ACD_CLASS") = 17 Then
                                'strClaseExcedente = "T01Lnn"
                                'str_detalle = str_detalle & strClaseExcedente & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                db_det = oDataRowCuatro("Prix_Cl01")
                                db_det_ejes = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                            ElseIf oDataRow("ACD_CLASS") = 18 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                'la tomamos como la 16
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                db_det = oDataRowCuatro("Prix_Cl09")
                                db_det_ejes = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")
                            Else
                                str_detalle = str_detalle & ",,"
                            End If

                        Else
                            str_detalle = str_detalle & ",,"
                        End If


                        'Importe eje excedente detectado ECT 	Decimal 	>9.99 
                        'Código de vehículo marcado C-R	Caracter 	X(6)
                        If Not IsDBNull(oDataRow("CLASE_MARCADA")) Then

                            strClaseExcedente = ""
                            strCodigoVhMarcado = ""
                            If Trim(oDataRow("CLASE_MARCADA")) = "T01A" Then
                                str_detalle = str_detalle & "T01" & ",A,"
                                str_marc = "T01"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T01M" Then
                                str_detalle = str_detalle & "T01" & ",M,"
                                str_marc = "T01M"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T01T" Then
                                'str_detalle = str_detalle & "T01" & ",T,"
                                'T01,T => T09P01,C
                                str_detalle = str_detalle & "T09P01" & ",C,"
                                str_marc = "T09P01"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T02B" Then
                                str_detalle = str_detalle & "T02" & ",B,"
                                str_marc = "T02"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T03B" Then
                                str_detalle = str_detalle & "T03" & ",B,"
                                str_marc = "T03"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T04B" Then
                                str_detalle = str_detalle & "T04" & ",B,"
                                str_marc = "T04"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T02C" Then
                                str_detalle = str_detalle & "T02" & ",C,"
                                str_marc = "T02"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T03C" Then
                                str_detalle = str_detalle & "T03" & ",C,"
                                str_marc = "T03"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T04C" Then
                                str_detalle = str_detalle & "T04" & ",C,"
                                str_marc = "T04"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T05C" Then
                                str_detalle = str_detalle & "T05" & ",C,"
                                str_marc = "T05"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T06C" Then
                                str_detalle = str_detalle & "T06" & ",C,"
                                str_marc = "T06"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T07C" Then
                                str_detalle = str_detalle & "T07" & ",C,"
                                str_marc = "T07"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T08C" Then
                                str_detalle = str_detalle & "T08" & ",C,"
                                str_marc = "T08"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T09C" Then
                                str_detalle = str_detalle & "T09" & ",C,"
                                str_marc = "T09"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TL01A" Then
                                str_detalle = str_detalle & "T01L01" & ",A,"
                                str_marc = "T01L01"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TL02A" Then
                                str_detalle = str_detalle & "T01L02" & ",A,"
                                str_marc = "T01L02"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TLnnA" Then
                                'str_detalle = str_detalle & "T01Lnn" & ",A,"
                                'strClaseExcedente = "T01L"
                                'strCodigoVhMarcado = "A,"
                                str_detalle = str_detalle & "T01L" & IIf(Len(oDataRow("CODE_GRILLE_TARIF")) = 1, "0" & oDataRow("CODE_GRILLE_TARIF"), oDataRow("CODE_GRILLE_TARIF")) & ",A,"
                                str_marc = "T01L" & IIf(Len(oDataRow("CODE_GRILLE_TARIF")) = 1, "0" & oDataRow("CODE_GRILLE_TARIF"), oDataRow("CODE_GRILLE_TARIF"))
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "T01P" Then
                                str_detalle = str_detalle & "T01P" & ",A,"
                                str_marc = "T01P"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TP01C" Then
                                str_detalle = str_detalle & "T09P01" & ",C,"
                                str_marc = "T09P01"
                            ElseIf Trim(oDataRow("CLASE_MARCADA")) = "TPnnC" Then
                                'str_detalle = str_detalle & "T09Pnn" & ",C,"
                                'strClaseExcedente = "T09P"
                                'strCodigoVhMarcado = "C,"

                                'strClaseExcedente = "T09P,"
                                'strCodigoVhMarcado = strClaseExcedente & "C,"
                                str_detalle = str_detalle & "T09P" & IIf(Len(oDataRow("CODE_GRILLE_TARIF")) = 1, "0" & oDataRow("CODE_GRILLE_TARIF"), oDataRow("CODE_GRILLE_TARIF")) & ",C,"
                                str_marc = "T09P" & IIf(Len(oDataRow("CODE_GRILLE_TARIF")) = 1, "0" & oDataRow("CODE_GRILLE_TARIF"), oDataRow("CODE_GRILLE_TARIF"))
                            Else
                                str_detalle = str_detalle & "No detectada" & ",0,"
                            End If

                        Else
                            str_detalle = str_detalle & ",0,"
                        End If

                        'Tipo de vehículo marcado C-R	Caracter 	X(1)
                        'Código de usuario pago marcado C-R	Caracter 	X(3)

                        If Trim(oDataRow("ID_PAIEMENT")) = 1 Then
                            'str_detalle = str_detalle & "NOR" & ","
                            strCodigoVhPagoMarcado = "NOR" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                            'str_detalle = str_detalle & "CRE" & ","
                            'strCodigoVhPagoMarcado = "CRE" & ","
                            'TRAMO CORTO
                            strCodigoVhPagoMarcado = "NOR" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 27 Then
                            'str_detalle = str_detalle & "VSC" & ","
                            strCodigoVhPagoMarcado = "VSC" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 9 Then
                            'str_detalle = str_detalle & "FCUR" & ","
                            strCodigoVhPagoMarcado = "FCUR" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 10 Then
                            'str_detalle = str_detalle & "RPI" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Then
                            'str_detalle = str_detalle & "Tag" & ","
                            strCodigoVhPagoMarcado = "TDC" & ","

                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                            'str_detalle = str_detalle & "Tag" & ","
                            strCodigoVhPagoMarcado = "TDD" & ","

                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                            'str_detalle = str_detalle & "IAV" & ","
                            strCodigoVhPagoMarcado = "IAV" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 13 Then
                            'str_detalle = str_detalle & "ELU" & ","
                            strCodigoVhPagoMarcado = "ELU" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 71 Then
                            'str_detalle = str_detalle & "RP1" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 72 Then
                            'str_detalle = str_detalle & "RP2" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 73 Then
                            'str_detalle = str_detalle & "RP3" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 74 Then
                            'str_detalle = str_detalle & "RP4" & ","
                            strCodigoVhPagoMarcado = "RPI" & ","
                        ElseIf Trim(oDataRow("ID_PAIEMENT")) = 75 Then
                            'str_detalle = str_detalle & "RP4" & ","
                            strCodigoVhPagoMarcado = "RSP" & ","
                        Else
                            'str_detalle = str_detalle & ","
                            strCodigoVhPagoMarcado = ","
                        End If



                        'Importe vehículo marcado C-R[1]	Decimal 	>>9.99
                        strQuerys = "SELECT " &
       "TYPE_PAIEMENT.libelle_paiement_L2 " &
       ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
       ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
       ",Prix_Cl19, Prix_Cl20 " &
       ",TYPE_PAIEMENT.libelle_paiement " &
       ",TABLE_TARIF.CODE " &
       "FROM TABLE_TARIF, " &
       "TYPE_PAIEMENT " &
       "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                        'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "


                        'borrar
                        strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRow("Version_Tarif") & " " &
                            "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                       "ORDER BY TABLE_TARIF.CODE "

                        ' strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5 " & _
                        '     "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                        '"ORDER BY TABLE_TARIF.CODE "

                        If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                            If oDataRow("TAB_ID_CLASSE") > 0 And oDataRow("TAB_ID_CLASSE") <= 9 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRow("MONTO_MARCADO") & ",,"
                            ElseIf oDataRow("TAB_ID_CLASSE") >= 12 And oDataRow("TAB_ID_CLASSE") <= 15 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRow("MONTO_MARCADO") & ",,"
                                'EXCEDENTES
                            ElseIf oDataRow("TAB_ID_CLASSE") >= 10 And oDataRow("TAB_ID_CLASSE") <= 11 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                            ElseIf oDataRow("TAB_ID_CLASSE") = 16 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                            ElseIf oDataRow("TAB_ID_CLASSE") = 17 Then
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                            ElseIf oDataRow("TAB_ID_CLASSE") = 18 Then
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                'la tomamos como la 16
                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                            Else
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & ",,"
                            End If

                        Else
                            str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                            str_detalle = str_detalle & ",,"
                        End If


                        bl_sin_pre = False
                        If id_plaza_cobro = 108 Then
                            If Trim(oDataRow("Voie")) = "A14" Or Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "B14" Or Trim(oDataRow("Voie")) = "B10" Then
                                bl_sin_pre = True
                            End If
                        End If

                        'determino si tengo pre
                        bl_det_a_pos = False
                        If id_plaza_cobro = 108 Then

                            If Trim(oDataRow("Voie")) = "A08" Or Trim(oDataRow("Voie")) = "A09" Or Trim(oDataRow("Voie")) = "B01" Then
                                bl_det_a_pos = True
                            End If
                        End If

                        'determino si tengo pre
                        'tres marias
                        bl_sin_pre = False
                        If id_plaza_cobro = 109 Then

                            If Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "B02" Then
                                bl_sin_pre = True
                            End If
                        End If

                        'paso morelos
                        bl_det_a_pos = False
                        If id_plaza_cobro = 102 Then

                            If Trim(oDataRow("Voie")) = "A02" Or Trim(oDataRow("Voie")) = "B07" Then
                                bl_det_a_pos = True
                            End If

                            If Trim(oDataRow("Voie")) = "A09" Or Trim(oDataRow("Voie")) = "B10" Then
                                bl_sin_pre = True
                            End If
                        End If


                        'determino si tengo pre
                        '   ÁEROPUERTO()
                        bl_sin_pre = False
                        If id_plaza_cobro = 106 Then

                            If Trim(oDataRow("Voie")) = "B01" Or Trim(oDataRow("Voie")) = "B03" Or Trim(oDataRow("Voie")) = "A02" Or Trim(oDataRow("Voie")) = "A04" Then
                                bl_sin_pre = True
                            End If
                        End If

                        'tlalpan
                        bl_sin_pre = False
                        If id_plaza_cobro = 108 Then
                            If Trim(oDataRow("Voie")) = "B01" Or Trim(oDataRow("Voie")) = "B04" Or Trim(oDataRow("Voie")) = "A21" Then
                                bl_sin_pre = True
                            End If
                        End If

                        'xochitepec
                        bl_sin_pre = False
                        If id_plaza_cobro = 105 Then

                            If Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "A02" Then
                                bl_sin_pre = True
                            End If
                        End If

                        'Alpuyeca
                        bl_sin_pre = False
                        If id_plaza_cobro = 101 Then

                            If Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "A02" And Trim(oDataRow("Voie")) = "B03" Or Trim(oDataRow("Voie")) = "B04" Then
                                bl_sin_pre = True
                            End If
                        End If

                        'PALO BLANCO
                        bl_sin_pre = False
                        If id_plaza_cobro = 103 Then

                            If Trim(oDataRow("Voie")) = "A09" Or Trim(oDataRow("Voie")) = "B01" Then
                                bl_sin_pre = True
                            End If
                        End If

                        'LA VENTA
                        bl_sin_pre = False
                        If id_plaza_cobro = 104 Then

                            If Trim(oDataRow("Voie")) = "A08" Or Trim(oDataRow("Voie")) = "B01" Then
                                bl_sin_pre = True
                            End If
                        End If
                        bl_sin_pre = False
                        'emiliano zapata pre
                        If id_plaza_cobro = 107 Then

                            'If Trim(oDataRow("Voie")) = "A08" Or Trim(oDataRow("Voie")) = "B01" Then
                            bl_sin_pre = True
                            'End If
                        End If


                        If bl_det_a_pos = True Then

                            str_detalle = str_detalle & str_det & ","
                            str_pre = str_det

                            str_detalle = str_detalle & db_det & "," & db_det_ejes

                        Else

                            If bl_sin_pre = False Then


                                'PRECLASIFICADOS
                                If Not IsDBNull(oDataRow("CLASE_PRE")) Then

                                    strClaseExcedente = ""
                                    If Trim(oDataRow("CLASE_PRE")) = "T01A" Then
                                        str_detalle = str_detalle & "T01" & ","
                                        str_pre = "T01"
                                        'c_cod_veh_ect = "T01"
                                        'c_tpo_veh_ect = "A"
                                        'c_ect_tpo_eje = "L"

                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T01M" Then
                                        str_detalle = str_detalle & "T01" & ","
                                        str_pre = "T01"
                                        'c_cod_veh_ect = "T01"
                                        'c_tpo_veh_ect = "M"
                                        'c_ect_tpo_eje = "L"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T01T" Then
                                        'str_detalle = str_detalle & "T01" & ","
                                        'T01,T => T09P01,C
                                        str_detalle = str_detalle & "T09P01" & ","
                                        str_pre = "T09P01"
                                        'c_cod_veh_ect = "T09P01"
                                        'c_tpo_veh_ect = "A"
                                        'c_ect_tpo_eje = "L"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T02B" Then
                                        str_detalle = str_detalle & "T02" & ","
                                        str_pre = "T02"
                                        'c_cod_veh_ect = "T02"
                                        'c_tpo_veh_ect = "B"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T03B" Then
                                        str_detalle = str_detalle & "T03" & ","
                                        str_pre = "T03"
                                        'c_cod_veh_ect = "T03"
                                        'c_tpo_veh_ect = "B"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T04B" Then
                                        str_detalle = str_detalle & "T04" & ","
                                        str_pre = "T04"
                                        'c_cod_veh_ect = "T04"
                                        'c_tpo_veh_ect = "B"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T02C" Then
                                        str_detalle = str_detalle & "T02" & ","
                                        str_pre = "T02"
                                        'c_cod_veh_ect = "T02"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T03C" Then
                                        str_detalle = str_detalle & "T03" & ","
                                        str_pre = "T03"
                                        'c_cod_veh_ect = "T03"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T04C" Then
                                        str_detalle = str_detalle & "T04" & ","
                                        str_pre = "T04"
                                        'c_cod_veh_ect = "T04"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T05C" Then
                                        str_detalle = str_detalle & "T05" & ","
                                        str_pre = "T05"
                                        'c_cod_veh_ect = "T05"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T06C" Then
                                        str_detalle = str_detalle & "T06" & ","
                                        str_pre = "T06"
                                        'c_cod_veh_ect = "T06"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T07C" Then
                                        str_detalle = str_detalle & "T07" & ","
                                        str_pre = "T07"
                                        'c_cod_veh_ect = "T07"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T08C" Then
                                        str_detalle = str_detalle & "T08" & ","
                                        str_pre = "T08"
                                        'c_cod_veh_ect = "T08"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T09C" Then
                                        str_detalle = str_detalle & "T09" & ","
                                        str_pre = "T09"
                                        'c_cod_veh_ect = "T09"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "TL01A" Then
                                        str_detalle = str_detalle & "T01L01" & ","
                                        str_pre = "T01L01"
                                        'c_cod_veh_ect = "T01L01"
                                        'c_tpo_veh_ect = "A"
                                        'c_ect_tpo_eje = "L"
                                        'c_ect_cant_eje = 1
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "TL02A" Then
                                        str_detalle = str_detalle & "T01L02" & ","
                                        str_pre = "T01L02"
                                        'c_cod_veh_ect = "T01L02"
                                        'c_tpo_veh_ect = "A"
                                        'c_ect_tpo_eje = "L"
                                        'c_ect_cant_eje = 2
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "TLnnA" Then
                                        '04/12/2013
                                        'str_detalle = str_detalle & "T01Lnn" & ","
                                        'strClaseExcedente = "T01L"
                                        'str_detalle = str_detalle & "T01L" & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX")) & ","

                                        If IsNumeric(oDataRow("PRE_EJES_EX")) Then



                                            If oDataRow("PRE_EJES_EX") <> 0 Then
                                                str_detalle = str_detalle & "T01L" & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX")) & ","
                                                str_pre = "T01L" & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX"))
                                                'c_cod_veh_ect = "T01L" & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX"))
                                                'c_tpo_veh_ect = "A"
                                                'c_ect_tpo_eje = "L"
                                                'c_ect_cant_eje = oDataRow("PRE_EJES_EX")

                                            Else
                                                str_detalle = str_detalle & "T01L08" & ","
                                                str_pre = "T01L08"
                                                'c_cod_veh_ect = "T01L08" ' & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX"))
                                                'c_tpo_veh_ect = "A"
                                                'c_ect_tpo_eje = "L"
                                                'c_ect_cant_eje = 8

                                            End If



                                        Else
                                            str_detalle = str_detalle & "T01L08" & ","
                                            str_pre = "T01L08"
                                            'c_cod_veh_ect = "T01L08" ' & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX"))
                                            'c_tpo_veh_ect = "A"
                                            'c_ect_tpo_eje = "L"
                                            'c_ect_cant_eje = 8
                                        End If


                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "T01P" Then
                                        str_detalle = str_detalle & "T01P" & ","
                                        str_pre = "T01P"
                                        'c_cod_veh_ect = "T01P"
                                        'c_tpo_veh_ect = "A"
                                        'c_ect_tpo_eje = "L"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "TP01C" Then
                                        str_detalle = str_detalle & "T09P01" & ","
                                        str_pre = "T09P01"
                                        'c_cod_veh_ect = "T09P01"
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                    ElseIf Trim(oDataRow("CLASE_PRE")) = "TPnnC" Then
                                        '04/12/2013
                                        'str_detalle = str_detalle & "T09Pnn" & ","
                                        'strClaseExcedente = "T09P" & ","
                                        'str_detalle = str_detalle & "T09P" & ","



                                        If IsNumeric(oDataRow("PRE_EJES_EX")) Then
                                            str_detalle = str_detalle & "T09P" & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX")) & ","
                                            str_pre = "T09P" & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX"))

                                        Else

                                            str_detalle = str_detalle & "T01L08" & ","
                                            str_pre = "T01L08"
                                        End If


                                        'c_cod_veh_ect = "T09P" & IIf(Len(oDataRow("PRE_EJES_EX")) = 1, "0" & oDataRow("PRE_EJES_EX"), oDataRow("PRE_EJES_EX"))
                                        'c_tpo_veh_ect = "C"
                                        'c_ect_tpo_eje = "P"
                                        'c_ect_cant_eje = oDataRow("PRE_EJES_EX")
                                    Else
                                        str_detalle = str_detalle & "T01L08" & ","
                                        str_pre = "T01L08"
                                        'str_detalle = str_detalle & "No detectada" & ","

                                        'c_cod_veh_ect = "0"
                                        'c_tpo_veh_ect = "0"
                                        'c_ect_tpo_eje = "0"
                                        'c_ect_cant_eje = 0

                                    End If

                                Else
                                    str_detalle = str_detalle & "T01L08" & ","
                                    str_pre = "T01L08"
                                    'str_detalle = str_detalle & ","

                                    'c_cod_veh_ect = "0"
                                    'c_tpo_veh_ect = "0"
                                    'c_ect_tpo_eje = "0"
                                    'c_ect_cant_eje = 0

                                End If


                                strQuerys = "SELECT " &
        "TYPE_PAIEMENT.libelle_paiement_L2 " &
        ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
        ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
        ",Prix_Cl19, Prix_Cl20 " &
        ",TYPE_PAIEMENT.libelle_paiement " &
        ",TABLE_TARIF.CODE " &
        "FROM TABLE_TARIF, " &
        "TYPE_PAIEMENT " &
        "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                                'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "

                                '28_04
                                strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRow("Version_Tarif") & " " &
                                    "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                               "ORDER BY TABLE_TARIF.CODE "

                                ' strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5 " & _
                                '     "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                                '"ORDER BY TABLE_TARIF.CODE "

                                If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then



                                    If oDataRow("ID_CLASE_PRE") = 1 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 2 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl02") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 3 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl03") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 4 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl04") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 5 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl05") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 6 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl06") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 7 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl07") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 8 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl08") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 9 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 12 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl12") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 13 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl13") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 14 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl14") & ","
                                    ElseIf oDataRow("ID_CLASE_PRE") = 15 Then
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl15") & ","

                                        'EXCEDENTES
                                    ElseIf oDataRow("ID_CLASE_PRE") = 10 Then
                                        'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                        'c_imp_eje_ect = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                                        str_detalle = str_detalle & 1 * oDataRowCuatro("Prix_Cl17")

                                    ElseIf oDataRow("ID_CLASE_PRE") = 11 Then
                                        'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                        'c_imp_eje_ect = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                                        str_detalle = str_detalle & 2 * oDataRowCuatro("Prix_Cl17")

                                    ElseIf oDataRow("ID_CLASE_PRE") = 16 Then

                                        If IsNumeric(oDataRow("PRE_EJES_EX")) Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ","
                                            str_detalle = str_detalle & oDataRow("PRE_EJES_EX") * oDataRowCuatro("Prix_Cl16")
                                        Else
                                            str_detalle = str_detalle & "0,"
                                            str_detalle = str_detalle & "0"
                                        End If


                                    ElseIf oDataRow("ID_CLASE_PRE") = 17 Then
                                        'strClaseExcedente = "T01Lnn"

                                        If IsNumeric(oDataRow("PRE_EJES_EX")) Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                            str_detalle = str_detalle & oDataRow("PRE_EJES_EX") * oDataRowCuatro("Prix_Cl17")
                                        Else
                                            str_detalle = str_detalle & "0,"
                                            str_detalle = str_detalle & "0"
                                        End If


                                    ElseIf oDataRow("ID_CLASE_PRE") = 18 Then
                                        'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                        'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                        'la tomamos como la 16
                                        'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                        str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ","
                                        str_detalle = str_detalle & 1 * oDataRowCuatro("Prix_Cl16")
                                    Else
                                        str_detalle = str_detalle & ",,"
                                    End If

                                Else
                                    str_detalle = str_detalle & ",,"
                                End If

                            End If
                            'FIN PRE CLASIFICADOS
                        End If









                        'Importe eje excedente marcado C-R 	Decimal 	>9.99 
                        'Número de tarjeta Pagos Electrónicos[2]	Caracter 	X(20)
                        'str_detalle = str_detalle & Trim(oDataRow("CONTENU_ISO")) & ","
                        'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","


                        'Situación de la tarjeta Pagos Electrónicos	Caracter 	X(1)
                        'If Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                        '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","
                        '    str_detalle = str_detalle & "V" & ","

                        'Else
                        '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                        '    str_detalle = str_detalle & ","
                        'End If

                        '' ''tag_iag = ""
                        '' ''tarjeta = ""

                        '' ''If Trim(oDataRow("ID_PAIEMENT")) = 15 Then

                        '' ''    'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","

                        '' ''    tag_iag = IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO")))

                        '' ''    tag_iag = Trim(Mid(tag_iag, 1, 16))

                        '' ''    str_detalle = str_detalle & tag_iag & ","
                        '' ''    'If IsNumeric(tag_iag) Then

                        '' ''    '    tarjeta = Trim(tag_iag)

                        '' ''    '    If Len(tarjeta) <> 11 Then

                        '' ''    '        'si es menor a 4000 verifico si existe 003
                        '' ''    '        If CDbl(tarjeta) <= 4000 Then

                        '' ''    '            strQuerysTag = "SELECT roll FROM roll WHERE roll = " & CDbl(tarjeta)

                        '' ''    '            If objQuerys_SqlServer.QueryDataSet_SqlServerDos(strQuerysTag, "roll") = 1 Then

                        '' ''    '                'si esta en la lista le pongo el 099
                        '' ''    '                tarjeta = "099" & tarjeta.PadLeft(8, "0")

                        '' ''    '            Else
                        '' ''    '                'si no esta en la lista le pongo el 003
                        '' ''    '                tarjeta = "003" & tarjeta.PadLeft(8, "0")

                        '' ''    '            End If

                        '' ''    '        ElseIf IsNumeric(tarjeta) >= 16000000 Then
                        '' ''    '            tarjeta = "003" & tarjeta.PadLeft(8, "0")

                        '' ''    '        Else
                        '' ''    '            tarjeta = "099" & tarjeta.PadLeft(8, "0")
                        '' ''    '            'no es menor a 4000 meto 099
                        '' ''    '        End If

                        '' ''    '    End If

                        '' ''    '    str_detalle = str_detalle & tarjeta & ","
                        '' ''    'Else
                        '' ''    '    str_detalle = str_detalle & tag_iag & ","
                        '' ''    'End If


                        '' ''    str_detalle = str_detalle & "V" & ","
                        '' ''    str_detalle = str_detalle & ","
                        '' ''    str_detalle = str_detalle & ","

                        '' ''ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then


                        '' ''    'str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID").PadLeft(16, "*")) & ","
                        '' ''    str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID")) & ","
                        '' ''    str_detalle = str_detalle & "V" & ","
                        '' ''    'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) & ","

                        '' ''    If IsNumeric(Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) Then
                        '' ''        str_detalle = str_detalle & Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6) & ","
                        '' ''    Else
                        '' ''        str_detalle = str_detalle & "0,"
                        '' ''    End If


                        '' ''    str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","

                        '' ''Else
                        '' ''    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                        '' ''    str_detalle = str_detalle & ","

                        '' ''    str_detalle = str_detalle & ","
                        '' ''    str_detalle = str_detalle & ","
                        '' ''End If


                        'If Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                        '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("ISSUER_ID").PadLeft(16, "*"))) & ","
                        '    str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                        'Else
                        '    str_detalle = str_detalle & ","
                        '    str_detalle = str_detalle & ","
                        'End If

                        'FIN UNO
                        'str_detalle = Replace(str_detalle, "T01,T", "T09P01,C")
                    Else

                        'inicio clase detectada 

                        strQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " &
    "TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " &
    "ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, " &
    "TYPE_CLASSE_PRE.LIBELLE_COURT1 AS CLASE_PRE, TRANSACTION.ID_CLASSE AS ID_CLASE_PRE, CODE1 AS PRE_EJES_EX, ISSUER_ID " &
    "FROM TRANSACTION " &
    "JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " &
    "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " &
    "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_PRE  ON TRANSACTION.ID_CLASSE = TYPE_CLASSE_PRE.ID_CLASSE " &
    "WHERE " &
    "(DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
    "AND VOIE = '" & oDataRow("VOIE") & "' " &
    "AND  ID_OBS_SEQUENCE = '7' " &
    "AND EVENT_NUMBER = " & oDataRow("EVENT_NUMBER") & " " &
    "AND (TRANSACTION.Id_Voie = '1' " &
    "OR TRANSACTION.Id_Voie = '2' " &
    "OR TRANSACTION.Id_Voie = '3' " &
    "OR TRANSACTION.Id_Voie = '4' " &
    "OR TRANSACTION.Id_Voie = 'X') " &
    "ORDER BY DATE_TRANSACTION"

                        If objQuerys.QueryDataSetTres(strQuerys, "TRANSACTION") = 1 Then

                            str_detalle = Format(oDataRowTres("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                            'Número de turno	Entero 	9
                            str_detalle = str_detalle & int_turno & ","
                            'Hora de evento 	Caracter 	hhmmss 
                            str_detalle = str_detalle & Format(oDataRowTres("DATE_TRANSACTION"), "HHmmss") & ","
                            'Clave de tramo	Entero 	>9
                            'Verificar 
                            'str_detalle = str_detalle & "247" & ","
                            'Número de carril	Entero 	>>9
                            If id_plaza_cobro = 184 Then

                                If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                    str_detalle = str_detalle & "340" & ","
                                Else
                                    str_detalle = str_detalle & "247" & ","
                                End If


                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2585" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2586" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "2587" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "2588" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "2589" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "2590" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "2591" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "2592" & ","
                                End If

                                'paso morelos
                            ElseIf id_plaza_cobro = 102 Then

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                        str_detalle = str_detalle & "261" & ","
                                    Else
                                        str_detalle = str_detalle & "249" & ","
                                    End If
                                    str_detalle = str_detalle & "1803" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1804" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1805" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1806" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1807" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1808" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1809" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    If Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                        str_detalle = str_detalle & "261" & ","
                                    Else
                                        str_detalle = str_detalle & "249" & ","
                                    End If
                                    str_detalle = str_detalle & "1810" & ","
                                    '--------------------------------------------
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1811" & ","

                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "250" & ","
                                    str_detalle = str_detalle & "1812" & ","
                                End If

                                'la venta
                            ElseIf id_plaza_cobro = 104 Then

                                str_detalle = str_detalle & "252" & ","

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1830" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1831" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1832" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1833" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1834" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1835" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1836" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1837" & ","

                                End If

                            ElseIf id_plaza_cobro = 161 Then

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "364" & "," & "2681" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "364" & "," & "2682" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "363" & "," & "2683" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "363" & "," & "2684" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "364" & "," & "2685" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "364" & "," & "2686" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "363" & "," & "2687" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "363" & "," & "2688" & ","

                                End If



                            ElseIf id_plaza_cobro = 103 Then

                                str_detalle = str_detalle & "251" & ","

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1816" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1817" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1818" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1819" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1820" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1821" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1822" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1823" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1824" & ","
                                End If

                                'álpuyeca
                                '101
                                '246
                                '1	1794
                                '2	1795
                                '3	1796
                                '4	1797

                            ElseIf id_plaza_cobro = 101 Then

                                str_detalle = str_detalle & "246" & ","

                                If CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1794" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1795" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1796" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1797" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1798" & ","
                                ElseIf CInt(Mid(Trim(oDataRowTres("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1799" & ","
                                End If
                                '++++++++++++++++++++++++++++++++++++++++++++
                            ElseIf id_plaza_cobro = 106 Then
                                If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

                                    str_detalle = str_detalle & "366" & ","

                                    If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                        str_detalle = str_detalle & "2735" & ","
                                    ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                        str_detalle = str_detalle & "2737" & ","
                                    End If

                                ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

                                    str_detalle = str_detalle & "367" & ","

                                    If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                        str_detalle = str_detalle & "2734" & ","
                                    ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                        str_detalle = str_detalle & "2736" & ","
                                    End If

                                End If

                                'aeropuerto
                                '106
                                'A 366
                                'B 367
                                '1		367	2734	B
                                '2		366	2735	A
                                '3		367	2736	B
                                '4		366	2737	A
                                'ElseIf id_plaza_cobro_local = 1 Then
                                '    If Mid(Trim(oDataRow("LANE")), 1, 1) = "A" Then

                                'str_detalle = str_detalle & "366" & ","

                                '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "2" Then
                                '            str_detalle = str_detalle & "2735" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "4" Then
                                '            str_detalle = str_detalle & "2737" & ","
                                '        End If

                                '    ElseIf Mid(Trim(oDataRow("LANE")), 1, 1) = "B" Then

                                'str_detalle = str_detalle & "367" & ","

                                '        If CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "1" Then
                                '            str_detalle = str_detalle & "2734" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("LANE")), 2, 2)) = "3" Then
                                '            str_detalle = str_detalle & "2736" & ","
                                '        End If

                                '    End If
                                '+++++++++++++++++++++++++++
                                'tlalpan
                            ElseIf id_plaza_cobro = 108 Then

                                str_detalle = str_detalle & "118" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                    str_detalle = str_detalle & "3076" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "3063" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "3064" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "3065" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "3066" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "3067" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "3068" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "3069" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "3070" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "3071" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "3072" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "3073" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                    str_detalle = str_detalle & "3074" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                    str_detalle = str_detalle & "3075" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                    str_detalle = str_detalle & "3077" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                    str_detalle = str_detalle & "3078" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                                    str_detalle = str_detalle & "3079" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                                    str_detalle = str_detalle & "3080" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                                    str_detalle = str_detalle & "3081" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                                    str_detalle = str_detalle & "3082" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                    str_detalle = str_detalle & "3083" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                    str_detalle = str_detalle & "3084" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                    str_detalle = str_detalle & "3085" & ","
                                End If

                                'xochitepec
                            ElseIf id_plaza_cobro = 105 Then

                                str_detalle = str_detalle & "365" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2727" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2728" & ","
                                End If
                                'CERRO GORDO
                            ElseIf id_plaza_cobro = 186 Then

                                str_detalle = str_detalle & "351" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                                    str_detalle = str_detalle & "3199" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                                    str_detalle = str_detalle & "3200" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                                    str_detalle = str_detalle & "3201" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                                    str_detalle = str_detalle & "3202" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                                    str_detalle = str_detalle & "3203" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    'str_detalle = str_detalle & "3186" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    'str_detalle = str_detalle & "3187" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    'str_detalle = str_detalle & "3188" & ","
                                    'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    'str_detalle = str_detalle & "3189" & ","
                                End If
                                'Queretaro
                            ElseIf id_plaza_cobro = 106 Then
                                str_detalle = str_detalle & "112" & ","
                                'Segmento B
                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1079" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1080" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1081" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1082" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1083" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1084" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1085" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1086" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1087" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "1088" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "1089" & ","
                                End If
                                'VillaGrand
                            ElseIf id_plaza_cobro = 183 Then

                                str_detalle = str_detalle & "170" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "2581" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "2582" & ","
                                End If


                                'tres marias
                            ElseIf id_plaza_cobro = 109 Then

                                str_detalle = str_detalle & "102" & ","

                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1020" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1021" & ","
                                End If

                                'Central de Abastos
                            ElseIf id_plaza_cobro = 107 Then
                                str_detalle = str_detalle & "368" & ","
                                'Segmento B
                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1843" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1844" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1845" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1846" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1847" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1848" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1849" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                    str_detalle = str_detalle & "1850" & ","
                                    'Segmento A
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                    str_detalle = str_detalle & "1851" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                    str_detalle = str_detalle & "1852" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                    str_detalle = str_detalle & "1853" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                    str_detalle = str_detalle & "1854" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                    str_detalle = str_detalle & "2743" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                    str_detalle = str_detalle & "2744" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                    str_detalle = str_detalle & "2745" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                                    str_detalle = str_detalle & "2746" & ","
                                End If

                            ElseIf id_plaza_cobro = 189 Then
                                str_detalle = str_detalle & "189" & ","
                                'Segmento B
                                If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                    str_detalle = str_detalle & "1891" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                    str_detalle = str_detalle & "1892" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                    str_detalle = str_detalle & "1893" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                    str_detalle = str_detalle & "1894" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                    str_detalle = str_detalle & "1895" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                    str_detalle = str_detalle & "1896" & ","
                                ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                    str_detalle = str_detalle & "1897" & ","
                                End If

                                'SAN MARCOS
                                'ElseIf id_plaza_cobro = 107 Then

                                '    str_detalle = str_detalle & "121" & ","

                                '    If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

                                '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                '            str_detalle = str_detalle & "1102" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                '            str_detalle = str_detalle & "1103" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                '            str_detalle = str_detalle & "1104" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                '            str_detalle = str_detalle & "1105" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                '            str_detalle = str_detalle & "1106" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                '            str_detalle = str_detalle & "1107" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                '            str_detalle = str_detalle & "1108" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                '            str_detalle = str_detalle & "1109" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                '            str_detalle = str_detalle & "1110" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                                '            str_detalle = str_detalle & "1101" & ","
                                '        End If

                                '    ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

                                '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                                '            str_detalle = str_detalle & "1097" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                                '            str_detalle = str_detalle & "1098" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                                '            str_detalle = str_detalle & "1099" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                                '            str_detalle = str_detalle & "1100" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                                '            str_detalle = str_detalle & "1101" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                                '            str_detalle = str_detalle & "1102" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                                '            str_detalle = str_detalle & "1103" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                                '            str_detalle = str_detalle & "1104" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                                '            str_detalle = str_detalle & "1105" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                                '            str_detalle = str_detalle & "1106" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                                '            str_detalle = str_detalle & "1107" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                                '            str_detalle = str_detalle & "1108" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                                '            str_detalle = str_detalle & "1109" & ","
                                '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                                '            str_detalle = str_detalle & "1110" & ","
                                '        End If

                                '    End If

                            Else
                                str_detalle = str_detalle & ","
                                str_detalle = str_detalle & ","
                            End If


                            'Cuerpo	Caracter 	X(1)
                            str_detalle = str_detalle & Mid(Trim(oDataRowTres("Voie")), 1, 1) & ","
                            'Número de evento 	Entero 	>>>>>>9
                            str_detalle = str_detalle & oDataRowTres("EVENT_NUMBER") & ","
                            'Número de folio 	Entero 	>>>>>>9 
                            '27_04
                            'str_detalle = str_detalle & oDataRow("FOLIO_ECT") & ","
                            'str_detalle = str_detalle & oDataRowTres("FOLIO_ECT") & ","
                            If oDataRowTres("FOLIO_ECT") = 0 Then
                                str_detalle = str_detalle & oDataRow("FOLIO_ECT") & ","
                            Else
                                str_detalle = str_detalle & oDataRowTres("FOLIO_ECT") & ","
                            End If


                            'Código de vehículo detectado ECT 	Caracter 	X(6)

                            str_pre = ""
                            str_det = ""
                            str_marc = ""
                            db_det = 0
                            db_det_ejes = 0

                            If Not IsDBNull(oDataRowTres("CLASE_DETECTADA")) Then

                                strClaseExcedente = ""
                                If Trim(oDataRowTres("CLASE_DETECTADA")) = "T01A" Then
                                    str_detalle = str_detalle & "T01" & ","
                                    str_det = "T01"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01M" Then
                                    str_detalle = str_detalle & "T01" & ","
                                    str_det = "T01"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01T" Then
                                    'str_detalle = str_detalle & "T01" & ","
                                    'T01,T => T09P01,C
                                    str_detalle = str_detalle & "T09P01" & ","
                                    str_det = "T09P01"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T02B" Then
                                    str_detalle = str_detalle & "T02" & ","
                                    str_det = "T02"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T03B" Then
                                    str_detalle = str_detalle & "T03" & ","
                                    str_det = "T03"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T04B" Then
                                    str_detalle = str_detalle & "T04" & ","
                                    str_det = "T04"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T02C" Then
                                    str_detalle = str_detalle & "T02" & ","
                                    str_det = "T02"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T03C" Then
                                    str_detalle = str_detalle & "T03" & ","
                                    str_det = "T03"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T04C" Then
                                    str_detalle = str_detalle & "T04" & ","
                                    str_det = "T04"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T05C" Then
                                    str_detalle = str_detalle & "T05" & ","
                                    str_det = "T05"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T06C" Then
                                    str_detalle = str_detalle & "T06" & ","
                                    str_det = "T06"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T07C" Then
                                    str_detalle = str_detalle & "T07" & ","
                                    str_det = "T07"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T08C" Then
                                    str_detalle = str_detalle & "T08" & ","
                                    str_det = "T08"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T09C" Then
                                    str_detalle = str_detalle & "T09" & ","
                                    str_det = "T09"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TL01A" Then
                                    str_detalle = str_detalle & "T01L01" & ","
                                    str_det = "T01L01"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TL02A" Then
                                    str_detalle = str_detalle & "T01L02" & ","
                                    str_det = "T01L02"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TLnnA" Then
                                    '04/12/2013
                                    'str_detalle = str_detalle & "T01Lnn" & ","
                                    'strClaseExcedente = "T01L"
                                    str_detalle = str_detalle & "T01L" & IIf(Len(oDataRowTres("ID_OBS_TT")) = 1, "0" & oDataRowTres("ID_OBS_TT"), oDataRowTres("ID_OBS_TT")) & ","
                                    str_det = "T01L" & IIf(Len(oDataRowTres("ID_OBS_TT")) = 1, "0" & oDataRowTres("ID_OBS_TT"), oDataRowTres("ID_OBS_TT"))
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "T01P" Then
                                    str_detalle = str_detalle & "T01P" & ","
                                    str_det = "T01P"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TP01C" Then
                                    str_detalle = str_detalle & "T09P01" & ","
                                    str_det = "T09P01"
                                ElseIf Trim(oDataRowTres("CLASE_DETECTADA")) = "TPnnC" Then
                                    '04/12/2013
                                    'str_detalle = str_detalle & "T09Pnn" & ","
                                    'strClaseExcedente = "T09P" & ","
                                    ' str_detalle = str_detalle & "T09P" & ","
                                    str_detalle = str_detalle & "T09P" & IIf(Len(oDataRowTres("ID_OBS_TT")) = 1, "0" & oDataRowTres("ID_OBS_TT"), oDataRowTres("ID_OBS_TT")) & ","
                                    str_det = "T09P" & IIf(Len(oDataRowTres("ID_OBS_TT")) = 1, "0" & oDataRowTres("ID_OBS_TT"), oDataRowTres("ID_OBS_TT"))
                                Else
                                    str_detalle = str_detalle & "No detectada" & ",0,"
                                    str_det = ""
                                End If

                            Else
                                str_detalle = str_detalle & ",0,"
                            End If

                            'Importe vehículo detectado ECT 	Decimal 	>>9.99 


                            strQuerys = "SELECT " &
                     "TYPE_PAIEMENT.libelle_paiement_L2 " &
                     ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
                     ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
                     ",Prix_Cl19, Prix_Cl20 " &
                     ",TYPE_PAIEMENT.libelle_paiement " &
                     ",TABLE_TARIF.CODE " &
                     "FROM TABLE_TARIF, " &
                     "TYPE_PAIEMENT " &
                     "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                            'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "

                            'borrar
                            strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRowTres("Version_Tarif") & " " &
                                "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                           "ORDER BY TABLE_TARIF.CODE "

                            ' strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5 " & _
                            '     "AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                            '"ORDER BY TABLE_TARIF.CODE "



                            If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                                If oDataRowTres("ACD_CLASS") > 0 And oDataRowTres("ACD_CLASS") <= 9 Then
                                    str_detalle = str_detalle & oDataRowTres("MONTO_DETECTADO") & ",,"
                                    db_det = oDataRowTres("MONTO_DETECTADO")
                                    db_det_ejes = 0

                                ElseIf oDataRowTres("ACD_CLASS") >= 12 And oDataRowTres("ACD_CLASS") <= 15 Then
                                    str_detalle = str_detalle & oDataRowTres("MONTO_DETECTADO") & ",,"
                                    db_det = oDataRowTres("MONTO_DETECTADO")
                                    db_det_ejes = 0
                                    'EXCEDENTES
                                ElseIf oDataRowTres("ACD_CLASS") >= 10 And oDataRowTres("ACD_CLASS") <= 11 Then
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                    db_det = oDataRowCuatro("Prix_Cl01")
                                    db_det_ejes = oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                                ElseIf oDataRowTres("ACD_CLASS") = 16 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    db_det = oDataRowCuatro("Prix_Cl09")
                                    db_det_ejes = oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")
                                ElseIf oDataRowTres("ACD_CLASS") = 17 Then
                                    'strClaseExcedente = "T01Lnn"
                                    'str_detalle = str_detalle & strClaseExcedente & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                    db_det = oDataRowCuatro("Prix_Cl01")
                                    db_det_ejes = oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                                ElseIf oDataRowTres("ACD_CLASS") = 18 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                    'la tomamos como la 16
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    db_det = oDataRowCuatro("Prix_Cl09")
                                    db_det_ejes = oDataRowTres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")
                                Else
                                    str_detalle = str_detalle & ",,"
                                End If

                            Else
                                str_detalle = str_detalle & ",,"
                            End If


                            'Importe eje excedente detectado ECT 	Decimal 	>9.99 
                            'Código de vehículo marcado C-R	Caracter 	X(6)
                            If Not IsDBNull(oDataRowTres("CLASE_MARCADA")) Then

                                strClaseExcedente = ""
                                strCodigoVhMarcado = ""
                                If Trim(oDataRowTres("CLASE_MARCADA")) = "T01A" Then
                                    str_detalle = str_detalle & "T01" & ",A,"
                                    str_marc = "T01"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T01M" Then
                                    str_detalle = str_detalle & "T01" & ",M,"
                                    str_marc = "T01M"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T01T" Then
                                    'str_detalle = str_detalle & "T01" & ",T,"
                                    'T01,T => T09P01,C
                                    str_detalle = str_detalle & "T09P01" & ",C,"
                                    str_marc = "T09P01"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T02B" Then
                                    str_detalle = str_detalle & "T02" & ",B,"
                                    str_marc = "T02"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T03B" Then
                                    str_detalle = str_detalle & "T03" & ",B,"
                                    str_marc = "T03"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T04B" Then
                                    str_detalle = str_detalle & "T04" & ",B,"
                                    str_marc = "T04"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T02C" Then
                                    str_detalle = str_detalle & "T02" & ",C,"
                                    str_marc = "T02"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T03C" Then
                                    str_detalle = str_detalle & "T03" & ",C,"
                                    str_marc = "T03"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T04C" Then
                                    str_detalle = str_detalle & "T04" & ",C,"
                                    str_marc = "T04"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T05C" Then
                                    str_detalle = str_detalle & "T05" & ",C,"
                                    str_marc = "T05"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T06C" Then
                                    str_detalle = str_detalle & "T06" & ",C,"
                                    str_marc = "T06"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T07C" Then
                                    str_detalle = str_detalle & "T07" & ",C,"
                                    str_marc = "T07"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T08C" Then
                                    str_detalle = str_detalle & "T08" & ",C,"
                                    str_marc = "T08"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T09C" Then
                                    str_detalle = str_detalle & "T09" & ",C,"
                                    str_marc = "T09"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TL01A" Then
                                    str_detalle = str_detalle & "T01L01" & ",A,"
                                    str_marc = "T01L01"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TL02A" Then
                                    str_detalle = str_detalle & "T01L02" & ",A,"
                                    str_marc = "T01L02"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TLnnA" Then
                                    'str_detalle = str_detalle & "T01Lnn" & ",A,"
                                    'strClaseExcedente = "T01L"
                                    'strCodigoVhMarcado = "A,"
                                    str_detalle = str_detalle & "T01L" & IIf(Len(oDataRowTres("CODE_GRILLE_TARIF")) = 1, "0" & oDataRowTres("CODE_GRILLE_TARIF"), oDataRowTres("CODE_GRILLE_TARIF")) & ",A,"
                                    str_marc = "T01L" & IIf(Len(oDataRowTres("CODE_GRILLE_TARIF")) = 1, "0" & oDataRowTres("CODE_GRILLE_TARIF"), oDataRowTres("CODE_GRILLE_TARIF"))
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "T01P" Then
                                    str_detalle = str_detalle & "T01P" & ",A,"
                                    str_marc = "T01P"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TP01C" Then
                                    str_detalle = str_detalle & "T09P01" & ",C,"
                                    str_marc = "T09P01"
                                ElseIf Trim(oDataRowTres("CLASE_MARCADA")) = "TPnnC" Then
                                    'str_detalle = str_detalle & "T09Pnn" & ",C,"
                                    'strClaseExcedente = "T09P"
                                    'strCodigoVhMarcado = "C,"

                                    ' strClaseExcedente = "T09P,"
                                    ' strCodigoVhMarcado = strClaseExcedente & "C,"
                                    str_detalle = str_detalle & "T09P" & IIf(Len(oDataRowTres("CODE_GRILLE_TARIF")) = 1, "0" & oDataRowTres("CODE_GRILLE_TARIF"), oDataRowTres("CODE_GRILLE_TARIF")) & ",C,"
                                    str_marc = "T09P" & IIf(Len(oDataRowTres("CODE_GRILLE_TARIF")) = 1, "0" & oDataRowTres("CODE_GRILLE_TARIF"), oDataRowTres("CODE_GRILLE_TARIF"))
                                Else
                                    str_detalle = str_detalle & "No detectada" & ",0,"
                                End If

                            Else
                                str_detalle = str_detalle & ",0,"
                            End If

                            'Tipo de vehículo marcado C-R	Caracter 	X(1)
                            'Código de usuario pago marcado C-R	Caracter 	X(3)

                            If Trim(oDataRow("ID_PAIEMENT")) = 1 Then
                                'str_detalle = str_detalle & "NOR" & ","
                                strCodigoVhPagoMarcado = "NOR" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 2 Then
                                'str_detalle = str_detalle & "CRE" & ","
                                'strCodigoVhPagoMarcado = "CRE" & ","
                                'TRAMO CORTO
                                strCodigoVhPagoMarcado = "NOR" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 27 Then
                                'str_detalle = str_detalle & "VSC" & ","
                                strCodigoVhPagoMarcado = "VSC" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 9 Then
                                'str_detalle = str_detalle & "FCUR" & ","
                                strCodigoVhPagoMarcado = "FCUR" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 10 Then
                                'str_detalle = str_detalle & "RPI" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Then
                                'str_detalle = str_detalle & "Tag" & ","
                                strCodigoVhPagoMarcado = "TDC" & ","

                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                                'str_detalle = str_detalle & "Tag" & ","
                                strCodigoVhPagoMarcado = "TDD" & ","

                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                                'str_detalle = str_detalle & "IAV" & ","
                                strCodigoVhPagoMarcado = "IAV" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 13 Then
                                'str_detalle = str_detalle & "ELU" & ","
                                strCodigoVhPagoMarcado = "ELU" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 71 Then
                                'str_detalle = str_detalle & "RP1" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 72 Then
                                'str_detalle = str_detalle & "RP2" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 73 Then
                                'str_detalle = str_detalle & "RP3" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 74 Then
                                'str_detalle = str_detalle & "RP4" & ","
                                strCodigoVhPagoMarcado = "RPI" & ","
                            ElseIf Trim(oDataRow("ID_PAIEMENT")) = 75 Then
                                'str_detalle = str_detalle & "RP4" & ","
                                strCodigoVhPagoMarcado = "RSP" & ","
                            Else
                                'str_detalle = str_detalle & ","
                                strCodigoVhPagoMarcado = ","
                            End If



                            'Importe vehículo marcado C-R[1]	Decimal 	>>9.99
                            strQuerys = "SELECT " &
           "TYPE_PAIEMENT.libelle_paiement_L2 " &
           ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
           ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
           ",Prix_Cl19, Prix_Cl20 " &
           ",TYPE_PAIEMENT.libelle_paiement " &
           ",TABLE_TARIF.CODE " &
           "FROM TABLE_TARIF, " &
           "TYPE_PAIEMENT " &
           "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                            'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "


                            'borrar
                            strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRowTres("Version_Tarif") & " " &
                                "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                           "ORDER BY TABLE_TARIF.CODE "

                            '                        strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = 5" & _
                            '"AND CODE = " & oDataRow("ID_PAIEMENT") & " " & _
                            '"ORDER BY TABLE_TARIF.CODE "


                            If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then

                                If oDataRowTres("TAB_ID_CLASSE") > 0 And oDataRowTres("TAB_ID_CLASSE") <= 9 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowTres("MONTO_MARCADO") & ",,"
                                ElseIf oDataRowTres("TAB_ID_CLASSE") >= 12 And oDataRowTres("TAB_ID_CLASSE") <= 15 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowTres("MONTO_MARCADO") & ",,"
                                    'EXCEDENTES
                                ElseIf oDataRowTres("TAB_ID_CLASSE") >= 10 And oDataRowTres("TAB_ID_CLASSE") <= 11 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                                ElseIf oDataRowTres("TAB_ID_CLASSE") = 16 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09")) / 50)) & ","
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                                ElseIf oDataRowTres("TAB_ID_CLASSE") = 17 Then
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                    'str_detalle = str_detalle & strClaseExcedente & IIf(Len(CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) = 1, "0" & CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30), CStr((oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")) / 30)) & ","
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl01") & ","
                                ElseIf oDataRowTres("TAB_ID_CLASSE") = 18 Then
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowtres("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","

                                    'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                    'la tomamos como la 16
                                    str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRowTres("MONTO_MARCADO") - oDataRowCuatro("Prix_Cl09") & ","
                                    '
                                Else
                                    str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                    str_detalle = str_detalle & ",,"
                                End If

                            Else
                                str_detalle = str_detalle & strCodigoVhMarcado & strCodigoVhPagoMarcado
                                str_detalle = str_detalle & ",,"
                            End If



                            bl_sin_pre = False
                            If id_plaza_cobro = 108 Then
                                If Trim(oDataRow("Voie")) = "A14" Or Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "B14" Or Trim(oDataRow("Voie")) = "B10" Then
                                    bl_sin_pre = True
                                End If
                            End If


                            'determino si tengo pre
                            bl_det_a_pos = False
                            If id_plaza_cobro = 108 Then

                                If Trim(oDataRow("Voie")) = "A08" Or Trim(oDataRow("Voie")) = "A09" Or Trim(oDataRow("Voie")) = "B01" Then
                                    bl_det_a_pos = True
                                End If

                            End If

                            'determino si tengo pre
                            'tres marias
                            bl_sin_pre = False
                            If id_plaza_cobro = 109 Then

                                If Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "B02" Then
                                    bl_sin_pre = True
                                End If
                            End If


                            'PASO MORELOS
                            If id_plaza_cobro = 102 Then

                                If Trim(oDataRow("Voie")) = "A02" Or Trim(oDataRow("Voie")) = "B07" Then
                                    bl_det_a_pos = True
                                End If


                                If Trim(oDataRow("Voie")) = "A09" Or Trim(oDataRow("Voie")) = "B10" Then
                                    bl_sin_pre = True
                                End If

                            End If

                            'determino si tengo pre
                            '   ÁEROPUERTO()
                            bl_sin_pre = False
                            If id_plaza_cobro = 106 Then

                                If Trim(oDataRow("Voie")) = "B01" Or Trim(oDataRow("Voie")) = "B03" Or Trim(oDataRow("Voie")) = "A02" Or Trim(oDataRow("Voie")) = "A04" Then
                                    bl_sin_pre = True
                                End If
                            End If


                            'xochitepec
                            bl_sin_pre = False
                            If id_plaza_cobro = 105 Then

                                If Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "A02" Then
                                    bl_sin_pre = True
                                End If
                            End If

                            'Alpuyeca
                            bl_sin_pre = False
                            If id_plaza_cobro = 101 Then

                                If Trim(oDataRow("Voie")) = "A01" Or Trim(oDataRow("Voie")) = "A02" And Trim(oDataRow("Voie")) = "B03" Or Trim(oDataRow("Voie")) = "B04" Then
                                    bl_sin_pre = True
                                End If
                            End If


                            'PALO BLANCO
                            bl_sin_pre = False
                            If id_plaza_cobro = 103 Then

                                If Trim(oDataRow("Voie")) = "A09" Or Trim(oDataRow("Voie")) = "B01" Then
                                    bl_sin_pre = True
                                End If
                            End If

                            'LA VENTA
                            bl_sin_pre = False
                            If id_plaza_cobro = 104 Then

                                If Trim(oDataRow("Voie")) = "A08" Or Trim(oDataRow("Voie")) = "B01" Then
                                    bl_sin_pre = True
                                End If
                            End If
                            'Emiliano zapata 
                            bl_sin_pre = False
                            If id_plaza_cobro = 107 Then
                                bl_sin_pre = True
                            End If


                            '''''''''''''''''''''''''''''''

                            If bl_det_a_pos = True Then

                                str_detalle = str_detalle & str_det & ","
                                str_pre = str_det

                                str_detalle = str_detalle & db_det & "," & db_det_ejes

                            Else

                                If bl_sin_pre = False Then



                                    'PRECLASIFICADOS
                                    If Not IsDBNull(oDataRowTres("CLASE_PRE")) Then

                                        strClaseExcedente = ""
                                        If Trim(oDataRowTres("CLASE_PRE")) = "T01A" Then
                                            str_detalle = str_detalle & "T01" & ","
                                            str_pre = "T01"
                                            'c_cod_veh_ect = "T01"
                                            'c_tpo_veh_ect = "A"
                                            'c_ect_tpo_eje = "L"

                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T01M" Then
                                            str_detalle = str_detalle & "T01" & ","
                                            str_pre = "T01"
                                            'c_cod_veh_ect = "T01"
                                            'c_tpo_veh_ect = "M"
                                            'c_ect_tpo_eje = "L"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T01T" Then
                                            'str_detalle = str_detalle & "T01" & ","
                                            'T01,T => T09P01,C
                                            str_detalle = str_detalle & "T09P01" & ","
                                            str_pre = "T09P01"
                                            'c_cod_veh_ect = "T09P01"
                                            'c_tpo_veh_ect = "A"
                                            'c_ect_tpo_eje = "L"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T02B" Then
                                            str_detalle = str_detalle & "T02" & ","
                                            str_pre = "T02"
                                            'c_cod_veh_ect = "T02"
                                            'c_tpo_veh_ect = "B"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T03B" Then
                                            str_detalle = str_detalle & "T03" & ","
                                            str_pre = "T03"
                                            'c_cod_veh_ect = "T03"
                                            'c_tpo_veh_ect = "B"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T04B" Then
                                            str_detalle = str_detalle & "T04" & ","
                                            str_pre = "T04"
                                            'c_cod_veh_ect = "T04"
                                            'c_tpo_veh_ect = "B"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T02C" Then
                                            str_detalle = str_detalle & "T02" & ","
                                            str_pre = "T02"
                                            'c_cod_veh_ect = "T02"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T03C" Then
                                            str_detalle = str_detalle & "T03" & ","
                                            str_pre = "T03"
                                            'c_cod_veh_ect = "T03"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T04C" Then
                                            str_detalle = str_detalle & "T04" & ","
                                            str_pre = "T04"
                                            'c_cod_veh_ect = "T04"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T05C" Then
                                            str_detalle = str_detalle & "T05" & ","
                                            str_pre = "T05"
                                            'c_cod_veh_ect = "T05"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T06C" Then
                                            str_detalle = str_detalle & "T06" & ","
                                            str_pre = "T06"
                                            'c_cod_veh_ect = "T06"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T07C" Then
                                            str_detalle = str_detalle & "T07" & ","
                                            str_pre = "T07"
                                            'c_cod_veh_ect = "T07"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T08C" Then
                                            str_detalle = str_detalle & "T08" & ","
                                            str_pre = "T08"
                                            'c_cod_veh_ect = "T08"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T09C" Then
                                            str_detalle = str_detalle & "T09" & ","
                                            str_pre = "T09"
                                            'c_cod_veh_ect = "T09"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "TL01A" Then
                                            str_detalle = str_detalle & "T01L01" & ","
                                            str_pre = "T01L01"
                                            'c_cod_veh_ect = "T01L01"
                                            'c_tpo_veh_ect = "A"
                                            'c_ect_tpo_eje = "L"
                                            'c_ect_cant_eje = 1
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "TL02A" Then
                                            str_detalle = str_detalle & "T01L02" & ","
                                            str_pre = "T01L02"
                                            'c_cod_veh_ect = "T01L02"
                                            'c_tpo_veh_ect = "A"
                                            'c_ect_tpo_eje = "L"
                                            'c_ect_cant_eje = 2
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "TLnnA" Then
                                            '04/12/2013
                                            'str_detalle = str_detalle & "T01Lnn" & ","
                                            'strClaseExcedente = "T01L"
                                            'str_detalle = str_detalle & "T01L" & IIf(Len(odatarowtres("PRE_EJES_EX")) = 1, "0" & odatarowtres("PRE_EJES_EX"), odatarowtres("PRE_EJES_EX")) & ","

                                            If oDataRowTres("PRE_EJES_EX") <> 0 Then
                                                str_detalle = str_detalle & "T01L" & IIf(Len(oDataRowTres("PRE_EJES_EX")) = 1, "0" & oDataRowTres("PRE_EJES_EX"), oDataRowTres("PRE_EJES_EX")) & ","
                                                str_pre = "T01L" & IIf(Len(oDataRowTres("PRE_EJES_EX")) = 1, "0" & oDataRowTres("PRE_EJES_EX"), oDataRowTres("PRE_EJES_EX"))
                                            Else
                                                str_detalle = str_detalle & "T01L08" & ","
                                                str_pre = "T01L08"
                                            End If
                                            'c_cod_veh_ect = "T01L" & IIf(Len(oDataRowTres("PRE_EJES_EX")) = 1, "0" & oDataRowTres("PRE_EJES_EX"), oDataRowTres("PRE_EJES_EX"))
                                            'c_tpo_veh_ect = "A"
                                            'c_ect_tpo_eje = "L"
                                            'c_ect_cant_eje = oDataRowTres("PRE_EJES_EX")
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "T01P" Then
                                            str_detalle = str_detalle & "T01P" & ","
                                            str_pre = "T01P"
                                            'c_cod_veh_ect = "T01P"
                                            'c_tpo_veh_ect = "A"
                                            'c_ect_tpo_eje = "L"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "TP01C" Then
                                            str_detalle = str_detalle & "T09P01" & ","
                                            str_pre = "T09P01"
                                            'c_cod_veh_ect = "T09P01"
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                        ElseIf Trim(oDataRowTres("CLASE_PRE")) = "TPnnC" Then
                                            '04/12/2013
                                            'str_detalle = str_detalle & "T09Pnn" & ","
                                            'strClaseExcedente = "T09P" & ","
                                            'str_detalle = str_detalle & "T09P" & ","
                                            'str_detalle = str_detalle & "T09P" & IIf(Len(odatarowtres("ID_OBS_TT")) = 1, "0" & odatarowtres("ID_OBS_TT"), odatarowtres("ID_OBS_TT")) & ","
                                            If oDataRowTres("PRE_EJES_EX") <> 0 Then
                                                str_detalle = str_detalle & "T09P" & IIf(Len(oDataRowTres("PRE_EJES_EX")) = 1, "0" & oDataRowTres("PRE_EJES_EX"), oDataRowTres("PRE_EJES_EX")) & ","
                                                str_pre = "T09P" & IIf(Len(oDataRowTres("PRE_EJES_EX")) = 1, "0" & oDataRowTres("PRE_EJES_EX"), oDataRowTres("PRE_EJES_EX"))
                                            Else
                                                str_detalle = str_detalle & "T01L08" & ","
                                                str_pre = "T01L08"
                                            End If

                                            'c_cod_veh_ect = "T09P" & IIf(Len(oDataRowTres("PRE_EJES_EX")) = 1, "0" & oDataRowTres("PRE_EJES_EX"), oDataRowTres("PRE_EJES_EX"))
                                            'c_tpo_veh_ect = "C"
                                            'c_ect_tpo_eje = "P"
                                            'c_ect_cant_eje = oDataRowTres("PRE_EJES_EX")
                                        Else
                                            str_detalle = str_detalle & "T01L08" & ","
                                            str_pre = "T01L08"
                                            'str_detalle = str_detalle & "No detectada" & ","

                                            'c_cod_veh_ect = "0"
                                            'c_tpo_veh_ect = "0"
                                            'c_ect_tpo_eje = "0"
                                            'c_ect_cant_eje = 0

                                        End If

                                    Else

                                        str_detalle = str_detalle & "T01L08" & ","
                                        str_pre = "T01L08"
                                        'str_detalle = str_detalle & ","

                                        'c_cod_veh_ect = "0"
                                        'c_tpo_veh_ect = "0"
                                        'c_ect_tpo_eje = "0"
                                        'c_ect_cant_eje = 0

                                    End If


                                    strQuerys = "SELECT " &
                                                "TYPE_PAIEMENT.libelle_paiement_L2 " &
                    ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " &
            ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " &
            ",Prix_Cl19, Prix_Cl20 " &
            ",TYPE_PAIEMENT.libelle_paiement " &
            ",TABLE_TARIF.CODE " &
            "FROM TABLE_TARIF, " &
            "TYPE_PAIEMENT " &
            "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) "


                                    'strQuerys = strQuerys & "AND TABLE_TARIF.Id_Gare = '" & int_id_gare & "' "


                                    strQuerys = strQuerys & "AND TABLE_TARIF.Version_Tarif = " & oDataRowTres("Version_Tarif") & " " &
                                        "AND CODE = " & oDataRow("ID_PAIEMENT") & " " &
                                   "ORDER BY TABLE_TARIF.CODE "

                                    If objQuerys.QueryDataSetCuatro(strQuerys, "TABLE_TARIF") = 1 Then



                                        If oDataRow("ID_CLASE_PRE") = 1 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 2 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl02") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 3 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl03") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 4 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl04") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 5 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl05") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 6 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl06") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 7 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl07") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 8 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl08") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 9 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ","

                                        ElseIf oDataRow("ID_CLASE_PRE") = 12 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl12") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 13 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl13") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 14 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl14") & ","
                                        ElseIf oDataRow("ID_CLASE_PRE") = 15 Then
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl15") & ","

                                            'EXCEDENTES
                                        ElseIf oDataRow("ID_CLASE_PRE") = 10 Then
                                            'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                            'c_imp_eje_ect = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                                            str_detalle = str_detalle & 1 * oDataRowCuatro("Prix_Cl17")

                                        ElseIf oDataRow("ID_CLASE_PRE") = 11 Then
                                            'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01") & ","
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                            'c_imp_eje_ect = oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl01")
                                            str_detalle = str_detalle & 2 * oDataRowCuatro("Prix_Cl17")

                                        ElseIf oDataRow("ID_CLASE_PRE") = 16 Then
                                            If IsNumeric(oDataRow("PRE_EJES_EX")) Then
                                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ","
                                                str_detalle = str_detalle & oDataRow("PRE_EJES_EX") * oDataRowCuatro("Prix_Cl16")
                                            Else
                                                str_detalle = str_detalle & "0,"
                                                str_detalle = str_detalle & "0"
                                            End If

                                        ElseIf oDataRow("ID_CLASE_PRE") = 17 Then
                                            'strClaseExcedente = "T01Lnn"

                                            If IsNumeric(oDataRow("PRE_EJES_EX")) Then
                                                str_detalle = str_detalle & oDataRowCuatro("Prix_Cl01") & ","
                                                str_detalle = str_detalle & oDataRow("PRE_EJES_EX") * oDataRowCuatro("Prix_Cl17")
                                            Else
                                                str_detalle = str_detalle & "0,"
                                                str_detalle = str_detalle & "0"
                                            End If

                                        ElseIf oDataRow("ID_CLASE_PRE") = 18 Then
                                            'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                            'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ",50,"
                                            'la tomamos como la 16
                                            'str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & "," & oDataRow("MONTO_DETECTADO") - oDataRowCuatro("Prix_Cl09") & ","
                                            str_detalle = str_detalle & oDataRowCuatro("Prix_Cl09") & ","
                                            str_detalle = str_detalle & 1 * oDataRowCuatro("Prix_Cl16")
                                        Else
                                            str_detalle = str_detalle & ",,"
                                        End If

                                    Else
                                        str_detalle = str_detalle & ",,"
                                    End If


                                End If

                                'FIN PRE CLASIFICADOS
                            End If







                            'Importe eje excedente marcado C-R 	Decimal 	>9.99 
                            'Número de tarjeta Pagos Electrónicos[2]	Caracter 	X(20)
                            ' str_detalle = str_detalle & Trim(oDataRowTres("CONTENU_ISO")) & ","
                            'lo elimino para solo madar el campo cuando es tag
                            'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & "," & ","

                            'Situación de la tarjeta Pagos Electrónicos	Caracter 	X(1)
                            'If Trim(oDataRow("ID_PAIEMENT")) = 15 Then
                            '    'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO"))) & ","

                            '    '27_04
                            '    'tag_iag = IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("CONTENU_ISO")))
                            '    tag_iag = IIf(Trim(oDataRowTres("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRowTres("CONTENU_ISO")))

                            '    tag_iag = Trim(Mid(tag_iag, 1, 16))

                            '    str_detalle = str_detalle & tag_iag & ","

                            '    'If IsNumeric(tag_iag) Then

                            '    '    tarjeta = Trim(tag_iag)

                            '    '    If Len(tarjeta) <> 11 Then

                            '    '        'si es menor a 4000 verifico si existe 003
                            '    '        If CDbl(tarjeta) <= 4000 Then

                            '    '            strQuerysTag = "SELECT roll FROM roll WHERE roll = " & CDbl(tarjeta)

                            '    '            If objQuerys_SqlServer.QueryDataSet_SqlServerDos(strQuerysTag, "roll") = 1 Then

                            '    '                'si esta en la lista le pongo el 099
                            '    '                tarjeta = "099" & tarjeta.PadLeft(8, "0")

                            '    '            Else
                            '    '                'si no esta en la lista le pongo el 003
                            '    '                tarjeta = "003" & tarjeta.PadLeft(8, "0")

                            '    '            End If

                            '    '        ElseIf IsNumeric(tarjeta) >= 16000000 Then
                            '    '            tarjeta = "003" & tarjeta.PadLeft(8, "0")

                            '    '        Else
                            '    '            tarjeta = "099" & tarjeta.PadLeft(8, "0")
                            '    '            'no es menor a 4000 meto 099
                            '    '        End If

                            '    '    End If

                            '    '    str_detalle = str_detalle & tarjeta & ","
                            '    'Else
                            '    '    str_detalle = str_detalle & tag_iag & ","
                            '    'End If







                            '    str_detalle = str_detalle & "V" & ","

                            '    str_detalle = str_detalle & ","
                            '    str_detalle = str_detalle & ","

                            'ElseIf Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then


                            '    'str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID").PadLeft(16, "*")) & ","
                            '    '27_04
                            '    'str_detalle = str_detalle & Trim(oDataRow("ISSUER_ID")) & ","
                            '    str_detalle = str_detalle & Trim(oDataRowTres("ISSUER_ID")) & ","
                            '    str_detalle = str_detalle & "V" & ","
                            '    'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) & ","

                            '    '27_04
                            '    'If IsNumeric(Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6)) Then
                            '    '    str_detalle = str_detalle & Mid(Trim(oDataRow("CONTENU_ISO")), 1, 6) & ","
                            '    'Else
                            '    '    str_detalle = str_detalle & "0,"
                            '    'End If

                            '    If IsNumeric(Mid(Trim(oDataRowTres("CONTENU_ISO")), 1, 6)) Then
                            '        str_detalle = str_detalle & Mid(Trim(oDataRowTres("CONTENU_ISO")), 1, 6) & ","
                            '    Else
                            '        str_detalle = str_detalle & "0,"
                            '    End If

                            '    '27_04
                            '    'str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                            '    str_detalle = str_detalle & Format(oDataRowTres("DATE_TRANSACTION"), "dd/MM/yyyy") & ","

                            'Else
                            '    '27_04
                            '    'str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                            '    str_detalle = str_detalle & IIf(Trim(oDataRowTres("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") & ","
                            '    str_detalle = str_detalle & ","

                            '    str_detalle = str_detalle & ","
                            '    str_detalle = str_detalle & ","
                            'End If



                            'If Trim(oDataRow("ID_PAIEMENT")) = 12 Or Trim(oDataRow("ID_PAIEMENT")) = 14 Then
                            '    str_detalle = str_detalle & IIf(Trim(oDataRow("CONTENU_ISO")) = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", Trim(oDataRow("ISSUER_ID").PadLeft(16, "*"))) & ","
                            '    str_detalle = str_detalle & Format(oDataRow("DATE_TRANSACTION"), "dd/MM/yyyy") & ","
                            'Else
                            '    str_detalle = str_detalle & ","
                            '    str_detalle = str_detalle & ","
                            'End If



                        End If
                        'fin clase detectada


                    End If

                    'cont

                    If bl_sin_pre = False Then

                        If str_pre <> str_det Or str_det <> str_marc Or str_marc <> str_pre Then

                            ReDim Preserve info(cont_info)

                            info(cont_info) = str_detalle
                            'oSW.WriteLine(str_detalle)
                            cont_info = cont_info + 1
                        Else

                        End If

                    Else

                        If str_det <> str_marc Then

                            ReDim Preserve info(cont_info)

                            info(cont_info) = str_detalle
                            'oSW.WriteLine(str_detalle)
                            cont_info = cont_info + 1


                        End If


                    End If



                Next


            Else

                cabecera = cabecera & "00000"
                oSW.WriteLine(cabecera)
                oSW.Flush()
            End If


            'fin detalle

            'Write

            'If info IsNot Nothing Then





            If info IsNot Nothing Then
                'dbl_registros = info.Count - menos

                dbl_registros = cont_info


                If Len(CStr(dbl_registros)) = 1 Then
                    no_registros = "0000" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 2 Then
                    no_registros = "000" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 3 Then
                    no_registros = "00" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 4 Then
                    no_registros = "0" & dbl_registros
                ElseIf Len(CStr(dbl_registros)) = 5 Then
                    no_registros = dbl_registros
                End If


                cabecera = cabecera & no_registros

                oSW.WriteLine(cabecera)

                'detalle
                For Each detalle As String In info
                    If InStr(detalle, "-") <= 1 Then

                        oSW.WriteLine(detalle)
                    End If
                Next

                oSW.Flush()
                oSW.Close()
                ProgressBar1.Value = ProgressBar1.Value + 20

            End If


        Catch ex As Exception
            MsgBox(ex.Message.ToString, MsgBoxStyle.Critical)
        End Try

    End Sub

    Private Sub btnAutomatico_Click(sender As Object, e As EventArgs) Handles btnAutomatico.Click
        automatico()
    End Sub

    Private Sub automatico()

        Dim int_dia_inicial As Integer
        Dim int_dia_final As Integer = 142


        If cmbDelegacion.Text = "" Then
            MsgBox("Favor de seleccionar la delegación", MsgBoxStyle.Information, "Verificar")
            Exit Sub
        End If

        If cmbPlazaCobro.SelectedValue = Nothing Then
            MsgBox("Favor de seleccionar la plaza de cobro", MsgBoxStyle.Information, "Verificar")
            Exit Sub
        End If

        If cmbTurnoBlo.Text = "" Then
            MsgBox("Favor de seleccionar el turno", MsgBoxStyle.Information, "Verificar")
            Exit Sub
        End If

        If Not IsNumeric(txtDias.Text) Then
            MsgBox("Favor de seleccionar el n´mero de dias", MsgBoxStyle.Information, "Verificar")
            Exit Sub
        End If

        If CDbl(txtDias.Text) = 0 Or CDbl(txtDias.Text) > 96 Then
            MsgBox("Favor de seleccionar el numero de dias", MsgBoxStyle.Information, "Verificar")
            Exit Sub
        End If

        dt_Fecha_Inicio = CDate(Format(dtpFechaInicio.Value, "MM/dd/yyyy") & " 00:00:00") '"09/02/2014"
        int_dia_final = CDbl(txtDias.Text) * 3


        '22:00 - 06:00
        '06:00 - 14:00
        '14:00 - 22:00

        For int_dia_inicial = 0 To int_dia_final


            '--------------------------------------------------------------
            objControl.limpia_Catalogos()

            str_Plaza_Cobro = "1" & lblPlazaCobro.Text
            id_plaza_cobro = CInt("1" & Trim(CStr(cmbPlazaCobro.SelectedValue)))

            'dt_Fecha_Inicio = dtpFechaInicio.Value
            'dt_Fecha_Fin = dtpFechaFin.Value

            str_delegacion = cmbDelegacion.Text

            str_Turno_block = Trim(cmbTurnoBlo.Text)

            archivo_1 = ""
            archivo_2 = ""
            archivo_3 = ""
            archivo_4 = ""
            archivo_5 = ""

            generar_bitacora_operacion()
            Preliquidaciones_de_cajero_receptor_para_transito_vehicular()
            eventos_detectados_y_marcados_en_el_ECT()
            eventos_detectados_y_marcados_en_el_ECT_EAP()
            registro_usuarios_telepeaje()
            encriptar()
            Comprimir()
            '--------------------------------------------------------------


            If cmbTurnoBlo.Text = "22:00 - 06:00" Then
                cmbTurnoBlo.Text = "06:00 - 14:00"
            ElseIf cmbTurnoBlo.Text = "06:00 - 14:00" Then
                cmbTurnoBlo.Text = "14:00 - 22:00"
            ElseIf cmbTurnoBlo.Text = "14:00 - 22:00" Then
                cmbTurnoBlo.Text = "22:00 - 06:00"
                dt_Fecha_Inicio = DateAdd(DateInterval.Day, 1, dt_Fecha_Inicio)
            End If
        Next

        MsgBox("Exportación Terminada", MsgBoxStyle.Information, "Exportación")

    End Sub

    '    Private Sub NoNacionalCarril()

    '        Dim strQuerys As String
    '        Dim cont2
    '        'CARRILES CERRADOS DOS
    '        'SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD;

    '        '************************************************
    '        '************************************************
    '        strQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD "

    '        If id_plaza_cobro = 106 Then
    '            strQuerys = strQuerys & "where VOIE <> 'B04' and VOIE <> 'A03' "
    '        End If

    '        If objQuerys.QueryDataSetCuatro(strQuerys, "SEQ_VOIE_TOD") = 1 Then

    '            For cont2 = 0 To oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Count - 1

    '                oDataRowCuatro = oDataSetCuatro.Tables("SEQ_VOIE_TOD").Rows.Item(cont2)


    '                strQuerys = "SELECT	* FROM 	FIN_POSTE " &
    '                        "WHERE	VOIE = '" & oDataRowCuatro("VOIE") & "' " &
    '                        "AND ((DATE_DEBUT_POSTE >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
    '                        "AND (DATE_DEBUT_POSTE <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) "

    '                If objQuerys.QueryDataSet(strQuerys, "FIN_POSTE") = 0 Then


    '                    strQuerys = "SELECT * " &
    '"FROM CLOSED_LANE_REPORT, SITE_GARE " &
    '"where " &
    '"CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " &
    ' "AND	SITE_GARE.id_Site		=	'" & Mid(id_plaza_cobro, 2, 2) & "' " &
    ' "AND	LANE		=	'" & oDataRowCuatro("VOIE") & "' " &
    ' "AND ((BEGIN_DHM >= TO_DATE('" & Format(h_inicio_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS')) " &
    '"AND (BEGIN_DHM <= TO_DATE('" & Format(h_fin_turno, "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS'))) " &
    '"order by BEGIN_DHM"

    '                    If objQuerys.QueryDataSet(strQuerys, "CLOSED_LANE_REPORT") = 0 Then




    '                        str_detalle = ""
    '                        'Fecha base de operación 	Fecha 	dd/mm/aaaa
    '                        str_detalle = Format(dt_Fecha_Inicio, "dd/MM/yyyy") & ","
    '                        'Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
    '                        str_detalle = str_detalle & int_turno & ","
    '                        'Hora inicial de operación 	Caracter 	hhmmss 	
    '                        str_detalle = str_detalle & Format(h_inicio_turno, "HHmmss") & ","
    '                        'Hora final de operación 	Caracter 	hhmmss 	
    '                        'str_detalle = str_detalle & Format(h_fin_turno, "HHmmss") & ","
    '                        str_detalle = str_detalle & Format(DateAdd(DateInterval.Second, 1, h_fin_turno), "HHmmss") & ","
    '                        '                        ''Número de carril 	Entero 	>>9	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
    '                        If id_plaza_cobro = 184 Then
    '                            str_detalle = str_detalle & "247" & ","

    '                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "2585" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "2586" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
    '                                str_detalle = str_detalle & "2587" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
    '                                str_detalle = str_detalle & "2588" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
    '                                str_detalle = str_detalle & "2589" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
    '                                str_detalle = str_detalle & "2590" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
    '                                str_detalle = str_detalle & "2591" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
    '                                str_detalle = str_detalle & "2592" & ","
    '                            End If

    '                            'paso morelos
    '                        ElseIf id_plaza_cobro = 102 Then

    '                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "249" & ","
    '                                str_detalle = str_detalle & "1803" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1804" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1805" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1806" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1807" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1808" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1809" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
    '                                str_detalle = str_detalle & "261" & ","
    '                                str_detalle = str_detalle & "1810" & ","
    '                                '-------------------------------------------------
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1811" & ","

    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
    '                                str_detalle = str_detalle & "250" & ","
    '                                str_detalle = str_detalle & "1812" & ","
    '                            End If

    '                            'la venta
    '                        ElseIf id_plaza_cobro = 104 Then
    '                            str_detalle = str_detalle & "252" & ","

    '                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "1830" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "1831" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
    '                                str_detalle = str_detalle & "1832" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
    '                                str_detalle = str_detalle & "1833" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
    '                                str_detalle = str_detalle & "1834" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
    '                                str_detalle = str_detalle & "1835" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
    '                                str_detalle = str_detalle & "1836" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
    '                                str_detalle = str_detalle & "1837" & ","

    '                            End If

    '                        ElseIf id_plaza_cobro = 103 Then
    '                            str_detalle = str_detalle & "251" & ","

    '                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "1816" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "1817" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
    '                                str_detalle = str_detalle & "1818" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
    '                                str_detalle = str_detalle & "1819" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "5" Then
    '                                str_detalle = str_detalle & "1820" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "6" Then
    '                                str_detalle = str_detalle & "1821" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "7" Then
    '                                str_detalle = str_detalle & "1822" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "8" Then
    '                                str_detalle = str_detalle & "1823" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "9" Then
    '                                str_detalle = str_detalle & "1824" & ","
    '                            End If

    '                            'álpuyeca
    '                            '101
    '                            '246
    '                            '1	1794
    '                            '2	1795
    '                            '3	1796
    '                            '4	1797
    '                        ElseIf id_plaza_cobro = 101 Then
    '                            str_detalle = str_detalle & "246" & ","

    '                            If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "1794" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "1795" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
    '                                str_detalle = str_detalle & "1796" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
    '                                str_detalle = str_detalle & "1797" & ","
    '                            End If


    '                            'aeropuerto
    '                            '106
    '                            '1		367	2734	B
    '                            '2		366	2735	A
    '                            '3		367	2736	B
    '                            '4		366	2737	A
    '                        ElseIf id_plaza_cobro = 106 Then
    '                            If Mid(Trim(oDataRowCuatro("VOIE")), 1, 1) = "A" Then

    '                                str_detalle = str_detalle & "366" & ","

    '                                If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "2" Then
    '                                    str_detalle = str_detalle & "2735" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "4" Then
    '                                    str_detalle = str_detalle & "2737" & ","
    '                                End If

    '                            ElseIf Mid(Trim(oDataRowCuatro("VOIE")), 1, 1) = "B" Then

    '                                str_detalle = str_detalle & "367" & ","

    '                                If CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "1" Then
    '                                    str_detalle = str_detalle & "2734" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("VOIE")), 2, 2)) = "3" Then
    '                                    str_detalle = str_detalle & "2736" & ","
    '                                End If

    '                            End If

    '                            'tlalpan
    '                        ElseIf id_plaza_cobro = 108 Then

    '                            str_detalle = str_detalle & "118" & ","

    '                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
    '                                str_detalle = str_detalle & "3076" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "3063" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "3064" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
    '                                str_detalle = str_detalle & "3065" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
    '                                str_detalle = str_detalle & "3066" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
    '                                str_detalle = str_detalle & "3067" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
    '                                str_detalle = str_detalle & "3068" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
    '                                str_detalle = str_detalle & "3069" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
    '                                str_detalle = str_detalle & "3070" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
    '                                str_detalle = str_detalle & "1010" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
    '                                str_detalle = str_detalle & "1011" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
    '                                str_detalle = str_detalle & "1012" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
    '                                str_detalle = str_detalle & "1013" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
    '                                str_detalle = str_detalle & "3075" & ","
    '                            End If

    '                            'xochitepec
    '                        ElseIf id_plaza_cobro = 105 Then

    '                            str_detalle = str_detalle & "365" & ","

    '                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "2727" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "2728" & ","
    '                            End If

    '                            'tres marias
    '                        ElseIf id_plaza_cobro = 109 Then

    '                            str_detalle = str_detalle & "102" & ","

    '                            If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
    '                                str_detalle = str_detalle & "1020" & ","
    '                            ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
    '                                str_detalle = str_detalle & "1021" & ","
    '                            End If



    '                            'SAN MARCOS
    '                        ElseIf id_plaza_cobro = 107 Then

    '                            str_detalle = str_detalle & "121" & ","

    '                            If Mid(Trim(oDataRowCuatro("Voie")), 1, 1) = "A" Then

    '                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
    '                                    str_detalle = str_detalle & "1102" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
    '                                    str_detalle = str_detalle & "1103" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
    '                                    str_detalle = str_detalle & "1104" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
    '                                    str_detalle = str_detalle & "1105" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
    '                                    str_detalle = str_detalle & "1106" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
    '                                    str_detalle = str_detalle & "1107" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
    '                                    str_detalle = str_detalle & "1108" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
    '                                    str_detalle = str_detalle & "1109" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
    '                                    str_detalle = str_detalle & "1110" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "15" Then
    '                                    str_detalle = str_detalle & "1101" & ","
    '                                End If

    '                            ElseIf Mid(Trim(oDataRowCuatro("Voie")), 1, 1) = "B" Then

    '                                If CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "1" Then
    '                                    str_detalle = str_detalle & "1097" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "2" Then
    '                                    str_detalle = str_detalle & "1098" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "3" Then
    '                                    str_detalle = str_detalle & "1099" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "4" Then
    '                                    str_detalle = str_detalle & "1100" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "5" Then
    '                                    str_detalle = str_detalle & "1101" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "6" Then
    '                                    str_detalle = str_detalle & "1102" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "7" Then
    '                                    str_detalle = str_detalle & "1103" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "8" Then
    '                                    str_detalle = str_detalle & "1104" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "9" Then
    '                                    str_detalle = str_detalle & "1105" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "10" Then
    '                                    str_detalle = str_detalle & "1106" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "11" Then
    '                                    str_detalle = str_detalle & "1107" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "12" Then
    '                                    str_detalle = str_detalle & "1108" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "13" Then
    '                                    str_detalle = str_detalle & "1109" & ","
    '                                ElseIf CInt(Mid(Trim(oDataRowCuatro("Voie")), 2, 2)) = "14" Then
    '                                    str_detalle = str_detalle & "1110" & ","
    '                                End If

    '                            End If



    '                        Else
    '                            str_detalle = str_detalle & ","
    '                            str_detalle = str_detalle & ","
    '                        End If


    '                        'Cuerpo 	Caracter 	X(1)	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
    '                        str_detalle = str_detalle & Mid(Trim(oDataRowCuatro("VOIE")), 1, 1) & ","

    '                        'Identificador de operación	Caracter 	X(2)	Valores posibles:  Tabla 17 - Códigos de Operación por Carril.
    '                        str_detalle = str_detalle & "X" & Mid(Trim(oDataRowCuatro("VOIE")), 1, 1) & ","


    '                        If Trim(strEncargadoTurno) = "" Then
    '                            strEncargadoTurno = "encargado_plaza"
    '                        End If
    '                        'No. empleado C-R 	Entero 	>>>>>9	
    '                        str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", strEncargadoTurno) & ","
    '                        'No. empleado encargado de turno 	Entero 	>>>>>9 	
    '                        str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", strEncargadoTurno) & ","
    '                        'No. empleado Admón. Gral. 	Entero 	>>>>>9 	
    '                        str_detalle = str_detalle & objControl.LeeINI(Application.StartupPath & "\operadores.ini", "operadores", "encargado_plaza") & ","
    '                        'No. de control de preliquidación  	Entero 	>>>9 	
    '                        str_detalle = str_detalle & ","

    '                        oSW.WriteLine(str_detalle)
    '                        '----------------------



    '                    End If


    '                End If

    '            Next

    '        End If
    '        '************************************************
    '        '************************************************

    '        'FIN CARRILES CERRADO DOS
    '    End Sub

    Private Sub ConversionNoCarril()
        Dim str_detalle_tc As String
        str_detalle_tc = str_detalle
        If id_plaza_cobro = 184 Then
            str_detalle = str_detalle & "247" & ","

            If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then
                str_detalle_tc = str_detalle_tc & "340" & ","
            End If

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "2585" & ","
                str_detalle_tc = str_detalle_tc & "2585" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "2586" & ","
                str_detalle_tc = str_detalle_tc & "2586" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "2587" & ","
                str_detalle_tc = str_detalle_tc & "2587" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "2588" & ","
                str_detalle_tc = str_detalle_tc & "2588" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "2589" & ","
                str_detalle_tc = str_detalle_tc & "2589" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "2590" & ","
                str_detalle_tc = str_detalle_tc & "2590" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "2591" & ","
                str_detalle_tc = str_detalle_tc & "2591" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "2592" & ","
                str_detalle_tc = str_detalle_tc & "2592" & ","
            End If

            'paso morelos
        ElseIf id_plaza_cobro = 102 Then

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "249" & ","
                str_detalle = str_detalle & "1803" & ","
                str_detalle_tc = str_detalle_tc & "261" & ","
                str_detalle_tc = str_detalle_tc & "1803" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1804" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1805" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1806" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1807" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1808" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1809" & ","

            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "249" & ","
                str_detalle = str_detalle & "1810" & ","

                str_detalle_tc = str_detalle_tc & "261" & ","
                str_detalle_tc = str_detalle_tc & "1810" & ","

                '--------------------------------------------
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1811" & ","

            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                str_detalle = str_detalle & "250" & ","
                str_detalle = str_detalle & "1812" & ","
            End If


            'la venta
        ElseIf id_plaza_cobro = 104 Then
            str_detalle = str_detalle & "252" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "1830" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "1831" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "1832" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "1833" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "1834" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "1835" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "1836" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "1837" & ","
            End If

        ElseIf id_plaza_cobro = 161 Then


            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "364" & "2681" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "364" & "2682" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "363" & "2683" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "363" & "2684" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "364" & "2685" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "364" & "2686" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "363" & "2687" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "363" & "2688" & ","
            End If

        ElseIf id_plaza_cobro = 103 Then
            str_detalle = str_detalle & "251" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "1816" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "1817" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "1818" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "1819" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "1820" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "1821" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "1822" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "1823" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                str_detalle = str_detalle & "1824" & ","
            End If

            'álpuyeca
        ElseIf id_plaza_cobro = 101 Then
            str_detalle = str_detalle & "246" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "1794" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "1795" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "1796" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "1797" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "1798" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "1799" & ","
            End If

            'álpuyeca
            '101


            'aeropuerto
            '106
            '1		367	2734	B
            '2		366	2735	A
            '3		367	2736	B
            '4		366	2737	A


            'tlalpan
        ElseIf id_plaza_cobro = 108 Then

            str_detalle = str_detalle & "118" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                str_detalle = str_detalle & "3076" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "3063" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "3064" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "3065" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "3066" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "3067" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "3068" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "3069" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "3070" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                str_detalle = str_detalle & "3071" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                str_detalle = str_detalle & "3072" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                str_detalle = str_detalle & "3073" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                str_detalle = str_detalle & "3074" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                str_detalle = str_detalle & "3075" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                str_detalle = str_detalle & "3077" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                str_detalle = str_detalle & "3078" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "17" Then
                str_detalle = str_detalle & "3079" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "18" Then
                str_detalle = str_detalle & "3080" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "19" Then
                str_detalle = str_detalle & "3081" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "20" Then
                str_detalle = str_detalle & "3082" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                str_detalle = str_detalle & "3083" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                str_detalle = str_detalle & "3084" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                str_detalle = str_detalle & "3085" & ","
            End If

            'xochitepec
        ElseIf id_plaza_cobro = 105 Then

            str_detalle = str_detalle & "365" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "2727" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "2728" & ","
            End If

            'CERRO GORDO
        ElseIf id_plaza_cobro = 186 Then

            str_detalle = str_detalle & "351" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "21" Then
                str_detalle = str_detalle & "3199" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "22" Then
                str_detalle = str_detalle & "3200" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "23" Then
                str_detalle = str_detalle & "3201" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "24" Then
                str_detalle = str_detalle & "3202" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "25" Then
                str_detalle = str_detalle & "3203" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                'str_detalle = str_detalle & "3185" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                'str_detalle = str_detalle & "3186" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                'str_detalle = str_detalle & "3187" & ","
                'ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                'str_detalle = str_detalle & "3188" & ","
            End If

            'Queretaro
        ElseIf id_plaza_cobro = 106 Then
            str_detalle = str_detalle & "112" & ","
            'Segmento B
            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "1079" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "1080" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "1081" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "1082" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "1083" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "1084" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "1085" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "1086" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                str_detalle = str_detalle & "1087" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                str_detalle = str_detalle & "1088" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                str_detalle = str_detalle & "1089" & ","
            End If
            'VillaGrand
        ElseIf id_plaza_cobro = 183 Then

            str_detalle = str_detalle & "170" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "2581" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "2582" & ","
            End If
            'tres marias
        ElseIf id_plaza_cobro = 109 Then

            str_detalle = str_detalle & "102" & ","

            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "1020" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "1021" & ","
            End If
            'Central de Abastos
        ElseIf id_plaza_cobro = 107 Then
            str_detalle = str_detalle & "368" & ","
            'Segmento B
            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "1843" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "1844" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "1845" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "1846" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "1847" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "1848" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "1849" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
                str_detalle = str_detalle & "1850" & ","
                'Segmento A
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
                str_detalle = str_detalle & "1851" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
                str_detalle = str_detalle & "1852" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
                str_detalle = str_detalle & "1853" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
                str_detalle = str_detalle & "1854" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
                str_detalle = str_detalle & "2743" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
                str_detalle = str_detalle & "2744" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
                str_detalle = str_detalle & "2745" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "16" Then
                str_detalle = str_detalle & "2746" & ","
            End If
        ElseIf id_plaza_cobro = 189 Then
            str_detalle = str_detalle & "189" & ","
            'Segmento B
            If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
                str_detalle = str_detalle & "1891" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
                str_detalle = str_detalle & "1892" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
                str_detalle = str_detalle & "1893" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
                str_detalle = str_detalle & "1894" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
                str_detalle = str_detalle & "1895" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
                str_detalle = str_detalle & "1896" & ","
            ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
                str_detalle = str_detalle & "1897" & ","
            End If
            '    'SAN MARCOS
            'ElseIf id_plaza_cobro = 107 Then

            '    str_detalle = str_detalle & "121" & ","

            '    If Mid(Trim(oDataRow("Voie")), 1, 1) = "A" Then

            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
            '            str_detalle = str_detalle & "1102" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
            '            str_detalle = str_detalle & "1103" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
            '            str_detalle = str_detalle & "1104" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
            '            str_detalle = str_detalle & "1105" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
            '            str_detalle = str_detalle & "1106" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
            '            str_detalle = str_detalle & "1107" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
            '            str_detalle = str_detalle & "1108" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
            '            str_detalle = str_detalle & "1109" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
            '            str_detalle = str_detalle & "1110" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "15" Then
            '            str_detalle = str_detalle & "1101" & ","
            '        End If

            '    ElseIf Mid(Trim(oDataRow("Voie")), 1, 1) = "B" Then

            '        If CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "1" Then
            '            str_detalle = str_detalle & "1097" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "2" Then
            '            str_detalle = str_detalle & "1098" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "3" Then
            '            str_detalle = str_detalle & "1099" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "4" Then
            '            str_detalle = str_detalle & "1100" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "5" Then
            '            str_detalle = str_detalle & "1101" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "6" Then
            '            str_detalle = str_detalle & "1102" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "7" Then
            '            str_detalle = str_detalle & "1103" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "8" Then
            '            str_detalle = str_detalle & "1104" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "9" Then
            '            str_detalle = str_detalle & "1105" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "10" Then
            '            str_detalle = str_detalle & "1106" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "11" Then
            '            str_detalle = str_detalle & "1107" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "12" Then
            '            str_detalle = str_detalle & "1108" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "13" Then
            '            str_detalle = str_detalle & "1109" & ","
            '        ElseIf CInt(Mid(Trim(oDataRow("Voie")), 2, 2)) = "14" Then
            '            str_detalle = str_detalle & "1110" & ","
            '        End If

            '    End If


        Else
            str_detalle = str_detalle & ","
            str_detalle = str_detalle & ","
        End If
    End Sub


End Class

