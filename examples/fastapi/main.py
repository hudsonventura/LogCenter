from fastapi import FastAPI
from fastapi.responses import JSONResponse
from middleware import LogMiddleware

app = FastAPI()

# Adicionando o middleware
app.add_middleware(LogMiddleware)

@app.get("/")
async def root():
    return {"message": "Hello, FastAPI!"}

@app.post("/echo")
async def echo(data: dict):
    return JSONResponse(content={"received": data})

# X-Trace-Id
# traceId
# tranceparent
# Trace-Id
# TODO: permitir que o usuario escolha o header do traceId