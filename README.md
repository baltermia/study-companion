# 🤖 Telegram Study Companion

## Installation

### Prerequisites

You only need:
- Docker installed
- A Telegram Bot-Token (follow [this](https://core.telegram.org/bots/tutorial#obtain-your-bot-token) tutorial)
- A OpenAI API-Key (obtain it from [here](https://platform.openai.com/api-keys))

Unfortunately, as far as I know, the OpenAI API isn't free. I've only implemented OpenAI yet, if you need another model, there's some code changes needed. If you're really interested, create an Issue and I will add it for you :)

### Docker Compose

After getting your keys the rest is very easy, you can use the following compose.

Fill in the `BotToken` and `OpenAI__Key`. There's also some logical options which you can tailor to your needs. You could also use a `.env` file instead, but you'd need to manually edit the compose for that.

```yml
services:
  app:
    image: ghcr.io/baltermia/study-companion:latest
    container_name: studycompanion_app
    restart: unless-stopped
    environment:
      BotToken: ${BOT_TOKEN}
      OpenAI__KEY: ${OPENAI_KEY}
      AppOptions__DefaultTimezone: "Europe/Berlin"
      AppOptions__CalendarCheckMinutes: 10
      AppOptions__CalendarRefreshHours: 12
      AppOptions__CalendarEventOffsetMinutes: 60
      AppOptions__MorningReminderTime: "08:00"
      ConnectionStrings__DefaultConnection: Host=db;Port=5432;Database=${POSTGRES_DB:-studydb};Username=${POSTGRES_USER:-study};Password=${POSTGRES_PASSWORD:-study}
      ConnectionStrings__RedisConnection: redis:6379
    depends_on:
      - db
      - redis
    ports:
      - "8080:8080"

  db:
    image: postgres:latest
    container_name: studycompanion_db
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-study}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-study}
      POSTGRES_DB: ${POSTGRES_DB:-studydb}
    volumes:
      - pgdata:/var/lib/postgresql
    ports:
      - "5432:5432"

  redis:
    image: redis:latest
    container_name: studycompanion_redis
    restart: unless-stopped
    ports:
      - "6379:6379"
    volumes:
      - redisdata:/data

volumes:
  pgdata:
  redisdata:
```

## Development

Best practice for storing secrets during development is the dotnet user-secrets tool. Add the following secrets:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" ""
dotnet user-secrets set "ConnectionStrings:RedisConnection" ""
dotnet user-secrets set "BotToken" ""
dotnet user-secrets set "OpenAI:Key" ""
```

### Redis & Postgres

If you dont want to install the two DBs manually, you can use the [`docker-compose.dev.yml`](./docker-compose.dev.yml) file.

You can use the secrets below as defaults (if you haven't changed the compose file manually):

```
ConnectionStrings:RedisConnection = localhost:6379
ConnectionStrings:DefaultConnection = Host=localhost;Port=5432;Database=studydb;Username=study;Password=study
```

Run the compose file as follows:
```
docker compose -f docker-compose.dev.yml
```

Add `-d` for the detached mode, otherwise ditch it which will show you the logs.
