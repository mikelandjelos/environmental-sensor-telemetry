from datetime import datetime
from typing import Optional

from pydantic import BaseModel, Field


class ReadOverTimeSpanQuery(BaseModel):
    start_time: datetime
    stop_time: Optional[datetime] = Field(default=None)
    page_size: Optional[int] = Field(default=None)
    offset: Optional[int] = Field(default=None)
