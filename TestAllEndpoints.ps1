# ==============================================================================
# TestAllEndpoints.ps1 - Complete API End-to-End Testing Suite
# MovieTicketBooking ASP.NET Core Web API
# ==============================================================================

$baseUrl = "http://localhost:5208"
$testResults = [System.Collections.Generic.List[PSObject]]::new()

function Record-Result {
    param(
        [string]$Controller,
        [string]$Endpoint,
        [string]$Method,
        [string]$Scenario,
        [string]$Expected,
        [string]$Actual,
        [string]$Status
    )
    $obj = [PSCustomObject]@{
        Controller = $Controller
        Endpoint   = $Endpoint
        Method     = $Method
        Scenario   = $Scenario
        Expected   = $Expected
        Actual     = $Actual
        Status     = $Status
    }
    $testResults.Add($obj)

    $color = if ($Status -eq "PASS") { "Green" } elseif ($Status -eq "FAIL") { "Red" } else { "Yellow" }
    Write-Host "[$Status] $Controller $Method $Endpoint - $Scenario (Expected: $Expected | Got: $Actual)" -ForegroundColor $color
}

function Invoke-Api {
    param(
        [string]$Uri,
        [string]$Method = "Get",
        [object]$Body = $null,
        [int[]]$ExpectedStatusCodes = @(200)
    )

    $jsonBody = if ($Body -ne $null) { $Body | ConvertTo-Json -Depth 10 } else { $null }
    $actualCode = 0
    $responseObj = $null
    $rawContent = ""

    try {
        $params = @{
            Uri = $Uri
            Method = $Method
            TimeoutSec = 15
            UseBasicParsing = $true
        }
        if ($jsonBody -ne $null) {
            $params["ContentType"] = "application/json"
            $params["Body"] = $jsonBody
        }

        $res = Invoke-WebRequest @params
        $actualCode = $res.StatusCode
        $rawContent = $res.Content
        if ($rawContent) {
            try { $responseObj = $rawContent | ConvertFrom-Json } catch { $responseObj = $rawContent }
        }
    }
    catch {
        if ($_.Exception.Response) {
            $actualCode = [int]$_.Exception.Response.StatusCode
            $stream = $_.Exception.Response.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $rawContent = $reader.ReadToEnd()
                try { $responseObj = $rawContent | ConvertFrom-Json } catch { $responseObj = $rawContent }
            }
        }
        else {
            $actualCode = 0
            $rawContent = $_.Exception.Message
        }
    }

    $isPass = $ExpectedStatusCodes -contains $actualCode
    return @{
        Pass = $isPass
        StatusCode = $actualCode
        Data = $responseObj
        Raw = $rawContent
    }
}

Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "  MOVIE TICKET BOOKING SYSTEM - COMPREHENSIVE API TEST SUITE" -ForegroundColor Cyan
Write-Host "====================================================================" -ForegroundColor Cyan

# 0. Start API Process if not already listening
$existingConnection = Get-NetTCPConnection -LocalPort 5208 -ErrorAction SilentlyContinue
$apiProcess = $null

if (-not $existingConnection) {
    Write-Host "Starting Web API on $baseUrl..." -ForegroundColor Cyan
    $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --urls $baseUrl" -PassThru -WindowStyle Hidden
    for ($waitIdx = 0; $waitIdx -lt 20; $waitIdx++) {
        Start-Sleep -Seconds 1
        $conn = Get-NetTCPConnection -LocalPort 5208 -ErrorAction SilentlyContinue
        if ($conn) {
            Write-Host "Web API is up and listening on $baseUrl." -ForegroundColor Green
            break
        }
    }
} else {
    Write-Host "Web API is already running and listening on $baseUrl." -ForegroundColor Cyan
}

