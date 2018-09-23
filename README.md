<h1 align="center">

<img src="https://raw.githubusercontent.com/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt/master/readme_banner.png" width=650 alt="CaseOnline.Azure.WebJobs.Extensions.Mqtt"/>
<br/>
Mqtt Bindings for Azure Functions
</h1>

<div align="center">

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt/blob/master/LICENSE)
[![BCH compliance](https://bettercodehub.com/edge/badge/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branch=master)](https://bettercodehub.com/)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=CaseOnline.Azure.WebJobs.Extensions.Mqtt&metric=coverage)](https://sonarcloud.io/dashboard?id=CaseOnline.Azure.WebJobs.Extensions.Mqtt)
[![Maintainability](https://sonarcloud.io/api/project_badges/measure?project=CaseOnline.Azure.WebJobs.Extensions.Mqtt&metric=sqale_rating)]()
</div>

This repository contains the code for the CaseOnline.Azure.WebJobs.Extensions.Mqtt NuGet Package. 

This package enables you to:

* Trigger an Azure Function based on a MQTT Subscription
* Publish a message to a MQTT topic as a result of an Azure Function

Are you curious what MQTT is? Check [this page](http://mqtt.org/faq)!

## How to use

**Important note:** This extension only works when you use AppService Plan! **Do not use this with Consumption plan!**

* [Getting Started](/../../wiki/Getting-started)
* [Publish via output](/../../wiki/Publish-via-output)
* [Subscribe via trigger](/../../wiki/Subscribe-via-trigger)
* [Integrate with Azure IoT Hub](/../../Azure-IoT-Hub)
* [And more in the Wiki](/../../wiki)

## Where to get

Install stable releases via Nuget; development releases are available via MyGet.

|                     | Master > NuGet | Dev > MyGet |
|--------------------------------|-----------------|-----------------|
| Build status |  [![Build Status](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_apis/build/status/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branchName=master)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_build/index?definitionId=11)   | [![Build Status](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_apis/build/status/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branchName=dev)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_build/index?definitionId=11)
| Deployment Status | [![Deployment Status](https://caseonline.vsrm.visualstudio.com/_apis/public/Release/badge/4df87c38-5691-4d04-8373-46c830209b7e/1/2)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_releases2?definitionId=1)|[![Deployment Status](https://caseonline.vsrm.visualstudio.com/_apis/public/Release/badge/4df87c38-5691-4d04-8373-46c830209b7e/1/3)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_releases2?definitionId=1)|
| Package | [![NuGet](https://img.shields.io/nuget/v/CaseOnline.Azure.WebJobs.Extensions.Mqtt.svg)](https://www.nuget.org/packages/CaseOnline.Azure.WebJobs.Extensions.Mqtt/) | [![MyGet](https://img.shields.io/myget/caseonline/v/CaseOnline.Azure.WebJobs.Extensions.Mqtt.svg)](https://www.myget.org/feed/caseonline/package/nuget/CaseOnline.Azure.WebJobs.Extensions.Mqtt) | 
## Examples

This is a simple example, receicing messages for topic ```my/topic/in``` and publishing messages on topic ```testtopic/out```.

``` csharp
public static class ExampleFunctions
{
    [FunctionName("SimpleFunction")]
    public static void SimpleFunction(
        [MqttTrigger("my/topic/in")] IMqttMessage message,
        [Mqtt] out IMqttMessage outMessage,
        ILogger logger)
    {
        var body = message.GetMessage();
        var bodyString = Encoding.UTF8.GetString(body);
        logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}: {bodyString}");
        outMessage = new MqttMessage("testtopic/out", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);
    }
}
```

Please find all working examples in the [sample project](./src/ExampleFunctions/). 


## References

- [MQTTnet](https://github.com/chkr1011/MQTTnet)

## MIT License
Copyright (c) 2018 Kees Schollaart

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
