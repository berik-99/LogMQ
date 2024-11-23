# 🌐 LogMQ - Cross-Platform Centralized Logging System

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="Assets/Logo/LogMQ_logo_horizontal_light.svg">
  <source media="(prefers-color-scheme: light)" srcset="Assets/Logo/LogMQ_logo_horizontal_dark.svg">
  <img alt="Logo" src="path/to/logo-dark.png">
</picture>

## ⚠️ Disclaimer

LogMQ is currently under **active development** and is not yet considered stable.  
Please use it at your own risk in non-production environments.

---

**LogMQ** is a client-server system for structured logging that allows you to use your preferred logger (Serilog, Log4net, NLog, etc.) to write logs to a centralized system, which can be local or remote. The system automatically routes messages by application and automatically saves not only the log message but also various metadata such as file, line, method, class, and more.

🔧 **Current State**: Written in **.NET** for **.NET** applications, LogMQ leverages popular loggers (Serilog, Log4net, NLog, etc.) to send log messages to the central broker, which is responsible for routing and persisting the log messages.  
🚀 **Future Plans**: Future versions will expand support to include other loggers and may support loggers from other languages, ensuring greater flexibility and compatibility in diverse environments.

## 🎯 Key Features

- **Centralized log management** across multiple applications and platforms.
- **Modular design** for easy extension and integration with other logging frameworks and communication channels.
- **Scalable architecture** leveraging a Message Queue for efficient log routing.
- **Future-proof**: Planned support for non-.NET loggers and other programming languages.

## 📖 Overview

The primary goal of **LogMQ** is to provide a robust, modular, yet simple solution for collecting, monitoring, and managing logs from applications distributed across multiple platforms. This enhances visibility and traceability of events, issues, and anomalies in real time or during post-event analysis.

## ⚙️ Architecture

### Client Side

- **Provider**: Components that write logs to a defined communication channel (e.g., TCP socket, message queue, etc.).
- **Logger**: Extensions for existing logging systems (e.g., Serilog, Log4net) that utilize providers to write logs.

### Server Side

- **Broker**: The central component that handles the reception, routing, and persistence of logs.
  - **Receiver**: Receives logs from the corresponding provider and saves them to the Storage.
  - **Storage**: Persists data to disk.
- **Viewer**: A separate service that allows viewing, filtering, and exporting logs saved in the storage.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Feel free to open issues or contribute if you'd like to help turn **LogMQ** into a stable, production-ready solution! 💡
