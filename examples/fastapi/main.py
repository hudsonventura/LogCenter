import contextvars
import os
import sys
import uuid
from fastapi import Depends, FastAPI
from fastapi.responses import JSONResponse



sys.path.append(os.path.abspath("../../src/libs/Pypi/RequestLogger/src"))  #Used on debug
sys.path.append("/home/hudsonventura/source/LogCenter/src/libs/Pypi/RequestLogger/src")  #Used on debug
from LogCenterInterceptor import InterceptorMiddleware, InterceptorOptions, LoggerFactory, SaveFormatType

sys.path.append(os.path.abspath("../../src/libs/Pypi/Logger/src"))  #Used on debug
sys.path.append("/home/hudsonventura/source/LogCenter/src/libs/Pypi/Logger/src")  #Used on debug
from LogCenter import LogCenterOptions,LogCenterLogger, LogLevel

app = FastAPI()

# Add middleware
options = InterceptorOptions(
    url="http://localhost:9200",
    table="example_interceptor",
    token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AYWRtaW4uY29tIiwibmFtZSI6ImV4YW1wbGVfaW50ZXJjZXB0b3IiLCJ0YWJsZXMiOiJleGFtcGxlX2ludGVyY2VwdG9yIiwiZXhwIjoyMDU3NTI4NjM5LCJpc3MiOiJTZXVJc3N1ZXIiLCJhdWQiOiJTZXVBdWRpZW5jZSJ9.h7HW5m4FIxM7tanZmjWxCMHIfUSz-1MVFYOCv1k69UI",
    FormatType=SaveFormatType.Json,
    HideResponseExceptions=True,
    LogGetRequest=True,
    TraceIdReponseHeader="X-Trace-Id",
)
factory = LoggerFactory(options)
logger = factory.GetLogger()
get_logger = factory.GetLogger
app.add_middleware(InterceptorMiddleware, options=options, logger=logger)




@app.get("/")
async def root(logger=Depends(get_logger)):
    logger.LogAsync(LogLevel.FATAL, "Hellooooo World 321", [{"test": 123}])
    return {"message": "Hello, FastAPI!"}

@app.post("/echo")
async def echo(data: dict):
    return JSONResponse(content={"received": data})

