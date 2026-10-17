#requires -Version 5.1
<#
.SYNOPSIS
Comprueba la API de LibraryLab contra su MySQL configurado.
.DESCRIPTION
Requiere la API en ejecucion. No inicia servidores, aplica migraciones ni borra datos.
Crea registros nuevos con prefijo VERIFY y comprueba respuestas HTTP reales.
.EXAMPLE
.\scripts\Verify-LibraryLab.ps1 -ApiBaseUrl http://localhost:5109
#>
[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string] $ApiBaseUrl = 'http://localhost:5109'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.Net.Http
$baseUri = $null
if (-not [Uri]::TryCreate($ApiBaseUrl, [UriKind]::Absolute, [ref] $baseUri) -or
    $baseUri.Scheme -notin @('http', 'https')) {
    Write-Error 'ApiBaseUrl debe ser una URL HTTP o HTTPS absoluta.'
    exit 1
}
$handler = [System.Net.Http.HttpClientHandler]::new()
$handler.AllowAutoRedirect = $false
$client = [System.Net.Http.HttpClient]::new($handler)
$client.BaseAddress = [Uri] ($ApiBaseUrl.TrimEnd('/') + '/')
$client.Timeout = [TimeSpan]::FromSeconds(30)
$script:assertions = 0
$script:requests = 0
$runId = [Guid]::NewGuid().ToString('N')

function Assert-Condition {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
    $script:assertions++
}
function New-ApiRequest {
    param([string] $Method, [string] $Path, [object] $Body = $null)
    $request = [System.Net.Http.HttpRequestMessage]::new(
        [System.Net.Http.HttpMethod]::new($Method), $Path.TrimStart('/'))
    if ($null -ne $Body) {
        $json = ConvertTo-Json -InputObject $Body -Depth 10 -Compress
        $request.Content = [System.Net.Http.StringContent]::new($json, [Text.Encoding]::UTF8, 'application/json')
    }
    return $request
}
function Read-ApiResponse {
    param([System.Net.Http.HttpResponseMessage] $Response)
    $raw = $Response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    $body = $null
    if (-not [string]::IsNullOrWhiteSpace($raw)) {
        try { $body = ConvertFrom-Json -InputObject $raw }
        catch { throw "La API devolvio contenido que no es JSON (HTTP $([int] $Response.StatusCode))." }
    }
    $script:requests++
    return [pscustomobject] @{ Status = [int] $Response.StatusCode; Body = $body }
}
function Invoke-Api {
    param([string] $Method, [string] $Path, [object] $Body = $null)
    $request = New-ApiRequest $Method $Path $Body
    $response = $null
    try {
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        return Read-ApiResponse $response
    }
    finally {
        if ($null -ne $response) { $response.Dispose() }
        $request.Dispose()
    }
}
function Assert-HttpStatus {
    param([object] $Response, [int] $Expected, [string] $Operation)
    Assert-Condition ($Response.Status -eq $Expected) (
        '{0}: se esperaba HTTP {1}, se recibio HTTP {2}.' -f $Operation, $Expected, $Response.Status)
}
function Assert-Identifier {
    param([object] $Body, [string] $Operation)
    $id = [Guid]::Empty
    Assert-Condition ($null -ne $Body -and [Guid]::TryParse([string] $Body.id, [ref] $id) -and $id -ne [Guid]::Empty) (
        "$Operation no devolvio un identificador valido.")
}
function Assert-ResourceAvailability {
    param([string] $Path, [bool] $Expected, [string] $Operation)
    $response = Invoke-Api GET $Path
    Assert-HttpStatus $response 200 $Operation
    Assert-Condition ($response.Body.isAvailable -eq $Expected) ("${Operation}: disponibilidad incorrecta.")
}
function Verify-LoanLifecycle {
    param([string] $ResourceId, [int] $ResourceKind, [string] $ResourcePath, [string] $Label)
    $inputBody = @{ userId = $script:userId; resourceId = $ResourceId; dueAt = $script:futureDueAt }
    $created = Invoke-Api POST 'api/loans' $inputBody
    Assert-HttpStatus $created 201 "Crear prestamo de $Label"
    Assert-Identifier $created.Body "Crear prestamo de $Label"
    $loanId = [string] $created.Body.id
    Assert-Condition ($created.Body.userId -eq $script:userId) 'El prestamo tiene otro usuario.'
    Assert-Condition ($created.Body.resourceId -eq $ResourceId) 'El prestamo tiene otro recurso.'
    Assert-Condition ([int] $created.Body.resourceKind -eq $ResourceKind) 'El tipo de recurso no coincide.'
    Assert-Condition ([int] $created.Body.status -eq 1) 'El prestamo nuevo no esta activo.'
    Assert-Condition (-not $created.Body.isOverdue) 'El prestamo con vencimiento futuro aparece vencido.'
    Assert-ResourceAvailability $ResourcePath $false "Consultar $Label prestado"
    $detail = Invoke-Api GET "api/loans/$loanId"
    Assert-HttpStatus $detail 200 "Consultar prestamo de $Label"
    Assert-Condition ($detail.Body.id -eq $loanId) 'La consulta devolvio otro prestamo.'
    Assert-HttpStatus (Invoke-Api POST 'api/loans' $inputBody) 409 "Intentar prestar $Label no disponible"
    $returned = Invoke-Api POST "api/loans/$loanId/return"
    Assert-HttpStatus $returned 200 "Devolver $Label"
    Assert-Condition ([int] $returned.Body.status -eq 2) 'La devolucion no cambio el estado.'
    Assert-Condition ($null -ne $returned.Body.returnedAt) 'La devolucion no registro su fecha.'
    Assert-ResourceAvailability $ResourcePath $true "Consultar $Label devuelto"
    Assert-HttpStatus (Invoke-Api POST "api/loans/$loanId/return") 409 "Repetir devolucion de $Label"
    Write-Host "Correcto: prestamo y devolucion de $Label."
}

