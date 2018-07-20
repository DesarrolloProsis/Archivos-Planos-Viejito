<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FRM1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.OpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.Lista = New System.Windows.Forms.ListBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.guardar = New System.Windows.Forms.FolderBrowserDialog()
        Me.SuspendLayout()
        '
        'OpenFile
        '
        Me.OpenFile.FileName = "OpenFile"
        '
        'Lista
        '
        Me.Lista.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Lista.FormattingEnabled = True
        Me.Lista.Location = New System.Drawing.Point(13, 13)
        Me.Lista.Name = "Lista"
        Me.Lista.Size = New System.Drawing.Size(338, 108)
        Me.Lista.TabIndex = 0
        '
        'Button2
        '
        Me.Button2.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button2.Location = New System.Drawing.Point(25, 156)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(141, 31)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "Seleccionar Archivos"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.Location = New System.Drawing.Point(198, 156)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(141, 31)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "Encriptar Archivos"
        Me.Button1.UseVisualStyleBackColor = True
        Me.Button1.Visible = False
        '
        'FRM1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.DodgerBlue
        Me.ClientSize = New System.Drawing.Size(363, 215)
        Me.Controls.Add(Me.Lista)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FRM1"
        Me.Text = "Encriptado de Archivos"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents OpenFile As OpenFileDialog
    Friend WithEvents Lista As ListBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents guardar As FolderBrowserDialog
End Class
