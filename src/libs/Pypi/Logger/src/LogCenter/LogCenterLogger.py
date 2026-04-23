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
        self.trace_id = trace_id if trace_id else str(uuid.uuid4())
        self.consoleLog = options.consoleLog
        self.consoleLogEntireObject = options.consoleLogEntireObject



    def _log_private(self, level, message, data=None, timestamp=None, traceId:str=None):
        headers = {
            "Authorization": f"Bearer {self.token}",
            "Content-Type": "application/json"
        }

        trace_id = traceId if traceId is not None else self.trace_id
        payload = {
            "message": message,
            "category": self._resolve_category(message),
            "timestamp": timestamp,
            "level": self._resolve_level(level),
            "traceId": trace_id,
            "content": data
        }

        try:
            response = requests.post(f"{self.url}/{self.table}", headers=headers, json=payload)
            if response.status_code not in range(200, 300):
                logging.error(f"Failed to log: {response.text}")
        except Exception as e:
          logging.error(f"Failed to log: {str(e)}")

    def _ConsoleLog(self, level, message, data=None, timestamp=None):
        if self.consoleLogEntireObject == False:
            return

        if data is not None and self.consoleLogEntireObject == True:
            jsonstr = json.dumps(data, indent=4, default=str)
            print(f"{timestamp.strftime('%Y-%m-%d %H:%M:%S.%f')} [{level.upper()}] {message}\n{jsonstr}")
        else:
            print(f"{timestamp.strftime('%Y-%m-%d %H:%M:%S.%f')} [{level.upper()}] {message}")

    def Log(self, level, message, data=None,traceId:str=None):
        """Start a thread to send the log to LogCenter"""
        timestamp = datetime.now(timezone.utc)
        self._ConsoleLog(level, message, data, timestamp)
        self._log_private(level, message, data, timestamp.isoformat(timespec='microseconds'), traceId)

    def LogAsync(self, level, message, data=None, traceId:str=None):
        """Send the log to LogCenter, and wait for a response"""
        timestamp = datetime.now(timezone.utc)
        self._ConsoleLog(level, message, data, timestamp)
        def chamar_log():
            self._log_private(level, message, data, timestamp.isoformat(timespec='microseconds'), traceId)

        # Criar e iniciar a thread
        thread = threading.Thread(target=chamar_log)
        thread.start()

    def _resolve_level(self, level):
        normalized = str(level).strip().lower()
        return {
            "trace": 0,
            "info": 1,
            "information": 1,
            "debug": 2,
            "warning": 3,
            "error": 4,
            "critical": 5,
            "success": 6,
            "fatal": 7,
        }.get(normalized, 1)

    def _resolve_category(self, message):
        normalized = str(message).strip().lower()
        if normalized == "request":
            return 3
        if normalized == "response":
            return 4
        return 0
