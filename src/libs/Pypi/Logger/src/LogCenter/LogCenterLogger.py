from datetime import datetime, timezone
import logging
import threading
import uuid
import json
import requests



class LogCenterLogger:

    def __init__(self, options, trace_id: str = None):
        self.url = options.url
        self.table = options.table
        self.token = options.token
        self.trace_id = trace_id
        self.consoleLog = options.consoleLog
        self.consoleLogEntireObject = options.consoleLogEntireObject


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

    def _ConsoleLog(self, level, message, data=None, timestamp=None):
        if self.consoleLogEntireObject == False:
            return

        if data is not None and self.consoleLogEntireObject == True:
            jsonstr = json.dumps(data, indent=4)
            print(f"{timestamp.strftime('%Y-%m-%d %H:%M:%S.%f')} [{level.upper()}] {message}\n{jsonstr}")
        else:
            print(f"{timestamp.strftime('%Y-%m-%d %H:%M:%S.%f')} [{level.upper()}] {message}")

    def Log(self, level, message, data=None):
        """Start a thread to send the log to LogCenter"""
        timestamp = datetime.now(timezone.utc)
        self._ConsoleLog(level, message, data, timestamp)
        self._log_private(level, message, data, timestamp.isoformat(timespec='microseconds'))

    def LogAsync(self, level, message, data=None):
        """Send the log to LogCenter, and wait for a response"""
        timestamp = datetime.now(timezone.utc)
        self._ConsoleLog(level, message, data, timestamp)
        def chamar_log():
            self._log_private(level, message, data, timestamp.isoformat(timespec='microseconds'))

        # Criar e iniciar a thread
        thread = threading.Thread(target=chamar_log)
        thread.start()
