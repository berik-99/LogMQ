# TODO List

## 🛑 Critical 
- [ ] **Add a user interface**

## ⚠️ Important
- [ ] **Write tests**
- [ ] **Use multi-target framework** in LogMQ modules
- [ ] **Improve Exception management in LogMessage serialization and deserialization**

## ✨ Enhancement
- [ ] **Implement other Provider-Receiver methods** (e.g., gRPC, WebSocket, SignalR, etc.)
- [ ] **Implement new Loggers** (e.g., log4net, NLog, etc.)
- [ ] **Add a TraceID to log metadata** (identifies the call stack)
- [ ] **Add ASP.NET Core support** (e.g., use specific metadata for HTTP requests)
- [ ] **Add support for logging to cloud services** (e.g., Azure Monitor, AWS CloudWatch)
- [ ] **Add alerting system with email or other messaging types**
- [ ] **Create extension method to register Serilog enricher**
- [ ] **Define Viewer UI** as Figma prototype
- [ ] **Split documentation for each Logger** to explain in detail how to use each one
- [ ] **Better communication error management** for eventual messages not sent by the log provider

## 🟢 Low Priority
- [ ] **Implement a LogMQ-specific Logger**
- [ ] **Add more detailed documentation**
- [ ] **Refactor code for better readability**
- [ ] **Add more examples and tutorials**
- [ ] **Replace WatsonWebSocket with standard tcp socket**

## 🔄 In Progress
- [ ] **Define Plugin protocol and create Plugin installer** to manage plugin installation (separate executable from broker and viewer)

## ✅ Done
- [x] **Divide the project into multiple projects** (by log providers)
- [x] **Use Application object inside LogMessage**
- [x] **Use a custom implementation for MSMQ** (MSMQ.Messaging is not maintained and contains vulnerabilities)
- [x] **Use dynamic logo theme in README**
- [x] **Fix wrong method metadata when Serilog async**
- [x] **Create extension method to register Serilog enricher**
- [x] **Add plugin system** in the Broker to allow the community to create new communication systems
- [x] **Improve fallback logger management** (allow users to choose the size of the retry queue)
- [x] **Implement new Loggers** Microsoft.Extensions.Logging