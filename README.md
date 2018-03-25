# Mqtt Trigger Binding for Azure Functions

This is a work-in-progress of a Mqtt Trigger for Azure Functions.

The code currently works as it is but I have to make it better configurable before I publish it as a NuGet Package. Things that have to be done:
- Unit Tests & Integration tests
- extensions.json is now manually created, which should not be needed
- Use ILogger instead of TraceWriter which currently does not output for some reason?
- Figure out if this is stable in the long run, will the connection persist days (currently tested for hours)

[![Build Status](https://caseonline.visualstudio.com/_apis/public/build/definitions/4df87c38-5691-4d04-8373-46c830209b7e/11/badge)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_build/index?definitionId=1) 
[![BCH compliance](https://bettercodehub.com/edge/badge/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branch=master)](https://bettercodehub.com/)

## How to use
- Create an Azure Function using [Visual Studio](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs) or using [Visual Studio Code](https://code.visualstudio.com/tutorials/functions-extension/getting-started)
- Make sure your ```Microsoft.NET.Sdk.Functions``` package version is 1.0.7 or higher
- Install the [CaseOnline.Azure.WebJobs.Extensions.Mqtt](https://www.nuget.org/packages/CaseOnline.Azure.WebJobs.Extensions.Mqtt/) NuGet package in you Functions Project
- Add an [extensions.json](./src/ExampleFunctions/extensions.json) to your Functions project root directory and [make sure it is copied to the build/publish folder](./src/ExampleFunctions/ExampleFunctions.csproj#L23-L25)
    - This is temporary for now. This should not be needed, monitor [issue/bug here](https://github.com/Azure/Azure-Functions/issues/624)) 
- Add the Mqtt server settings to your 'local.settings.json' during development time or to your appsettings when running on Azure:
    - MqttServer (just the dns/hostname)
    - MqttUsername
    - MqttPassword
    - MqttPort (optional, defaults to 1883)
    - MqttClientId (optional, defaults to a random Guid)
- Add a ```MqttTrigger``` attribute to your function parameters:

    ```
    public static void MyFunction([MqttTrigger(new[] { "my/mqtt/topic" })]PublishedMqttMessage message) 
    ```

- Deploy / Run your function. Azure function will subscripe to the Mqtt server/topic(s) and trigger your function when messages get published

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

## MIT License
Copyright (c) 2018 Kees Schollaart

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
