import os
import sys
from enum import Enum



sys.path.append(os.path.abspath("../../src/libs/Pypi/Logger/src"))

from LogCenter import LogCenterOptions


class SaveFormatType(Enum):
    HTTPText = 1
    Json = 2


class InterceptorOptions(LogCenterOptions):
    def __init__(
        self,
        url: str,
        table: str,
        token: str,
        consoleLog: bool = True,
        consoleLogEntireObject: bool = False,
        TraceIdReponseHeader: str = "X-Trace-Id",
        FormatType: SaveFormatType = SaveFormatType.HTTPText,
        HideResponseExceptions: bool = False,
        LogGetRequest: bool = False,
    ):
        super().__init__(
            url, table, token, consoleLog, consoleLogEntireObject
        )


