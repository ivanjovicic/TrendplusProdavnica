[CmdletBinding()]
param(
    [string]$OpenSearchUri = $(if ($env:OPENSEARCH_URI) { $env:OPENSEARCH_URI } else { 'http://localhost:9200' }),
    [string]$IndexName = $(if ($env:OPENSEARCH_INDEX) { $env:OPENSEARCH_INDEX } else { 'products' }),
    [string]$QueryText = 'sandale',
    [int]$Iterations = 20,
    [int]$WarmupIterations = 5,
    [int]$PageSize = 24,
    [int]$FacetBucketSize = 20,
    [string]$Username = $env:OPENSEARCH_USERNAME,
    [string]$Password = $env:OPENSEARCH_PASSWORD,
    [switch]$SkipCertificateCheck
)

$ErrorActionPreference = 'Stop'

function New-BaseQuery {
    param(
        [string]$Text
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return @{ match_all = @{} }
    }

    return @{
        bool = @{
            must = @(
                @{
                    multi_match = @{
                        query = $Text
                        operator = 'and'
                        fields = @(
                            'name^4',
                            'shortDescription^1.5',
                            'searchKeywords^3',
                            'brandName^2',
                            'primaryCategory^1.5',
                            'secondaryCategories^1.2'
                        )
                    }
                }
            )
        }
    }
}

function New-Sort {
    param(
        [bool]$HasSearchText
    )

    if ($HasSearchText) {
        return @(
            @{ _score = @{ order = 'desc' } },
            @{ sortRank = @{ order = 'desc' } },
            @{ publishedAtUtc = @{ order = 'desc' } }
        )
    }

    return @(
        @{ sortRank = @{ order = 'desc' } },
        @{ publishedAtUtc = @{ order = 'desc' } }
    )
}

function New-SearchBody {
    param(
        [string]$Text,
        [bool]$IncludeAggregations
    )

    $query = New-BaseQuery -Text $Text
    $body = @{
        size = $PageSize
        track_total_hits = $true
        query = $query
        sort = New-Sort -HasSearchText (-not [string]::IsNullOrWhiteSpace($Text))
    }

    if ($IncludeAggregations) {
        $body.aggs = @{
            brands_scope = @{
                filter = $query
                aggs = @{
                    brands = @{
                        terms = @{
                            field = 'brandName.keyword'
                            size = $FacetBucketSize
                        }
                    }
                }
            }
            sizes_scope = @{
                filter = $query
                aggs = @{
                    sizes = @{
                        terms = @{
                            field = 'availableSizes'
                            size = $FacetBucketSize
                        }
                    }
                }
            }
            colors_scope = @{
                filter = $query
                aggs = @{
                    colors = @{
                        terms = @{
                            field = 'primaryColorName.keyword'
                            size = $FacetBucketSize
                        }
                    }
                }
            }
            price_scope = @{
                filter = $query
                aggs = @{
                    price_min = @{ min = @{ field = 'minPrice' } }
                    price_max = @{ max = @{ field = 'maxPrice' } }
                }
            }
            availability_scope = @{
                filter = $query
                aggs = @{
                    availability_in_stock = @{
                        filter = @{
                            term = @{
                                inStock = $true
                            }
                        }
                    }
                    availability_out_of_stock = @{
                        filter = @{
                            term = @{
                                inStock = $false
                            }
                        }
                    }
                }
            }
            sale_scope = @{
                filter = $query
                aggs = @{
                    sale_true = @{
                        filter = @{
                            term = @{
                                isOnSale = $true
                            }
                        }
                    }
                    sale_false = @{
                        filter = @{
                            term = @{
                                isOnSale = $false
                            }
                        }
                    }
                }
            }
            new_scope = @{
                filter = $query
                aggs = @{
                    new_true = @{
                        filter = @{
                            term = @{
                                isNew = $true
                            }
                        }
                    }
                    new_false = @{
                        filter = @{
                            term = @{
                                isNew = $false
                            }
                        }
                    }
                }
            }
        }
    }

    return $body
}

function Invoke-OpenSearchQuery {
    param(
        [hashtable]$Body
    )

    $json = $Body | ConvertTo-Json -Depth 20
    $uri = '{0}/{1}/_search' -f $OpenSearchUri.TrimEnd('/'), $IndexName
    $headers = @{}

    if (-not [string]::IsNullOrWhiteSpace($Username) -and -not [string]::IsNullOrWhiteSpace($Password)) {
        $token = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("${Username}:${Password}"))
        $headers.Authorization = "Basic $token"
    }

    $requestParams = @{
        Uri = $uri
        Method = 'POST'
        Body = $json
        ContentType = 'application/json'
        Headers = $headers
        TimeoutSec = 60
    }

    if ($SkipCertificateCheck -and (Get-Command Invoke-RestMethod).Parameters.ContainsKey('SkipCertificateCheck')) {
        $requestParams.SkipCertificateCheck = $true
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-RestMethod @requestParams
    $stopwatch.Stop()

    $hitCount = 0
    if ($null -ne $response.hits -and $null -ne $response.hits.total) {
        if ($response.hits.total -is [long] -or $response.hits.total -is [int]) {
            $hitCount = [int64]$response.hits.total
        }
        elseif ($null -ne $response.hits.total.value) {
            $hitCount = [int64]$response.hits.total.value
        }
    }

    [pscustomobject]@{
        RequestMs = [math]::Round($stopwatch.Elapsed.TotalMilliseconds, 2)
        TookMs = [double]$response.took
        Hits = $hitCount
        TimedOut = [bool]$response.timed_out
    }
}

