import os
import sys
from fastapi import FastAPI
from fastapi.responses import JSONResponse



sys.path.append(os.path.abspath("../../src/libs/Pypi/RequestLogger/src"))
from LogCenterInterceptor import InterceptorMiddleware, InterceptorOptions, SaveFormatType


app = FastAPI()

# Add middleware
options = InterceptorOptions(
    url="http://localhost:9200",
    table="example_interceptor",
    token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AYWRtaW4uY29tIiwibmFtZSI6ImV4YW1wbGVfaW50ZXJjZXB0b3IiLCJ0YWJsZXMiOiJleGFtcGxlX2ludGVyY2VwdG9yIiwiZXhwIjoyMDU3NTI4NjM5LCJpc3MiOiJTZXVJc3N1ZXIiLCJhdWQiOiJTZXVBdWRpZW5jZSJ9.h7HW5m4FIxM7tanZmjWxCMHIfUSz-1MVFYOCv1k69UI",
    FormatType=SaveFormatType.Json,
    HideResponseExceptions=True,
    LogGetRequest=True,
    TraceIdReponseHeader="X-Trace-Id"
)
app.add_middleware(InterceptorMiddleware, options=options)



@app.get("/")
async def root():
    return {"message": "Hello, FastAPI!"}

@app.post("/echo")
async def echo(data: dict):
    return JSONResponse(content={"received": data})

