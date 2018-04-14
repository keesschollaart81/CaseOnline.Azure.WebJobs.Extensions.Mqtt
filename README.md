# Mqtt Trigger Binding for Azure Functions
[![Build Status](https://caseonline.visualstudio.com/_apis/public/build/definitions/4df87c38-5691-4d04-8373-46c830209b7e/11/badge)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_build/index?definitionId=1) 
[![BCH compliance](https://bettercodehub.com/edge/badge/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branch=master)](https://bettercodehub.com/)
[![NuGet](https://img.shields.io/nuget/v/CaseOnline.Azure.WebJobs.Extensions.Mqtt.svg)](https://www.nuget.org/packages/CaseOnline.Azure.WebJobs.Extensions.Mqtt/)

This repository contains the code for the CaseOnline.Azure.WebJobs.Extensions.Mqtt NuGet Package. 

This package enables you to:

* Trigger an Azure Function based on a MQTT Subscription
* Publish a message to a MQTT topic as a result of an Azure Function. 


## How to use

* [Getting Started](wiki/Getting-started)
* [Publish via output](wiki/Publish-via-output)
* [Subscribe via trigger](wiki/Subscribe-via-trigger)
* [And more in the Wiki](wiki)

## Examples

Please find some examples here in the [sample project](./src/ExampleFunctions/). 

## References

- [MQTTnet](https://github.com/chkr1011/MQTTnet)

## Roadmap

- 1.0.0 Initial release, april 2018

## MIT License
Copyright (c) 2018 Kees Schollaart

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
