from google.protobuf import empty_pb2 as _empty_pb2
from google.protobuf import timestamp_pb2 as _timestamp_pb2
from google.protobuf import struct_pb2 as _struct_pb2
from google.protobuf import duration_pb2 as _duration_pb2
from google.protobuf.internal import containers as _containers
from google.protobuf.internal import enum_type_wrapper as _enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Iterable as _Iterable, Mapping as _Mapping, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class AggregationType(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    NIL: _ClassVar[AggregationType]
    MIN: _ClassVar[AggregationType]
    MAX: _ClassVar[AggregationType]
    MEAN: _ClassVar[AggregationType]
    SUM: _ClassVar[AggregationType]
NIL: AggregationType
MIN: AggregationType
MAX: AggregationType
MEAN: AggregationType
SUM: AggregationType

class EnvironmentalSensorTelemetryData(_message.Message):
    __slots__ = ("timestamp", "device", "carbon_oxide", "humidity", "light", "liquid_petroleum_gas", "motion", "smoke", "temperature")
    TIMESTAMP_FIELD_NUMBER: _ClassVar[int]
    DEVICE_FIELD_NUMBER: _ClassVar[int]
    CARBON_OXIDE_FIELD_NUMBER: _ClassVar[int]
    HUMIDITY_FIELD_NUMBER: _ClassVar[int]
    LIGHT_FIELD_NUMBER: _ClassVar[int]
    LIQUID_PETROLEUM_GAS_FIELD_NUMBER: _ClassVar[int]
    MOTION_FIELD_NUMBER: _ClassVar[int]
    SMOKE_FIELD_NUMBER: _ClassVar[int]
    TEMPERATURE_FIELD_NUMBER: _ClassVar[int]
    timestamp: _timestamp_pb2.Timestamp
    device: str
    carbon_oxide: float
    humidity: float
    light: bool
    liquid_petroleum_gas: float
    motion: bool
    smoke: float
    temperature: float
    def __init__(self, timestamp: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., device: _Optional[str] = ..., carbon_oxide: _Optional[float] = ..., humidity: _Optional[float] = ..., light: bool = ..., liquid_petroleum_gas: _Optional[float] = ..., motion: bool = ..., smoke: _Optional[float] = ..., temperature: _Optional[float] = ...) -> None: ...

class FluxRecordCompact(_message.Message):
    __slots__ = ("field_name", "timestamp", "value")
    FIELD_NAME_FIELD_NUMBER: _ClassVar[int]
    TIMESTAMP_FIELD_NUMBER: _ClassVar[int]
    VALUE_FIELD_NUMBER: _ClassVar[int]
    field_name: str
    timestamp: _timestamp_pb2.Timestamp
    value: _struct_pb2.Value
    def __init__(self, field_name: _Optional[str] = ..., timestamp: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., value: _Optional[_Union[_struct_pb2.Value, _Mapping]] = ...) -> None: ...

class ResponseMetadata(_message.Message):
    __slots__ = ("status_code", "message")
    STATUS_CODE_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status_code: int
    message: str
    def __init__(self, status_code: _Optional[int] = ..., message: _Optional[str] = ...) -> None: ...

class PaginationInfo(_message.Message):
    __slots__ = ("page_size", "offset")
    PAGE_SIZE_FIELD_NUMBER: _ClassVar[int]
    OFFSET_FIELD_NUMBER: _ClassVar[int]
    page_size: int
    offset: int
    def __init__(self, page_size: _Optional[int] = ..., offset: _Optional[int] = ...) -> None: ...

class PingResponse(_message.Message):
    __slots__ = ("pong", "metadata")
    PONG_FIELD_NUMBER: _ClassVar[int]
    METADATA_FIELD_NUMBER: _ClassVar[int]
    pong: str
    metadata: ResponseMetadata
    def __init__(self, pong: _Optional[str] = ..., metadata: _Optional[_Union[ResponseMetadata, _Mapping]] = ...) -> None: ...

class WriteBatchRequest(_message.Message):
    __slots__ = ("data",)
    DATA_FIELD_NUMBER: _ClassVar[int]
    data: _containers.RepeatedCompositeFieldContainer[EnvironmentalSensorTelemetryData]
    def __init__(self, data: _Optional[_Iterable[_Union[EnvironmentalSensorTelemetryData, _Mapping]]] = ...) -> None: ...

class WriteDataResponse(_message.Message):
    __slots__ = ("written_at", "metadata")
    WRITTEN_AT_FIELD_NUMBER: _ClassVar[int]
    METADATA_FIELD_NUMBER: _ClassVar[int]
    written_at: _timestamp_pb2.Timestamp
    metadata: ResponseMetadata
    def __init__(self, written_at: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., metadata: _Optional[_Union[ResponseMetadata, _Mapping]] = ...) -> None: ...

class ReadMeasurementsOverTimeSpanRequest(_message.Message):
    __slots__ = ("start_time", "stop_time", "pagination_info")
    START_TIME_FIELD_NUMBER: _ClassVar[int]
    STOP_TIME_FIELD_NUMBER: _ClassVar[int]
    PAGINATION_INFO_FIELD_NUMBER: _ClassVar[int]
    start_time: _timestamp_pb2.Timestamp
    stop_time: _timestamp_pb2.Timestamp
    pagination_info: PaginationInfo
    def __init__(self, start_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., stop_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., pagination_info: _Optional[_Union[PaginationInfo, _Mapping]] = ...) -> None: ...

class ReadMeasurementsOverTimeSpanResponse(_message.Message):
    __slots__ = ("records", "metadata")
    RECORDS_FIELD_NUMBER: _ClassVar[int]
    METADATA_FIELD_NUMBER: _ClassVar[int]
    records: _containers.RepeatedCompositeFieldContainer[EnvironmentalSensorTelemetryData]
    metadata: ResponseMetadata
    def __init__(self, records: _Optional[_Iterable[_Union[EnvironmentalSensorTelemetryData, _Mapping]]] = ..., metadata: _Optional[_Union[ResponseMetadata, _Mapping]] = ...) -> None: ...

class ReadFieldsOverTimeSpanRequest(_message.Message):
    __slots__ = ("start_time", "stop_time", "pagination_info", "field_name")
    START_TIME_FIELD_NUMBER: _ClassVar[int]
    STOP_TIME_FIELD_NUMBER: _ClassVar[int]
    PAGINATION_INFO_FIELD_NUMBER: _ClassVar[int]
    FIELD_NAME_FIELD_NUMBER: _ClassVar[int]
    start_time: _timestamp_pb2.Timestamp
    stop_time: _timestamp_pb2.Timestamp
    pagination_info: PaginationInfo
    field_name: str
    def __init__(self, start_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., stop_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., pagination_info: _Optional[_Union[PaginationInfo, _Mapping]] = ..., field_name: _Optional[str] = ...) -> None: ...

class ReadFieldsOverTimeSpanResponse(_message.Message):
    __slots__ = ("records", "metadata")
    RECORDS_FIELD_NUMBER: _ClassVar[int]
    METADATA_FIELD_NUMBER: _ClassVar[int]
    records: _containers.RepeatedCompositeFieldContainer[FluxRecordCompact]
    metadata: ResponseMetadata
    def __init__(self, records: _Optional[_Iterable[_Union[FluxRecordCompact, _Mapping]]] = ..., metadata: _Optional[_Union[ResponseMetadata, _Mapping]] = ...) -> None: ...

class AggregateOverTimeSpanRequest(_message.Message):
    __slots__ = ("start_time", "stop_time", "field_name", "window_duration", "aggregation_type")
    START_TIME_FIELD_NUMBER: _ClassVar[int]
    STOP_TIME_FIELD_NUMBER: _ClassVar[int]
    FIELD_NAME_FIELD_NUMBER: _ClassVar[int]
    WINDOW_DURATION_FIELD_NUMBER: _ClassVar[int]
    AGGREGATION_TYPE_FIELD_NUMBER: _ClassVar[int]
    start_time: _timestamp_pb2.Timestamp
    stop_time: _timestamp_pb2.Timestamp
    field_name: str
    window_duration: _duration_pb2.Duration
    aggregation_type: AggregationType
    def __init__(self, start_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., stop_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., field_name: _Optional[str] = ..., window_duration: _Optional[_Union[_duration_pb2.Duration, _Mapping]] = ..., aggregation_type: _Optional[_Union[AggregationType, str]] = ...) -> None: ...

class AggregateOverTimeSpanResponse(_message.Message):
    __slots__ = ("aggregate_windows", "metadata")
    class AggregateWindow(_message.Message):
        __slots__ = ("start", "stop", "value", "device")
        START_FIELD_NUMBER: _ClassVar[int]
        STOP_FIELD_NUMBER: _ClassVar[int]
        VALUE_FIELD_NUMBER: _ClassVar[int]
        DEVICE_FIELD_NUMBER: _ClassVar[int]
        start: _timestamp_pb2.Timestamp
        stop: _timestamp_pb2.Timestamp
        value: float
        device: str
        def __init__(self, start: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., stop: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., value: _Optional[float] = ..., device: _Optional[str] = ...) -> None: ...
    AGGREGATE_WINDOWS_FIELD_NUMBER: _ClassVar[int]
    METADATA_FIELD_NUMBER: _ClassVar[int]
    aggregate_windows: _containers.RepeatedCompositeFieldContainer[AggregateOverTimeSpanResponse.AggregateWindow]
    metadata: ResponseMetadata
    def __init__(self, aggregate_windows: _Optional[_Iterable[_Union[AggregateOverTimeSpanResponse.AggregateWindow, _Mapping]]] = ..., metadata: _Optional[_Union[ResponseMetadata, _Mapping]] = ...) -> None: ...

class DeleteForTimeSpanRequest(_message.Message):
    __slots__ = ("start_time", "stop_time", "device")
    START_TIME_FIELD_NUMBER: _ClassVar[int]
    STOP_TIME_FIELD_NUMBER: _ClassVar[int]
    DEVICE_FIELD_NUMBER: _ClassVar[int]
    start_time: _timestamp_pb2.Timestamp
    stop_time: _timestamp_pb2.Timestamp
    device: str
    def __init__(self, start_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., stop_time: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., device: _Optional[str] = ...) -> None: ...

class DeleteForTimeSpanResponse(_message.Message):
    __slots__ = ("metadata",)
    METADATA_FIELD_NUMBER: _ClassVar[int]
    metadata: ResponseMetadata
    def __init__(self, metadata: _Optional[_Union[ResponseMetadata, _Mapping]] = ...) -> None: ...
