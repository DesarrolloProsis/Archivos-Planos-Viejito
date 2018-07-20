<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmR1
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblFecha2 = New System.Windows.Forms.Label()
        Me.lblEncargadoTurno = New System.Windows.Forms.Label()
        Me.lblTurno = New System.Windows.Forms.Label()
        Me.dtpFechaFin = New System.Windows.Forms.DateTimePicker()
        Me.txtEncargadoTurno = New System.Windows.Forms.TextBox()
        Me.cmbTurnoBlo = New System.Windows.Forms.ComboBox()
        Me.lblPlazaCobro = New System.Windows.Forms.Label()
        Me.btnGenerarReporte = New System.Windows.Forms.Button()
        Me.lblDelegacion = New System.Windows.Forms.Label()
        Me.tm_automatizacion = New System.Windows.Forms.Timer(Me.components)
        Me.btnAutomatico = New System.Windows.Forms.Button()
        Me.txtDias = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmbPlazaCobro = New System.Windows.Forms.ComboBox()
        Me.cmbDelegacion = New System.Windows.Forms.ComboBox()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.dtpFechaInicio = New System.Windows.Forms.DateTimePicker()
        Me.lblFecha1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblFecha2
        '
        Me.lblFecha2.AutoSize = True
        Me.lblFecha2.Location = New System.Drawing.Point(20, 169)
        Me.lblFecha2.Name = "lblFecha2"
        Me.lblFecha2.Size = New System.Drawing.Size(68, 12)
        Me.lblFecha2.TabIndex = 35
        Me.lblFecha2.Text = "Fecha Fin"
        '
        'lblEncargadoTurno
        '
        Me.lblEncargadoTurno.AutoSize = True
        Me.lblEncargadoTurno.Location = New System.Drawing.Point(20, 143)
        Me.lblEncargadoTurno.Name = "lblEncargadoTurno"
        Me.lblEncargadoTurno.Size = New System.Drawing.Size(131, 12)
        Me.lblEncargadoTurno.TabIndex = 33
        Me.lblEncargadoTurno.Text = "Encargado de Turno"
        '
        'lblTurno
        '
        Me.lblTurno.AutoSize = True
        Me.lblTurno.Location = New System.Drawing.Point(20, 117)
        Me.lblTurno.Name = "lblTurno"
        Me.lblTurno.Size = New System.Drawing.Size(40, 12)
        Me.lblTurno.TabIndex = 31
        Me.lblTurno.Text = "Turno"
        '
        'dtpFechaFin
        '
        Me.dtpFechaFin.CustomFormat = "MM/dd/yyyy HH:mm:ss"
        Me.dtpFechaFin.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpFechaFin.Location = New System.Drawing.Point(163, 163)
        Me.dtpFechaFin.Name = "dtpFechaFin"
        Me.dtpFechaFin.Size = New System.Drawing.Size(172, 20)
        Me.dtpFechaFin.TabIndex = 28
        '
        'txtEncargadoTurno
        '
        Me.txtEncargadoTurno.Location = New System.Drawing.Point(163, 135)
        Me.txtEncargadoTurno.Name = "txtEncargadoTurno"
        Me.txtEncargadoTurno.Size = New System.Drawing.Size(172, 20)
        Me.txtEncargadoTurno.TabIndex = 26
        '
        'cmbTurnoBlo
        '
        Me.cmbTurnoBlo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbTurnoBlo.FormattingEnabled = True
        Me.cmbTurnoBlo.Items.AddRange(New Object() {"22:00 - 06:00", "06:00 - 14:00", "14:00 - 22:00"})
        Me.cmbTurnoBlo.Location = New System.Drawing.Point(164, 109)
        Me.cmbTurnoBlo.Name = "cmbTurnoBlo"
        Me.cmbTurnoBlo.Size = New System.Drawing.Size(172, 20)
        Me.cmbTurnoBlo.TabIndex = 24
        '
        'lblPlazaCobro
        '
        Me.lblPlazaCobro.AutoSize = True
        Me.lblPlazaCobro.Location = New System.Drawing.Point(21, 91)
        Me.lblPlazaCobro.Name = "lblPlazaCobro"
        Me.lblPlazaCobro.Size = New System.Drawing.Size(103, 12)
        Me.lblPlazaCobro.TabIndex = 21
        Me.lblPlazaCobro.Text = "Plaza de Cobro"
        '
        'btnGenerarReporte
        '
        Me.btnGenerarReporte.Location = New System.Drawing.Point(110, 275)
        Me.btnGenerarReporte.Name = "btnGenerarReporte"
        Me.btnGenerarReporte.Size = New System.Drawing.Size(138, 21)
        Me.btnGenerarReporte.TabIndex = 20
        Me.btnGenerarReporte.Text = "&Exportar"
        Me.btnGenerarReporte.UseVisualStyleBackColor = True
        '
        'lblDelegacion
        '
        Me.lblDelegacion.AutoSize = True
        Me.lblDelegacion.Location = New System.Drawing.Point(21, 64)
        Me.lblDelegacion.Name = "lblDelegacion"
        Me.lblDelegacion.Size = New System.Drawing.Size(75, 12)
        Me.lblDelegacion.TabIndex = 18
        Me.lblDelegacion.Text = "Delegación"
        '
        'btnAutomatico
        '
        Me.btnAutomatico.Location = New System.Drawing.Point(22, 15)
        Me.btnAutomatico.Name = "btnAutomatico"
        Me.btnAutomatico.Size = New System.Drawing.Size(87, 21)
        Me.btnAutomatico.TabIndex = 36
        Me.btnAutomatico.Text = "Automatico"
        Me.btnAutomatico.UseVisualStyleBackColor = True
        '
        'txtDias
        '
        Me.txtDias.Location = New System.Drawing.Point(163, 16)
        Me.txtDias.Name = "txtDias"
        Me.txtDias.Size = New System.Drawing.Size(116, 20)
        Me.txtDias.TabIndex = 37
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(302, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(33, 12)
        Me.Label1.TabIndex = 38
        Me.Label1.Text = "dias"
        '
        'cmbPlazaCobro
        '
        Me.cmbPlazaCobro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPlazaCobro.FormattingEnabled = True
        Me.cmbPlazaCobro.Items.AddRange(New Object() {"Delegación  IV"})
        Me.cmbPlazaCobro.Location = New System.Drawing.Point(164, 83)
        Me.cmbPlazaCobro.Name = "cmbPlazaCobro"
        Me.cmbPlazaCobro.Size = New System.Drawing.Size(172, 20)
        Me.cmbPlazaCobro.TabIndex = 22
        '
        'cmbDelegacion
        '
        Me.cmbDelegacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDelegacion.FormattingEnabled = True
        Me.cmbDelegacion.Items.AddRange(New Object() {"Delegación III QUERETARO"})
        Me.cmbDelegacion.Location = New System.Drawing.Point(164, 56)
        Me.cmbDelegacion.Name = "cmbDelegacion"
        Me.cmbDelegacion.Size = New System.Drawing.Size(172, 20)
        Me.cmbDelegacion.TabIndex = 19
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(23, 232)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(313, 23)
        Me.ProgressBar1.TabIndex = 39
        '
        'dtpFechaInicio
        '
        Me.dtpFechaInicio.CustomFormat = "MM/dd/yyyy HH:mm:ss"
        Me.dtpFechaInicio.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpFechaInicio.Location = New System.Drawing.Point(164, 196)
        Me.dtpFechaInicio.Name = "dtpFechaInicio"
        Me.dtpFechaInicio.Size = New System.Drawing.Size(172, 20)
        Me.dtpFechaInicio.TabIndex = 27
        '
        'lblFecha1
        '
        Me.lblFecha1.AutoSize = True
        Me.lblFecha1.Location = New System.Drawing.Point(20, 204)
        Me.lblFecha1.Name = "lblFecha1"
        Me.lblFecha1.Size = New System.Drawing.Size(89, 12)
        Me.lblFecha1.TabIndex = 34
        Me.lblFecha1.Text = "Fecha Inicio"
        '
        'frmR1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.PowderBlue
        Me.ClientSize = New System.Drawing.Size(347, 320)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtDias)
        Me.Controls.Add(Me.btnAutomatico)
        Me.Controls.Add(Me.lblFecha2)
        Me.Controls.Add(Me.lblFecha1)
        Me.Controls.Add(Me.lblEncargadoTurno)
        Me.Controls.Add(Me.lblTurno)
        Me.Controls.Add(Me.dtpFechaFin)
        Me.Controls.Add(Me.dtpFechaInicio)
        Me.Controls.Add(Me.txtEncargadoTurno)
        Me.Controls.Add(Me.cmbTurnoBlo)
        Me.Controls.Add(Me.cmbPlazaCobro)
        Me.Controls.Add(Me.lblPlazaCobro)
        Me.Controls.Add(Me.btnGenerarReporte)
        Me.Controls.Add(Me.cmbDelegacion)
        Me.Controls.Add(Me.lblDelegacion)
        Me.Font = New System.Drawing.Font("Lucida Sans Typewriter", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "frmR1"
        Me.Text = "frmR1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblFecha2 As System.Windows.Forms.Label
    Friend WithEvents lblEncargadoTurno As System.Windows.Forms.Label
    Friend WithEvents lblTurno As System.Windows.Forms.Label
    Friend WithEvents dtpFechaFin As System.Windows.Forms.DateTimePicker
    Friend WithEvents txtEncargadoTurno As System.Windows.Forms.TextBox
    Friend WithEvents cmbTurnoBlo As System.Windows.Forms.ComboBox
    Friend WithEvents lblPlazaCobro As System.Windows.Forms.Label
    Friend WithEvents btnGenerarReporte As System.Windows.Forms.Button
    Friend WithEvents lblDelegacion As System.Windows.Forms.Label
    Friend WithEvents tm_automatizacion As System.Windows.Forms.Timer
    Friend WithEvents btnAutomatico As System.Windows.Forms.Button
    Friend WithEvents txtDias As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmbPlazaCobro As ComboBox
    Friend WithEvents cmbDelegacion As ComboBox
    Friend WithEvents ProgressBar1 As ProgressBar
    Friend WithEvents dtpFechaInicio As DateTimePicker
    Friend WithEvents lblFecha1 As Label
End Class
