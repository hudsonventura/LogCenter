from dataclasses import dataclass
import uuid

@dataclass
class LogCenterOptions:

    url: str
    table: str
    token: str
    trace_id: str = None

    def __init__(self, url: str, table: str, token: str, trace_id: str = None):
        """Configura es para conex o com o LogCenter."""
        self.url = url
        self.table = table
        self.token = token
        self.trace_id = trace_id if trace_id is not None else str(uuid.uuid4())
