# Ziel

Hier entsteht eine Anwendung um den aktuellen Status eines GWM ORA Funky Cat per MQTT zu veröffentlichen und über MQTT Befehle an das Auto zu senden.

# Ich will mitmachen...

Sehr gern! Ich hab aktuell auch noch keinen Plan, was die nächsten Schritte sind. Daher ist es am besten wenn du [einen Issue aufmachst](https://github.com/zivillian/ora2mqtt/issues/new) und sagst was du vorhast, kannst, willst, brauchst...

Was ich gemacht habe um zu dem aktuellen Stand zu kommen findest du unter [How to...?](#how-to)

Falls du dabei Hilfe brauchst, kannst du auch einfach [einen Issue aufmachst](https://github.com/zivillian/ora2mqtt/issues/new).

# Status

Es gibt eine Kommandozeilenanwendung die unter Windows läuft und die aktuellen Werte auslesen und per MQTT veröffentlichen kann.

Im ersten Schritt muss die Konfigurationsdatei mit `ora2mqtt configure` erstellt werden. Anschließend kann die Anwendung mit `ora2mqtt run` oder einfach `ora2mqtt` gestartet werden. Damit sollten die aktuellen Werte im MQTT zu sehen sein.

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
    topic: GWM/<vin>/status/items/2013021/value
    timeout: 1m
  range:
    source: mqtt
    topic: GWM/<vin>/status/items/2011007/value
    timeout: 1m
  odometer:
    source: mqtt
    topic: GWM/<vin>/status/items/2103010/value
    timeout: 1m
```

Ich habe das inzwischen auch ein paar Stunden laufen lassen, während ich mit dem Auto unterwegs war. Die Daten werden auch dann übermittelt, wenn die offizielle App nicht genutzt wird. Auch der Token Refresh war erfolgreich.

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

## App Version >= 1.9.4

Seit Version 1.9.4 scheint [KiwiVM](https://github.com/iKiwiSec/KiwiVM) genutzt zu werden - daher kann man mit apktool nicht mehr in den Code schauen. Ich habe versucht die App (ohne sie zu verändern) mit apktool zu zerlegen, wieder zusammenzubauen und mit uber-apk-signer zu signieren, aber anschließend startet sie nicht mehr.

Um mit einer älteren Version und mitmproxy mitzulesen muss die Versionsprüfung verhindert werden. Das geht in mitmproxy mit dem [Interceptor](https://docs.mitmproxy.org/stable/mitmproxytutorial-interceptrequests/) `~q & ~u /app-api/api/v1.0/complaintsComments/findLastVersion`. Der App ist es glücklicherweise egal, ob der Request abgebrochen wird oder in ein Timeout läuft. Wichtig ist nur, dass der Request im mitmproxy nicht mit `a` einfach durchgelassen wird - am besten den request mit `X` killen.

Wenn jemand eine Idee hat, wie man in der aktuellen Version das Root Zertifikat tauschen kann, einfach ein Issue aufmachen.
