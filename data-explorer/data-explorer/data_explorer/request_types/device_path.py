from pydantic import BaseModel


class DevicePath(BaseModel):
    device: str
