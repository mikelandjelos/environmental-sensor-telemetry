from pydantic import BaseModel


class ErrorResponse(BaseModel):
    exception_message: str
