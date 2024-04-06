import csv
import datetime
from dataclasses import dataclass, fields
from typing import List


@dataclass
class EnvironmentalSensorTelemetryData:
    timestamp: datetime
    device: str
    carbon_oxide: float
    humidity: float
    light: bool
    liquid_petroleum_gas: float
    motion: bool
    smoke: float
    temperature: float


if __name__ == "__main__":
    csv_records: List[EnvironmentalSensorTelemetryData] = []

    with open(
        "./iot_telemetry_data.csv",
        "rt",
    ) as input_csv_file:
        csv_reader = csv.reader(input_csv_file)

        # Skip the first row
        next(csv_reader)

        for row in csv_reader:
            timestamp = datetime.datetime.fromtimestamp(float(row[0]))
            rfc3339_timestamp = timestamp.isoformat() + "Z"
            (
                device,
                carbon_oxide,
                humidity,
                light,
                liquid_petroleum_gas,
                motion,
                smoke,
                temperature,
            ) = row[1:]
            record = EnvironmentalSensorTelemetryData(
                timestamp=rfc3339_timestamp,
                device=device,
                carbon_oxide=float(carbon_oxide),
                humidity=float(humidity),
                light=light.lower() == "true",
                liquid_petroleum_gas=float(liquid_petroleum_gas),
                motion=motion.lower() == "true",
                smoke=float(smoke),
                temperature=float(temperature),
            )
            csv_records.append(record)

    # Write to another file, ready for getting ingested by the InfluxDB
    with open(
        "./environmental_sensor_telemetry_data.csv",
        "wt",
        newline="",
    ) as output_csv_file:
        csv_writer = csv.writer(output_csv_file)

        # Write the header
        csv_writer.writerows(
            [
                [
                    "#datatype measurement",
                    "time",
                    "tag",
                    "double",
                    "double",
                    "boolean",
                    "double",
                    "boolean",
                    "double",
                    "double",
                ],
                [
                    "est_data",
                    "timestamp",
                    "device",
                    "carbon_oxide",
                    "humidity",
                    "light",
                    "liquid_petroleum_gas",
                    "motion",
                    "smoke",
                    "temperature",
                ],
            ]
        )

        for record in csv_records:
            # Convert boolean values to lowercase strings
            light = "true" if record.light else "false"
            motion = "true" if record.motion else "false"
            csv_writer.writerow(
                [
                    "sensor_data",
                    record.timestamp,
                    record.device,
                    record.carbon_oxide,
                    record.humidity,
                    light,
                    record.liquid_petroleum_gas,
                    motion,
                    record.smoke,
                    record.temperature,
                ]
            )

    print("CSV write complete!")
