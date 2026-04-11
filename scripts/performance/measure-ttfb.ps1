param(
    [string]$HomeUrl,
    [string]$PlpUrl,
    [string]$PdpUrl,
    [string]$SearchUrl,
    [int]$Passes = 2
)

$targets = @(
    [pscustomobject]@{ Name = 'home'; Url = $HomeUrl },
    [pscustomobject]@{ Name = 'plp'; Url = $PlpUrl },
    [pscustomobject]@{ Name = 'pdp'; Url = $PdpUrl },
    [pscustomobject]@{ Name = 'search'; Url = $SearchUrl }
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Url) }

if ($targets.Count -eq 0) {
    Write-Error "Prosledi bar jedan URL preko -HomeUrl, -PlpUrl, -PdpUrl ili -SearchUrl."
    exit 1
}

$format = @'
code=%{http_code}
ttfb=%{time_starttransfer}
total=%{time_total}
remote_ip=%{remote_ip}
size_download=%{size_download}
'@

foreach ($target in $targets) {
    Write-Host ""
    Write-Host "=== $($target.Name.ToUpperInvariant()) :: $($target.Url) ==="

    for ($pass = 1; $pass -le $Passes; $pass++) {
        Write-Host "-- pass $pass --"
        $headersFile = [System.IO.Path]::GetTempFileName()

        try {
            $metrics = & curl.exe -sS -D $headersFile -o NUL -w $format $target.Url
            Write-Host $metrics.Trim()

            $interestingHeaders = Get-Content $headersFile | Where-Object {
                $_ -match '^(cache-control|server-timing|cf-cache-status|age|x-robots-tag):'
            }

            if ($interestingHeaders) {
                Write-Host "headers:"
                $interestingHeaders | ForEach-Object { Write-Host "  $_" }
            }
        }
        finally {
            Remove-Item -LiteralPath $headersFile -Force -ErrorAction SilentlyContinue
        }
    }
}
