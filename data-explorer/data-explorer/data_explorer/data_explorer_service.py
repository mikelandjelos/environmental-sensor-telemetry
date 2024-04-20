import logging
import os
from http import HTTPStatus

import grpc
import grpc._typing
from flask import Response, request
from flask_openapi3 import Info, OpenAPI, Tag
from google.protobuf.duration_pb2 import Duration
from google.protobuf.empty_pb2 import Empty
from google.protobuf.json_format import MessageToDict

from .grpc_types.environmental_sensor_telemetry_service_pb2 import (
    AggregateOverTimeSpanRequest,
    AggregateOverTimeSpanResponse,
    DeleteForTimeSpanRequest,
    DeleteForTimeSpanResponse,
    EnvironmentalSensorTelemetryData,
    PaginationInfo,
    PingResponse,
    ReadFieldsOverTimeSpanRequest,
    ReadFieldsOverTimeSpanResponse,
    ReadMeasurementsOverTimeSpanRequest,
    ReadMeasurementsOverTimeSpanResponse,
    WriteBatchRequest,
    WriteDataResponse,
)
from .grpc_types.environmental_sensor_telemetry_service_pb2_grpc import (
    EnvironmentalSensorTelemetryStub,
)
from .request_types import (
    AggregateTimeSpanQuery,
    DeleteTimeSpanQuery,
    DevicePath,
    FieldNamePath,
    ReadOverTimeSpanQuery,
    SensorDataBatchBody,
)
from .response_types import DataExplorerResponse, ErrorResponse
from .utils import datetime_to_timestamp, to_pb_aggregation_type

GRPC_SERVER_ADDRESS = os.getenv("GRPC_SERVER_ADDRESS")

if GRPC_SERVER_ADDRESS is None:
    raise EnvironmentError("GRPC_SERVER_ADDRESS environment variable not set")

info = Info(title="Environmental Sensor Telemetry - Data explorer API", version="1.0.0")

data_explorer_service = OpenAPI(__name__, info=info)

__logger = logging.getLogger(__name__)

__channel = grpc.insecure_channel(GRPC_SERVER_ADDRESS)
__clientStub = EnvironmentalSensorTelemetryStub(__channel)


@data_explorer_service.before_request
def log_request_info():
    data_explorer_service.logger.info(f"Remote address: {request.remote_addr}")
    data_explorer_service.logger.info(f"Endpoint: {request.endpoint}")
    data_explorer_service.logger.info(f"Headers:\n{request.headers}")


@data_explorer_service.get(
    "/ping",
    summary="Used to check status of all connected services.",
    tags=[Tag(name="ping")],
    responses={
        HTTPStatus.OK: DataExplorerResponse,
        HTTPStatus.INTERNAL_SERVER_ERROR: ErrorResponse,
    },
)
def ping():
    """
    Round-trip ping across the services:
        DataExplorer(Flask)<--ping/pong-->DataProvider(.NET Core)<--ping/pong-->InfluxDB
    """
    try:
        ping_response: PingResponse = __clientStub.Ping(Empty())

        response = DataExplorerResponse(
            inner_response=MessageToDict(ping_response),
            message="Data explorer service pinged succesfully!",
        )

        return Response(
            content_type="application/json",
            status=HTTPStatus.OK,
            response=response.model_dump_json(),
        )
    except Exception as ex:
        __logger.error(ex, exc_info=True)

        return Response(
            content_type="application/json",
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            response=ErrorResponse(exception_message=str(ex)).model_dump_json(),
        )


@data_explorer_service.post(
    "/writeMeasurementsBatched",
    summary="Used to write batch of sensor measurements into DB.",
    tags=[Tag(name="writeMeasurementsBatched")],
    responses={
        HTTPStatus.OK: DataExplorerResponse,
        HTTPStatus.INTERNAL_SERVER_ERROR: ErrorResponse,
    },
)
def write_measurements_batched(body: SensorDataBatchBody):
    try:
        write_batch_request = WriteBatchRequest(
            data=[
                EnvironmentalSensorTelemetryData(
                    timestamp=datetime_to_timestamp(sensor_data.timestamp),
                    device=sensor_data.device,
                    carbon_oxide=sensor_data.carbon_oxide,
                    humidity=sensor_data.humidity,
                    light=sensor_data.light,
                    liquid_petroleum_gas=sensor_data.liquid_petroleum_gas,
                    motion=sensor_data.motion,
                    smoke=sensor_data.smoke,
                    temperature=sensor_data.temperature,
                )
                for sensor_data in body.data
            ]
        )

        write_batch_response: WriteDataResponse = __clientStub.WriteMeasurementsBatched(
            write_batch_request
        )

        response: DataExplorerResponse = DataExplorerResponse(
            inner_response=MessageToDict(write_batch_response),
            message="Succesfully invoked writting of data",
        )

        return Response(
            content_type="application/json",
            status=HTTPStatus.OK,
            response=response.model_dump_json(),
        )
    except Exception as ex:
        __logger.error(ex, exc_info=True)

        return Response(
            content_type="application/json",
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            response=ErrorResponse(exception_message=str(ex)).model_dump_json(),
        )


