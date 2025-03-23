import os
import sys
from enum import Enum



sys.path.append(os.path.abspath("../../src/libs/Pypi/Logger/src")) #Used on debug
sys.path.append("/home/hudsonventura/source/LogCenter/src/libs/Pypi/Logger/src")  #Used on debug
from LogCenter import LogCenterLogger, LogCenterOptions



class LoggerFactory(LogCenterOptions):

    _logger: LogCenterLogger

    def __init__(self, options: LogCenterOptions):
        self.options = options
        self._logger= LogCenterLogger(options)


    def GetLogger(self):
        return self._logger