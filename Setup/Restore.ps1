param([string]$Server, [string]$Catalog, [string]$Folder)
$ErrorActionPreference = 'Stop'
if ($Catalog -notmatch '^[A-Za-z0-9_]+$') { throw 'Недопустимое имя базы.' }
$cs = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$cs['Data Source'] = $Server
$cs['Initial Catalog'] = 'master'
$cs['Integrated Security'] = $true
$conn = New-Object System.Data.SqlClient.SqlConnection($cs.ConnectionString)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandTimeout = 180
    $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name=N'$Catalog'"
    if ([int]$cmd.ExecuteScalar() -eq 0) {
        $backup = (Resolve-Path -LiteralPath "$Folder\Database\Skachki.bak").Path.Replace("'","''")
        $data = (New-Item -ItemType Directory -Force -Path "$Folder\Data").FullName.Replace("'","''")
        $cmd.CommandText = "RESTORE DATABASE [$Catalog] FROM DISK=N'$backup' WITH CHECKSUM, MOVE N'Yakupova_Skachki3' TO N'$data\$Catalog.mdf', MOVE N'Yakupova_Skachki3_log' TO N'$data\${Catalog}_log.ldf'"
        [void]$cmd.ExecuteNonQuery()
    }
    $conn.ChangeDatabase($Catalog)
    $cmd.CommandText = 'SELECT COUNT(*) FROM dbo.AppAccounts WHERE Login IN (''admin'',''user'',''guest'',''viewer'')'
    if ([int]$cmd.ExecuteScalar() -ne 4) { throw 'В базе отсутствуют учебные учётные записи.' }
} finally { $conn.Dispose() }
