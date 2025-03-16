from dataclasses import dataclass
import uuid

@dataclass
class LogCenterOptions:

    url: str
    table: str
    token: str
    trace_id: str = None

    def __init__(self, url: str, table: str, token: str, consoleLog: bool = True, consoleLogEntireObject: bool = False):
        """Configura conexão com o LogCenter."""
        self.url = url
        self.table = table
        self.token = token
        self.consoleLog = consoleLog
        self.consoleLogEntireObject = consoleLogEntireObject
