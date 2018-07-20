Imports Oracle.DataAccess.Client
Imports System.Data

Public Class clsQuerys

    Dim conn As OracleConnection
    Dim data As DataTable
    Dim da As OracleDataAdapter
    Dim cb As OracleCommandBuilder


    Public Function QueryDataSet(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try

reintentar:

            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSet.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.SelectCommand.CommandTimeout = 10000
            oDataAdapter.Fill(oDataSet, strClsTabla)


            If glblDataSetAlmacen = True Then
                oDataAdapter.Fill(glDataSetAlacen, strClsTabla)

                glblDataSetAlmacen = False
            End If

            oDataAdapter = Nothing

            iPosicionFilaActual = 0
            oDataRow = Nothing

            If glblDataSetAlmacen = False Then

                If oDataSet.Tables(strClsTabla).Rows.Count > 0 Then
                    oDataRow = oDataSet.Tables(strClsTabla).Rows(iPosicionFilaActual)
                    QueryDataSet = 1
                Else
                    QueryDataSet = 0


                End If

            End If

            gl_time_querys = Now

        Catch myerror As Exception

            'MessageBox.Show(strClsQuery)

            'objControl.ControlaErrores("QueryDataSet", "clsQuerys", Err.Number)

            GoTo reintentar
        End Try

    End Function


    Public Function QueryDataSetDos(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try

reintentar:

            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSetDos.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.SelectCommand.CommandTimeout = 100000
            oDataAdapter.Fill(oDataSetDos, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualDos = 0
            oDataRowDos = Nothing

            If oDataSetDos.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowDos = oDataSetDos.Tables(strClsTabla).Rows(iPosicionFilaActualDos)
                QueryDataSetDos = 1
            Else
                QueryDataSetDos = 0
            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function


    Public Function QueryDataSetTres(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try

reintentar:

            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSetTres.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.Fill(oDataSetTres, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualTres = 0
            oDataRowTres = Nothing

            If oDataSetTres.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowTres = oDataSetTres.Tables(strClsTabla).Rows(iPosicionFilaActualTres)
                QueryDataSetTres = 1
            Else
                QueryDataSetTres = 0

            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function

    Public Function SQLEjecutar(ByVal strInsert As String) As Int16

        Try



reintentar:



            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            Dim oComando As New OracleCommand(strInsert, oConexion)


            If oConexion.State = ConnectionState.Closed Then

                oConexion.Open()
            End If

            oComando.ExecuteNonQuery()

            oComando = Nothing

            gl_time_querys = Now

            SQLEjecutar = 1

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("Error, favor de contactar con el departamento de sistemas", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If
        End Try
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub


    Public Function QueryDataSetCuatro(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try
reintentar:

            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSetCuatro.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.SelectCommand.CommandTimeout = 100000
            oDataAdapter.Fill(oDataSetCuatro, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualCuatro = 0
            oDataRowCuatro = Nothing

            If oDataSetCuatro.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowCuatro = oDataSetCuatro.Tables(strClsTabla).Rows(iPosicionFilaActualCuatro)
                QueryDataSetCuatro = 1
            Else
                QueryDataSetCuatro = 0

            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function


    Public Function QueryDataSetSeis(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try



reintentar:

            'Public gloDataSetSeis As New DataSet
            'Public glbloDataSetSeis As Boolean


            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                gloDataSetSeis.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.Fill(gloDataSetSeis, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualCuatro = 0
            oDataRowSeis = Nothing

            If gloDataSetSeis.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowSeis = gloDataSetSeis.Tables(strClsTabla).Rows(iPosicionFilaActualCuatro)
                QueryDataSetSeis = 1
            Else
                QueryDataSetSeis = 0

            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function



    Public Function QueryDataSetSiete(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try


reintentar:

            'Public gloDataSetSiete As New DataSet
            'Public glbloDataSetSiete As Boolean


            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSetSiete.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.Fill(oDataSetSiete, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualCuatro = 0
            oDataRowSiete = Nothing

            If oDataSetSiete.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowSiete = oDataSetSiete.Tables(strClsTabla).Rows(iPosicionFilaActualCuatro)
                QueryDataSetSiete = 1
            Else
                QueryDataSetSiete = 0

            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function

    'ocho
    Public Function QueryDataSetOcho(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try


reintentar:

            'Public gloDataSetOcho As New DataSet
            'Public glbloDataSetOcho As Boolean


            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSetOcho.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.Fill(oDataSetOcho, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualCuatro = 0
            oDataRowOcho = Nothing

            If oDataSetOcho.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowOcho = oDataSetOcho.Tables(strClsTabla).Rows(iPosicionFilaActualCuatro)
                QueryDataSetOcho = 1
            Else
                QueryDataSetOcho = 0

            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function

    'nueve
    Public Function QueryDataSetNueve(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try


reintentar:

            'Public gloDataSetNueve As New DataSet
            'Public glbloDataSetNueve As Boolean


            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSetNueve.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.Fill(oDataSetNueve, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualCuatro = 0
            oDataRowNueve = Nothing

            If oDataSetNueve.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowNueve = oDataSetNueve.Tables(strClsTabla).Rows(iPosicionFilaActualCuatro)
                QueryDataSetNueve = 1
            Else
                QueryDataSetNueve = 0

            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function



    Public Function QueryDataSetTarifa(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try
reintentar:

            If oConexion.State = ConnectionState.Closed Then
                oConexion.ConnectionString = gl_DNS
                oConexion.Open()
            End If

            If glblClearDataSet = False Then
                oDataSetTarifa.Clear()
            Else
                glblClearDataSet = False
            End If

            Dim oDataAdapter = New OracleDataAdapter(strClsQuery, oConexion)

            oDataAdapter.SelectCommand.CommandTimeout = 100000
            oDataAdapter.Fill(oDataSetTarifa, strClsTabla)

            oDataAdapter = Nothing

            iPosicionFilaActualTarifa = 0
            oDataRowTarifa = Nothing

            If oDataSetTarifa.Tables(strClsTabla).Rows.Count > 0 Then
                oDataRowTarifa = oDataSetTarifa.Tables(strClsTabla).Rows(iPosicionFilaActualTarifa)
                QueryDataSetTarifa = 1
            Else
                QueryDataSetTarifa = 0

            End If

            gl_time_querys = Now

        Catch oExcep As Exception

            If InStr(oExcep.Message, "Access denied for user:") > 0 Then
                MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
                MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                If Err.Number = 5 Then
                    GoTo reintentar
                End If

                MessageBox.Show("" & vbCrLf & oExcep.Message, "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End Try

    End Function




End Class
