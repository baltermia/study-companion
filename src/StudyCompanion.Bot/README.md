# StudyCompanion Telegram Bot

This is a Telegram bot client for the StudyCompanion application.

## Configuration

The bot requires a Telegram Bot Token to run. You can obtain one from [@BotFather](https://t.me/botfather) on Telegram.

### Option 1: Using appsettings.json

Edit `appsettings.json` and add your bot token:

```json
{
  "BotConfiguration": {
    "BotToken": "YOUR_BOT_TOKEN_HERE"
  }
}
```

### Option 2: Using Environment Variables

Set the environment variable:

```bash
export BotConfiguration__BotToken="YOUR_BOT_TOKEN_HERE"
```

Or on Windows:

```cmd
set BotConfiguration__BotToken=YOUR_BOT_TOKEN_HERE
```

## Running the Bot

```bash
dotnet run
```

## Features

Currently, the bot:
- Receives messages from users
- Echoes back the received message
- Logs all activities

This is a starting point for building more sophisticated bot functionality.
