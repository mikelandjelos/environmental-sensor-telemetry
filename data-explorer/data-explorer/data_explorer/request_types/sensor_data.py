from datetime import datetime

from pydantic import BaseModel


class SensorData(BaseModel):
    timestamp: datetime
    device: str
    carbon_oxide: float
    humidity: float
    light: bool
    liquid_petroleum_gas: float
    motion: bool
    smoke: float
    temperature: float