try {
    # Unique suffix to avoid collisions
    $runId = (Get-Random -Minimum 1000 -Maximum 9999).ToString()

    # =========================================================================
    # A. MOVIES CONTROLLER TESTING
    # =========================================================================
    Write-Host "`n--- Testing Movies Controller ---" -ForegroundColor Yellow

    # Insert 5 valid movies + 1 for deletion test
    $movieIds = [System.Collections.Generic.List[int]]::new()
    $movieDataList = @(
        @{ title = "Interstellar_$runId"; description = "Exploration beyond stars"; language = "English"; genre = "Sci-Fi"; durationInMinutes = 169; releaseDate = "2014-11-07T00:00:00Z" },
        @{ title = "The Dark Knight_$runId"; description = "Gotham vigilante vs Joker"; language = "English"; genre = "Action"; durationInMinutes = 152; releaseDate = "2008-07-18T00:00:00Z" },
        @{ title = "Oppenheimer_$runId"; description = "Story of J. Robert Oppenheimer"; language = "English"; genre = "Biography"; durationInMinutes = 180; releaseDate = "2023-07-21T00:00:00Z" },
        @{ title = "Gladiator II_$runId"; description = "Epic historical drama sequel"; language = "English"; genre = "Action"; durationInMinutes = 148; releaseDate = "2024-11-22T00:00:00Z" },
        @{ title = "Dune Part Two_$runId"; description = "Paul Atreides unites Fremen"; language = "English"; genre = "Sci-Fi"; durationInMinutes = 166; releaseDate = "2024-03-01T00:00:00Z" },
        @{ title = "Temporary Movie_$runId"; description = "To be deleted"; language = "English"; genre = "Drama"; durationInMinutes = 110; releaseDate = "2025-01-01T00:00:00Z" }
    )

    $tempMovieId = 0
    for ($i = 0; $i -lt $movieDataList.Count; $i++) {
        $m = $movieDataList[$i]
        $res = Invoke-Api -Uri "$baseUrl/api/movies" -Method Post -Body $m -ExpectedStatusCodes @(201)
        if ($res.Pass) {
            $createdId = [int]$res.Data.id
            if ($i -eq 5) {
                $tempMovieId = $createdId
            } else {
                $movieIds.Add($createdId)
            }
            Record-Result "MoviesController" "/api/movies" "POST" "Create Movie ($($m.title))" "201 Created" "$($res.StatusCode) Created (ID: $createdId)" "PASS"
        } else {
            Record-Result "MoviesController" "/api/movies" "POST" "Create Movie ($($m.title))" "201 Created" "$($res.StatusCode) Failure" "FAIL"
        }
    }

    # GET All Movies (Pagination)
    $res = Invoke-Api -Uri "$baseUrl/api/movies?pageNumber=1&pageSize=10" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.totalCount -ge 5) {
        Record-Result "MoviesController" "/api/movies?pageNumber=1&pageSize=10" "GET" "Retrieve All Movies with Pagination" "200 OK with totalCount >= 5" "$($res.StatusCode) OK (Total: $($res.Data.totalCount))" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies" "GET" "Retrieve All Movies with Pagination" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Movie by ID
    $targetMovieId = $movieIds[0]
    $res = Invoke-Api -Uri "$baseUrl/api/movies/$targetMovieId" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.id -eq $targetMovieId) {
        Record-Result "MoviesController" "/api/movies/{id}" "GET" "Retrieve Movie by Valid ID" "200 OK" "$($res.StatusCode) OK (Title: $($res.Data.title))" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies/{id}" "GET" "Retrieve Movie by Valid ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Search Movies by Title
    $res = Invoke-Api -Uri "$baseUrl/api/movies/search?title=Interstellar" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -gt 0) {
        Record-Result "MoviesController" "/api/movies/search?title=..." "GET" "Search Movies by Title" "200 OK with matching movies" "$($res.StatusCode) OK (Found: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies/search?title=..." "GET" "Search Movies by Title" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # PUT Update Movie
    $updateMovieBody = @{
        title = "Interstellar Remastered_$runId"
        description = "Updated description with IMAX remaster"
        language = "English"
        genre = "Sci-Fi"
        durationInMinutes = 175
        releaseDate = "2014-11-07T00:00:00Z"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/movies/$targetMovieId" -Method Put -Body $updateMovieBody -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.title -eq $updateMovieBody.title) {
        Record-Result "MoviesController" "/api/movies/{id}" "PUT" "Update Existing Movie" "200 OK with updated title" "$($res.StatusCode) OK (Updated: $($res.Data.title))" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies/{id}" "PUT" "Update Existing Movie" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # DELETE Movie when no show references it
    $res = Invoke-Api -Uri "$baseUrl/api/movies/$tempMovieId" -Method Delete -ExpectedStatusCodes @(204)
    if ($res.Pass) {
        Record-Result "MoviesController" "/api/movies/{id}" "DELETE" "Delete Movie with No Shows" "204 No Content" "$($res.StatusCode) No Content" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies/{id}" "DELETE" "Delete Movie with No Shows" "204 No Content" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Invalid Duration (<= 0)
    $invalidMovieBody = @{
        title = "Invalid Duration Movie"
        description = "Test"
        language = "English"
        genre = "Action"
        durationInMinutes = 0
        releaseDate = "2025-01-01T00:00:00Z"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/movies" -Method Post -Body $invalidMovieBody -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "MoviesController" "/api/movies" "POST" "Negative: Duration <= 0" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies" "POST" "Negative: Duration <= 0" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Missing Required Title
    $missingTitleBody = @{
        title = ""
        description = "No title"
        language = "English"
        genre = "Drama"
        durationInMinutes = 120
        releaseDate = "2025-01-01T00:00:00Z"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/movies" -Method Post -Body $missingTitleBody -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "MoviesController" "/api/movies" "POST" "Negative: Missing Required Title" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies" "POST" "Negative: Missing Required Title" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: GET Nonexistent Movie ID
    $res = Invoke-Api -Uri "$baseUrl/api/movies/999999" -Method Get -ExpectedStatusCodes @(404)
    if ($res.Pass) {
        Record-Result "MoviesController" "/api/movies/999999" "GET" "Negative: Nonexistent Movie ID" "404 Not Found" "$($res.StatusCode) Not Found" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies/999999" "GET" "Negative: Nonexistent Movie ID" "404 Not Found" "$($res.StatusCode)" "FAIL"
    }

    # =========================================================================
    # B. THEATRES CONTROLLER TESTING
    # =========================================================================
    Write-Host "`n--- Testing Theatres Controller ---" -ForegroundColor Yellow

    $theatreIds = [System.Collections.Generic.List[int]]::new()
    $theatreDataList = @(
        @{ name = "Cinepolis VIP Mall_$runId"; location = "North Avenue, Downtown" },
        @{ name = "PVR Superplex_$runId"; location = "Cyber City Sector 29" },
        @{ name = "INOX Megaplex_$runId"; location = "Phoenix City Boulevard" },
        @{ name = "Miraj Cinemas_$runId"; location = "Central Square Mall" },
        @{ name = "Wave Platinum Cinema_$runId"; location = "Metro Plaza West" },
        @{ name = "Temporary Theatre_$runId"; location = "Isolated Strip Mall" }
    )

    $tempTheatreId = 0
    for ($i = 0; $i -lt $theatreDataList.Count; $i++) {
        $t = $theatreDataList[$i]
        $res = Invoke-Api -Uri "$baseUrl/api/theatres" -Method Post -Body $t -ExpectedStatusCodes @(201)
        if ($res.Pass) {
            $createdId = [int]$res.Data.id
            if ($i -eq 5) {
                $tempTheatreId = $createdId
            } else {
                $theatreIds.Add($createdId)
            }
            Record-Result "TheatresController" "/api/theatres" "POST" "Create Theatre ($($t.name))" "201 Created" "$($res.StatusCode) Created (ID: $createdId)" "PASS"
        } else {
            Record-Result "TheatresController" "/api/theatres" "POST" "Create Theatre ($($t.name))" "201 Created" "$($res.StatusCode) Failure" "FAIL"
        }
    }

    # GET All Theatres
    $res = Invoke-Api -Uri "$baseUrl/api/theatres" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -ge 5) {
        Record-Result "TheatresController" "/api/theatres" "GET" "Retrieve All Theatres" "200 OK with Count >= 5" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "TheatresController" "/api/theatres" "GET" "Retrieve All Theatres" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Theatre by ID
    $targetTheatreId = $theatreIds[0]
    $res = Invoke-Api -Uri "$baseUrl/api/theatres/$targetTheatreId" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.id -eq $targetTheatreId) {
        Record-Result "TheatresController" "/api/theatres/{id}" "GET" "Retrieve Theatre by Valid ID" "200 OK" "$($res.StatusCode) OK (Name: $($res.Data.name))" "PASS"
    } else {
        Record-Result "TheatresController" "/api/theatres/{id}" "GET" "Retrieve Theatre by Valid ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # PUT Update Theatre
    $updateTheatreBody = @{
        name = "Cinepolis VIP IMAX Remodel_$runId"
        location = "North Avenue, Downtown Level 3"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/theatres/$targetTheatreId" -Method Put -Body $updateTheatreBody -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.name -eq $updateTheatreBody.name) {
        Record-Result "TheatresController" "/api/theatres/{id}" "PUT" "Update Existing Theatre" "200 OK with updated name" "$($res.StatusCode) OK (Name: $($res.Data.name))" "PASS"
    } else {
        Record-Result "TheatresController" "/api/theatres/{id}" "PUT" "Update Existing Theatre" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # DELETE Theatre when no screens reference it
    $res = Invoke-Api -Uri "$baseUrl/api/theatres/$tempTheatreId" -Method Delete -ExpectedStatusCodes @(204)
    if ($res.Pass) {
        Record-Result "TheatresController" "/api/theatres/{id}" "DELETE" "Delete Theatre with No Screens" "204 No Content" "$($res.StatusCode) No Content" "PASS"
    } else {
        Record-Result "TheatresController" "/api/theatres/{id}" "DELETE" "Delete Theatre with No Screens" "204 No Content" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Missing Required Name
    $missingNameTheatre = @{ name = ""; location = "Somewhere" }
    $res = Invoke-Api -Uri "$baseUrl/api/theatres" -Method Post -Body $missingNameTheatre -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "TheatresController" "/api/theatres" "POST" "Negative: Missing Required Theatre Name" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "TheatresController" "/api/theatres" "POST" "Negative: Missing Required Theatre Name" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Nonexistent Theatre ID
    $res = Invoke-Api -Uri "$baseUrl/api/theatres/999999" -Method Get -ExpectedStatusCodes @(404)
    if ($res.Pass) {
        Record-Result "TheatresController" "/api/theatres/999999" "GET" "Negative: Nonexistent Theatre ID" "404 Not Found" "$($res.StatusCode) Not Found" "PASS"
    } else {
        Record-Result "TheatresController" "/api/theatres/999999" "GET" "Negative: Nonexistent Theatre ID" "404 Not Found" "$($res.StatusCode)" "FAIL"
    }

    # =========================================================================
    # C. SCREENS CONTROLLER TESTING
    # =========================================================================
    Write-Host "`n--- Testing Screens Controller ---" -ForegroundColor Yellow

    $screenIds = [System.Collections.Generic.List[int]]::new()
    $screenDataList = @(
        @{ theatreId = $theatreIds[0]; screenName = "IMAX Laser Screen 1" },
        @{ theatreId = $theatreIds[0]; screenName = "4DX Dynamic Screen 2" },
        @{ theatreId = $theatreIds[1]; screenName = "Screen 1 Atmos" },
        @{ theatreId = $theatreIds[2]; screenName = "Screen Gold Class" },
        @{ theatreId = $theatreIds[3]; screenName = "Main Auditorium A" },
        @{ theatreId = $theatreIds[4]; screenName = "Temporary Screen For Delete" }
    )

    $tempScreenId = 0
    for ($i = 0; $i -lt $screenDataList.Count; $i++) {
        $s = $screenDataList[$i]
        $res = Invoke-Api -Uri "$baseUrl/api/screens" -Method Post -Body $s -ExpectedStatusCodes @(201)
        if ($res.Pass) {
            $createdId = [int]$res.Data.id
            if ($i -eq 5) {
                $tempScreenId = $createdId
            } else {
                $screenIds.Add($createdId)
            }
            Record-Result "ScreensController" "/api/screens" "POST" "Create Screen ($($s.screenName))" "201 Created" "$($res.StatusCode) Created (ID: $createdId)" "PASS"
        } else {
            Record-Result "ScreensController" "/api/screens" "POST" "Create Screen ($($s.screenName))" "201 Created" "$($res.StatusCode) Failure" "FAIL"
        }
    }

    # GET All Screens
    $res = Invoke-Api -Uri "$baseUrl/api/screens" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -ge 5) {
        Record-Result "ScreensController" "/api/screens" "GET" "Retrieve All Screens" "200 OK with Count >= 5" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens" "GET" "Retrieve All Screens" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Screen by ID
    $targetScreenId = $screenIds[0]
    $res = Invoke-Api -Uri "$baseUrl/api/screens/$targetScreenId" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.id -eq $targetScreenId) {
        Record-Result "ScreensController" "/api/screens/{id}" "GET" "Retrieve Screen by Valid ID" "200 OK" "$($res.StatusCode) OK (Name: $($res.Data.screenName))" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens/{id}" "GET" "Retrieve Screen by Valid ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Screens by Theatre ID
    $res = Invoke-Api -Uri "$baseUrl/api/screens/theatre/$($theatreIds[0])" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -ge 2) {
        Record-Result "ScreensController" "/api/screens/theatre/{theatreId}" "GET" "Retrieve Screens by Theatre ID" "200 OK with Count >= 2" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens/theatre/{theatreId}" "GET" "Retrieve Screens by Theatre ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # PUT Update Screen
    $updateScreenBody = @{
        theatreId = $theatreIds[0]
        screenName = "IMAX Laser Screen 1 (Upgraded)"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/screens/$targetScreenId" -Method Put -Body $updateScreenBody -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.screenName -eq $updateScreenBody.screenName) {
        Record-Result "ScreensController" "/api/screens/{id}" "PUT" "Update Existing Screen" "200 OK with updated name" "$($res.StatusCode) OK (Name: $($res.Data.screenName))" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens/{id}" "PUT" "Update Existing Screen" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # DELETE Screen when no shows reference it
    $res = Invoke-Api -Uri "$baseUrl/api/screens/$tempScreenId" -Method Delete -ExpectedStatusCodes @(204)
    if ($res.Pass) {
        Record-Result "ScreensController" "/api/screens/{id}" "DELETE" "Delete Screen with No Shows" "204 No Content" "$($res.StatusCode) No Content" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens/{id}" "DELETE" "Delete Screen with No Shows" "204 No Content" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Nonexistent Theatre ID
    $invalidScreenBody = @{ theatreId = 999999; screenName = "Ghost Screen" }
    $res = Invoke-Api -Uri "$baseUrl/api/screens" -Method Post -Body $invalidScreenBody -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "ScreensController" "/api/screens" "POST" "Negative: Nonexistent Theatre ID" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens" "POST" "Negative: Nonexistent Theatre ID" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Duplicate Screen Name within same Theatre
    $dupScreenBody = @{ theatreId = $theatreIds[0]; screenName = "IMAX Laser Screen 1 (Upgraded)" }
    $res = Invoke-Api -Uri "$baseUrl/api/screens" -Method Post -Body $dupScreenBody -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "ScreensController" "/api/screens" "POST" "Negative: Duplicate Screen Name in Same Theatre" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens" "POST" "Negative: Duplicate Screen Name in Same Theatre" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Delete Theatre with existing screens
    $res = Invoke-Api -Uri "$baseUrl/api/theatres/$($theatreIds[0])" -Method Delete -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "TheatresController" "/api/theatres/{id}" "DELETE" "Negative: Delete Theatre with Associated Screens" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "TheatresController" "/api/theatres/{id}" "DELETE" "Negative: Delete Theatre with Associated Screens" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # =========================================================================
    # D. SEATS CONTROLLER TESTING
    # =========================================================================
    Write-Host "`n--- Testing Seats Controller ---" -ForegroundColor Yellow

    # Insert 6 physical seats per screen (using both single POST and bulk POST /bulk)
    # Total 5 screens * 6 seats = 30 seats
    $screenSeatMap = @{}

    foreach ($scId in $screenIds) {
        $screenSeatMap[$scId] = [System.Collections.Generic.List[int]]::new()

        # Seat 1: via single POST
        $singleSeatBody = @{
            screenId = $scId
            rowName = "A"
            seatNumber = 1
            seatType = "Regular"
        }
        $res = Invoke-Api -Uri "$baseUrl/api/seats" -Method Post -Body $singleSeatBody -ExpectedStatusCodes @(201)
        if ($res.Pass) {
            $createdSeatId = [int]$res.Data.id
            $screenSeatMap[$scId].Add($createdSeatId)
            Record-Result "SeatsController" "/api/seats" "POST" "Create Single Seat (Screen $scId A1)" "201 Created" "$($res.StatusCode) Created (ID: $createdSeatId)" "PASS"
        } else {
            Record-Result "SeatsController" "/api/seats" "POST" "Create Single Seat (Screen $scId A1)" "201 Created" "$($res.StatusCode) Failure" "FAIL"
        }

        # Seats 2-6: via Bulk POST
        $bulkSeatBody = @{
            seats = @(
                @{ screenId = $scId; rowName = "A"; seatNumber = 2; seatType = "Regular" },
                @{ screenId = $scId; rowName = "A"; seatNumber = 3; seatType = "Regular" },
                @{ screenId = $scId; rowName = "B"; seatNumber = 1; seatType = "Premium" },
                @{ screenId = $scId; rowName = "B"; seatNumber = 2; seatType = "Premium" },
                @{ screenId = $scId; rowName = "B"; seatNumber = 3; seatType = "Recliner" }
            )
        }
        $res = Invoke-Api -Uri "$baseUrl/api/seats/bulk" -Method Post -Body $bulkSeatBody -ExpectedStatusCodes @(200, 201)
        if ($res.Pass) {
            foreach ($s in $res.Data) {
                $screenSeatMap[$scId].Add([int]$s.id)
            }
            Record-Result "SeatsController" "/api/seats/bulk" "POST" "Bulk Create Seats (Screen $scId, 5 seats)" "200/201 Success" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
        } else {
            Record-Result "SeatsController" "/api/seats/bulk" "POST" "Bulk Create Seats (Screen $scId, 5 seats)" "200/201 Success" "$($res.StatusCode) Failure" "FAIL"
        }
    }

    # Seat for standalone deletion test
    $tempSeatBody = @{
        screenId = $screenIds[0]
        rowName = "Z"
        seatNumber = 99
        seatType = "Regular"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/seats" -Method Post -Body $tempSeatBody -ExpectedStatusCodes @(201)
    $tempSeatId = [int]$res.Data.id

    # GET Seat by ID
    $targetSeatId = $screenSeatMap[$screenIds[0]][0]
    $res = Invoke-Api -Uri "$baseUrl/api/seats/$targetSeatId" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.id -eq $targetSeatId) {
        Record-Result "SeatsController" "/api/seats/{id}" "GET" "Retrieve Seat by Valid ID" "200 OK" "$($res.StatusCode) OK (Seat: $($res.Data.rowName)$($res.Data.seatNumber))" "PASS"
    } else {
        Record-Result "SeatsController" "/api/seats/{id}" "GET" "Retrieve Seat by Valid ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Seats by Screen ID
    $res = Invoke-Api -Uri "$baseUrl/api/seats/screen/$($screenIds[0])" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -ge 6) {
        Record-Result "SeatsController" "/api/seats/screen/{screenId}" "GET" "Retrieve Seats by Screen ID" "200 OK with Count >= 6" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "SeatsController" "/api/seats/screen/{screenId}" "GET" "Retrieve Seats by Screen ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # PUT Update Seat
    $updateSeatBody = @{
        screenId = $screenIds[0]
        rowName = "A"
        seatNumber = 1
        seatType = "Ultra Premium"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/seats/$targetSeatId" -Method Put -Body $updateSeatBody -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.seatType -eq "Ultra Premium") {
        Record-Result "SeatsController" "/api/seats/{id}" "PUT" "Update Existing Seat Type" "200 OK with updated type" "$($res.StatusCode) OK (Type: $($res.Data.seatType))" "PASS"
    } else {
        Record-Result "SeatsController" "/api/seats/{id}" "PUT" "Update Existing Seat Type" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # DELETE Seat with No Dependent Records
    $res = Invoke-Api -Uri "$baseUrl/api/seats/$tempSeatId" -Method Delete -ExpectedStatusCodes @(204)
    if ($res.Pass) {
        Record-Result "SeatsController" "/api/seats/{id}" "DELETE" "Delete Seat with No ShowSeats/Bookings" "204 No Content" "$($res.StatusCode) No Content" "PASS"
    } else {
        Record-Result "SeatsController" "/api/seats/{id}" "DELETE" "Delete Seat with No ShowSeats/Bookings" "204 No Content" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Duplicate Seat Coordinates in same screen
    $dupSeatBody = @{
        screenId = $screenIds[0]
        rowName = "A"
        seatNumber = 1
        seatType = "Regular"
    }
    $res = Invoke-Api -Uri "$baseUrl/api/seats" -Method Post -Body $dupSeatBody -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "SeatsController" "/api/seats" "POST" "Negative: Duplicate Seat Coordinate on Same Screen" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "SeatsController" "/api/seats" "POST" "Negative: Duplicate Seat Coordinate on Same Screen" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Duplicate Entries inside Bulk Request
    $dupBulkBody = @{
        seats = @(
            @{ screenId = $screenIds[0]; rowName = "C"; seatNumber = 1; seatType = "Regular" },
            @{ screenId = $screenIds[0]; rowName = "C"; seatNumber = 1; seatType = "Regular" }
        )
    }
    $res = Invoke-Api -Uri "$baseUrl/api/seats/bulk" -Method Post -Body $dupBulkBody -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "SeatsController" "/api/seats/bulk" "POST" "Negative: Duplicate Entries Within Bulk Request" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "SeatsController" "/api/seats/bulk" "POST" "Negative: Duplicate Entries Within Bulk Request" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # =========================================================================
    # E. SHOWS CONTROLLER TESTING
    # =========================================================================
    Write-Host "`n--- Testing Shows Controller ---" -ForegroundColor Yellow

    # Insert 5 valid shows on distinct screens with future times
    $showIds = [System.Collections.Generic.List[int]]::new()
    $baseTime = (Get-Date).AddDays(1)

    $showDataList = @(
        @{ movieId = $movieIds[0]; screenId = $screenIds[0]; startTime = $baseTime.AddHours(10).ToString("o"); endTime = $baseTime.AddHours(13).ToString("o"); ticketPrice = 300.00 },
        @{ movieId = $movieIds[1]; screenId = $screenIds[1]; startTime = $baseTime.AddHours(11).ToString("o"); endTime = $baseTime.AddHours(14).ToString("o"); ticketPrice = 280.00 },
        @{ movieId = $movieIds[2]; screenId = $screenIds[2]; startTime = $baseTime.AddHours(14).ToString("o"); endTime = $baseTime.AddHours(17).ToString("o"); ticketPrice = 350.00 },
        @{ movieId = $movieIds[3]; screenId = $screenIds[3]; startTime = $baseTime.AddHours(16).ToString("o"); endTime = $baseTime.AddHours(19).ToString("o"); ticketPrice = 250.00 },
        @{ movieId = $movieIds[4]; screenId = $screenIds[4]; startTime = $baseTime.AddHours(18).ToString("o"); endTime = $baseTime.AddHours(21).ToString("o"); ticketPrice = 220.00 },
        @{ movieId = $movieIds[0]; screenId = $screenIds[0]; startTime = $baseTime.AddHours(14).ToString("o"); endTime = $baseTime.AddHours(17).ToString("o"); ticketPrice = 250.00 }
    )

    $tempShowId = 0
    for ($i = 0; $i -lt $showDataList.Count; $i++) {
        $sh = $showDataList[$i]
        $res = Invoke-Api -Uri "$baseUrl/api/shows" -Method Post -Body $sh -ExpectedStatusCodes @(201)
        if ($res.Pass) {
            $createdId = [int]$res.Data.id
            if ($i -eq 5) {
                $tempShowId = $createdId
            } else {
                $showIds.Add($createdId)
            }
            Record-Result "ShowsController" "/api/shows" "POST" "Create Show on Screen $($sh.screenId)" "201 Created" "$($res.StatusCode) Created (ID: $createdId, Price: $($res.Data.ticketPrice))" "PASS"
        } else {
            Record-Result "ShowsController" "/api/shows" "POST" "Create Show on Screen $($sh.screenId)" "201 Created" "$($res.StatusCode) Failure" "FAIL"
        }
    }

    # GET All Shows
    $res = Invoke-Api -Uri "$baseUrl/api/shows" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -ge 5) {
        Record-Result "ShowsController" "/api/shows" "GET" "Retrieve All Shows" "200 OK with Count >= 5" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows" "GET" "Retrieve All Shows" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Show by ID
    $targetShowId = $showIds[0]
    $res = Invoke-Api -Uri "$baseUrl/api/shows/$targetShowId" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.id -eq $targetShowId) {
        Record-Result "ShowsController" "/api/shows/{id}" "GET" "Retrieve Show by Valid ID" "200 OK" "$($res.StatusCode) OK (Price: $($res.Data.ticketPrice))" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}" "GET" "Retrieve Show by Valid ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # PUT Update Show
    $updateShowBody = @{
        movieId = $movieIds[0]
        screenId = $screenIds[0]
        startTime = $baseTime.AddHours(10).ToString("o")
        endTime = $baseTime.AddHours(13).ToString("o")
        ticketPrice = 320.00
    }
    $res = Invoke-Api -Uri "$baseUrl/api/shows/$targetShowId" -Method Put -Body $updateShowBody -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.ticketPrice -eq 320.00) {
        Record-Result "ShowsController" "/api/shows/{id}" "PUT" "Update Show Ticket Price" "200 OK with updated price" "$($res.StatusCode) OK (New Price: $($res.Data.ticketPrice))" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}" "PUT" "Update Show Ticket Price" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # DELETE Show with No Bookings
    $res = Invoke-Api -Uri "$baseUrl/api/shows/$tempShowId" -Method Delete -ExpectedStatusCodes @(204)
    if ($res.Pass) {
        Record-Result "ShowsController" "/api/shows/{id}" "DELETE" "Delete Show with No Bookings" "204 No Content" "$($res.StatusCode) No Content" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}" "DELETE" "Delete Show with No Bookings" "204 No Content" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Overlapping Show on same screen
    $overlapShowBody = @{
        movieId = $movieIds[1]
        screenId = $screenIds[0]
        startTime = $baseTime.AddHours(11).ToString("o")
        endTime = $baseTime.AddHours(14).ToString("o")
        ticketPrice = 250.00
    }
    $res = Invoke-Api -Uri "$baseUrl/api/shows" -Method Post -Body $overlapShowBody -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "ShowsController" "/api/shows" "POST" "Negative: Overlapping Show On Same Screen" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows" "POST" "Negative: Overlapping Show On Same Screen" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # Negative: EndTime before StartTime
    $invalidTimeShow = @{
        movieId = $movieIds[0]
        screenId = $screenIds[0]
        startTime = $baseTime.AddHours(20).ToString("o")
        endTime = $baseTime.AddHours(19).ToString("o")
        ticketPrice = 200.00
    }
    $res = Invoke-Api -Uri "$baseUrl/api/shows" -Method Post -Body $invalidTimeShow -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "ShowsController" "/api/shows" "POST" "Negative: EndTime <= StartTime" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows" "POST" "Negative: EndTime <= StartTime" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Delete Movie associated with scheduled show
    $res = Invoke-Api -Uri "$baseUrl/api/movies/$($movieIds[0])" -Method Delete -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "MoviesController" "/api/movies/{id}" "DELETE" "Negative: Delete Movie with Associated Shows" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "MoviesController" "/api/movies/{id}" "DELETE" "Negative: Delete Movie with Associated Shows" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Delete Screen associated with scheduled show
    $res = Invoke-Api -Uri "$baseUrl/api/screens/$($screenIds[0])" -Method Delete -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "ScreensController" "/api/screens/{id}" "DELETE" "Negative: Delete Screen with Associated Shows" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "ScreensController" "/api/screens/{id}" "DELETE" "Negative: Delete Screen with Associated Shows" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # =========================================================================
    # F. SHOW SEAT AVAILABILITY TESTING
    # =========================================================================
    Write-Host "`n--- Testing Show Seat Availability ---" -ForegroundColor Yellow

    # Verify automatic ShowSeat generation for Show 0
    $res = Invoke-Api -Uri "$baseUrl/api/shows/$targetShowId/seats" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -eq 6) {
        Record-Result "ShowsController" "/api/shows/{id}/seats" "GET" "Automatic ShowSeat Generation Count Verification" "200 OK with Count = 6" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}/seats" "GET" "Automatic ShowSeat Generation Count Verification" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # Verify all seats initial status is "Available"
    $resAvail = Invoke-Api -Uri "$baseUrl/api/shows/$targetShowId/seats/available" -Method Get -ExpectedStatusCodes @(200)
    if ($resAvail.Pass -and $resAvail.Data.Count -eq 6) {
        Record-Result "ShowsController" "/api/shows/{id}/seats/available" "GET" "Initial Available Seats Count" "200 OK with Count = 6" "$($resAvail.StatusCode) OK (Available: $($resAvail.Data.Count))" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}/seats/available" "GET" "Initial Available Seats Count" "200 OK" "$($resAvail.StatusCode)" "FAIL"
    }

    # Verify initial booked seats count is 0
    $resBooked = Invoke-Api -Uri "$baseUrl/api/shows/$targetShowId/seats/booked" -Method Get -ExpectedStatusCodes @(200)
    if ($resBooked.Pass -and $resBooked.Data.Count -eq 0) {
        Record-Result "ShowsController" "/api/shows/{id}/seats/booked" "GET" "Initial Booked Seats Count" "200 OK with Count = 0" "$($resBooked.StatusCode) OK (Booked: $($resBooked.Data.Count))" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}/seats/booked" "GET" "Initial Booked Seats Count" "200 OK" "$($resBooked.StatusCode)" "FAIL"
    }

    # Negative: Nonexistent Show ID on availability endpoints
    $res = Invoke-Api -Uri "$baseUrl/api/shows/999999/seats" -Method Get -ExpectedStatusCodes @(404)
    if ($res.Pass) {
        Record-Result "ShowsController" "/api/shows/999999/seats" "GET" "Negative: Nonexistent Show ID on /seats" "404 Not Found" "$($res.StatusCode) Not Found" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/999999/seats" "GET" "Negative: Nonexistent Show ID on /seats" "404 Not Found" "$($res.StatusCode)" "FAIL"
    }

    # =========================================================================
    # G. BOOKINGS CONTROLLER TESTING
    # =========================================================================
    Write-Host "`n--- Testing Bookings Controller ---" -ForegroundColor Yellow

    # Create 5 distinct bookings across shows
    $bookingIds = [System.Collections.Generic.List[int]]::new()

    # Booking 1: Show 0, Seats A1, A2
    $b1Seats = @($screenSeatMap[$screenIds[0]][0], $screenSeatMap[$screenIds[0]][1])
    $b1Payload = @{
        customerName = "Alice Morgan"
        customerEmail = "alice.morgan@example.com"
        customerPhone = "9811223344"
        showId = $showIds[0]
        seatIds = $b1Seats
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $b1Payload -ExpectedStatusCodes @(201)
    if ($res.Pass -and $res.Data.totalAmount -eq (2 * 320.00)) {
        $bookingIds.Add([int]$res.Data.id)
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 1 (2 seats, server amount calculation)" "201 Created with TotalAmount = 640" "$($res.StatusCode) Created (Amount: $($res.Data.totalAmount))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 1" "201 Created" "$($res.StatusCode)" "FAIL"
    }

    # Booking 2: Show 0, Seat A3
    $b2Seats = @($screenSeatMap[$screenIds[0]][2])
    $b2Payload = @{
        customerName = "Bob Jenkins"
        customerEmail = "bob.jenkins@example.com"
        customerPhone = "9822334455"
        showId = $showIds[0]
        seatIds = $b2Seats
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $b2Payload -ExpectedStatusCodes @(201)
    if ($res.Pass) {
        $bookingIds.Add([int]$res.Data.id)
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 2 (1 seat)" "201 Created" "$($res.StatusCode) Created (ID: $($res.Data.id))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 2" "201 Created" "$($res.StatusCode)" "FAIL"
    }

    # Booking 3: Show 1, Seats B1, B2
    $b3Seats = @($screenSeatMap[$screenIds[1]][3], $screenSeatMap[$screenIds[1]][4])
    $b3Payload = @{
        customerName = "Charlie Adams"
        customerEmail = "charlie.adams@example.com"
        customerPhone = "9833445566"
        showId = $showIds[1]
        seatIds = $b3Seats
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $b3Payload -ExpectedStatusCodes @(201)
    if ($res.Pass) {
        $bookingIds.Add([int]$res.Data.id)
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 3 (Show 1, 2 seats)" "201 Created" "$($res.StatusCode) Created (ID: $($res.Data.id))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 3" "201 Created" "$($res.StatusCode)" "FAIL"
    }

    # Booking 4: Show 2, Seat A1
    $b4Seats = @($screenSeatMap[$screenIds[2]][0])
    $b4Payload = @{
        customerName = "Diana Prince"
        customerEmail = "diana.prince@example.com"
        customerPhone = "9844556677"
        showId = $showIds[2]
        seatIds = $b4Seats
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $b4Payload -ExpectedStatusCodes @(201)
    if ($res.Pass) {
        $bookingIds.Add([int]$res.Data.id)
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 4 (Show 2, 1 seat)" "201 Created" "$($res.StatusCode) Created (ID: $($res.Data.id))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 4" "201 Created" "$($res.StatusCode)" "FAIL"
    }

    # Booking 5: Show 3, Seats A1, A2
    $b5Seats = @($screenSeatMap[$screenIds[3]][0], $screenSeatMap[$screenIds[3]][1])
    $b5Payload = @{
        customerName = "Evan Wright"
        customerEmail = "evan.wright@example.com"
        customerPhone = "9855667788"
        showId = $showIds[3]
        seatIds = $b5Seats
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $b5Payload -ExpectedStatusCodes @(201)
    if ($res.Pass) {
        $bookingIds.Add([int]$res.Data.id)
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 5 (Show 3, 2 seats)" "201 Created" "$($res.StatusCode) Created (ID: $($res.Data.id))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Create Booking 5" "201 Created" "$($res.StatusCode)" "FAIL"
    }

    # GET Booking by ID
    $targetBookingId = $bookingIds[0]
    $res = Invoke-Api -Uri "$baseUrl/api/bookings/$targetBookingId" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.id -eq $targetBookingId -and $res.Data.bookingSeats.Count -eq 2) {
        Record-Result "BookingsController" "/api/bookings/{id}" "GET" "Retrieve Booking by Valid ID with Seat Items" "200 OK with BookingSeats" "$($res.StatusCode) OK (Seats: $($res.Data.bookingSeats.Count))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings/{id}" "GET" "Retrieve Booking by Valid ID" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET All Bookings
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -ge 5) {
        Record-Result "BookingsController" "/api/bookings" "GET" "Retrieve All Bookings" "200 OK with Count >= 5" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "GET" "Retrieve All Bookings" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Bookings with showId filter
    $res = Invoke-Api -Uri "$baseUrl/api/bookings?showId=$($showIds[0])" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -eq 2) {
        Record-Result "BookingsController" "/api/bookings?showId=..." "GET" "Filter Bookings by showId" "200 OK with Count = 2" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings?showId=..." "GET" "Filter Bookings by showId" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # GET Bookings with status filter
    $res = Invoke-Api -Uri "$baseUrl/api/bookings?status=Confirmed" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.Count -ge 5) {
        Record-Result "BookingsController" "/api/bookings?status=Confirmed" "GET" "Filter Bookings by status" "200 OK with Count >= 5" "$($res.StatusCode) OK (Count: $($res.Data.Count))" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings?status=Confirmed" "GET" "Filter Bookings by status" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # Verify ShowSeat availability changed to Booked for Show 0
    $resAvailAfter = Invoke-Api -Uri "$baseUrl/api/shows/$($showIds[0])/seats/available" -Method Get -ExpectedStatusCodes @(200)
    $resBookedAfter = Invoke-Api -Uri "$baseUrl/api/shows/$($showIds[0])/seats/booked" -Method Get -ExpectedStatusCodes @(200)
    if ($resAvailAfter.Data.Count -eq 3 -and $resBookedAfter.Data.Count -eq 3) {
        Record-Result "ShowsController" "/api/shows/{id}/seats/booked" "GET" "Verification of ShowSeat Status after Booking" "Available = 3, Booked = 3" "Available: $($resAvailAfter.Data.Count), Booked: $($resBookedAfter.Data.Count)" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}/seats/booked" "GET" "Verification of ShowSeat Status after Booking" "Available = 3, Booked = 3" "Got Avail=$($resAvailAfter.Data.Count), Booked=$($resBookedAfter.Data.Count)" "FAIL"
    }

    # Negative: Duplicate Seat IDs in single booking request
    $dupSeatBooking = @{
        customerName = "Fail Test"
        customerEmail = "fail@example.com"
        customerPhone = "9999999999"
        showId = $showIds[0]
        seatIds = @($screenSeatMap[$screenIds[0]][3], $screenSeatMap[$screenIds[0]][3])
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $dupSeatBooking -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Duplicate Seat IDs in Request" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Duplicate Seat IDs in Request" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Attempt to book already-booked seat
    $alreadyBookedPayload = @{
        customerName = "Collision Test"
        customerEmail = "collision@example.com"
        customerPhone = "9999999999"
        showId = $showIds[0]
        seatIds = @($screenSeatMap[$screenIds[0]][0]) # A1 is already booked
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $alreadyBookedPayload -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Attempt to Book Already-Booked Seat" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Attempt to Book Already-Booked Seat" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Seat belonging to a different screen
    $wrongScreenSeatPayload = @{
        customerName = "Wrong Screen Test"
        customerEmail = "wrong@example.com"
        customerPhone = "9999999999"
        showId = $showIds[0]
        seatIds = @($screenSeatMap[$screenIds[1]][0]) # Belongs to screen 1, not screen 0
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $wrongScreenSeatPayload -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Seat from Another Screen" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Seat from Another Screen" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Empty seat selection
    $emptySeatPayload = @{
        customerName = "Empty Test"
        customerEmail = "empty@example.com"
        customerPhone = "9999999999"
        showId = $showIds[0]
        seatIds = @()
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $emptySeatPayload -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Empty Seat Selection" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Empty Seat Selection" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Nonexistent show ID on booking
    $invalidShowBooking = @{
        customerName = "Ghost Show"
        customerEmail = "ghost@example.com"
        customerPhone = "9999999999"
        showId = 999999
        seatIds = @(1)
    }
    $res = Invoke-Api -Uri "$baseUrl/api/bookings" -Method Post -Body $invalidShowBooking -ExpectedStatusCodes @(400)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Nonexistent Show ID" "400 Bad Request" "$($res.StatusCode) Bad Request" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings" "POST" "Negative: Nonexistent Show ID" "400 Bad Request" "$($res.StatusCode)" "FAIL"
    }

    # =========================================================================
    # H. CANCELLATION TESTING
    # =========================================================================
    Write-Host "`n--- Testing Cancellation Module ---" -ForegroundColor Yellow

    # Cancel Booking 1 (Alice Morgan: 2 seats on Show 0)
    $res = Invoke-Api -Uri "$baseUrl/api/bookings/$($bookingIds[0])/cancel" -Method Post -ExpectedStatusCodes @(200)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Cancel Booking 1" "200 OK" "$($res.StatusCode) OK" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Cancel Booking 1" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # Cancel Booking 2 (Bob Jenkins: 1 seat on Show 0)
    $res = Invoke-Api -Uri "$baseUrl/api/bookings/$($bookingIds[1])/cancel" -Method Post -ExpectedStatusCodes @(200)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Cancel Booking 2" "200 OK" "$($res.StatusCode) OK" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Cancel Booking 2" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # Cancel Booking 3 (Charlie Adams: 2 seats on Show 1)
    $res = Invoke-Api -Uri "$baseUrl/api/bookings/$($bookingIds[2])/cancel" -Method Post -ExpectedStatusCodes @(200)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Cancel Booking 3" "200 OK" "$($res.StatusCode) OK" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Cancel Booking 3" "200 OK" "$($res.StatusCode)" "FAIL"
    }

    # Verify Booking Status is "Cancelled" and BookingSeats history is preserved
    $res = Invoke-Api -Uri "$baseUrl/api/bookings/$($bookingIds[0])" -Method Get -ExpectedStatusCodes @(200)
    if ($res.Pass -and $res.Data.bookingStatus -eq "Cancelled" -and $res.Data.bookingSeats.Count -eq 2) {
        Record-Result "BookingsController" "/api/bookings/{id}" "GET" "Verify Booking Status Cancelled & History Preserved" "Status = Cancelled, Seats = 2" "Status: $($res.Data.bookingStatus), Seats: $($res.Data.bookingSeats.Count)" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings/{id}" "GET" "Verify Booking Status Cancelled" "Status = Cancelled" "$($res.Data.bookingStatus)" "FAIL"
    }

    # Verify ShowSeats on Show 0 restored to Available = 6, Booked = 0
    $resAvailRestore = Invoke-Api -Uri "$baseUrl/api/shows/$($showIds[0])/seats/available" -Method Get -ExpectedStatusCodes @(200)
    $resBookedRestore = Invoke-Api -Uri "$baseUrl/api/shows/$($showIds[0])/seats/booked" -Method Get -ExpectedStatusCodes @(200)
    if ($resAvailRestore.Data.Count -eq 6 -and $resBookedRestore.Data.Count -eq 0) {
        Record-Result "ShowsController" "/api/shows/{id}/seats/available" "GET" "Verify Restored Available Seats after Cancellation" "Available = 6, Booked = 0" "Available: $($resAvailRestore.Data.Count), Booked: $($resBookedRestore.Data.Count)" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}/seats/available" "GET" "Verify Restored Available Seats after Cancellation" "Available = 6, Booked = 0" "Avail: $($resAvailRestore.Data.Count), Booked: $($resBookedRestore.Data.Count)" "FAIL"
    }

    # Negative: Repeated Cancellation on already cancelled booking
    $res = Invoke-Api -Uri "$baseUrl/api/bookings/$($bookingIds[0])/cancel" -Method Post -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Negative: Repeated Cancellation on Cancelled Booking" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings/{id}/cancel" "POST" "Negative: Repeated Cancellation on Cancelled Booking" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Cancel Nonexistent Booking ID
    $res = Invoke-Api -Uri "$baseUrl/api/bookings/999999/cancel" -Method Post -ExpectedStatusCodes @(404)
    if ($res.Pass) {
        Record-Result "BookingsController" "/api/bookings/999999/cancel" "POST" "Negative: Cancel Nonexistent Booking ID" "404 Not Found" "$($res.StatusCode) Not Found" "PASS"
    } else {
        Record-Result "BookingsController" "/api/bookings/999999/cancel" "POST" "Negative: Cancel Nonexistent Booking ID" "404 Not Found" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Delete Show when active or cancelled bookings exist
    $res = Invoke-Api -Uri "$baseUrl/api/shows/$($showIds[0])" -Method Delete -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "ShowsController" "/api/shows/{id}" "DELETE" "Negative: Delete Show with Existing Bookings" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "ShowsController" "/api/shows/{id}" "DELETE" "Negative: Delete Show with Existing Bookings" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    # Negative: Delete Seat when linked to ShowSeats / Bookings
    $res = Invoke-Api -Uri "$baseUrl/api/seats/$($b1Seats[0])" -Method Delete -ExpectedStatusCodes @(409)
    if ($res.Pass) {
        Record-Result "SeatsController" "/api/seats/{id}" "DELETE" "Negative: Delete Seat with ShowSeats/Bookings" "409 Conflict" "$($res.StatusCode) Conflict" "PASS"
    } else {
        Record-Result "SeatsController" "/api/seats/{id}" "DELETE" "Negative: Delete Seat with ShowSeats/Bookings" "409 Conflict" "$($res.StatusCode)" "FAIL"
    }

    Write-Host "`n====================================================================" -ForegroundColor Cyan
    Write-Host "  TEST EXECUTION COMPLETED" -ForegroundColor Cyan
    Write-Host "====================================================================" -ForegroundColor Cyan

    $passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
    $failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
    $totalCount = $testResults.Count

    Write-Host "Total Tests Executed : $totalCount" -ForegroundColor White
    Write-Host "Passed Tests         : $passCount" -ForegroundColor Green
    Write-Host "Failed Tests         : $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
}
catch {
    Write-Host "FATAL TEST RUN ERROR: $_" -ForegroundColor Red
}
finally {
    if ($apiProcess) {
        Write-Host "Stopping Web API process (PID: $($apiProcess.Id))..." -ForegroundColor Cyan
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }
}
