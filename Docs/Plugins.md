# Gestione dei Plugin in LogMQ

## Formato dei Plugin

I plugin avranno estensione `.lmqex`. Il formato `.lmqex` è uno zip contenente:
- Un file `manifest.json` che conterrà tutti i metadati del plugin.
- Una DLL che al suo interno deve implementare la classe astratta `LogMQReceiverBase`.

## Struttura del Manifest

Il file `manifest.json` deve avere la seguente struttura:
```json
{
  "Id": "string",          // Un identificatore univoco per il plugin (GUID)
  "Name": "string",        // Il nome del plugin
  "Version": "string",     // La versione del plugin (es: "1.0.0")
  "Author": "string",      // L'autore del plugin
  "Description": "string", // Una breve descrizione del plugin
  "Type": "string",        // Il tipo di plugin (es: "Receiver" o "Storage")
  "EntryPoint": "string"   // Il file DLL che contiene l'implementazione del plugin
}
```

## Struttura del File System

I plugin andranno salvati nel file system secondo queste variabili .NET:
- `public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogMQ", "Plugins");`
- `public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");`
- `public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");`

I plugin presenti saranno salvati nella cartella `PluginBinariesFolder`, mentre lo schema sarà rappresentato in `PluginConfigFile`.

## Configurazione dei Plugin

Il file `PluginConfigFile` è un file JSON che contiene una lista di oggetti del tipo:
```json
[
    {
        "Id": "string",                    // Guid
        "Name": "string",                  // Nome del plugin
        "Type": "string",                  // Enum: "Receiver" | "Storage"
        "Versions": [                      // Array di versioni
        {
            "Version": "string",           // Version (es: "1.0.0")
            "Status": "string"             // Enum: "Enabled" | "Disabled" | "Enlisted" | "Delisted"
        }
        ]
    }
]
```

## Installazione dei Plugin

La pre-installazione dei plugin avverrà tramite l'interfaccia grafica nel viewer o tramite console (progetto CLI), che sfrutteranno le API della libreria condivisa `LogMQ.Services.Shared.PluginInstaller`. I plugin verranno pre-installati con lo stato di 'Enlisted'.

I plugin verranno installati effettivamente solo dopo il riavvio del broker, il quale si occuperà di caricarli e aggiornare lo stato a `Enabled` nel config.

I plugin verranno disinstallati effettivamente solo dopo il riavvio del broker, il quale si occuperà di rimuovere tutti i plugin con stato `Delisted` e aggiornare il config.

## Interfaccia CLI

L'interfaccia CLI avrà le seguenti opzioni:

### Installazione
- `-i|--install <plugin.lmqex>`: Registra il plugin previa conferma (y/n).
- `-I|--install-force <plugin.lmqex>`: Registra il plugin e riavvia il broker per caricare immediatamente il plugin previa conferma (y/n).


### Abilitazione, Disabilitazione e Disinstallazione
- `-e|--enable <pluginName | pluginId>`: Abilita il plugin.
- `-d|--disable <pluginName | pluginId>`: Disabilita il plugin.
- `-u|--uninstall <pluginName | pluginId>`: Disinstalla il plugin previa conferma (y/n).
- `-U|--uninstall-force <pluginName | pluginId>`: Disinstalla il plugin e riavvia il broker per rimuovere immediatamente il plugin previa conferma (y/n).
  - `-v|--version <version>`: Esegue l'azione precedente sulla versione specifica del plugin con questo nome o guid previa conferma (y/n).

### Riavvio del Broker
- `-r|--restart`: Riavvia il broker.

### Lista dei Plugin
- `-l|--list`: Restituisce la lista di tutti i plugin.
- `-le|--list-enabled`: Restituisce la lista solo dei plugin abilitati.
- `-ld|--list-disabled`: Restituisce la lista solo dei plugin disabilitati.
- `-lr|--list-registered`: Restituisce la lista solo dei plugin registrati.
- `-lu|--list-unregistered`: Restituisce la lista solo dei plugin deregistrati.

### Reset della Configurazione
- `-rc|--reset-config`: Aggiorna il file `PluginConfigFile` in base a quanto presente nella cartella `PluginBinariesFolder` e restituisce un report su quanto modificato, poi chiede conferma (y/n).

### Opzioni Generali
- `-y`: Non chiede conferma su tutte le azioni che ne hanno bisogno.

### Esempi di Utilizzo
- Installazione di un plugin:
  ```
  logmq -i plugin.lmqex
  ```
- Abilitazione di un plugin specifico:
  ```
  logmq -e pluginName
  ```
- Disabilitazione di una versione specifica di un plugin:
  ```
  logmq -d pluginName -v 1.0.0
  ```
- Riavvio del broker:
  ```
  logmq -r
  ```
- Lista dei plugin abilitati:
  ```
  logmq -le
  ```

## Ripristino dello Stato dei Plugin

In caso sia stato cambiato lo stato di un plugin, questo può essere riportato allo stato precedente senza conseguenze con la rispettiva azione, purché non sia già stato riavviato il broker.

## Viewer

Le stesse azioni potranno essere eseguite dal viewer.