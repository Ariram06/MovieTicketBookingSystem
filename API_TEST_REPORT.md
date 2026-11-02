# Comprehensive API Test Report – Movie Ticket Booking System

**Test Execution Date:** 2026-11-02  
**Environment:** ASP.NET Core Web API (.NET 10)  
**Database:** Microsoft SQL Server Express (`.\SQLEXPRESS` / `MovieTicketBookingDB`)  
**Base URL:** `http://localhost:5208`  
**Test Automation Script:** `TestAllEndpoints.ps1`  
**Tester Role:** Senior Backend API Tester  

---

## 1. Executive Summary

| Metric | Count / Status |
|---|---|
| **Total Endpoints Discovered** | 35 |
| **Total Endpoints Tested** | 35 |
| **Total Test Cases Executed** | 98 |
| **Successful Test Cases** | 97 |
| **Failed Test Cases** | 1 |
| **Skipped Test Cases** | 0 |
| **Overall Pass Rate** | **98.98%** |

---

## 2. Records Inserted During Test Run

All main entities had at least 5 valid records inserted following strict dependency order, plus additional test records for validation and negative testing:

| Entity | Target | Inserted in Test Run | Cumulative Total in DB | Notes |
|---|---|---|---|---|
| **Movies** | 5 | 6 | 8 | 5 for shows + 1 deleted successfully in isolation test. |
| **Theatres** | 5 | 6 | 7 | 5 for screens + 1 deleted successfully in isolation test. |
| **Screens** | 5 | 6 | 7 | Distributed across created theatres; 1 deleted in isolation test. |
| **Seats** | 5 per screen | 31 | 40 | Both single POST and bulk `/bulk` tested; 1 deleted in isolation test. |
| **Shows** | 5 | 6 | 8 | Future dates, non-overlapping on identical screens. |
| **ShowSeats** | Auto | 36 | 46 | Automatically generated upon show creation (`Status = 'Available'`). |
| **Bookings** | 5 | 5 | 7 | Distributed across different shows and screens; server-calculated prices. |
| **BookingSeats** | N/A | 8 | 12 | Persisted with historical seat prices. |

---

## 3. End-to-End Test Execution Matrix

