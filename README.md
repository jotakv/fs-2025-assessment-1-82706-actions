# DublinBikes – Blazor UI (Client) + API V2 (Cosmos) HTTP Local Setup

This Blazor UI consumes the **DublinBikes API V2 (Cosmos DB)**.  
For local development, **we run everything using HTTP** (not HTTPS) to avoid certificate/TLS issues.
******************************************************************************************************************************************************************************
Youtube video DEMO:
https://youtu.be/AREbE0_ji2M
******************************************************************************************************************************************************************************

<img width="1894" height="866" alt="image" src="https://github.com/user-attachments/assets/babd79b0-652d-42a0-89d2-a154b6abba61" />
<img width="1603" height="587" alt="image" src="https://github.com/user-attachments/assets/766def4f-7cf9-4566-8e07-cc69c17e5eb7" />
<img width="1595" height="857" alt="image" src="https://github.com/user-attachments/assets/8062a2e6-1571-4d75-9dff-f43368b3aa4b" />


---

## Prerequisites

- **.NET 8 SDK**
- Access to an **Azure Cosmos DB** account (or Cosmos emulator), with the following settings available:
  - `CosmosDb:EndpointUri`
  - `CosmosDb:PrimaryKey`
  - `CosmosDb:DatabaseName`
  - `CosmosDb:ContainerName`

---

## Local URLs (your ports)

- **API V2 (Cosmos):** `http://localhost:5044`
- **Blazor UI:** `http://localhost:5024`

---

## 1) Download / Clone the repository

Clone the repo or download it as a ZIP, then open a terminal in the **repository root**.

---

## 2) Configure Cosmos DB for API V2

The API V2 project requires Cosmos DB configuration (either in `appsettings.json` for the V2 project or via environment variables):

- `CosmosDb:EndpointUri`
- `CosmosDb:PrimaryKey`
- `CosmosDb:DatabaseName`
- `CosmosDb:ContainerName`

---

## 3) Run the API V2 (Cosmos DB)

From the repository root:

```bash
dotnet restore
dotnet run --project fs-2025-b-api-cosmosdb-20251106
```

Verify the API is running by opening:

- `http://localhost:5044/api/v2/stations/summary`

---

## 4) Configure the API Base URL in the Blazor project

Open:

`fs-2025-assessment-1-82706-blazor/appsettings.json`

Set:

```json
"DublinBikesApi": {
  "BaseUrl": "http://localhost:5044"
}
```

> Note: The `StationsApiClient` uses this key (`DublinBikesApi:BaseUrl`) to build requests to the API.

---

## 5) Run the Blazor UI

From the repository root:

```bash
dotnet run --project fs-2025-assessment-1-82706-blazor
```

Open the UI in your browser:

- `http://localhost:5024/stations`

---

## Features implemented

- **Master/Detail** view on `/stations`
- Master list shows:
  - Station **name**
  - **address**
  - **status** (OPEN/CLOSED)
  - **available bikes / total stands**
- Detail view shows:
  - Status badge
  - Bikes/stands numbers
  - Occupancy progress bar
  - Last update (friendly format)
  - Latitude/Longitude + link to Google Maps
- **Search & Filters**
  - Text search (name/address)
  - Status filter (All/OPEN/CLOSED)
  - Minimum available bikes filter
- **Sorting & Paging**
  - Sort by name / available bikes / occupancy
  - Simple paging buttons (Prev/Next)
- **CRUD**
  - Create a new station (POST)
  - Update selected station (PUT)
  - Delete selected station (DELETE)
- Simple loading, error and empty states

---

## Quick troubleshooting

If the UI shows connection errors:

1. Make sure the API V2 is running on `http://localhost:5044`
2. Confirm `DublinBikesApi:BaseUrl` is set to `http://localhost:5044`
3. Test the API directly in the browser:
   - `http://localhost:5044/api/v2/stations`
   - `http://localhost:5044/api/v2/stations/summary`































































******************************************************************************************************************************************************************************
Youtube video DEMO:
https://youtu.be/8BD-UmW7-LM

*******************************************************************************************************************************************************************************
---
# Instructions

Simple explanation of how to run the API and the basic design choices.

---

## 1. How to run the API

From the repository root:

1. Restore dependencies:

   ```bash
   dotnet restore
   ```

2. Run **V1 (JSON API)**:

   ```bash
   dotnet run --project fs-2025-a-api-demo-002
   ```

   This version reads the `Data/dublinbike.json` file into memory.

3. Run **V2 (Cosmos DB API)**:

   ```bash
   dotnet run --project fs-2025-b-api-cosmosdb-20251106
   ```

   This version uses Azure Cosmos DB and needs the Cosmos connection settings to be configured.

The APIs usually listen on URLs like:

- `https://localhost:7259`  
- `https://localhost:7289`  

Check the console output to see the exact ports. You can open the Swagger URL shown in the console to test the endpoints.

