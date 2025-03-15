from datetime import datetime, timezone
import logging
import threading
import uuid
import json
import requests



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
        
        try:
            response = requests.post(f"{self.url}/{self.table}", headers=headers, 
                                 data=json.dumps(payload))
            if response.status_code not in range(200, 300):
                logging.error(f"Failed to log: {response.text}")
        except Exception as e:
          logging.error(f"Failed to log: {str(e)}")



    def Log(self, level, message, data=None):
        """Start a thread to send the log to LogCenter"""
        timestamp = datetime.now(timezone.utc).isoformat(timespec='microseconds')
        self._log_private(level, message, data, timestamp)

    def LogAsync(self, level, message, data=None):
        """Send the log to LogCenter, and wait for a response"""
        timestamp = datetime.now(timezone.utc).isoformat(timespec='microseconds')

        def chamar_log():
            self._log_private(level, message, data, timestamp)

        # Criar e iniciar a thread
        thread = threading.Thread(target=chamar_log)
        thread.start()
