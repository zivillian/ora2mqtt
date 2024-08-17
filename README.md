# Ziel

Hier entsteht eine Anwendung um den aktuellen Status eines GWM ORA Funky Cat per MQTT zu veröffentlichen und über MQTT Befehle an das Auto zu senden.

# Ich will mitmachen...

Sehr gern! Ich hab aktuell auch noch keinen Plan, was die nächsten Schritte sind. Daher ist es am besten wenn du [einen Issue aufmachst](https://github.com/zivillian/ora2mqtt/issues/new) und sagst was du vorhast, kannst, willst, brauchst...

Was ich gemacht habe um zu dem aktuellen Stand zu kommen findest du unter [How to...?](#how-to)

Falls du dabei Hilfe brauchst, kannst du auch einfach [einen Issue aufmachst](https://github.com/zivillian/ora2mqtt/issues/new).

# Status

Es gibt eine Kommandozeilenanwendung die unter Windows läuft und die aktuellen Werte auslesen und per MQTT veröffentlichen kann.

Im ersten Schritt muss die Konfigurationsdatei mit `ora2mqtt configure` erstellt werden. Dafür am besten einen zusätzlichen Account anlegen und das Auto für diesen Account freigeben. Anschließend kann die Anwendung mit `ora2mqtt run` oder einfach `ora2mqtt` gestartet werden. Damit sollten die aktuellen Werte im MQTT zu sehen sein.

Die Werte (SOC, Range und Odometer) können in [evcc](https://github.com/evcc-io/evcc/) mit der folgenden Konfiguration eingebunden werden:

```yaml
vehicles:
- name: ora
  type: custom
  title: Ora Funky Cat
  capacity: 45
  phases: 3
  soc:
    source: mqtt
    topic: GWM/<vehicleId>/status/items/2013021/value
    timeout: 1m
  range:
    source: mqtt
    topic: GWM/<vehicleId>/status/items/2011501/value
    timeout: 1m
  odometer:
    source: mqtt
    topic: GWM/<vehicleId>/status/items/2103010/value
    timeout: 1m
```

Ich habe das inzwischen auch ein paar Stunden laufen lassen, während ich mit dem Auto unterwegs war. Die Daten werden auch dann übermittelt, wenn die offizielle App nicht genutzt wird. Auch der Token Refresh war erfolgreich.

## Linux

Damit die Binaries unter Linux laufen, muss das root Zertifikat installiert werden. Dazu das [`gwm_root.pem`](libgwmapi/Resources/gwm_root.pem) Zertifikat aus dem Repository herunterladen und mit `sudo cp gwm_root.pem /etc/ssl/certs/` in den Zertifikate Ordner des Systems kopieren.

Außerdem muss die [`openssl.cnf`](openssl.cnf) aus dem Repository heruntergeladen werden. Danach kann man die Binaries aus dem Release mit dem folgen Skript starten.

```
#/bin/bash

export OPENSSL_CONF=/path/to/the/file/openssl.cnf
cd /path/to/the/binary/ora2mqtt/

# restart when failed
while :
do
    ./ora2mqtt -i 60
    sleep 30
done
```

Das Skript startet das Programm in einer Endlosschleife neu, falls die Verbindung verloren wird. Außerdem wird das Polling-Interval von 10s auf 60s erhöht um die Anzahl der Anfragen an den GMW Server zu reduzieren.

## Docker

Inzwischen gibt es auch einen Docker Container. Die config muss vorher mit `ora2mqtt configure` erstellt werden:

```bash
docker run -d --restart=unless-stopped -v ./ora2mqtt.yml:/config/ora2mqtt.yml zivillian/ora2mqtt:latest
```

# Datenpunkte

Folgende Datenpunkte kann ich auslesen:

| Datenpunkt | Beschreibung
| ---------- | ------------
| 2011501    | Reichweite in km
| 2013021    | SOC
| 2013022    | verbleibende Ladedauer in Minuten
| 2013023    | 
| 2041142    | Ladevorgang aktiv
| 2041301    | SOCE
| 2042071    | 
| 2042082    | bool Flag, nur aktiv wenn geladen wird (aber nicht immer)
| 2078020    | 
| 2101001    | Reifendruck vl in kPa
| 2101002    | Reifendruck vr in kPa
| 2101003    | Reifendruck hl in kPa
| 2101004    | Reifendruck hr in kPa
| 2101005    | Reifentemperatur vl in °C
| 2101006    | Reifentemperatur vr in °C
| 2101007    | Reifentemperatur hl in °C
| 2101008    | Reifentemperatur hr in °C
| 2102001    | 
| 2102002    | 
| 2102003    | 
| 2102004    | 
| 2102007    | 
| 2102008    | 
| 2102009    | 
| 2102010    | 
| 2103010    | Kilometerstand in km
| 2201001    | Innenraumtemperatur in zehntel °C
| 2202001    | Klimaanlage an
| 2208001    | Schloss offen
| 2210001    | Fenster geschlossen vl
| 2210002    | Fenster geschlossen vr
| 2210003    | Fenster geschlossen hl
| 2210004    | Fenster geschlossen hr
| 2210010    | 
| 2210011    | 
| 2210012    | 
| 2210013    | 
| 2222001    | 
| 2310001    | 

# How it started?

Bei evcc hat [jemand vorgeschlagen](https://github.com/evcc-io/evcc/discussions/9524#discussioncomment-6832420), dass man sich die App mal anschauen müsste...

# How it's going...

## Endpunkte

Es gibt mind. 4 API Endpunkte (für jede Region):

### https://eu-h5-gateway.gwmcloud.com

Das ist der Standard Endpunkt für die App. Hier findet die Authentifizierung statt, wird das User Profil verwaltet und früher gab es auch mal eine _Community_.

### https://eu-app-gateway.gwmcloud.com

Über den Endpunkt findet die Kommunikation mit dem Auto statt. Dieser Endpunkt benötigt ein Client Zertifikat der GWM CA. Glücklicherweise liefert die APP [eins mit](#client-cert) das funktioniert.

### https://eu-data-upload-gateway.gwmcloud.com

Hier wird initial die Konfiguration für das tracking abgerufen und dann jeder Klick in der App als gzipped Json hochgeladen.

### https://eu-app-gateway-common.gwmcloud.com

Bisher ist nur ein Request bekannt, über den sich die App ein individuelles Zertifikat ausstellt das für den Zugriff auf den `eu-app-gateway` Endpunkt genutzt wird.

## HTTP Header

Jede Anfrage enthält sehr viele nicht standardisierte HTTP Header. Nicht alle werden benötigt, daher hier nur die relevanten:

|Name       |Value     |Beschreibung                                                          |
|-----------|----------|----------------------------------------------------------------------|
|Rs         |         2|                                                             required |
|Terminal   |GW_APP_ORA|                                                             required |
|Brand      |         3|                                                             required |
|accessToken|       JWT|                                                   Ergebnis vom Login |
|language   | de/en/...| beeinflusst Fehlermeldungen und muss für einige Request gesetzt sein |
|systemType |         1|                                                   sometimes required |
|country    |        DE|             wenn sich der Wert ändert, wird das accessToken ungültig |

Wenn die Header fehlen liefert die API nur einen Fehler zurück. Manchmal steht drin, welcher Header fehlt.

## Cert pinning

Das Root Zertifikat für den `eu-app-gateway` Endpunkt ist in der App gepinnt. Dafür bringt die App das Global Sign Root Zertifikat als Ressource mit (`res/raw/globalsign_chain.crt`). Wenn das in der App (in Version 1.8.1) ersetzt wird, kann der Traffic mit [mitmproxy](https://mitmproxy.org/) mitgeschnitten werden.

## Client Cert

Der `eu-app-gateway` Endpunkt benötigt ein Client Zertifikat von der GWM CA. Die App enthält bereits ein Zertifikat. `assets/gwm_general.cer` enthält das Zertifikat, `assets/gwm_general.key` den dazu passenden private key und `assets/gwm_root.pem` die Zertifikatskette bis zur GWM CA.

Bei der ersten Anmeldung stellt sich die App ein eigenes Zertifikat aus. Das Zertfikat wird lokal auf dem Gerät abgelegt und kann ausgelesen werden, wenn das `android:debuggable` Flag gesetzt wurde.

Das Zertifikat liegt im Speicher unter `files/pki/cert/cert`, dazu gehören noch die Dateien `files/pkey_data11`, `files/pkey_data21` und `files/pkey_data31`. Die `1` steht dabei für das n-te Zertifikat (weil das irgendwann abläuft und erneuert werden muss). Der mitgeliefert Key wird in der Datei `files/pkey_data30` abgelegt.

### pkey_data1x

Das ist der Public Key.

### pkey_data2x

Das ist auch der Public Key, aber der RSA Parameter e wurde _transformiert_.

### pkey_data3x

Das ist der Private Key - der RSA Parameter d wurde _transformiert_.

### _Transformation_

Sowohl die mitgelieferten Schlüssel, als auch die Schlüssel des erstellten Client Zertifikats sind _transformiert_. Zusätzlich werden nur die RSA Parameter n, d und e abgespeichert - die weiteren Parameter p,q, dp, dq und qInv müssen berechnet werden. Der Code um die Transformation rückgängig zu machen und die fehlenden Paramter zu berechnen liegt in [CertificateHandler.cs](libgwmapi/CertificateHandler.cs).

Alternativ geht das auch in python mit [cryptography.hazmat.primitives.asymmetric.rsa](https://cryptography.io/en/latest/hazmat/primitives/asymmetric/rsa/#handling-partial-rsa-private-keys).

# How to...?

Ich habe die App mit [apktool](https://apktool.org/) zerlegt und wieder zusammengebaut. Damit lassen sich die Zertifikate auslesen und ersetzen. Um zu verstehen, was in den Zertifikaten drin steht und was wie _transformiert_ wird, war [asn1js](https://lapo.it/asn1js) sehr hilfreich.

Um die modifizierte App installieren zu können, muss sie signiert sein - das geht relativ einfach mit [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/).

Den Traffic kann man mit [mitmproxy](https://mitmproxy.org/) mitlesen. Dabei muss das Root Zertifikat auf dem Gerät oder Emulator [installiert werden](https://docs.mitmproxy.org/stable/concepts-certificates/#installing-the-mitmproxy-ca-certificate-manually) und das Client Zertifikat aus der App [extrahiert](#client-cert), [_transformiert_](#transformation) und [mit angegeben](https://docs.mitmproxy.org/stable/concepts-certificates/#using-a-client-side-certificate) werden.

Die App bringt einige native Binaries mit (relevant sind `libbean.so` und `libbeancrypto.so`) - da werden auch die Zertifikate und Private Keys verarbeitet. Mit [Ghidra](https://ghidra-sre.org/) lässt sich das aber sehr gut untersuchen. Für den Crypto Part wird [libtomcrypt](https://github.com/libtom/libtomcrypt/) genutzt - damit lässt sich dann auch die _Transformation_ der RSA Parameter nachvollziehen.
