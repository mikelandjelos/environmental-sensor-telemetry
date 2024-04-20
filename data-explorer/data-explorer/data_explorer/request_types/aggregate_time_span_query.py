from datetime import datetime, timedelta
from enum import Enum
from typing import Optional

from pydantic import BaseModel, Field


class AggregationType(Enum):
    MIN = "MIN"
    MAX = "MAX"
    MEAN = "MEAN"
    SUM = "SUM"


class AggregateTimeSpanQuery(BaseModel):
    start_time: datetime
    stop_time: datetime
    window_duration: timedelta
    aggregation_type: AggregationType
