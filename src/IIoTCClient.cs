// Copyright (c) Luca Druda. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using iotc_csharp_device_client.enums;
using System;
using System.Threading.Tasks;

public interface IIoTCClient
{
    /**
     * Set connection protocol (MQTT, AMQP, HTTPS). Must be called before Connect().
     * 
     * @param transport Trasport to set. Default: MQTT
     * 
     */
    void SetProtocol(IoTCProtocol transport);

    /**
     * Set DPS global endpoint. Must be called before Connect()
     * 
     * @param endpoint Endpoint of DPS server. Default:
     *                 "global.azure-devices-provisioning.net"
     * 
     */
    void SetGlobalEndpoint(string endpoint);

    /**
     * Set DPS API version. Default is "2018-09-01-preview"
     * @param apiversion the api version to set
     */
    void SetDPSApiVersion(string apiversion);

    /**
     * Set network proxy Must be called before Connect(). Default: no proxy
     * 
     * @param options Proxy options
     * 
     */
    void SetProxy(HttpProxyOptions options);

    /**
     * Set logging level (FULL, API_ONLY, DISABLED). Can be changed at any time.
     * 
     * @param logLevel The logging level. Default: DISABLED
     */
    void SetLogging(IoTCLogging logLevel);

    /**
     * Disconnect device.
     * 
     * @param callback Callback executing when device successfully disconnect.
     * 
     * @throws IoTCentralException if disconnection fails
     */
    Task Disconnect(Action<object> callback);

    /**
     * Connect device.
     * 
     * @throws IoTCentralException if connection fails
     */
    Task Connect();

    /**
     * Send a telemetry message.
     * 
     * @param payload  The telemetry object. Can include multiple values in a
     *                 flatten object. It can be a map, a POJO or a JSON string
     * @param callback The callback to execute when message is delivered to the hub
     * @throws IoTCentralException if connection is dropped
     */
    Task SendTelemetry(Object payload, Action<object> callback);

    /**
     * Send a state message
     * 
     * @param payload  The state object. Can include multiple values in a flatten
     *                 object. It can be a map, a POJO or a JSON string
     * @param callback The callback to execute when message is delivered to the hub
     * @throws IoTCentralException if connection is dropped
     */
    Task SendState(Object payload, Action<object> callback);

    /**
     * Send events
     * 
     * @param payload  The event object. Can include multiple events in a flatten
     *                 object. It can be a map, a POJO or a JSON string
     * @param callback The callback to execute when message is delivered to the hub
     * @throws IoTCentralException if connection is dropped
     */
    Task SendEvent(Object payload, Action<object> callback);

    /**
     * Send update values to properties.
     * 
     * @param payload  The property object. Can include multiple values in a flatten
     *                 object. It can be a map, a POJO or a JSON string. If property
     *                 is sent in the form {propertyName:{value:"value"}} and
     *                 propertyName is the name of a command, then it sends updates
     *                 to specific command tile in IoTCentral( e.g. command
     *                 progress)
     * @param callback The callback to execute when message is delivered to the hub
     * @throws IoTCentralException if connection is dropped
     */
    Task SendProperty(Object payload, Action<object> callback);

    /**
     * Listen to events.
     * 
     * @param event    The event to listen to.
     * @param callback The callback to execute when the event is triggered
     */
    void on(IoTCEvents iotcevent, Delegate callback);

}