| Controller | Endpoint | HTTP Method | Test Scenario | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| **MoviesController** | `/api/movies` | `POST` | Create Movie 1 (*Interstellar*) | `201 Created` | `201 Created` (ID: 5) | **PASS** |
| **MoviesController** | `/api/movies` | `POST` | Create Movie 2 (*The Dark Knight*) | `201 Created` | `201 Created` (ID: 6) | **PASS** |
| **MoviesController** | `/api/movies` | `POST` | Create Movie 3 (*Oppenheimer*) | `201 Created` | `201 Created` (ID: 7) | **PASS** |
| **MoviesController** | `/api/movies` | `POST` | Create Movie 4 (*Gladiator II*) | `201 Created` | `201 Created` (ID: 8) | **PASS** |
| **MoviesController** | `/api/movies` | `POST` | Create Movie 5 (*Dune Part Two*) | `201 Created` | `201 Created` (ID: 9) | **PASS** |
| **MoviesController** | `/api/movies` | `POST` | Create Standalone Movie (for delete test) | `201 Created` | `201 Created` (ID: 10) | **PASS** |
| **MoviesController** | `/api/movies` | `GET` | Retrieve All Movies with pagination | `200 OK` (Count >= 5) | `200 OK` (TotalCount: 9) | **PASS** |
| **MoviesController** | `/api/movies/{id}` | `GET` | Retrieve Movie by valid ID | `200 OK` | `200 OK` (Title matches) | **PASS** |
| **MoviesController** | `/api/movies/search` | `GET` | Search Movies by title substring (`?title=Interstellar`) | `200 OK` (Found > 0) | `200 OK` (Found: 2) | **PASS** |
| **MoviesController** | `/api/movies/{id}` | `PUT` | Update Movie details and duration | `200 OK` | `200 OK` (Updated title/duration) | **PASS** |
| **MoviesController** | `/api/movies/{id}` | `DELETE` | Delete Movie with no shows | `204 No Content` | `204 No Content` | **PASS** |
| **MoviesController** | `/api/movies` | `POST` | Negative: Duration <= 0 | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **MoviesController** | `/api/movies` | `POST` | Negative: Missing required title | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **MoviesController** | `/api/movies/{id}` | `GET` | Negative: Nonexistent Movie ID (`999999`) | `404 Not Found` | `404 Not Found` | **PASS** |
| **MoviesController** | `/api/movies/{id}` | `DELETE` | Negative: Delete Movie with scheduled shows | `409 Conflict` | `409 Conflict` | **PASS** |
| **TheatresController** | `/api/theatres` | `POST` | Create Theatre 1 (*Cinepolis VIP Mall*) | `201 Created` | `201 Created` (ID: 3) | **PASS** |
| **TheatresController** | `/api/theatres` | `POST` | Create Theatre 2 (*PVR Superplex*) | `201 Created` | `201 Created` (ID: 4) | **PASS** |
| **TheatresController** | `/api/theatres` | `POST` | Create Theatre 3 (*INOX Megaplex*) | `201 Created` | `201 Created` (ID: 5) | **PASS** |
| **TheatresController** | `/api/theatres` | `POST` | Create Theatre 4 (*Miraj Cinemas*) | `201 Created` | `201 Created` (ID: 6) | **PASS** |
| **TheatresController** | `/api/theatres` | `POST` | Create Theatre 5 (*Wave Platinum Cinema*) | `201 Created` | `201 Created` (ID: 7) | **PASS** |
| **TheatresController** | `/api/theatres` | `POST` | Create Standalone Theatre (for delete test) | `201 Created` | `201 Created` (ID: 8) | **PASS** |
| **TheatresController** | `/api/theatres` | `GET` | Retrieve All Theatres | `200 OK` (Count >= 5) | `200 OK` (Count: 8) | **PASS** |
| **TheatresController** | `/api/theatres/{id}` | `GET` | Retrieve Theatre by valid ID | `200 OK` | `200 OK` (Name matches) | **PASS** |
| **TheatresController** | `/api/theatres/{id}` | `PUT` | Update Theatre name and location | `200 OK` | `200 OK` (Updated details) | **PASS** |
| **TheatresController** | `/api/theatres/{id}` | `DELETE` | Delete Theatre with no screens | `204 No Content` | `204 No Content` | **PASS** |
| **TheatresController** | `/api/theatres` | `POST` | Negative: Missing required theatre name | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **TheatresController** | `/api/theatres/{id}` | `GET` | Negative: Nonexistent Theatre ID (`999999`) | `404 Not Found` | `404 Not Found` | **PASS** |
| **TheatresController** | `/api/theatres/{id}` | `DELETE` | Negative: Delete Theatre with assigned screens | `409 Conflict` | `409 Conflict` | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Create Screen 1 (*IMAX Laser Screen 1*) | `201 Created` | `201 Created` (ID: 3) | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Create Screen 2 (*4DX Dynamic Screen 2*) | `201 Created` | `201 Created` (ID: 4) | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Create Screen 3 (*Screen 1 Atmos*) | `201 Created` | `201 Created` (ID: 5) | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Create Screen 4 (*Screen Gold Class*) | `201 Created` | `201 Created` (ID: 6) | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Create Screen 5 (*Main Auditorium A*) | `201 Created` | `201 Created` (ID: 7) | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Create Standalone Screen (for delete test) | `201 Created` | `201 Created` (ID: 8) | **PASS** |
| **ScreensController** | `/api/screens` | `GET` | Retrieve All Screens | `200 OK` (Count >= 5) | `200 OK` (Count: 8) | **PASS** |
| **ScreensController** | `/api/screens/{id}` | `GET` | Retrieve Screen by valid ID | `200 OK` | `200 OK` (ScreenName matches) | **PASS** |
| **ScreensController** | `/api/screens/theatre/{id}` | `GET` | Retrieve Screens by Theatre ID | `200 OK` (Count >= 2) | `200 OK` (Count: 2) | **PASS** |
| **ScreensController** | `/api/screens/{id}` | `PUT` | Update Screen name | `200 OK` | `200 OK` (Updated ScreenName) | **PASS** |
| **ScreensController** | `/api/screens/{id}` | `DELETE` | Delete Screen with no shows | `204 No Content` | `204 No Content` | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Negative: Nonexistent Theatre ID | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **ScreensController** | `/api/screens` | `POST` | Negative: Duplicate Screen name in same theatre | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **ScreensController** | `/api/screens/{id}` | `DELETE` | Negative: Delete Screen with scheduled shows | `409 Conflict` | `409 Conflict` | **PASS** |
| **SeatsController** | `/api/seats` | `POST` | Create single Seat (Screen 3, `A1`) | `201 Created` | `201 Created` (ID: 11) | **PASS** |
| **SeatsController** | `/api/seats/bulk` | `POST` | Bulk Create Seats (Screen 3, `A2,A3,B1,B2,B3`) | `200 OK` (5 seats) | `200 OK` (5 seats created) | **PASS** |
| **SeatsController** | `/api/seats` | `POST` | Create single Seat (Screen 4, `A1`) | `201 Created` | `201 Created` (ID: 17) | **PASS** |
| **SeatsController** | `/api/seats/bulk` | `POST` | Bulk Create Seats (Screen 4, `A2,A3,B1,B2,B3`) | `200 OK` (5 seats) | `200 OK` (5 seats created) | **PASS** |
| **SeatsController** | `/api/seats` | `POST` | Create single Seat (Screen 5, `A1`) | `201 Created` | `201 Created` (ID: 23) | **PASS** |
| **SeatsController** | `/api/seats/bulk` | `POST` | Bulk Create Seats (Screen 5, `A2,A3,B1,B2,B3`) | `200 OK` (5 seats) | `200 OK` (5 seats created) | **PASS** |
| **SeatsController** | `/api/seats` | `POST` | Create single Seat (Screen 6, `A1`) | `201 Created` | `201 Created` (ID: 29) | **PASS** |
| **SeatsController** | `/api/seats/bulk` | `POST` | Bulk Create Seats (Screen 6, `A2,A3,B1,B2,B3`) | `200 OK` (5 seats) | `200 OK` (5 seats created) | **PASS** |
| **SeatsController** | `/api/seats` | `POST` | Create single Seat (Screen 7, `A1`) | `201 Created` | `201 Created` (ID: 35) | **PASS** |
| **SeatsController** | `/api/seats/bulk` | `POST` | Bulk Create Seats (Screen 7, `A2,A3,B1,B2,B3`) | `200 OK` (5 seats) | `200 OK` (5 seats created) | **PASS** |
| **SeatsController** | `/api/seats/{id}` | `GET` | Retrieve Seat by valid ID | `200 OK` | `200 OK` (Coordinates match) | **PASS** |
| **SeatsController** | `/api/seats/screen/{id}` | `GET` | Retrieve Seats by Screen ID | `200 OK` (Count >= 6) | `200 OK` (Count: 6) | **PASS** |
| **SeatsController** | `/api/seats/{id}` | `PUT` | Update Seat type to "Ultra Premium" | `200 OK` | `200 OK` (Updated type) | **PASS** |
| **SeatsController** | `/api/seats/{id}` | `DELETE` | Delete Seat with no show seats/bookings | `204 No Content` | `204 No Content` | **PASS** |
| **SeatsController** | `/api/seats` | `POST` | Negative: Duplicate coordinates on same screen | `409 Conflict` | `409 Conflict` | **PASS** |
| **SeatsController** | `/api/seats/bulk` | `POST` | Negative: Duplicate entries in bulk request | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **SeatsController** | `/api/seats/{id}` | `DELETE` | Negative: Delete Seat with show seats/bookings | `409 Conflict` | `409 Conflict` | **PASS** |
| **ShowsController** | `/api/shows` | `POST` | Create Show 1 (Screen 3, 10:00-13:00, 300.00) | `201 Created` | `201 Created` (ID: 3) | **PASS** |
| **ShowsController** | `/api/shows` | `POST` | Create Show 2 (Screen 4, 11:00-14:00, 280.00) | `201 Created` | `201 Created` (ID: 4) | **PASS** |
| **ShowsController** | `/api/shows` | `POST` | Create Show 3 (Screen 5, 14:00-17:00, 350.00) | `201 Created` | `201 Created` (ID: 5) | **PASS** |
| **ShowsController** | `/api/shows` | `POST` | Create Show 4 (Screen 6, 16:00-19:00, 250.00) | `201 Created` | `201 Created` (ID: 6) | **PASS** |
| **ShowsController** | `/api/shows` | `POST` | Create Show 5 (Screen 7, 18:00-21:00, 220.00) | `201 Created` | `201 Created` (ID: 7) | **PASS** |
| **ShowsController** | `/api/shows` | `POST` | Create Standalone Show (for delete test) | `201 Created` | `201 Created` (ID: 8) | **PASS** |
| **ShowsController** | `/api/shows` | `GET` | Retrieve All Shows | `200 OK` (Count >= 5) | `200 OK` (Count: 8) | **PASS** |
| **ShowsController** | `/api/shows/{id}` | `GET` | Retrieve Show by valid ID | `200 OK` | `200 OK` (Price matches) | **PASS** |
| **ShowsController** | `/api/shows/{id}` | `PUT` | Update Show ticket price (to 320.00) | `200 OK` | `200 OK` (Price: 320.00) | **PASS** |
| **ShowsController** | `/api/shows/{id}` | `DELETE` | Delete Show with no bookings | `204 No Content` | `500 Server Error` *(See Bug #1)* | **FAIL** |
| **ShowsController** | `/api/shows` | `POST` | Negative: Overlapping Show on same screen | `409 Conflict` | `409 Conflict` | **PASS** |
| **ShowsController** | `/api/shows` | `POST` | Negative: StartTime >= EndTime | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **ShowsController** | `/api/shows/{id}` | `DELETE` | Negative: Delete Show with active bookings | `409 Conflict` | `409 Conflict` | **PASS** |
| **ShowsController** | `/api/shows/{id}/seats` | `GET` | Automatic ShowSeat generation verification | `200 OK` (Count: 6) | `200 OK` (Count: 6) | **PASS** |
| **ShowsController** | `/api/shows/{id}/seats/available` | `GET` | Initial Available Seats check | `200 OK` (Count: 6) | `200 OK` (Count: 6) | **PASS** |
| **ShowsController** | `/api/shows/{id}/seats/booked` | `GET` | Initial Booked Seats check | `200 OK` (Count: 0) | `200 OK` (Count: 0) | **PASS** |
| **ShowsController** | `/api/shows/{id}/seats` | `GET` | Negative: Nonexistent Show ID (`999999`) | `404 Not Found` | `404 Not Found` | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Create Booking 1 (*Alice Morgan*, Show 3, 2 seats) | `201 Created` (Total: 640.00) | `201 Created` (Total: 640.00) | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Create Booking 2 (*Bob Jenkins*, Show 3, 1 seat) | `201 Created` (Total: 320.00) | `201 Created` (Total: 320.00) | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Create Booking 3 (*Charlie Adams*, Show 4, 2 seats) | `201 Created` (Total: 560.00) | `201 Created` (Total: 560.00) | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Create Booking 4 (*Diana Prince*, Show 5, 1 seat) | `201 Created` (Total: 350.00) | `201 Created` (Total: 350.00) | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Create Booking 5 (*Evan Wright*, Show 6, 2 seats) | `201 Created` (Total: 500.00) | `201 Created` (Total: 500.00) | **PASS** |
| **BookingsController** | `/api/bookings/{id}` | `GET` | Retrieve Booking by ID with seat line items | `200 OK` | `200 OK` (Seats: 2) | **PASS** |
| **BookingsController** | `/api/bookings` | `GET` | Retrieve All Bookings | `200 OK` (Count >= 5) | `200 OK` (Count: 7) | **PASS** |
| **BookingsController** | `/api/bookings` | `GET` | Filter Bookings by `showId` query parameter | `200 OK` (Count = 2) | `200 OK` (Count: 2) | **PASS** |
| **BookingsController** | `/api/bookings` | `GET` | Filter Bookings by `status=Confirmed` | `200 OK` (Count >= 5) | `200 OK` (Count: 5) | **PASS** |
| **ShowsController** | `/api/shows/{id}/seats/booked` | `GET` | Live availability check after bookings | `Available=3, Booked=3` | `Available=3, Booked=3` | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Negative: Duplicate seat IDs in booking payload | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Negative: Attempt to book already-booked seat | `409 Conflict` | `409 Conflict` | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Negative: Seat belonging to a different screen | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Negative: Empty seat selection list | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **BookingsController** | `/api/bookings` | `POST` | Negative: Nonexistent Show ID | `400 Bad Request` | `400 Bad Request` | **PASS** |
| **BookingsController** | `/api/bookings/{id}/cancel` | `POST` | Cancel Booking 1 (*Alice Morgan*) | `200 OK` | `200 OK` (Seats restored) | **PASS** |
| **BookingsController** | `/api/bookings/{id}/cancel` | `POST` | Cancel Booking 2 (*Bob Jenkins*) | `200 OK` | `200 OK` (Seats restored) | **PASS** |
| **BookingsController** | `/api/bookings/{id}/cancel` | `POST` | Cancel Booking 3 (*Charlie Adams*) | `200 OK` | `200 OK` (Seats restored) | **PASS** |
| **BookingsController** | `/api/bookings/{id}` | `GET` | Verify Booking marked "Cancelled" with history | `Status = Cancelled` | `Status = Cancelled, Seats = 2` | **PASS** |
| **ShowsController** | `/api/shows/{id}/seats/available` | `GET` | Verify restored seat availability after cancellation | `Available = 6, Booked = 0` | `Available = 6, Booked = 0` | **PASS** |
| **BookingsController** | `/api/bookings/{id}/cancel` | `POST` | Negative: Repeated cancellation of cancelled booking | `409 Conflict` | `409 Conflict` | **PASS** |
| **BookingsController** | `/api/bookings/999999/cancel`| `POST` | Negative: Cancel nonexistent booking ID | `404 Not Found` | `404 Not Found` | **PASS** |

---

## 4. Database Verification Results (Direct SQL Queries)

All data was directly queried in SQL Server Express (`MovieTicketBookingDB`) to confirm database consistency, calculations, and relationships:

### A. Total Database Record Counts
```sql
SELECT 'Movies' AS Entity, COUNT(*) AS RecordCount FROM Movies
UNION ALL SELECT 'Theatres', COUNT(*) FROM Theatres
UNION ALL SELECT 'Screens', COUNT(*) FROM Screens
UNION ALL SELECT 'Seats', COUNT(*) FROM Seats
UNION ALL SELECT 'Shows', COUNT(*) FROM Shows
UNION ALL SELECT 'ShowSeats', COUNT(*) FROM ShowSeats
UNION ALL SELECT 'Bookings', COUNT(*) FROM Bookings
UNION ALL SELECT 'BookingSeats', COUNT(*) FROM BookingSeats;
```
**Result:**
- Movies: 8
- Theatres: 7
- Screens: 7
- Seats: 40
- Shows: 8
- ShowSeats: 46
- Bookings: 7
- BookingSeats: 12

### B. Booking Total Amount vs. Individual Seat Prices
```sql
SELECT b.Id AS BookingId, b.CustomerName, b.TotalAmount, b.BookingStatus,
       COUNT(bs.Id) AS SeatCount, SUM(bs.Price) AS SumSeatPrices
FROM Bookings b
INNER JOIN BookingSeats bs ON b.Id = bs.BookingId
GROUP BY b.Id, b.CustomerName, b.TotalAmount, b.BookingStatus;
```
**Verification:**
Every booking's `TotalAmount` matches the sum of its child `BookingSeats.Price` down to the exact cent:
- Booking 3 (*Alice Morgan*): TotalAmount = `640.00`, SumSeatPrices = `640.00` (Cancelled, 2 seats retained)
- Booking 4 (*Bob Jenkins*): TotalAmount = `320.00`, SumSeatPrices = `320.00` (Cancelled, 1 seat retained)
- Booking 5 (*Charlie Adams*): TotalAmount = `560.00`, SumSeatPrices = `560.00` (Cancelled, 2 seats retained)
- Booking 6 (*Diana Prince*): TotalAmount = `350.00`, SumSeatPrices = `350.00` (Confirmed, 1 seat)
- Booking 7 (*Evan Wright*): TotalAmount = `500.00`, SumSeatPrices = `500.00` (Confirmed, 2 seats)

### C. Seat Restoration and Cancellation Audit
- Current `ShowSeats`: 43 Available, 3 Booked.
- Cancelled bookings remained in `Bookings` table with `BookingStatus = 'Cancelled'` and their historical `BookingSeat` line items intact.
- Seats associated with cancelled bookings were reverted to `Status = 'Available'` in `ShowSeats`.

---

## 5. Bugs Discovered & Developer Recommendations

### Bug #1: Foreign Key Conflict on Deleting Shows without Bookings
- **Endpoint:** `DELETE /api/shows/{id}`
- **Scenario:** A user attempts to delete a show that has no customer bookings.
- **Observed Behavior:** The API returns `500 Internal Server Error`.
- **Root Cause Analysis:** When a show is created, `ShowService.CreateShowAsync` automatically generates dependent `ShowSeat` records. In `ApplicationDbContext.cs`, the relationship between `Show` and `ShowSeat` is configured with `OnDelete(DeleteBehavior.Restrict)`. When `ShowService.DeleteShowAsync` deletes the `Show` entity without first deleting its child `ShowSeat` records, SQL Server triggers a foreign key constraint violation:
  ```text
  The DELETE statement conflicted with the REFERENCE constraint "FK_ShowSeats_Shows_ShowId".
  ```
- **Recommended Developer Fix:**
  In `ShowService.DeleteShowAsync`, before calling `_showRepository.DeleteAsync(show)`, delete all associated `ShowSeat` records if no bookings exist:
  ```csharp
  // When no bookings exist, clean up generated ShowSeats before deleting Show
  var showSeats = await _showRepository.GetShowSeatsByShowIdAsync(id);
  if (showSeats.Count > 0)
  {
      _context.ShowSeats.RemoveRange(showSeats);
  }
  ```
  *Alternatively*, update `ApplicationDbContext` to allow cascade delete from `Show` to `ShowSeats` when no bookings are attached.

