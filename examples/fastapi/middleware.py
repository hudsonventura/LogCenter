import logging
import json
import uuid
import asyncio
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from starlette.responses import Response
from contextvars import ContextVar

logging.basicConfig(level=logging.INFO)

trace_id_var: ContextVar[str] = ContextVar("trace_id", default="")

class LogMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        # Gera um TraceId único para a requisição
        trace_id = str(uuid.uuid4())
        trace_id_var.set(trace_id)


        # Captura os dados da request
        body = await request.body()
        request_data = {
            "method": request.method,
            "url": str(request.url),
            "headers": dict(request.headers),
            "body": body.decode() if body else None
        }
        asyncio.create_task(self.log_request_response(request_data, trace_id, type="Request"))

        # Processa a resposta normalmente
        response = await call_next(request)

        # Lê os dados da resposta (precisamos recriá-la)
        response_body = [section async for section in response.body_iterator]
        response_data = {
            "trace_id": trace_id,
            "status_code": response.status_code,
            "headers": dict(response.headers),
            "body": b"".join(response_body).decode()
        }

        # Recria a response e adiciona o TraceId no header
        new_response = Response(
            content=b"".join(response_body),
            status_code=response.status_code,
            headers={**response.headers, "X-Trace-Id": trace_id},
            media_type=response.media_type
        )

        # Faz o log em segundo plano sem atrasar a resposta ao cliente
        asyncio.create_task(self.log_request_response(response_data, trace_id, type="Response"))

        return new_response

    async def log_request_response(self, data, trace_id, type):
        """Faz o log da request e response em segundo plano."""
        logging.info(f"{type}:\n" + json.dumps(data, indent=4))