@data_explorer_service.post(
    "/writeMeasurementsStream",
    summary="Used to write batch of sensor measurements into DB, one by one in a stream.",
    tags=[Tag(name="writeMeasurementsStream")],
    responses={
        HTTPStatus.OK: DataExplorerResponse,
        HTTPStatus.INTERNAL_SERVER_ERROR: ErrorResponse,
    },
)
def write_measurements_stream(body: SensorDataBatchBody):
    try:
        written_data_info = []

        sensor_data_stream = (
            EnvironmentalSensorTelemetryData(
                timestamp=datetime_to_timestamp(sensor_data.timestamp),
                device=sensor_data.device,
                carbon_oxide=sensor_data.carbon_oxide,
                humidity=sensor_data.humidity,
                light=sensor_data.light,
                liquid_petroleum_gas=sensor_data.liquid_petroleum_gas,
                motion=sensor_data.motion,
                smoke=sensor_data.smoke,
                temperature=sensor_data.temperature,
            )
            for sensor_data in body.data
        )

        for info in __clientStub.WriteMeasurementsStream(sensor_data_stream):
            written_data_info.append(MessageToDict(info))

        response: DataExplorerResponse = DataExplorerResponse(
            inner_response={"response_stream": written_data_info},
            message="Succesfully finished writting stream of data",
        )

        return Response(
            content_type="application/json",
            status=HTTPStatus.OK,
            response=response.model_dump_json(),
        )
    except Exception as ex:
        __logger.error(ex, exc_info=True)

        return Response(
            content_type="application/json",
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            response=ErrorResponse(exception_message=str(ex)).model_dump_json(),
        )


@data_explorer_service.get(
    "/readMeasurementsOverTimeSpan",
    summary="Used to read measurements over given a time span.",
    tags=[Tag(name="readMeasurementsOverTimeSpan")],
    responses={
        HTTPStatus.OK: DataExplorerResponse,
        HTTPStatus.INTERNAL_SERVER_ERROR: ErrorResponse,
    },
)
def readMeasurementsOverTimeSpan(query: ReadOverTimeSpanQuery):
    try:
        request = ReadMeasurementsOverTimeSpanRequest(
            start_time=datetime_to_timestamp(query.start_time),
            stop_time=(
                datetime_to_timestamp(query.stop_time)
                if query.stop_time is not None
                else None
            ),
            pagination_info=(
                PaginationInfo(
                    page_size=query.page_size,
                    offset=query.offset,
                )
                if query.page_size is not None and query.offset is not None
                else None
            ),
        )

        read_measurements_over_time_span_response: (
            ReadMeasurementsOverTimeSpanResponse
        ) = __clientStub.ReadMeasurementsOverTimeSpan(request)

        response = DataExplorerResponse(
            inner_response=MessageToDict(read_measurements_over_time_span_response),
            message=f"Sucessfully read measurements between {query.start_time} and {query.stop_time or 'now'}",
        )

        return Response(
            content_type="application/json",
            status=HTTPStatus.OK,
            response=response.model_dump_json(),
        )
    except Exception as ex:
        __logger.error(ex, exc_info=True)

        return Response(
            content_type="application/json",
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            response=ErrorResponse(exception_message=str(ex)).model_dump_json(),
        )


