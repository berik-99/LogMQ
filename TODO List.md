# TODO List

## 🛑 Critical 
- [ ] **Add a user interface**

## ⚠️ Important
- [ ] **Write tests**
- [ ] **Improve fallback logger management** (allow users to choose whether to stop the application if logging fails)
- [ ] **Use multi target framework** in LogMQ modules

## ✨ Enhancement
- [ ] **Implement other data exchange methods** (e.g. gRPC, WebSocket, etc.)
- [ ] **Implement new loggers** (e.g. Log4net, NLog, etc.)
- [ ] **Add a TraceID to log metadata** (identifies the call stack)
- [ ] **Add ASP.NET Core support** (e.g., use specific metadata for HTTP requests)
- [ ] **Add support for logging to cloud services** (e.g., Azure Monitor, AWS CloudWatch)
- [ ] **Add alerting system with email or other messaging types**

## 🟢 Low Priority
- [ ] **Implement a custom logger**
- [ ] **Add more detailed documentation**
- [ ] **Refactor code for better readability**
- [ ] **Add more examples and tutorials**

## 🔄 In Progress
- [ ] **Add plugin system** in the Broker to allow the community to create new communication systems
- [ ] **Add local storage system** for messages not sent by the log provider that goes into exception after n messages

## ✅ Done
- [x] **Divide the project into multiple projects** (by log providers)
- [x] **Use Application object inside LogMessage**
- [x] **Use a custom implementation for MSMQ** (MSMQ.Messaging is not maintained and contains vulnerabilities)
- [x] **Use dynamic logo theme in README**