function Get-Percentile {
    param(
        [double[]]$Values,
        [double]$Percentile
    )

    if ($Values.Count -eq 0) {
        return 0
    }

    $sorted = $Values | Sort-Object
    $index = [math]::Ceiling(($Percentile / 100) * $sorted.Count) - 1
    $clampedIndex = [math]::Max(0, [math]::Min($sorted.Count - 1, [int]$index))
    return [math]::Round([double]$sorted[$clampedIndex], 2)
}

function Get-BenchmarkSummary {
    param(
        [string]$Name,
        [object[]]$Measurements
    )

    $requestValues = @($Measurements | ForEach-Object { [double]$_.RequestMs })
    $tookValues = @($Measurements | ForEach-Object { [double]$_.TookMs })
    $timeoutCount = @($Measurements | Where-Object { $_.TimedOut }).Count
    $lastHits = if ($Measurements.Count -gt 0) { $Measurements[-1].Hits } else { 0 }

    return [pscustomobject]@{
        Scenario = $Name
        Samples = $Measurements.Count
        Hits = $lastHits
        Timeouts = $timeoutCount
        RequestAvgMs = [math]::Round((($requestValues | Measure-Object -Average).Average), 2)
        RequestP50Ms = Get-Percentile -Values $requestValues -Percentile 50
        RequestP95Ms = Get-Percentile -Values $requestValues -Percentile 95
        RequestMaxMs = [math]::Round((($requestValues | Measure-Object -Maximum).Maximum), 2)
        TookAvgMs = [math]::Round((($tookValues | Measure-Object -Average).Average), 2)
        TookP50Ms = Get-Percentile -Values $tookValues -Percentile 50
        TookP95Ms = Get-Percentile -Values $tookValues -Percentile 95
        TookMaxMs = [math]::Round((($tookValues | Measure-Object -Maximum).Maximum), 2)
    }
}

function Run-Benchmark {
    param(
        [string]$Name,
        [hashtable]$Body
    )

    Write-Host ""
    Write-Host "== $Name =="

    for ($i = 1; $i -le $WarmupIterations; $i++) {
        [void](Invoke-OpenSearchQuery -Body $Body)
    }

    $measurements = @()
    for ($i = 1; $i -le $Iterations; $i++) {
        $measurement = Invoke-OpenSearchQuery -Body $Body
        $measurements += $measurement
        Write-Host ("[{0}/{1}] request={2}ms took={3}ms hits={4}" -f $i, $Iterations, $measurement.RequestMs, $measurement.TookMs, $measurement.Hits)
    }

    return $measurements
}

$searchOnlyBody = New-SearchBody -Text $QueryText -IncludeAggregations:$false
$facetedBody = New-SearchBody -Text $QueryText -IncludeAggregations:$true

Write-Host "OpenSearch benchmark"
Write-Host ("Endpoint: {0}" -f $OpenSearchUri)
Write-Host ("Index:    {0}" -f $IndexName)
Write-Host ("Query:    {0}" -f $(if ([string]::IsNullOrWhiteSpace($QueryText)) { '<match_all>' } else { $QueryText }))
Write-Host ("Warmup:   {0}" -f $WarmupIterations)
Write-Host ("Samples:  {0}" -f $Iterations)

$searchOnlyMeasurements = Run-Benchmark -Name 'search-only' -Body $searchOnlyBody
$facetedMeasurements = Run-Benchmark -Name 'search-with-facets' -Body $facetedBody

$searchOnlySummary = Get-BenchmarkSummary -Name 'search-only' -Measurements $searchOnlyMeasurements
$facetedSummary = Get-BenchmarkSummary -Name 'search-with-facets' -Measurements $facetedMeasurements

$facetRequestOverhead = [math]::Round($facetedSummary.RequestAvgMs - $searchOnlySummary.RequestAvgMs, 2)
$facetTookOverhead = [math]::Round($facetedSummary.TookAvgMs - $searchOnlySummary.TookAvgMs, 2)

Write-Host ""
Write-Host "== Summary =="
@($searchOnlySummary, $facetedSummary) |
    Format-Table Scenario, Samples, Hits, Timeouts, RequestAvgMs, RequestP50Ms, RequestP95Ms, TookAvgMs, TookP50Ms, TookP95Ms -AutoSize

Write-Host ""
Write-Host ("Facet overhead (avg request): {0} ms" -f $facetRequestOverhead)
Write-Host ("Facet overhead (avg took):    {0} ms" -f $facetTookOverhead)
