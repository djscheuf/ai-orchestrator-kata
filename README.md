# AI Orchestrator Kata

This repo is intended to house a basic working full-stack application that serves as a test bed for evaluating AI Software Development orchstrators. Not individual prompts, but the orchestrating tools, and set of tuned prompts/agents used by them to go from Proposed-feature to PR with working code. 

## Contents

Treat all content under `/app` as a mono-repo for the UI, Backend, database, and analytics side-car. The kata's assume that the orchestrator is operating against this directory and the directories under it. It is meant to represent a real-world application with multiple services, databases, and analytics requirements. Where the rest of the repo supports the kata's and provides context for the human operator.

Contents under `/backlog` are the set of proposed features, and a description of the existing core-features for the application. Use the feature documents to feed your orchestrator as the first input in your pipeline progressing towards code.

## Running the Application with Docker Compose

### Prerequisites
- Docker and Docker Compose installed on your system

### Quick Start
1. Navigate to the app directory:
   ```bash
   cd app
   ```

2. Start all services (database, API, and UI):
   ```bash
   docker-compose up --build
   ```

3. Access the application:
   - **UI**: http://localhost:3000
   - **API**: http://localhost:5000
   - **Database**: localhost:5432

### Configuration
You can customize the services by setting environment variables. Create a `.env` file in the `app` directory:

```
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=financial_app
DB_PORT=5432
API_ENVIRONMENT=Production
API_PORT=5000
API_BASE_URL=http://api:5000
UI_PORT=3000
```

### Stopping the Application
```bash
docker-compose down
```

To remove all data and start fresh:
```bash
docker-compose down -v
```

### Seed Data Access
The database is seeded with initial data from `/app/src/db/seed_data.sql`. There are 5 seed users, UN = user_{a|b|c|d|e}, PW = password123. You may use these for initial verification of the app.

## Proposed Kata Pattern
- Start by cloning the repo. 
- Verify the application is running correctly (see "Running the Application with Docker Compose" above). 
- Select a feature from the backlog. 
- Cut a branch for that feature
- Kick off your orchestrator, operating only within the `/app` directory
- Observe your results when you reach the PR stage, and document your findings against the `RUBRIC.md`
