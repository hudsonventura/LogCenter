import logging
import json
import os
import sys
import uuid
import asyncio
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from starlette.responses import Response
from contextvars import ContextVar

from LogCenterInterceptor import InterceptorOptions, SaveFormatType

sys.path.append(os.path.abspath("../../src/libs/Pypi/Logger/src")) # Just for API development
from LogCenter import LogCenterLogger, LogLevel 

logging.basicConfig(level=logging.INFO)

# Cria uma variável de contexto para o TraceID
trace_id_var: ContextVar[str] = ContextVar("trace_id", default="")

class TraceId():
    def Get():
        return trace_id_var.get(None)



class InterceptorMiddleware(BaseHTTPMiddleware):
    def __init__(self, app, options: InterceptorOptions):
        super().__init__(app)  # Chama o construtor da classe base
        self.options = options  # Agora options pode ser usado na classe
        self.logger = LogCenterLogger(options)

    async def dispatch(self, request: Request, call_next):
        # Gera um TraceId único para a requisição
        trace_id = str(uuid.uuid4())
        trace_id_var.set(trace_id)


        # Captura os dados da request
        body = await request.body()

        request_data = self._getBodyRequest(method=request.method, url=str(request.url), 
            headers=request.headers,
            body=body.decode() if body else None
        )

        # Send request if its not a GET or if LogGetRequest is True
        if(request.method != 'GET' or (request.method == 'GET' and self.options.LogGetRequest)):
            asyncio.create_task(self.send_log(request_data, trace_id, type="Request"))
        
        

        # Processa a resposta normalmente
        response = await call_next(request)

        # Lê os dados da resposta (precisamos recriá-la)
        response_body = [section async for section in response.body_iterator]
        # response_data = {
        #     "status_code": response.status_code,
        #     "headers": {**response.headers, self.options.TraceIdReponseHeader: trace_id},
        #     "body": b"".join(response_body).decode()
        # }
        response_data = self._getBodyResponse(
            status_code=response.status_code, 
            headers={**response.headers, self.options.TraceIdReponseHeader: trace_id},
            body=b"".join(response_body).decode()
        )

        #response_data = self._getBody(self.options.FormatType, trace_id, response.status_code, response.headers, response_body)

        # Recria a response e adiciona o TraceId no header
        new_response = Response(
            content=b"".join(response_body),
            status_code=response.status_code,
            headers={**response.headers, self.options.TraceIdReponseHeader: trace_id},
            media_type=response.media_type
        )

        # Determine log level based on status code family
        if 200 <= response.status_code < 300:
            log_level = LogLevel.SUCCESS
        elif 300 <= response.status_code < 400:
            log_level = LogLevel.WARNING
        elif 400 <= response.status_code < 500:
            log_level = LogLevel.ERROR
        else:
            log_level = LogLevel.FATAL

        # Send response if its not a GET or if LogGetRequest is True
        if(request.method != 'GET' or (request.method == 'GET' and self.options.LogGetRequest)):
            asyncio.create_task(self.send_log(data=response_data, trace_id=trace_id, type="Response", log_level=log_level))


        return new_response

    async def send_log(self, data:any, trace_id:str, type:str, log_level:LogLevel = LogLevel.INFO):
        """Log request or response in background"""
        #logging.info(f"{type}:\n" + json.dumps(data, indent=4))
        body = ""
        try:
          body = data
        except:
          body = ""
          
        self.logger.LogAsync(level=log_level, message=type, data=body, traceId=trace_id)



    def _getBodyRequest(self, method:str, url:str, headers:dict, body:any):

        if(self.options.FormatType == SaveFormatType.HTTPText):
            return f"{method} {url}\n" + "\n".join([f"{key}: {value}" for key, value in headers.items()]) + f"\n\n{body}"
    
        data = {
            "method": method,	
            "url": url,
            "headers": dict(headers), # type: ignore
            "body": body
        }

        return json.dumps(data, indent=4)
    
    def _getBodyResponse(self, status_code:int, headers:dict, body:any):

        if(self.options.FormatType == SaveFormatType.HTTPText):
            return f"StatusCode: {status_code}\n" + "\n".join([f"{key}: {value}" for key, value in headers.items()]) + f"\n\n{body}"

        data = {
            "status_code": status_code,	
            "headers": headers,
            "body": body
        }

        return json.dumps(data, indent=4)