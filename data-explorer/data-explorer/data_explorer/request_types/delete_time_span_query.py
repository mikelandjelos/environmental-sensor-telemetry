from datetime import datetime
from typing import Optional

from pydantic import BaseModel, Field


class DeleteTimeSpanQuery(BaseModel):
    start_time: datetime
    stop_time: datetime
