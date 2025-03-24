import os
import sys
from enum import Enum



sys.path.append(os.path.abspath("../../src/libs/Pypi/Logger/src")) #Used on debug
sys.path.append("/home/hudsonventura/source/LogCenter/src/libs/Pypi/Logger/src")  #Used on debug
from LogCenter import LogCenterLogger, LogCenterOptions


class SaveFormatType(Enum):
    HTTPText = 1
    Json = 2


class InterceptorOptions(LogCenterOptions):

    LogGetRequest: bool

    def __init__(
        self,
        url: str,
        table: str,
        token: str,
        consoleLog: bool = True,
        consoleLogEntireObject: bool = False,
        TraceIdReponseHeader: str = "X-Trace-Id", 
        FormatType: SaveFormatType = SaveFormatType.HTTPText,
        LogGetRequest: bool = False,
    ):
        super().__init__(url, table, token, consoleLog, consoleLogEntireObject)
        
        self.TraceIdReponseHeader = TraceIdReponseHeader
        self.FormatType = FormatType
        self.LogGetRequest = LogGetRequest,
