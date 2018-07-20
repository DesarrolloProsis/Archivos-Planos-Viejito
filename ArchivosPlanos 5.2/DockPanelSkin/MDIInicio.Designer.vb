<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MDIInicio
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.MenuStrip = New System.Windows.Forms.MenuStrip()
        Me.FileMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReportesMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.rpt4 = New System.Windows.Forms.ToolStripMenuItem()
        Me.rpt1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.V100ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.lblSac = New System.Windows.Forms.ToolStripStatusLabel()
        Me.MenuStrip.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStrip
        '
        Me.MenuStrip.BackgroundImage = Global.My.Resources.Resources._6389_babasse_SolidEdge3D
        Me.MenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileMenu, Me.ReportesMenu, Me.HelpMenu, Me.V100ToolStripMenuItem})
        Me.MenuStrip.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip.Name = "MenuStrip"
        Me.MenuStrip.Size = New System.Drawing.Size(436, 24)
        Me.MenuStrip.TabIndex = 9
        Me.MenuStrip.Text = "MenuStrip"
        '
        'FileMenu
        '
        Me.FileMenu.Name = "FileMenu"
        Me.FileMenu.Size = New System.Drawing.Size(41, 20)
        Me.FileMenu.Text = "&Salir"
        '
        'ReportesMenu
        '
        Me.ReportesMenu.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.rpt4, Me.rpt1})
        Me.ReportesMenu.Name = "ReportesMenu"
        Me.ReportesMenu.Size = New System.Drawing.Size(62, 20)
        Me.ReportesMenu.Text = "&Exportar"
        '
        'rpt4
        '
        Me.rpt4.Name = "rpt4"
        Me.rpt4.Size = New System.Drawing.Size(136, 22)
        Me.rpt4.Tag = "4"
        Me.rpt4.Text = "Exportación"
        '
        'rpt1
        '
        Me.rpt1.Name = "rpt1"
        Me.rpt1.Size = New System.Drawing.Size(136, 22)
        Me.rpt1.Text = "Encriptar"
        '
        'HelpMenu
        '
        Me.HelpMenu.Name = "HelpMenu"
        Me.HelpMenu.Size = New System.Drawing.Size(24, 20)
        Me.HelpMenu.Text = "?"
        '
        'V100ToolStripMenuItem
        '
        Me.V100ToolStripMenuItem.Name = "V100ToolStripMenuItem"
        Me.V100ToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.V100ToolStripMenuItem.Text = "V 5.2"
        '
        'Timer1
        '
        Me.Timer1.Interval = 5000
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblSac})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 298)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(436, 22)
        Me.StatusStrip1.TabIndex = 11
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'lblSac
        '
        Me.lblSac.Name = "lblSac"
        Me.lblSac.Size = New System.Drawing.Size(0, 17)
        '
        'MDIInicio
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = Global.My.Resources.Resources.carretera2
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(436, 320)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.MenuStrip)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.IsMdiContainer = True
        Me.Name = "MDIInicio"
        Me.Text = "Módulo Liquidación Exportación"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.MenuStrip.ResumeLayout(False)
        Me.MenuStrip.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents MenuStrip As System.Windows.Forms.MenuStrip
    Friend WithEvents FileMenu As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReportesMenu As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpMenu As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents rpt4 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents V100ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents lblSac As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents rpt1 As ToolStripMenuItem
End Class
