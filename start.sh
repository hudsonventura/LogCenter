#!/bin/bash

# 1. Stop and remove existing containers
sudo docker compose down

# 2. Remove the old compose file
rm docker-compose.yml

# 3. Download the latest version of the compose file
wget https://raw.githubusercontent.com/hudsonventura/LogCenter/refs/heads/main/docker-compose.yml

# 4. Remove old images to force a fresh pull
# (Fixed the typo from your snippet here)
sudo docker image rm -f \
    hudsonventura/logcenter-client:latest \
    hudsonventura/logcenter-server:latest

# 5. Start the services in detached mode
sudo docker compose up -d