---

## 2. Example requests (URLs and curl commands)

Here are some basic `curl` examples that you can also use in Postman.

### List stations (with optional filters for status, search and paging)

```bash
curl "https://localhost:7259/api/v1/stations?status=OPEN&minBikes=3&page=1&pageSize=5"
```

### Get a station by number

```bash
curl "https://localhost:7259/api/v1/stations/1"
```

### Get summary of totals

```bash
curl "https://localhost:7259/api/v1/stations/summary"
```

### Create a station (simple JSON body)

```bash
curl -X POST "https://localhost:7259/api/v1/stations" ^
  -H "Content-Type: application/json" ^
  -d "{
    \"number\": 200,
    \"name\": \"New Station\",
    \"address\": \"City Centre\",
    \"bike_stands\": 20,
    \"available_bikes\": 10,
    \"available_bike_stands\": 10,
    \"status\": \"OPEN\"
  }"
```

> Note:  
> - Change the port if the console shows a different one.  
> - For V2, call the host and port used by the Cosmos project and change the path to `/api/v2/...`.

---

## 3. Notes on design choices and assumptions

- **Separation of responsibilities**  
  The main logic lives in simple *services* (for example `StationService` or `CosmosStationService`).  
  The endpoints/controllers only receive the HTTP request and call the service.  
  This keeps controllers short and without complex calculations.

- **Clear names and low duplication**  
  We use direct names like `GetStations` or `GetStationByNumber` so the code is easy to understand.  
  We reuse methods inside the services so filters and sorting logic are not repeated in many places.

- **Simple dependency injection**  
  Services are registered in `Program.cs` or small startup files.  
  ASP.NET Core injects them into the endpoints when needed.  
  This avoids manual object creation and keeps the code more organised.

- **Separate projects by version**  
  There is one project for the JSON-based API and another project for the Cosmos DB API.  
  Each project has its own `Program.cs`, services, and background services, so data access details do not mix between V1 and V2.

---

# Dublin Bikes Simple APIs

This solution contains **two small .NET 8 Web APIs** that expose Dublin Bikes stations for a beginner assignment.  
Everything is built with minimal APIs and a few helper classes.

- **V1** (`fs-2025-a-api-demo-002`):  
  - Loads `Data/dublinbike.json` into memory at startup.  
  - Provides filtering, searching, sorting, paging, in-memory caching, and a background random updater.

- **V2** (`fs-2025-b-api-cosmosdb-20251106`):  
  - Uses Azure Cosmos DB as the data source.  
  - Exposes the same style of endpoints as V1, but reading from the database.

- A small **test project** shows how to:
  - Test the filtering/search logic.
  - Test one simple “happy path” endpoint.

---

## Running the APIs (summary)

1. Install the .NET 8 SDK.
2. From the repo root, you can run either project:

   ```bash
   dotnet run --project fs-2025-a-api-demo-002
   dotnet run --project fs-2025-b-api-cosmosdb-20251106
   ```

3. Open the Swagger URL shown in the console to try the routes.

---

### Cosmos settings

When running **V2**, you must set these configuration keys (in `appsettings.json` or as environment variables):

- `CosmosDb:EndpointUri`
- `CosmosDb:PrimaryKey`
- `CosmosDb:DatabaseName`
- `CosmosDb:ContainerName`

---

## Endpoints (both versions)

For both V1 and V2, the APIs expose:

- `GET /api/v#/stations` with query parameters:  
  - `status`  
  - `minBikes`  
  - `q` (search term in `name` and `address`)  
  - `sort` (`name | availableBikes | occupancy`)  
  - `dir` (`asc | desc`)  
  - `page`  
  - `pageSize`

- `GET /api/v#/stations/{number}`  
  Returns one station by its station number (404 if not found).

- `GET /api/v#/stations/summary`  
  Returns:
  - `totalStations`
  - `totalBikeStands`
  - `totalAvailableBikes`
  - counts of stations by status (OPEN/CLOSED)

- `POST /api/v#/stations`  
  Creates a new station.

- `PUT /api/v#/stations/{number}`  
  Updates an existing station.

> `v#` is `v1` for the JSON API and `v2` for the Cosmos DB API.

---

## Notes

- The `last_update` field (epoch milliseconds) is converted to UTC and also to Dublin local time on the model.
- Occupancy is protected so that if `bike_stands` is zero, occupancy returns 0 instead of causing a divide-by-zero error.
- A `BackgroundService` in each API updates station capacity (`bike_stands`) and bike counts every ~15–20 seconds to simulate a live data feed.  
  The service always keeps `available_bikes <= bike_stands`.

- GET queries are cached in memory for about five minutes.

---

## Tests

To run the tests:

```bash
dotnet test
```

The tests cover:

- Filtering and searching logic in the service layer.
- A simple GET endpoint (happy path) response.

This test suite is small, but it demonstrates the basic testing ideas requested by the assignment.
