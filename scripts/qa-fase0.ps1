$ErrorActionPreference = "Continue"
$API = "http://localhost:5100"

function Test-Endpoint($name, $url, $method, $headers, $body, $expected) {
    try {
        $params = @{ Uri = $url; Method = $method; UseBasicParsing = $true; TimeoutSec = 15 }
        if ($headers) { $params.Headers = $headers }
        if ($body) { $params.Body = $body; $params.ContentType = "application/json" }
        $r = Invoke-WebRequest @params
        $code = [int]$r.StatusCode
        $pass = $code -eq $expected
        Write-Output ("{0} {1}: {2} (esperado {3})" -f $(if($pass){"PASS"}else{"FAIL"}), $name, $code, $expected)
        return @{ Code = $code; Content = $r.Content }
    } catch {
        $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
        $pass = $code -eq $expected
        Write-Output ("{0} {1}: {2} (esperado {3})" -f $(if($pass){"PASS"}else{"FAIL"}), $name, $code, $expected)
        return @{ Code = $code; Content = $null }
    }
}

Write-Output "== H-39: gate de busqueda de candidatos =="
Test-Endpoint "T1 anon -> candidates/search" "$API/api/candidates/search" GET $null $null 401
Test-Endpoint "T2 anon -> candidates/search/skills" "$API/api/candidates/search/skills" GET $null $null 401

# candidato
$login = Test-Endpoint "T3 login candidato QA" "$API/api/auth/login" POST $null '{"email":"qa.candidato@test.dev","password":"QaTest2026!"}' 200
if ($login.Content) {
    $cand = $login.Content | ConvertFrom-Json
    $h = @{ Authorization = "Bearer $($cand.token)" }
    Test-Endpoint "T4 candidato -> candidates/search" "$API/api/candidates/search" GET $h $null 403
    Test-Endpoint "T5 candidato -> search/skills" "$API/api/candidates/search/skills" GET $h $null 403
    Test-Endpoint "T6 candidato -> candidates/me (control)" "$API/api/candidates/me" GET $h $null 200
}

# empresa verificada existente (Hotel Sol Caribe / Grupo La Paella: IsVerified=1)
# Necesitan credenciales: probar con el login de una empresa conocida o crear una nueva y verificarla por SQL.
Write-Output "== Registro empresa nueva (flujo unificado con codigo) =="
Test-Endpoint "T7 send-code correo existente -> ya no 409" "$API/api/auth/register/send-code" POST $null '{"email":"ana.martinez@gmail.com"}' 204
Test-Endpoint "T8 send-code empresa nueva" "$API/api/auth/register/send-code" POST $null '{"email":"qa.empresa2@test.dev","firstName":"QA"}' 204
Start-Sleep 1
$logLine = Select-String -Path "$env:TEMP\api-5100.log" -Pattern "qa.empresa2@test.dev" | Select-Object -Last 1
Write-Output "codigo log: $($logLine.Line.Trim())"
