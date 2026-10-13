<#
.SYNOPSIS
Joins an IRC channel as a second client and logs every line the server sends.

.DESCRIPTION
Use this script to see what other clients see when MutinyIRC sends to a channel. Start it in
the background before the app joins, then read the log. The connection is plain text (no TLS).
The script sends QUIT when the time limit ends or when the server closes the connection.

The log records hostmasks, which contain the public IP address of each client. Keep the log
out of the repository.

.EXAMPLE
pwsh -File tools/irc-observer.ps1 -Channel '#MutinyIRC' -Minutes 5 -LogPath "$env:TEMP\observer.log"
#>
param(
    [string]$Server = 'irc.libera.chat',
    [int]$Port = 6667,
    [string]$Channel = '#MutinyIRC',
    [double]$Minutes = 3,
    [string]$LogPath = (Join-Path $env:TEMP 'irc-observer.log')
)

$utf8 = [System.Text.UTF8Encoding]::new($false)
Set-Content $LogPath '' -Encoding utf8
$nick = 'mutinyobs' + (Get-Random -Maximum 9999)

$client = [System.Net.Sockets.TcpClient]::new($Server, $Port)
# A short receive timeout lets the loop check the deadline when the server is quiet.
$client.ReceiveTimeout = 2000
$stream = $client.GetStream()
$reader = [System.IO.StreamReader]::new($stream, $utf8)
$writer = [System.IO.StreamWriter]::new($stream, $utf8)
$writer.NewLine = "`r`n"
$writer.AutoFlush = $true

$writer.WriteLine("NICK $nick")
$writer.WriteLine("USER $nick 0 * :MutinyIRC observer")
Write-Host "Observer $nick on ${Server}:$Port, channel $Channel, log $LogPath"

$deadline = (Get-Date).AddMinutes($Minutes)
try {
    while ((Get-Date) -lt $deadline) {
        try {
            $line = $reader.ReadLine()
        }
        catch [System.IO.IOException] {
            # Only a receive timeout is expected here. Any other socket error ends the run.
            $socketError = $_.Exception.InnerException -as [System.Net.Sockets.SocketException]
            if ($socketError -and $socketError.SocketErrorCode -eq 'TimedOut') { continue }
            Add-Content $LogPath ">>> SOCKET ERROR: $($_.Exception.Message)" -Encoding utf8
            break
        }

        if ($null -eq $line) {
            Add-Content $LogPath '>>> SERVER CLOSED THE CONNECTION' -Encoding utf8
            break
        }

        Add-Content $LogPath $line -Encoding utf8
        if ($line -match '^PING (.*)') {
            $writer.WriteLine("PONG $($Matches[1])")
        }
        elseif ($line -match '^:\S+ 001 ') {
            $writer.WriteLine("JOIN $Channel")
            Add-Content $LogPath ">>> JOIN $Channel SENT" -Encoding utf8
        }
    }
}
finally {
    # The socket can close after the Connected check. The QUIT is then not necessary.
    if ($client.Connected) {
        try { $writer.WriteLine('QUIT :observer done') } catch [System.IO.IOException] { }
    }
    $client.Close()
}
