# AI Orchestrator Kata

This repo is intended to house a basic working full-stack application that serves as a test bed for evaluating AI Software Development orchstrators. Not individual prompts, but the orchestrating tools, and set of tuned prompts/agents used by them to go from Proposed-feature to PR with working code. 

## Contents

Treat all content under `/app` as a mono-repo for the UI, Backend, database, and analytics side-car. The kata's assume that the orchestrator is operating against this directory and the directories under it. It is meant to represent a real-world application with multiple services, databases, and analytics requirements. Where the rest of the repo supports the kata's and provides context for the human operator.

Contents under `/backlog` are the set of proposed features, and a description of the existing core-features for the application. Use the feature documents to feed your orchestrator as the first input in your pipeline progressing towards code.

## Proposed Kata Pattern
- Start by cloning the repo. 
- Verify the application is running correctly. 
- Select a feature from the backlog. 
- Cut a branch for that feature
- Kick off your orchestrator, operating only within the `/app` directory
- Observe your results when you reach the PR stage, and document your findings against the `RUBRIC.md`
