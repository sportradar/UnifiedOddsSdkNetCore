# UofSdk Examples

## Prerequisites

- .NET Core SDK
- Valid Sportradar access token
- Network access to Sportradar's Unified Odds Feed

## Configuration Setup

Before running any of the examples, ensure you have set up an accessToken and environment in App.config.

### Required Configuration
Edit `App.config` and set the following required attributes:
- `accessToken` - Your Sportradar access token for authentication
- `defaultLanguage` - Default language (e.g., "en" - comma delimited for multiple)
- `environment` - Target environment (GlobalIntegration, GlobalProduction, etc.)

### Optional Configuration
- `nodeId` - Unique identifier per SDK instance (recommended)
- `exceptionHandlingStrategy` - How to handle exceptions ('Catch' or 'Throw')
- `disabledProducers` - Comma-separated list of disabled producer IDs

Example configuration:
```xml
<uofSdkSection accessToken="YOUR_ACCESS_TOKEN" defaultLanguage="en" nodeId="1" environment="GlobalIntegration" />
```

## Project Structure

```
Sportradar.OddsFeed.SDK.DemoProject/
├── App.config              # SDK configuration
├── UofSdkExample.cs        # Main program with example selection
├── Example/                # Example implementations
│   ├── Basic.cs
│   ├── MultiSession.cs
│   ├── SpecificDispatchers.cs
│   ├── ShowMarketNames.cs
│   ├── ShowEventInfo.cs
│   ├── CompleteInfo.cs
│   ├── ShowMarketMappings.cs
│   ├── ReplayServer.cs
│   └── CacheExportImport.cs
├── Utils/                  # Utility classes
└── Logs/                   # Log output directory
```

Available examples: (can be selected at the start)
### 1. Basic SDK Setup
* Single UofSession with MessageInterest.AllMessages, full odds recovery from all producers
* Demonstrates: SDK initialization, handling global and message events

### 2. Multi-Session Architecture
* Two parallel sessions: High and Low priority
* Demonstrates: Session isolation (high and low priority), message filtering, concurrent event handling

### 3. With a specific event type handler
* Single session but each sport event type has its own handler (i.e. Match, Stage, Tournament, etc.)
* Demonstrates: Event type-specific processing, custom event handlers

### 4. Market info
* Single session with displaying market(s) info received on each message
* Demonstrates: Market metadata access, market/outcome structure

### 5. SportEvent info
* Single session with displaying SportEvent info associated with each message
* Demonstrates: SportEvent hierarchy, available calls on SportEvent

### 6. Complete info
* Single session with displaying Market and SportEvent info associated with each message
* Demonstrates: Limiting recovery timestamp, Market metadata access, market/outcome structure, SportEvent hierarchy, available calls on SportEvent

### 7. Extra: MarketMapping info
* Single session with displaying MarketMapping info associated with each message market
* Demonstrates: MarketMapping metadata access, market/outcome structure, specifiers

### 8. Extra: Replay server
* Single session connected to replay server
* Demonstrates: how to interact with Replay Server for replaying previous sport events or whole scenarios with various sport events to test integration

### 9. Extra: Cache Export/Import
* SDK cache serialization/deserialization for state persistence across restarts
* Demonstrates: Cache management, state persistence, performance optimization

## Logging

The examples use log4net for logging. Configuration is located in `log4net.config`. Logs are written to the `Logs/` directory by default.

## Notes

- Each example runs independently and can be selected from the interactive menu
- Press 'y' to run additional examples after completing one
- The SDK will handle message recovery and maintain the connection state automatically
- For production use, ensure proper error handling and monitoring
