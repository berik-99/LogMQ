# 🌐 LogMQ - Cross-Platform Centralized Logging System

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="Assets/Logo/LogMQ_logo_horizontal_light.svg">
  <source media="(prefers-color-scheme: light)" srcset="Assets/Logo/LogMQ_logo_horizontal_dark.svg">
  <img alt="Logo" src="path/to/logo-dark.png">
</picture>


**LogMQ** is a cross-platform centralized logging system built on a **Message Queue** architecture, designed to collect and store log messages from various local and networked applications. The system processes and routes log data according to predefined criteria, offering efficient and scalable log management.

🔧 **Current state**: Written in **.NET** for **.NET** applications, LogMQ leverages **Serilog** to send log messages to the central broker, which is responsible for distributing them to the appropriate destinations.  
🚀 **Future plans**: Future versions will expand support to include other loggers and may support loggers from other languages, ensuring greater flexibility and compatibility across diverse environments.

## 🎯 Key Features

- **Centralized log management** across multiple applications and platforms.
- **Modular design** for easy extension and integration with other logging frameworks.
- **Scalable architecture** using a Message Queue for efficient log routing.
- **Future-proof**: Planned support for non-.NET loggers and other languages.

## 📖 Overview

The primary goal of **LogMQ** is to provide a robust, modular solution for collecting, monitoring, and managing logs from distributed applications on multiple platforms. This enhances visibility and traceability of events, issues, and anomalies in real-time or during post-event analysis.

## ⚠️ Disclaimer

LogMQ is currently under **active development** and is not yet considered stable.  
Please use it at your own risk in non-production environments.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Feel free to open issues or contribute if you'd like to help shape **LogMQ** into a stable, production-ready solution! 💡
