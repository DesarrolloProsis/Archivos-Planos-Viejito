Imports System.Data
Imports System.Data.SqlClient


Public Class clsQuerysSqlServer


    'Dim strConexionMySql As String
    'Dim conn As MySqlConnection
    'Dim data As DataTable
    'Dim da As MySqlDataAdapter
    'Dim cb As MySqlCommandBuilder

    Dim conn As SqlConnection
    Dim data As DataTable
    Dim da As SqlDataAdapter
    Dim cb As SqlCommandBuilder

    Public Sub New()

    End Sub

    Public Function QueryDataSet_SqlServer(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try

            Dim ban_querys_time As Boolean

reintentar:

            If gl_bol_querys = True Then
                gl_time_querys = Now
                gl_bol_querys = False
            End If



            If oConexion_SqlServer.State = ConnectionState.Closed Then
                oConexion_SqlServer.ConnectionString = gl_DNS_SqlServer
                oConexion_SqlServer.Open()
            End If

            If glblClearDataSet_SqlServer = False Then
                oDataSet_SqlServer.Clear()
            Else
                glblClearDataSet_SqlServer = False
            End If

            Dim oDataAdapter = New SqlDataAdapter(strClsQuery, oConexion_SqlServer)

            oDataAdapter.SelectCommand.CommandTimeout = 10000
            oDataAdapter.Fill(oDataSet_SqlServer, strClsTabla)


            If glblDataSetAlmacen_SqlServer = True Then
                oDataAdapter.Fill(glDataSetAlacen_SqlServer, strClsTabla)

                glblDataSetAlmacen_SqlServer = False
            End If

            oDataAdapter = Nothing

            iPosicionFilaActual_SqlServer = 0
            oDataRow_SqlServer = Nothing

            If glblDataSetAlmacen_SqlServer = False Then

                If oDataSet_SqlServer.Tables(strClsTabla).Rows.Count > 0 Then
                    oDataRow_SqlServer = oDataSet_SqlServer.Tables(strClsTabla).Rows(iPosicionFilaActual_SqlServer)
                    QueryDataSet_SqlServer = 1
                Else
                    QueryDataSet_SqlServer = 0


                End If

            End If

            gl_time_querys = Now

        Catch myerror As Exception

            'MessageBox.Show(strClsQuery)

            'objControl.ControlaErrores("QueryDataSet", "clsQuerys", Err.Number)
            'MsgBox(myerror.Message)


            GoTo reintentar
        End Try

    End Function



    Public Function QueryDataSet_SqlServerDos(ByVal strClsQuery As String, ByVal strClsTabla As String) As Integer

        Try
            'glstrTabla_SqlServe
            Dim ban_querys_time As Boolean

reintentar:

            If gl_bol_querys = True Then
                gl_time_querys = Now
                gl_bol_querys = False
            End If



            If oConexion_SqlServer.State = ConnectionState.Closed Then
                oConexion_SqlServer.ConnectionString = gl_DNS_SqlServer
                oConexion_SqlServer.Open()
            End If

            If glblClearDataSet_SqlServer = False Then
                oDataSet_SqlServerDos.Clear()
            Else
                glblClearDataSet_SqlServer = False
            End If

            Dim oDataAdapter = New SqlDataAdapter(strClsQuery, oConexion_SqlServer)

            oDataAdapter.SelectCommand.CommandTimeout = 10000
            oDataAdapter.Fill(oDataSet_SqlServerDos, strClsTabla)


            If glblDataSetAlmacen_SqlServer = True Then
                oDataAdapter.Fill(glDataSetAlacen_SqlServerDos, strClsTabla)

                glblDataSetAlmacen_SqlServer = False
            End If

            oDataAdapter = Nothing

            iPosicionFilaActual_SqlServerDos = 0
            oDataRow_SqlServerDos = Nothing

            If glblDataSetAlmacen_SqlServerDos = False Then

                If oDataSet_SqlServerDos.Tables(strClsTabla).Rows.Count > 0 Then
                    oDataRow_SqlServerDos = oDataSet_SqlServerDos.Tables(strClsTabla).Rows(iPosicionFilaActual_SqlServerDos)
                    QueryDataSet_SqlServerDos = 1
                Else
                    QueryDataSet_SqlServerDos = 0


                End If

            End If

            gl_time_querys = Now

        Catch myerror As Exception

            'MessageBox.Show(strClsQuery)

            'objControl.ControlaErrores("QueryDataSet", "clsQuerys", Err.Number)
            'MsgBox(myerror.Message)


            GoTo reintentar
        End Try

    End Function









    Public Function SQLEjecutar_SqlServer(ByVal strInsert As String) As Int16

        Try

            Dim ban_querys_time As Boolean

reintentar:



            If oConexion_SqlServer.State = ConnectionState.Closed Then
                oConexion_SqlServer.ConnectionString = gl_DNS_SqlServer
                oConexion_SqlServer.Open()
            End If

            Dim oComando As New SqlCommand(strInsert, oConexion_SqlServer)


            If oConexion_SqlServer.State = ConnectionState.Closed Then

                oConexion_SqlServer.Open()
            End If

            oComando.ExecuteNonQuery()

            oComando = Nothing

            gl_time_querys = Now

            SQLEjecutar_SqlServer = 1

        Catch oExcep As Exception

            'If InStr(oExcep.Message, "Access denied for user:") > 0 Then
            '    MessageBox.Show("Error al conectar con datos,verificar usuario o contraseña", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            'ElseIf InStr(oExcep.Message, "Can't connect to") > 0 Then
            '    MessageBox.Show("Error al conectar con la base de datos,verificar conexión a Internet", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            'Else
            'objControl.ControlaErrores("QueryDataSet", "clsQuerys", Err.Number)
            '    If Err.Number = 5 Then
            GoTo reintentar
            '    End If

            'MessageBox.Show("Error, favor de contactar con el departamento de sistemas", "Conectar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            'End If
        End Try
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub


End Class
