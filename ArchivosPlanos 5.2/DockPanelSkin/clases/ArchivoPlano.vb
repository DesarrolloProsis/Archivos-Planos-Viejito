Public Class ArchivoPlano

    Public Function validaArchPlano1(registro As String, nlinea As Integer) As List(Of String)
        ' /* Formato de Archivo de Bitacora
        ' 1.- fecha base----->dd/mm/aaaa
        ' 2.- Numero turno--->Entero
        ' 3.- Hora inicial de operacion-->hhmmss
        ' 4.- Hora final de operacion-->hhmmss
        ' 5.- Clave de tramo-->Entero
        ' 6.- Numero de carril-->Entero
        ' 7.- Cuerpo------->Caracter
        ' 8.- Identificador de operación--->Caracter
        ' 9.- No. empleado C-R------------->Entero
        ' 10.- No. empleado encargado de turno --->Entero
        ' 11.- No. empleado Admón. Gral. -->Entero
        ' 12.- No. de control de preliquidación2-->Entero
        ' */
        'Validar el numero de preliquidación si se encuentra y que el carril haya sido abierto
        Dim lsterrores As List(Of String) = New List(Of String)()
        Dim result() As String
        result = registro.Split(","c)
        'Valida que el campo del N° de Preliquidación no este vacio.
        If result(11).ToString() = "" AndAlso (result(7).ToString() = "NA" OrElse result(7).ToString() = "NB") Then
            Dim errore As String = "Error en el archivo de 1 en la linea: " & nlinea.ToString() & ", el carril se encuentra  abierto  y no cuenta con un N°Preliquidación en la posición 12."
            lsterrores.Add(errore)


        Else
            If result(11).ToString() <> "" AndAlso (result(7).ToString() = "XA" OrElse result(7).ToString() = "XB") Then
                Dim errore As String = "Error en el archivo de 1 en la linea: " & nlinea.ToString() & ", el carril se encuentra  cerrado con un N°Preliquidación en la posición 8."
                lsterrores.Add(errore)

            End If

        End If

        Dim contadorTem As Integer = 1

        For Each registroTemp As String In result
            If contadorTem < 13 Then
                'Valida que en todo el registro no se encuentre la leyenda de "Pendiente"
                If registroTemp = "Pendiente" Then
                    Dim errore As String = "Error en el archivo de 1 en la linea: " & nlinea.ToString() & ",  contiene la leyenda de PENDIENTE en la posición" & contadorTem & "."
                    lsterrores.Add(errore)

                End If
                'Validas que ningun campo venga vacio a exepción
                If registroTemp = "" AndAlso contadorTem <> 12 Then
                    Dim errore As String = "Error en el archivo de 1 en la linea: " & nlinea.ToString() & ",  valor Vacio o en blanco en la posición " & contadorTem & "."
                    lsterrores.Add(errore)

                End If
                contadorTem += 1

            End If

        Next



        Return lsterrores


    End Function

    Public Function validaArchPlano2(registro As String, nlinea As Integer) As List(Of String)
        Dim lsterrores As List(Of String) = New List(Of String)()
        Dim result() As String
        result = registro.Split(","c)

        'Valida si el carril esta abierto o cerrado
        If result(7).ToString().Length = 7 Then

            'Valida el primer rollos de folios en las posciciones (53,54)
            If 0 > (Integer.Parse(result(53).ToString()) - Integer.Parse(result(52).ToString())) Then
                Dim errore As String = "Error en el archivo de 2 en la linea: " & nlinea.ToString() & ", el folio final es menor que el inicial del primer rollo en la posición 53 y 54."
                lsterrores.Add(errore)

            Else
                If 999999 < (Integer.Parse(result(53).ToString()) - Integer.Parse(result(52).ToString())) Then
                    Dim errore As String = "error en el archivo de 2 en la linea: " & nlinea.ToString() & ", la diferencia entre los folios del primer rollo excede el limite de 999,999 en la posición 53 y 54."
                    lsterrores.Add(errore)

                End If


            End If

            'Valida el segundo rollos de folios en las posciciones (56,57)
            If 0 > (Integer.Parse(result(56).ToString()) - Integer.Parse(result(55).ToString())) Then
                Dim errore As String = "Error en el archivo de 2 en la linea: " & nlinea.ToString() & ", el folio final es menor que el inicial del primer rollo en la posición 56 y 57."
                lsterrores.Add(errore)

            Else
                If 999999 < (Integer.Parse(result(56).ToString()) - Integer.Parse(result(55).ToString())) Then
                    Dim errore As String = "Error en el archivo de 2 en la linea: " & nlinea.ToString() & ", la diferencia entre los folios del primer rollo excede el limite de 999,999 en la posición 56 y 57."
                    lsterrores.Add(errore)

                End If


            End If

            'Valida el primer rollos de folios en las posciciones (59,60)
            If 0 > (Integer.Parse(result(59).ToString()) - Integer.Parse(result(58).ToString())) Then
                Dim errore As String = "Error en el archivo de 2 en la linea: " & nlinea.ToString() & ", el folio final es menor que el inicial del primer rollo en la posición 59 y 60."
                lsterrores.Add(errore)

            Else
                If 999999 < (Integer.Parse(result(59).ToString()) - Integer.Parse(result(58).ToString())) Then
                    Dim errore As String = "Error en el archivo de 2 en la linea: " & nlinea.ToString() & ", la diferencia entre los folios del primer rollo excede el limite de 999,999 en la posición 59 y 60."
                    lsterrores.Add(errore)

                End If


            End If

        End If
        Return lsterrores

    End Function

    Public Function validaArchPlano3(registro As String, nlinea As Integer) As List(Of String)
        Dim lsterrores As List(Of String) = New List(Of String)()
        Dim result() As String
        result = registro.Split(","c)
        'Valida que la clase detectada sea diferente de cero
        If result(8).ToString() = "0" Then
            Dim errore As String = "Error en el archivo de 9 en la linea: " & nlinea.ToString() & ", la clase detectada es cero en la posición 9."
            lsterrores.Add(errore)

        End If
        'Valida si hay algun signo negativo en todo el registro
        Dim contadorTem As Integer = 1
        For Each registroTemp As String In result
            Dim aa = registroTemp.IndexOf("-")
            If 0 = registroTemp.IndexOf("-") Then
                Dim errore As String = "Error en el archivo de 9 en la linea: " & nlinea.ToString() & ", se encontro un signo negativo en la posición " & contadorTem & "."
                lsterrores.Add(errore)

            End If

            contadorTem += 1

        Next
        'Validar que el numero de evento no se repita
        Return lsterrores

    End Function

    Public Function validaArchPlanoP(registro As String, nlinea As Integer) As List(Of String)
        Dim lsterrores As List(Of String) = New List(Of String)()
        Dim result() As String
        result = registro.Split(","c)
        'Valida que la clase detectada sea diferente de cero
        ' /*if (result[8].ToString() == "0")

        ' {
        ' string error = "Error en el archivo de 9 en la linea: " &  nlinea.ToString()  & ", la clase detectada es cero en la posición 9.";
        ' lsterrores.Add(errore);

        ' }
        ' */
        'Valida si hay algun signo negativo en todo el registro
        Dim contadorTem As Integer = 1
        For Each registroTemp As String In result
            Dim aa = registroTemp.IndexOf("-")
            If 0 = registroTemp.IndexOf("-") Then
                Dim errore As String = "Error en el archivo de P en la linea: " & nlinea.ToString() & ", se encontro un signo negativo en la posición " & contadorTem & "."
                lsterrores.Add(errore)

            End If

            contadorTem += 1

        Next
        'Validar que el numero de evento no se repita
        Return lsterrores

    End Function



End Class
