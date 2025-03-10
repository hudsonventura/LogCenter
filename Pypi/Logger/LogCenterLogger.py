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

        payload = None
        if data is not None:
            payload = data


        headers = {
            "Authorization": f"Bearer {self.token}",
            "Content-Type": "application/json",
            "trace_id": self.trace_id,
            "level": level,
            "message": message
        }
        response = requests.post(f"{self.url}/{self.table}", headers=headers, 
                                 data=json.dumps(payload))
        if response.status_code not in range(200, 300):
            logging.error(f"Failed to log: {response.text}")

    async def log_async(self, level, message, data=None):
        loop = asyncio.get_event_loop()
        await loop.run_in_executor(None, self.log, level, message, data)
