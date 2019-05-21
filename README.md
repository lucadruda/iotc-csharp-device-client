# Azure IoT Central C# SDK Documentation

[![Join the chat at https://gitter.im/iotdisc/community](https://badges.gitter.im/iotdisc.svg)](https://gitter.im/iotdisc/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Licensed under the MIT License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/lucadruda/iotc-csharp-device-client/blob/master/LICENSE)



### Usage

```
IoTCClient client = new IoTCClient(deviceId, scopeId, credType, credentials);
```
*deviceId*   : Device ID
*scopeId*    : IoT Central Scope ID
*credType*   : IOTC_CONNECT => `IOTC_CONNECT.SYMM_KEY` or `IOTC_CONNECT.DEVICE_KEY` or `IOTC_CONNECT_X509_CERT`
*credentials*  : SAS key or x509 Certificate



##### SetLogging
Change the logging level
```
client.SetLogging(logLevel);
```

*logLevel*   : (default value is `IoTCLogging.DISABLED`)
```
enum IoTCLogging:
  IoTCLogging.DISABLED,
  IoTCLogging.API_ONLY,
  IoTCLogging.ALL
```

*i.e.* => client.SetLogging(IoTCLogging.API_ONLY)

##### SetGlobalEndpoint
Change the service endpoint URL
```
client.SetGlobalEndpoint(url)
```

*url*    : URL for service endpoint. (default value is `global.azure-devices-provisioning.net`)

\**call this before connect*

##### SetApiVersion
Change the provisioning service API version
```
client.SetApiVersion(apiVersion)
```

*apiVersion*    : API version for service endpoint. (default value is `2018-09-01-preview`)

\**call this before connect*

##### Connect
Connect device client. It returns if successfull or throws exception if connection fails.

```
await client.Connect()
```

##### SendTelemetry
Sends telemetry

```
await client.SendTelemetry(payload,[callback])
```

*payload*  : A payload object. It can be a serializable object or a JSON object or a JSON string.

i.e. `await client.SendTelemetry($"{{\"temp\":{temperature},\"pressure\":{pressure}}}", null)`
where Sample is a class with public properties.

##### SendState
Sends device state

```
await client.SendState(payload)
```

*payload*  : A payload object. It can be a serializable object or a JSON object or a JSON string.

i.e. `await client.SendState(new Flag("on"))`
where Flag is a class with public properties.

##### SendProperty
Sends a property

```
await client.SendProperty(payload)
```

*payload*  : A payload object. It can be a serializable object or a JSON object or a JSON string.

i.e. `await client.SendState(new Property("value"))`
where Property is a class with public properties.`

##### Disconnect
Disconnects device client

```
client.Disconnect([callback])
```

i.e. `await client.Disconnect()`

##### on
set event callback to listen events

`ConnectionStatus` : connection status has changed
`MessageSent`      : message was sent
`Command`          : a command received from Azure IoT Central
`SettingsUpdated`  : device settings were updated

i.e.
```
client.on(event, callback)
```
*event*  : IoTCEvents type
```
IoTCEvents.ConnectionStatus, 
IoTCEvents.MessageSent,
IoTCEvents.Command,
IoTCEvents.SettingsUpdated
```
