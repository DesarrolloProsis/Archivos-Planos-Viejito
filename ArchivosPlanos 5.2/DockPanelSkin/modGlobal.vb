Imports System.Data
Imports Oracle.DataAccess.Client
Imports System.Data.SqlClient

Module modGlobal

    '--- Declaraciones para leer ficheros INI ---
    ' Leer todas las secciones de un fichero INI, esto seguramente no funciona en Win2000
    ' Esta función no estaba en las declaraciones del API que se incluye con el VB
    Public Declare Function GetPrivateProfileSectionNames Lib "kernel32" Alias "GetPrivateProfileSectionNamesA" (ByVal lpszReturnBuffer As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    ' Leer una sección completa
    Public Declare Function GetPrivateProfileSection Lib "kernel32" Alias "GetPrivateProfileSectionA" (ByVal lpAppName As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    ' Leer una clave de un fichero INI
    Public Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Public Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As Integer, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    ' Escribir una clave de un fichero INI (también para borrar claves y secciones)
    Public Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer
    Public Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As Integer, ByVal lpFileName As String) As Integer
    Public Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As Integer, ByVal lpString As Integer, ByVal lpFileName As String) As Integer

    'creo objeto para la conexión
    'Public oConexion As New OdbcConnection
    Public oConexion As OracleConnection = New OracleConnection

    Public gl_time_querys As Date
    Public gl_bol_querys As Boolean
    Public errores As Boolean
    Public gl_DNS As String
    Public dir_archivo_errores
    'bandera para no impiar el dataset
    Public glblClearDataSet As Boolean
    Public str_detalle As String
    Public oDataSet As New DataSet
    Public oDataRow As DataRow
    Public ArchivoZip As String
    'para solicitar la tabla en el data set
    Public glstrTabla As String


    '*-*-*-*-*-*-*-*-*-*
    '*.-.-.-.-variable contrato
    Public gloDataSetContrato As New Dataset
    Public glbloDataSetContrato As Boolean

    Public gloDataSetSeis As New DataSet
    Public glbloDataSetSeis As Boolean
    Public oDataRowSeis As DataRow
    Public dvRecolector As DataView

    Public glDataSetAlacen As New DataSet


    Public iPosicionFilaActual As Integer
    Public iPosicionFilaActual_2 As Integer


    Public oDataSetDos As New DataSet
    Public oDataRowDos As DataRow
    Public iPosicionFilaActualDos As Integer

    Public oDataSetTres As New DataSet
    Public oDataRowTres As DataRow
    Public iPosicionFilaActualTres As Integer

    Public oDataSetCuatro As New DataSet
    Public oDataRowCuatro As DataRow
    Public iPosicionFilaActualCuatro As Integer

    Public oDataSetTarifa As New DataSet
    Public oDataRowTarifa As DataRow
    Public iPosicionFilaActualTarifa As Integer

    Public oDataSetSiete As New DataSet
    Public oDataRowSiete As DataRow
    Public iPosicionFilaActualSiete As Integer


    '-----------------
    Public oDataSetOcho As New DataSet
    Public oDataRowOcho As DataRow
    Public iPosicionFilaActualOcho As Integer

    Public oDataSetNueve As New DataSet
    Public oDataRowNueve As DataRow
    Public iPosicionFilaActualNueve As Integer
    '-----------------

    Public oDataSetCTC As New DataSet
    Public oDataRowCTC As DataRow
    Public iPosicionFilaActualCTC As Integer

    Public glblDataSetAlmacen As Boolean


    'estructura de inicio
    Public Structure ES_usuarioInfo
        Public id_usuario As String
        Public nombre As String
        Public teminal As String
        Public puesto As String
        Public seguridad_padre_pantalla As String
        Public seguridad_pantalla As String
        Public prefijo_perfil As Int16
        Public prefijo_letra As Int16
        Public terminal_analisis As Int16
        Public terminal_compactas As Int16
        Public origen_universal As Int16

    End Structure

    'datos generales del usuario
    Public usuarioInfo As ES_usuarioInfo

    'bool inicio programa
    Public glblValidacionPwd As Boolean


    '''***Public oConexion As New SqlConnection
    Public objQuerys As New clsQuerys





    'variables inicio
    Public gl_jefe_cuenta As Int16


    '+-+-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    Public arreglo_datos_cliente(0) As String

    Public arreglo_agregar_producto(0, 0) As String

    Public arreglo_control_productos(0) As Int16

  

    '+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-++-+-+-+--+
    Public giintUsuarioMaestroVentas As Double

    'variables login
    Public glintUsuario As Integer
    Public glstrUsuario As String
    Public glstrMail As String

    Public glstrClaveUsuario As String
    Public glstrPWDUsuario As String

    Public glintTerminal As Integer
    Public glstrTerminal As String
    Public glintPlanta As Integer
    Public glstrPlanta As String
    Public glintPuesto As Integer
    Public glstrPuesto As String
    Public glstrSeguridadEmpresas As String
    Public glintEmpresaActiva As Integer

    Public objControl As New clsControl


    'variable cerrar conexion
    Public glblCerrarConexion As Boolean

    'Variables de catalogo
    Public intLinea As Integer

    Public strLinea As String
    Public strLinea1 As String
    Public strLinea2 As String
    Public strLinea3 As String
    Public strLinea4 As String
    Public strLinea5 As String
    Public strLinea6 As String
    Public strLinea7 As String
    Public strLinea8 As String
    Public strLinea9 As String
    Public strLinea10 As String
    Public strLinea11 As String
    Public strLinea12 As String
    Public strLinea13 As String

    Public strLinea14 As String
    Public strLinea15 As String
    Public strLinea16 As String
    Public strLinea17 As String
    Public strLinea18 As String
    Public strLinea19 As String
    Public strLinea20 As String

    Public strLinea_tag As String
    Public strLinea_tag1 As String
    Public strLinea_tag2 As String
    Public strLinea_tag3 As String
    Public strLinea_tag4 As String
    Public strLinea_tag5 As String
    Public strLinea_tag6 As String
    Public strLinea_tag7 As String
    Public strLinea_tag8 As String
    Public strLinea_tag9 As String
    Public strLinea_tag10 As String
    Public strLinea_tag11 As String
    Public strLinea_tag12 As String
    Public strLinea_tag13 As String
    Public strLinea_tag14 As String
    Public strLinea_tag15 As String
    Public strLinea_tag16 As String
    Public strLinea_tag17 As String
    Public strLinea_tag18 As String
    Public strLinea_tag19 As String
    Public strLinea_tag20 As String
    Public strLinea_tag21 As String
    Public strLinea_tag22 As String
    Public strLinea_tag23 As String
    Public strLinea_tag24 As String
    Public strLinea_tag25 As String
    Public strLinea_tag26 As String
    Public strLinea_tag27 As String
    Public strLinea_tag28 As String
    Public strLinea_tag29 As String
    Public strLinea_tag30 As String
    Public strLinea_tag31 As String
    Public strLinea_tag32 As String
    Public strLinea_tag33 As String
    Public strLinea_tag34 As String
    Public strLinea_tag35 As String

    'tamaño de pantalla anterior
    Public glintWidthAnt As Integer
    Public glintHeigtAnt As Integer

    'hora servidor
    Public gbdtHoraServidor As Date
    Public gbdtHoraMeridiano As Double = 1

    Public glstrUbicacion As String

    Public glArrayObs(1, 1) As String
    Public glintContObs As Int16


    'pantallas



    'contrato
    Public glClienteMex_Lat As Integer

    Public glarMaestroContratoInicioFinContrato(0) As String
    Public glarMaestroContratoInicioFinPeriodo(0, 0) As String
    Public glarMaestroContratoCanales_Monto(0, 0, 0) As String
    Public glarMaestroContrato_GarantizadoSubscriptores_CuotaFija(0, 0) As String

    Public glstrPalabraClave As String
    Public glbitPalabraClave As String

    'arreglo visualizar columans listview
    Public gbarrColumansLtv(0, 0) As String



    Public PosicionFilaActual As Integer

    Public arregloVersionBecalos(0, 0) As String

    Public bl_01_800 As Boolean

    Public bl_tablas_almacenamiento As Boolean


    'bandera para observaciones
    Public glTipoObs As String

    'asendente desendente list view
    Public glintAscDesLtv As Int16 = 1


    ' me dice que tipo de dirreccion
    Public gl_int_tipo_cliente As Integer


    'me dice el ajuste de horario del servidor
    Public gl_ajuste_horario As Int16

    Public gl_int_periodo_facturado As Integer
    Public gl_int_contrato_dias As Integer

    Public gl_int_periodo_facturado_04_12_2010 As Integer
    Public gl_int_periodo_facturado_31_10_2011 As Integer
    Public gl_int_periodo_facturado_31_12_2011 As Integer
    Public gl_int_periodo_facturado_31_03_2012 As Integer
    Public gl_int_periodo_facturado_31_10_2012 As Integer


    Public gl_int_contrato_dias_04_12_2010 As Integer
    Public gl_int_contrato_dias_31_10_2011 As Integer
    Public gl_int_contrato_dias_31_12_2011 As Integer
    Public gl_int_contrato_dias_31_03_2012 As Integer
    Public gl_int_contrato_dias_31_10_2012 As Integer

    'listas
    Public gl_hieght_int As Integer
    Public gl_width_int As Integer
    Public gl_bl_camibo_tamaño As Boolean



    Public str_Plaza_Cobro As String
    Public id_plaza_cobro As String

    Public str_Cajero_Receptor As String
    Public id_Cajero_Receptor As String

    Public id_Turno As String
    Public str_Turno_block As String

    Public dt_Fecha_Inicio As Date
    Public dt_Fecha_Fin As Date

    Public str_delegacion As String

    Public gl_turno_1 As String = "22:00 - 06:00"
    Public gl_turno_2 As String = "06:00 - 14:00"
    Public gl_turno_3 As String = "14:00 - 22:00"

    Public gl_bl_bolsa_automatica As Boolean = False
    Public gl_str_bolsa_automatica As String


    'sql server
    Public objQuerys_SqlServer As New clsQuerysSqlServer

    Public oConexion_SqlServer As SqlConnection = New SqlConnection


    Public gl_DNS_SqlServer As String
    Public glblClearDataSet_SqlServer As Boolean

    Public oDataSet_SqlServer As New DataSet
    Public oDataRow_SqlServer As DataRow

    Public glstrTabla_SqlServer As String

    Public iPosicionFilaActual_SqlServer As Integer
    Public iPosicionFilaActual_2_SqlServer As Integer

    Public glDataSetAlacen_SqlServer As New DataSet
    Public glblDataSetAlmacen_SqlServer As Boolean



    Public oDataSet_SqlServerDos As New DataSet
    Public oDataRow_SqlServerDos As DataRow

    Public glstrTabla_SqlServerDos As String

    Public iPosicionFilaActual_SqlServerDos As Integer
    Public iPosicionFilaActual_2_SqlServerDos As Integer

    Public glDataSetAlacen_SqlServerDos As New DataSet
    Public glblDataSetAlmacen_SqlServerDos As Boolean


End Module
