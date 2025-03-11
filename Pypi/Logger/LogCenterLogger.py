from datetime import datetime, timezone
import logging
import random
import threading
import uuid
import json
import requests
import asyncio
import LogLevel
import LogCenterOptions


class LogCenterLogger:
    def __init__(self, options):
        self.url = options.url
        self.table = options.table
        self.token = options.token
        self.trace_id = str(uuid.uuid4())

    def _log_private(self, level, message, data=None, timestamp=None):

        payload = None
        if data is not None:
            payload = data

        headers = {
            "Authorization": f"Bearer {self.token}",
            "Content-Type": "application/json",
            "TraceId": self.trace_id,
            "level": level,
            "message": message,
            "timestamp": timestamp
        }
        
        response = requests.post(f"{self.url}/{self.table}", headers=headers, 
                                 data=json.dumps(payload))
        if response.status_code not in range(200, 300):
            logging.error(f"Failed to log: {response.text}")

    def log(self, level, message, data=None):
        timestamp = datetime.now(timezone.utc).isoformat()
        self._log_private(level, message, data, timestamp)

    def log_async(self, level, message, data=None):
        
        timestamp = datetime.now(timezone.utc).isoformat()

        def chamar_log():
            self._log_private(level, message, data, timestamp)

        # Criar e iniciar a thread
        thread = threading.Thread(target=chamar_log)
        thread.start()
