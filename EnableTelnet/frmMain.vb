Imports System.Net.Sockets
Imports System.Net
Imports System.Text

Public Class frmMain
    Private WithEvents PortScanner As ScannerClass
    Private WithEvents TelnetScanner As ScannerClass
    Private closemsg As String
    Private timerctr As Integer
    Public clientSocket As Socket
    Private byteData As Byte() = New Byte(1023) {}
    Const rPort = 9999


    Private Sub OnSend(ar As IAsyncResult)
        Try
            clientSocket.EndSend(ar)
            'Me.Invoke(New MethodInvoker(Sub()
            'Me.Close()
            '                           End Sub))
            Me.Invoke(Sub()
                          lblStatus.Text = "Telnet enabled!"
                          closemsg = "Telnet enabled!"
                          timerctr = 5
                          tmrClose.Start()
                      End Sub)
        Catch ex As Exception
            Me.Invoke(Sub()
                          lblStatus.Text = "Communication error..."
                      End Sub)
        End Try
    End Sub

    Private Sub OnConnect(ar As IAsyncResult)
        Try
            clientSocket.EndConnect(ar)

            Dim ipTableMsg As String = Chr(&H1C) & Chr(&HAC) & Chr(&HAC) & Chr(&HAC) & Chr(&H0) & Chr(&H0) & Chr(&H0) & Chr(&H2) & Chr(&H0) & Chr(&H0) & Chr(&H1) & Chr(&H0) & "iptables -I INPUT_SERVICE_ACL -i br0 -p tcp -m iprange --src-range 192.168.1.1-192.168.255.254 --dport 23 -j ACCEPT 2>/dev/null" & New String(Chr(&H0), 129)

            Dim b As Byte() = Encoding.Default.GetBytes(ipTableMsg)

            clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, New AsyncCallback(AddressOf OnSend), clientSocket)
            Me.Invoke(Sub()
                          lblStatus.Text = "Enabling Telnet..."
                      End Sub)
        Catch ex As Exception
            Me.Invoke(Sub()
                          lblStatus.Text = "Communication error..."
                      End Sub)
        End Try
    End Sub

    'Private Sub OnReceive(ar As IAsyncResult)
    'Try
    '        clientSocket.EndReceive(ar)

    'Dim msgReceived As String = byteData.ToString
    '        MsgBox(msgReceived)
    '    Catch generatedExceptionName As ObjectDisposedException
    '    Catch ex As Exception
    '        MessageBox.Show(ex.Message)
    '    End Try
    'End Sub

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            TelnetScanner = New ScannerClass("192.168.254.1", 23, 23)
            lblStatus.Text = "Waiting for the modem..."
            TelnetScanner.Start()
        Catch ex As Exception
            Me.Invoke(Sub()
                          lblStatus.Text = "Error occured..."
                      End Sub)
        End Try
    End Sub

    Private Sub PortScanner_PortClosed(Host As String, Port As Integer) Handles PortScanner.PortClosed

        Me.Invoke(Sub()
                      PortScanner = New ScannerClass("192.168.254.1", rPort, rPort)
                      lblStatus.Text = "Waiting for the modem..."
                      PortScanner.Start()
                      'lblStatus.Text = "Can't find modem..."
                  End Sub)
    End Sub

    Private Sub PortScanner_PortOpen(Host As String, Port As Integer) Handles PortScanner.PortOpen
        Me.Invoke(Sub()
                      Try
                          clientSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

                          Dim ipAddress As IPAddress = ipAddress.Parse("192.168.254.1")

                          Dim ipEndPoint As New IPEndPoint(ipAddress, rPort)

                          clientSocket.BeginConnect(ipEndPoint, New AsyncCallback(AddressOf OnConnect), Nothing)
                          'clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, New AsyncCallback(AddressOf OnReceive), clientSocket)
                      Catch ex As Exception
                          'MsgBox(ex.Message)
                          lblStatus.Text = "Error connecting to the modem..."
                      End Try
                  End Sub)
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        System.Diagnostics.Process.Start("http://codemonkeydev.blogspot.com/")
    End Sub

    Private Sub TelnetScanner_PortClosed(Host As String, Port As Integer) Handles TelnetScanner.PortClosed
        Me.Invoke(Sub()
                      PortScanner = New ScannerClass("192.168.254.1", rPort, rPort)
                      lblStatus.Text = "Waiting for the modem..."
                      PortScanner.Start()
                  End Sub)
    End Sub

    Private Sub TelnetScanner_PortOpen(Host As String, Port As Integer) Handles TelnetScanner.PortOpen
        Me.Invoke(Sub()
                      lblStatus.Text = "Telnet is already open..."
                      closemsg = "Telnet is already open..."
                      timerctr = 5
                      tmrClose.Start()
                  End Sub)
    End Sub

    Private Sub tmrClose_Tick(sender As Object, e As EventArgs) Handles tmrClose.Tick
        If timerctr = 0 Then
            Me.Close()
        Else
            timerctr = timerctr - 1
            lblStatus.Text = closemsg & " (" & timerctr & ")"
        End If
    End Sub
End Class
