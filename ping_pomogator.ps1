param (
    [string]$HealthCheckUrl = "http://localhost:5023/live",
    [int]$Timeout = 10
)

try
{
    Write-Host "Проверяем PomogatorBot по адресу: $HealthCheckUrl"

    $requestParams = @{
        Uri = $HealthCheckUrl
        Method = 'GET'
        TimeoutSec = $Timeout
        ContentType = 'application/json'
    }

    $response = Invoke-RestMethod @requestParams -ErrorAction Stop

    if ($response -eq "Healthy")
    {
        Write-Host "Статус PomogatorBot: ЗДОРОВ (Healthy)" -ForegroundColor Green
        exit 0
    }
    elseif ($response -eq "Degraded")
    {
        Write-Host "Статус PomogatorBot: ДЕГРАДИРОВАН (Degraded)" -ForegroundColor Yellow
        Write-Host "Подробности:"
        $response.entries | Format-Table -AutoSize
        exit 1
    }
    else
    {
        Write-Host "Статус PomogatorBot: НЕЗДОРОВ (Unhealthy)" -ForegroundColor Red
        Write-Host "Подробности:"
        $response.entries | Format-Table -AutoSize
        exit 2
    }
}
catch
{
    Write-Host "Ошибка при проверке PomogatorBot: $_" -ForegroundColor Red
    exit 3
}
