from typing import Any

from pydantic import BaseModel, Field


class DataExplorerResponse(BaseModel):
    inner_response: dict[str, Any] | None = Field(default=None)
    message: str
