from pydantic import BaseModel


class FieldNamePath(BaseModel):
    field_name: str
