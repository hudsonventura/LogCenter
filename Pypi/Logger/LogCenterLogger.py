import logging
import uuid
import json
import requests
import asyncio
import LogLevel
import LogCenterOptions


class LogCenterLogger:
    def __init__(self, options: LogCenterOptions):
        self.url = options.url
        self.table = options.table
        self.token = options.token
        self.trace_id = str(uuid.uuid4())
       

    def log(self, level, message, data=None):
        payload = {
            "trace_id": self.trace_id,
            "level": level,
            "message": message,
            "data": data or {}
        }
        response = requests.post(f"{self.url}/logs", #headers=self.headers, 
                                 data=json.dumps(payload))
        if response.status_code != 200:
            logging.error(f"Failed to log: {response.text}")

    async def log_async(self, level, message, data=None):
        loop = asyncio.get_event_loop()
        await loop.run_in_executor(None, self.log, level, message, data)