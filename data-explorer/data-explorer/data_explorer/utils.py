from datetime import datetime

from google.protobuf.timestamp_pb2 import Timestamp

from .grpc_types.environmental_sensor_telemetry_service_pb2 import (
    AggregationType as PB_AggregationType,
)
from .request_types import AggregationType


def to_pb_aggregation_type(aggregation_type: AggregationType) -> PB_AggregationType:
    if aggregation_type == AggregationType.MIN:
        return PB_AggregationType.MIN

    if aggregation_type == AggregationType.MAX:
        return PB_AggregationType.MAX

    if aggregation_type == AggregationType.MEAN:
        return PB_AggregationType.MEAN

    if aggregation_type == AggregationType.SUM:
        return PB_AggregationType.SUM


def datetime_to_timestamp(dt: datetime) -> Timestamp:
    seconds = int(dt.timestamp())
    nanos = int(dt.timestamp() % 1 * 1e9)

    return Timestamp(nanos=nanos, seconds=seconds)
