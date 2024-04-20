from pydantic import BaseModel, Field

from .sensor_data import SensorData


class SensorDataBatchBody(BaseModel):
    data: list[SensorData] = Field(default_factory=list)
