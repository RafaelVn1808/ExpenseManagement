# Adiciona a pasta do projeto como exclusão no Windows Defender
# Execute como Administrador: clique direito no arquivo > "Executar com PowerShell"
# Ou no PowerShell (Admin): .\Add-WindowsDefenderExclusion.ps1

$projectPath = "C:\Projetos\ExpenseManagement"

Write-Host "Adicionando exclusão no Windows Defender para: $projectPath" -ForegroundColor Cyan

try {
    Add-MpPreference -ExclusionPath $projectPath -ErrorAction Stop
    Write-Host "`nExclusao adicionada com sucesso!" -ForegroundColor Green
    Write-Host "Agora feche e reabra o Visual Studio, depois execute o projeto novamente." -ForegroundColor Yellow
} catch {
    Write-Host "`nErro: $_" -ForegroundColor Red
    Write-Host "`nCertifique-se de executar este script como Administrador:" -ForegroundColor Yellow
    Write-Host "  Clique direito no arquivo > Executar com PowerShell" -ForegroundColor White
    Write-Host "  Ou abra PowerShell como Admin e execute: .\Add-WindowsDefenderExclusion.ps1" -ForegroundColor White
    exit 1
}
