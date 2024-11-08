# TODO List

## 🛑 Critical 
- [ ] **Add an interface**

## ⚠️ Important
- [ ] **Write tests**
- [ ] **Improve fallback logger management** (allow users to choose whether to stop the application if logging fails)

## ✨ Enhancement
- [ ] **Consider using other data exchange methods** (e.g., gRPC, Websocket or similar)
- [ ] **Implement new loggers** (e.g., log4net, etc.)
- [ ] **Implement a custom logger**
- [ ] **Add a TraceID to log metadata** (identifies the call stack from the controller to the DB read and back to the response)
- [ ] **Add support for logging to cloud services** (e.g., Azure Monitor, AWS CloudWatch)
- [ ] **Add alerting system with email or other messaging types**
- [ ] **Add local storage system for messages not sent by the log provider that goes into exception after n messages (n configured by the user)**