@data_explorer_service.get(
    "/readFieldsOverTimeSpan/<string:field_name>",
    summary="Used to read measurement fields over a given time span.",
    tags=[Tag(name="readFieldsOverTimeSpan")],
    responses={
        HTTPStatus.OK: DataExplorerResponse,
        HTTPStatus.INTERNAL_SERVER_ERROR: ErrorResponse,
    },
)
def read_fields_over_time_span(path: FieldNamePath, query: ReadOverTimeSpanQuery):
    try:
        request = ReadFieldsOverTimeSpanRequest(
            field_name=path.field_name,
            start_time=datetime_to_timestamp(query.start_time),
            stop_time=(
                datetime_to_timestamp(query.stop_time)
                if query.stop_time is not None
                else None
            ),
            pagination_info=(
                PaginationInfo(
                    page_size=query.page_size,
                    offset=query.offset,
                )
                if query.page_size is not None and query.offset is not None
                else None
            ),
        )

        read_fields_over_time_span_response: ReadFieldsOverTimeSpanResponse = (
            __clientStub.ReadFieldsOverTimeSpan(request)
        )

        response = DataExplorerResponse(
            inner_response=MessageToDict(read_fields_over_time_span_response),
            message=f"Sucessfully read measurements between {query.start_time} and {query.stop_time or 'now'}",
        )

        return Response(
            content_type="application/json",
            status=HTTPStatus.OK,
            response=response.model_dump_json(),
        )
    except Exception as ex:
        __logger.error(ex, exc_info=True)

        return Response(
            content_type="application/json",
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            response=ErrorResponse(exception_message=str(ex)).model_dump_json(),
        )


@data_explorer_service.get(
    "/aggregateOverTimeSpan/<string:field_name>",
    summary="Used to aggregate values of given field over a given time span.",
    tags=[Tag(name="aggregateOverTimeSpan")],
    responses={
        HTTPStatus.OK: DataExplorerResponse,
        HTTPStatus.INTERNAL_SERVER_ERROR: ErrorResponse,
    },
)
def aggregate_over_time_span(path: FieldNamePath, query: AggregateTimeSpanQuery):
    try:
        window_duration_pb = Duration()
        window_duration_pb.FromTimedelta(td=query.window_duration)

        request = AggregateOverTimeSpanRequest(
            field_name=path.field_name,
            start_time=datetime_to_timestamp(query.start_time),
            stop_time=(
                datetime_to_timestamp(query.stop_time)
                if query.stop_time is not None
                else None
            ),
            window_duration=window_duration_pb,
            aggregation_type=to_pb_aggregation_type(query.aggregation_type),
        )

        aggregate_over_time_span_response: AggregateOverTimeSpanResponse = (
            __clientStub.AggregateOverTimeSpan(request)
        )

        response = DataExplorerResponse(
            inner_response=MessageToDict(aggregate_over_time_span_response),
            message=f"Sucessfully aggregated field values between {query.start_time} and {query.stop_time or 'now'}",
        )

        return Response(
            content_type="application/json",
            status=HTTPStatus.OK,
            response=response.model_dump_json(),
        )
    except Exception as ex:
        __logger.error(ex, exc_info=True)

        return Response(
            content_type="application/json",
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            response=ErrorResponse(exception_message=str(ex)).model_dump_json(),
        )


@data_explorer_service.delete(
    "/deleteForTimeSpan/<string:device>",
    summary="Used to values on given device taken during a given time span.",
    tags=[Tag(name="deleteForTimeSpan")],
    responses={
        HTTPStatus.OK: DataExplorerResponse,
        HTTPStatus.INTERNAL_SERVER_ERROR: ErrorResponse,
    },
)
def delete_for_time_span(path: DevicePath, query: DeleteTimeSpanQuery):
    try:
        request = DeleteForTimeSpanRequest(
            start_time=datetime_to_timestamp(query.start_time),
            stop_time=(datetime_to_timestamp(query.stop_time)),
            device=path.device,
        )

        delete_for_time_span_response: DeleteForTimeSpanResponse = (
            __clientStub.DeleteForTimeSpan(request)
        )

        response = DataExplorerResponse(
            inner_response=MessageToDict(delete_for_time_span_response),
            message=f"Sucessfully deleted measurements between {query.start_time} and {query.stop_time or 'now'}",
        )

        return Response(
            content_type="application/json",
            status=HTTPStatus.OK,
            response=response.model_dump_json(),
        )
    except Exception as ex:
        __logger.error(ex, exc_info=True)

        return Response(
            content_type="application/json",
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            response=ErrorResponse(exception_message=str(ex)).model_dump_json(),
        )
