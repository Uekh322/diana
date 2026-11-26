$ErrorActionPreference = 'Stop'
try {
    $fx = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full'
    if ($fx.Release -lt 461808) { throw 'Требуется .NET Framework 4.7.2 или новее.' }
    $tool = Join-Path $env:ProgramFiles 'Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe'
    if (!(Test-Path -LiteralPath $tool)) {
        Write-Host 'Установите Microsoft SQL Server 2022 LocalDB в открывшемся окне.'
        $installer = Start-Process msiexec.exe -ArgumentList ('/i "' + "$PSScriptRoot\Setup\SQLLOCALDB.MSI" + '"') -Wait -PassThru
        if ($installer.ExitCode -eq 3010) { throw 'Перезагрузите компьютер и повторите запуск.' }
        if ($installer.ExitCode -ne 0 -or !(Test-Path -LiteralPath $tool)) { throw 'Установка LocalDB не завершена.' }
    }
    $instance = 'SkachkiCourse2022'
    $instances = @(& $tool info)
    if ($instances -notcontains $instance) {
        & $tool create $instance 16.0
        if ($LASTEXITCODE) { throw 'Не удалось создать экземпляр LocalDB.' }
    }
    & $tool start $instance
    if ($LASTEXITCODE) { throw 'Не удалось запустить LocalDB.' }
    & "$PSScriptRoot\Setup\Restore.ps1" "(localdb)\$instance" Yakupova_Skachki3 $PSScriptRoot
    Start-Process -FilePath "$PSScriptRoot\Program\Yakupova_Skachki.exe" -WorkingDirectory "$PSScriptRoot\Program"
} catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