try {
    Write-Host "Verificacion real en $($client.BaseAddress). Ejecucion VERIFY-$runId."
    $userInput = @{ name = "VERIFY usuario $runId"; email = "verify-$runId@example.test" }
    $user = Invoke-Api POST 'api/users' $userInput
    Assert-HttpStatus $user 201 'Crear usuario'
    Assert-Identifier $user.Body 'Crear usuario'
    $script:userId = [string] $user.Body.id
    Assert-Condition ($user.Body.name -eq $userInput.name) 'El nombre del usuario no coincide.'
    Assert-Condition ($user.Body.email -eq $userInput.email) 'El correo del usuario no coincide.'
    $users = Invoke-Api GET 'api/users'
    Assert-HttpStatus $users 200 'Listar usuarios'
    Assert-Condition (@($users.Body | Where-Object { $_.id -eq $script:userId }).Count -eq 1) (
        'El usuario no aparece una sola vez en el listado.')
    Assert-HttpStatus (Invoke-Api POST 'api/users' $userInput) 409 'Duplicar correo'

    $bookInput = @{ isbn = 'VERIFY-' + $runId.Substring(0, 13); title = "VERIFY libro $runId"; author = 'VERIFY autor' }
    $book = Invoke-Api POST 'api/books' $bookInput
    Assert-HttpStatus $book 201 'Crear libro'
    Assert-Identifier $book.Body 'Crear libro'
    $bookId = [string] $book.Body.id
    Assert-Condition ($book.Body.isbn -eq $bookInput.isbn) 'El ISBN no coincide.'
    Assert-ResourceAvailability "api/books/$bookId" $true 'Consultar libro nuevo'
    $books = Invoke-Api GET 'api/books'
    Assert-HttpStatus $books 200 'Listar libros'
    Assert-Condition (@($books.Body | Where-Object { $_.id -eq $bookId }).Count -eq 1) (
        'El libro no aparece una sola vez en el listado.')
    Assert-HttpStatus (Invoke-Api POST 'api/books' $bookInput) 409 'Duplicar ISBN'

    $equipmentInput = @{ inventoryCode = "VERIFY-$runId"; name = "VERIFY equipo $runId"; type = 1 }
    $equipment = Invoke-Api POST 'api/equipment' $equipmentInput
    Assert-HttpStatus $equipment 201 'Crear equipo'
    Assert-Identifier $equipment.Body 'Crear equipo'
    $equipmentId = [string] $equipment.Body.id
    Assert-Condition ($equipment.Body.inventoryCode -eq $equipmentInput.inventoryCode) 'El inventario no coincide.'
    Assert-ResourceAvailability "api/equipment/$equipmentId" $true 'Consultar equipo nuevo'
    $equipmentList = Invoke-Api GET 'api/equipment'
    Assert-HttpStatus $equipmentList 200 'Listar equipos'
    Assert-Condition (@($equipmentList.Body | Where-Object { $_.id -eq $equipmentId }).Count -eq 1) (
        'El equipo no aparece una sola vez en el listado.')
    Assert-HttpStatus (Invoke-Api POST 'api/equipment' $equipmentInput) 409 'Duplicar inventario'
    Write-Host 'Correcto: creacion, consulta y duplicados de usuarios, libros y equipos.'

    $missingId = [Guid]::NewGuid().ToString()
    foreach ($path in @("api/books/$missingId", "api/equipment/$missingId", "api/loans/$missingId")) {
        Assert-HttpStatus (Invoke-Api GET $path) 404 "Consultar inexistente $path"
    }
    Assert-HttpStatus (Invoke-Api POST "api/loans/$missingId/return") 404 'Devolver prestamo inexistente'
    $invalidLoan = @{ userId = $script:userId; resourceId = $bookId; dueAt = [DateTimeOffset]::UtcNow.AddDays(-1).ToString('o') }
    Assert-HttpStatus (Invoke-Api POST 'api/loans' $invalidLoan) 400 'Crear prestamo vencido'
    Assert-ResourceAvailability "api/books/$bookId" $true 'Consultar libro tras prestamo invalido'
    Write-Host 'Correcto: errores 400 y 404 sin reservar el recurso.'

    $script:futureDueAt = [DateTimeOffset]::UtcNow.AddDays(7).ToString('o')
    Verify-LoanLifecycle $bookId 1 "api/books/$bookId" 'libro'
    Verify-LoanLifecycle $equipmentId 2 "api/equipment/$equipmentId" 'equipo'

    $raceInput = @{ isbn = 'VERIFY-' + [Guid]::NewGuid().ToString('N').Substring(0, 13); title = "VERIFY concurrencia $runId"; author = 'VERIFY autor' }
    $raceBook = Invoke-Api POST 'api/books' $raceInput
    Assert-HttpStatus $raceBook 201 'Crear libro para concurrencia'
    Assert-Identifier $raceBook.Body 'Crear libro para concurrencia'
    $raceId = [string] $raceBook.Body.id
    $loanInput = @{ userId = $script:userId; resourceId = $raceId; dueAt = $script:futureDueAt }
    $request1 = New-ApiRequest POST 'api/loans' $loanInput
    $request2 = New-ApiRequest POST 'api/loans' $loanInput
    $response1 = $null
    $response2 = $null
    try {
        # Se envian las dos solicitudes antes de esperar sus resultados.
        $task1 = $client.SendAsync($request1)
        $task2 = $client.SendAsync($request2)
        $response1 = $task1.GetAwaiter().GetResult()
        $response2 = $task2.GetAwaiter().GetResult()
        $result1 = Read-ApiResponse $response1
        $result2 = Read-ApiResponse $response2
        $statuses = @(@($result1.Status, $result2.Status) | Sort-Object)
        Assert-Condition ($statuses.Count -eq 2 -and $statuses[0] -eq 201 -and $statuses[1] -eq 409) (
            'Concurrencia: se esperaba un HTTP 201 y un HTTP 409; se recibio {0}.' -f ($statuses -join ', '))
    }
    finally {
        if ($null -ne $response1) { $response1.Dispose() }
        if ($null -ne $response2) { $response2.Dispose() }
        $request1.Dispose()
        $request2.Dispose()
    }
    Assert-ResourceAvailability "api/books/$raceId" $false 'Consultar recurso tras prestamos concurrentes'
    $loans = Invoke-Api GET 'api/loans'
    Assert-HttpStatus $loans 200 'Listar prestamos'
    $activeLoans = @($loans.Body | Where-Object { $_.resourceId -eq $raceId -and [int] $_.status -eq 1 })
    Assert-Condition ($activeLoans.Count -eq 1) 'La concurrencia dejo un numero incorrecto de prestamos activos.'
    Assert-HttpStatus (Invoke-Api POST "api/loans/$($activeLoans[0].id)/return") 200 'Devolver recurso de concurrencia'
    Assert-ResourceAvailability "api/books/$raceId" $true 'Consultar recurso de concurrencia devuelto'
    Write-Host 'Correcto: concurrencia con un unico prestamo activo y devolucion posterior.'
    Write-Host ('VERIFICACION CORRECTA: {0} aserciones y {1} respuestas HTTP reales. Se conservan los registros VERIFY.' -f $script:assertions, $script:requests)
    exit 0
}
catch {
    Write-Error -Message ('VERIFICACION FALLIDA tras {0} aserciones y {1} respuestas HTTP: {2}' -f $script:assertions, $script:requests, $_.Exception.Message) -ErrorAction Continue
    exit 1
}
finally {
    $client.Dispose()
    $handler.Dispose()
}
