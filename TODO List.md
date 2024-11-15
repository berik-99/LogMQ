# TODO List

## 🛑 Critical 
- [ ] **Add a user interface**

## ⚠️ Important
- [ ] **Write tests**
- [ ] **Improve fallback logger management** (allow users to choose whether to stop the application if logging fails)
- [ ] **Use multi target framework** in LogMQ package and Serilog sinks
- [x] ~~**Use a custom implementation for MSMQ** (MSMQ.Messaging is not maintained and contains vulnerabilities)~~

## ✨ Enhancement
- [ ] **Consider using other data exchange methods** (e.g., gRPC, WebSocket or similar)
- [ ] **Implement new log providers** (e.g., log4net, etc.)
- [ ] **Implement a custom log provider**
- [ ] **Add a TraceID to log metadata** (identifies the call stack)
- [ ] **Add ASP.NET Core support** (e.g., use specific metadata for HTTP requests)
- [ ] **Add support for logging to cloud services** (e.g., Azure Monitor, AWS CloudWatch)
- [ ] **Add alerting system with email or other messaging types**
- [ ] **Add local storage system** for messages not sent by the log provider that goes into exception after n messages
- [ ] **Add plugin system** in the Broker to allow the community to create new communication systems
- [x] ~~**Use dynamic logo theme in README**~~
