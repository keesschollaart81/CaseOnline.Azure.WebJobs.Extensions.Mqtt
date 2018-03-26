# Mqtt Trigger Binding for Azure Functions
[![Build Status](https://caseonline.visualstudio.com/_apis/public/build/definitions/4df87c38-5691-4d04-8373-46c830209b7e/11/badge)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_build/index?definitionId=1) 
[![BCH compliance](https://bettercodehub.com/edge/badge/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branch=master)](https://bettercodehub.com/)
[![NuGet](https://img.shields.io/nuget/v/CaseOnline.Azure.WebJobs.Extensions.Mqtt.svg)](https://www.nuget.org/packages/CaseOnline.Azure.WebJobs.Extensions.Mqtt/)


This repository contains the code for the CaseOnline.Azure.WebJobs.Extensions.Mqtt NuGet Package. This package enables you to trigger an Azure Function based on a MQTT Subscription. By binding a MqttTrigger attribute as an input parameter for your function, you'll receive messages of type PublishedMqttMessage. Internally this is wired up using [MQTTnet](https://github.com/chkr1011/MQTTnet).

## How to use
- Create an Azure Function using [Visual Studio](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs) or using [Visual Studio Code](https://code.visualstudio.com/tutorials/functions-extension/getting-started)
- Make sure your ```Microsoft.NET.Sdk.Functions``` package version is 1.0.7 or higher
- Install the [CaseOnline.Azure.WebJobs.Extensions.Mqtt](https://www.nuget.org/packages/CaseOnline.Azure.WebJobs.Extensions.Mqtt/) NuGet package in you Functions Project
- Add the Mqtt server settings to your 'local.settings.json' during development time or to your appsettings when running on Azure:
    - MqttServer (just the dns/hostname)
    - MqttUsername
    - MqttPassword
    - MqttPort (optional, defaults to 1883)
    - MqttClientId (optional, defaults to a random Guid)
- When deploying/running on Azure set/ad the application-setting ```FUNCTIONS_EXTENSION_VERSION``` to ```beta```
- Add a ```MqttTrigger``` attribute to your function parameters:

    ```
    public static void MyFunction([MqttTrigger(new[] { "my/mqtt/topic" })]PublishedMqttMessage message) 
    ```

- Deploy / Run your function. Azure function will subscripe to the Mqtt server/topic(s) and trigger your function when messages get published

## Beta - Work in progress!!
The code currently works as it is but I have to make it better configurable before I publish it as a NuGet Package. Things that have to be done:
- Have more beta-testers running/using it (please help! 😇)
- Create more Unit Tests & Integration tests and get code coverage to >80%
- Use ILogger instead of TraceWriter which currently does not output on dev machine for some reason? 
- Create demos for integrations with CloudMqtt.net and Azure IoT Hub

Expect a 1.0.0 version of the NuGet package in April '18.

## Custom MQTT-Client Configuration
Internally the [MQTTnet](https://github.com/chkr1011/MQTTnet) is used for the Mqtt implementation.  For complexer configurations, for example if you want to 
- use Tls
- pecial certificates 
- custom logic for setting the topics/clientId 
- connecting over websockets
- control the QoS level per topic

To do this, implemented a custom ```ICreateMqttConfig``` and provide this Type as parameter to the ```MqttTrigger``` like this:
    
```
public static void MyFunction([MqttTrigger(typeof(MyMqttConfigProvider))]PublishedMqttMessage message)
```
     
In your implementation of ```ICreateMqttConfig``` you need to return an instance of abstract class ```MqttConfig``` which requires you to implement a property of type ```IManagedMqttClientOptions```. Examples on how to build an instance of ```IManagedMqttClientOptions``` are available in the  [ManagedClient wiki of MQTTnet](https://github.com/chkr1011/MQTTnet/wiki/Client).

An example of this custom client configuration is implemented in [this function](./src/ExampleFunctions/ExampleFunctions.cs#L34). 

## Examples
Please find some samples here in the [sample project](./src/ExampleFunctions/). The simple example subscribes to a topic where [Owntracks](http://owntracks.org/) publishes location information.

## References
- [MQTTnet](https://github.com/chkr1011/MQTTnet)

## Roadmap
- 1.0.0 Initial release, april 2018
- 1.5.0 Output binding for publishing MQTT messages, june 2018

## MIT License
Copyright (c) 2018 Kees Schollaart

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
