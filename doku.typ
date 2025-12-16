#import "@preview/bubble:0.2.2": *
#import "@preview/codly:1.3.0": *
#import "@preview/codly-languages:0.1.1": *

#show: bubble.with(
  title: "Raspberry Pi Projekt: Study Companion",
  subtitle: "Computer Science (cds-205) HS25",
  author: "Baltermia Clopath",
  affiliation: "FHGR",
  date: datetime.today().display(),
  //year: "2025",
  class: "AISE25",
  // other: ("Made with Typst", "https://typst.com"),
  main-color: "C51A4A", //set the main color
  //color-words: ("Raspberry Pi",)
  // logo: image("logo.png"), //set the logo
) 

#set heading(numbering: "1.")

#set text(lang: "de")

#set quote(block: true)

#show: codly-init.with()

#set figure.caption(position: bottom)

#codly(languages: codly-languages)

#outline(title: "Inhaltsverzeichnis")
#pagebreak()

= Abstract

Das Projekt beschreibt die Entwicklung eines Telegram-Bots, der Studierenden beim Organisieren ihres Studienalltags unterstützt. Ziel ist es, mit automatischen Erinnerungen, Tagesübersichten und KI-generierten Zusammenfassungen eine einfache, digitale Lernhilfe zu schaffen. Der Bot liest den Stundenplan über einen iCal-Kalender der FHGR ein und fasst die Lektionen zusammen. Technisch basiert der Code vom Bot auf dem .NET-Framework und wird in einem Docker-Container-Image geliefert, um eine einfache Installation auf einem Raspberry Pi zu ermöglichen. Ergänzend wurden Redis und PostgreSQL für die persistente Datenspeicherung integriert. Das Docker-Image wird automatisch über GitHub Actions bei jedem Update neu gebaut und veröffentlicht.\
Repository: https://github.com/baltermia/study-companion/

= Einführung

== Themenwahl / Relevanz

Während einer Präsentation eines alten Studenten vom Modul in unserer Blockwoche kam mir die Idee, etwas zu machen, was mir das ganze Studium über helfen könnte. Letztendlich habe ich mich dann für einen Telegram Bot entschieden. Einerseits, da ich Telegram viel im Alltag nutze, anderseits, weil ich damit auch schon einige Erfahrung habe. 

Der Bot - den ich _Study Companion_ nenne - soll Studierenden helfen, den überblick über Lektionen, Prüfungen, Hausaufgaben und allgemeine Notizen zu behalten. Das macht er, indem er einerseits tägliche Erinnerungen schickt, aber auch auf Anfrage jegliche Informationen bereitstellt. Das wichtigste Feature wird das Einlesen und Verarbeiten des von der FHGR bereitgestellten iCal Stundenplans sein, womit der Bot automatisch weiss, wann welche Lektion stattfindet. 

Das Thema ist relevant, einerseits weil ich es andern Studierenden so einfach wie möglichen machen möchte, den Bot selbst auf einem Raspberry laufen zu lassen. Anderseits werde ich künstliche Intelligenz verwenden, um schlanke Zusammenfassungen zu generieren.

== Eingrenzung des Themas

Der Bot soll im moment wirklich nur das nötigste können. Das Ziel ist es, auch in späteren Modulen - wenn sich die Möglichkeit ergibt - weitere Features hinzuzufügen. Das wichtigste:
- Einlesen von iCal
- Tägliche Zusammenfassung generiert mit AI
- Einfache Installation auf einem Raspberry Pi

== Fragestellung

Die zentrale Fragestellung des Projekts lautet:\
Wie kann man einen Telegram-Bot erstellen, welcher Studierenden mit automatischen Erinnerungen und Zusammenfassungen, durch Bereitstellung eines iCal-Kalenders und manueller Eingabe von Hausaufgaben den Alltag erleichtert und zudem leichte installation erlaubt?

Dazu sind noch konkretere Unterfragen formuliert:\
1. Welche Frameworks und Bibliotheken eignen sich für die Entwicklung des Bots?
2. Wie kann der Bot den iCal-Kalender effizient einlesen und verarbeiten?
3. Wie kann die Installation auf einem Raspberry Pi möglichst einfach gestaltet werden?

== Verwendung von Typst

Ich möchte noch ganz kurz über die Verwendung von #link("https://typst.app/")[Typst] in meinem Projekt schreiben, da es meiner Meinung nach auch direkt mit der Projektarbeit zu tun hat. Typst ist eine Sprache welche ähnliche Ziele verfolgt wie LaTeX, aber meiner persönlichen Erfahrung nach viel einfacher zu verwenden ist. Da dies eine der ersten Projektarbeiten in unserem Studium ist, und auch LaTex empfohlen wurde, wollte ich dies hier kurz erwähnen.
Ich empfehle, den Quellcode dieses Dokumentes anzuschauen, um einen Eindruck von der Sprache zu bekommen. (Verfügbar #link("https://github.com/baltermia/study-companion/blob/main/doku.typ")[im Repository]).

= Durchführung

== Raspberry Pi Installation

Die Installation des Raspberry Pi's wird kurz gehalten. Da der Bot den Raspberry Pi lediglich als Host verwendet, ist die Installation eines Betriebssystem mit einer Grafischen Benutzeroberfläche nicht nötig. Daher habe ich mich entschieden, Debian zu installieren.
Debian stellt eigene Images für den Raspberry Pi zur Verfügung und auch direkt ein Online-Tutorial, welches zeigt, wie man das Betriebssystem auf eine SD-Karte schreibt. @debian
Da Docker verwendet wird, um den Bot laufen zu lassen, wird dies auch benötigt. Dazu gibt es auch eine offizielle Anleitung für Debian. @docker-engine

== Gewähltes Framework für den Telegram Bot

Durch meine mehrjährige Erfahrung mit dem .NET Framework habe ich mich auch für dieses für den Bot entschieden. Telegram stellt bereits eine eigene Bibliothek zur Verfügung. @telegram-bots Ich habe mich aber für eine erweiterte Bibliothek - _Minimal Telegram Bot_ - entschieden, da diese viel Boilerplate Code abnimmt und, laut den Entwicklern selbst, "die Entwicklung an die Workflows von .NET anpasst". @minimal-telegram-bot Im hintergrund verwendet diese Bibliothek aber immer noch die offizielle Telegram Bibliothek.


Die verwendete Telegram-Bibliothek bietet eine `StateMachine` zur Verwaltung von Konversationen und Zuständen. Dadurch kann der Bot parallel mit mehreren Nutzern ohne Konflikte interagieren. Zustände werden standardmässig im Arbeitsspeicher gehalten, können aber auch persistent (z. B. in einer Datenbank) gespeichert werden. Für kleine Anwendungen ist das meist übertrieben, aber beim Testen ist Persistenz nützlich, da im-Memory-Zustände bei jedem Neustart verloren gehen. Deshalb dafür wird Redis verwendet. @redis

Beim erstellen von Hausaufgaben wird ein Datum benötigt. Da es für Nutzer etwas aufwändig ist, immer das volle Datum anzugeben, wird die .NET _Recognizers Text_ Bibliothek genutzt, welche es ermöglicht, natürliche Sprache in strukturierte Daten umzuwandeln. Das macht sie, indem sie ein Machine Learning Modell verwendet, welches auf verschiedene Sprachen trainiert wurde und komplett lokal läuft. @recognizers-text

== iCal Verarbeitung

Auch für die iCal Verarbeitung gibt es eine geeignete .NET Bibliothek, nämlich _iCal.NET_. @ical 

Erst muss der Kalender heruntergeladen werden, dazu kann direkt die `.ics`-Datei über den .NET-integrierten `HttpClient` heruntergeladen werden. Danach kann die Datei mit der `Calendar.Load` Methode der iCal.NET Bibliothek eingelesen werden. 

Da der User den Kalender jederzeit anschauen kann, macht es keinen Sinn, diesen bei jeder Anfrage neu herunterzuladen. Daher wird diese Datei lokal in einer Datenbank gespeichert. Ich habe mich für eine Postgres Datenbank entschieden, es kann aber dank der Nutzung des .NET Entity Frameworks auch einfach auf eine andere Datenbank gewechselt werden. Die Nutzereinstellungen werden auch in der Datenbank gespeichert. 
Das Entity Framework ist eine Art Repository-Pattern, welches die Datenbankzugriffe abstrahiert und es so ermöglicht, mit verschiedenen Datenbanken zu arbeiten, ohne den Code anpassen zu müssen. @ef

== AI Integration

Der Bot muss keine komplexen Probleme mit AI Lösen, sondern lediglich Zusammenfassungen generieren. Dazu gibt es ja heutzutage schon eine Vielzahl an vortrainierten Modelle zur Verfügung, welche dies ermöglichen. @summarization-benchmark Ich habe mich für die Verwendung der OpenAI API entschieden, da diese aus Erfahrung einfach zu verwenden ist und gute Resultate liefert. 

== Dockerization <docker>

Um den Bot möglichst einfach auf einem Raspberry Pi installieren zu können, habe ich mich entschieden, den Bot in einem Docker Container laufen zu lassen. Docker ermöglicht es, Anwendungen in Containern zu verpacken, welche alle Abhängigkeiten enthalten und somit auf jedem System mit Docker-Unterstützung laufen können. @what-is-docker

Docker hat ein Erweitertes Tool namens Docker Compose, welches es ermöglicht, mehrere Container zu orchestrieren. Einerseits ist es nützlich, da der Bot mehrere Abhängigkeiten hat (Postgres, Redis), anderseits ermöglicht es auch eine einfache Konfiguration der Container (welche auch gespeichert werden kann). @docker-compose

Bereits für Testing habe ich ein Docker‑Compose erstellt, das Postgres und Redis enthält. Für die Produktion gibt es ein weiteres Docker‑Compose, das zusätzlich den Bot‑Container enthält.


=== Image

Für das laufen lassen von Containern wird entweder ein `Dockerfile` mit dem Source-Code benötigt, oder ein bereits gebautes `Image`. Um den Bot so einfach wie möglich installieren zu können, macht es Sinn, ein Image des Bots zu erstellen und dieses auf einem Container Repository wie Docker Hub zu speichern. @docker-hub

Ich wollte den ganzen Prozess aber automatisieren, daher habe ich mich entschieden, GitHub Actions zu verwenden, um bei jedem Push auf den `main` Branch ein neues Image zu bauen und dieses auf das GitHub Container Repository zu pushen. GitHub stellt dafür eigene CI/CD Workflows sowie auch ein eigenes Container Registry zur Verfügung. @github-actions @github-container-registry

Die erstellen Images können #link("https://github.com/baltermia/study-companion/pkgs/container/study-companion")[im Repository] abgerufen werden.

= Resultate

== Bot Installation

Das Installieren des Bots auf einen Raspberry Pi ist so einfach wie geplant. Es gibt nur 3 Vorgaben:
1. Docker und Docker Compose müssen installiert sein.
2. Ein Telegram Bot Token wird benötigt (kann über den BotFather auf Telegram erstellt werden @botfather).
3. Ein OpenAI API Key wird benötigt @openai

Danach muss nur das Docker-Compose File angepasst werden, um die beiden Keys einzutragen. Danach kann der Bot mit den folgenden Befehlen gestartet werden:

```bash
docker compose pull # zieht das neuste Image vom Repository
docker compose up -d # startet den Bot im Hintergrund
```

Die gleichen Kommandos können für eine Update des Bots verwendet werden. Denn das `pull` Kommando zieht immer das neuste Image vom Repository.

Die genauen Installationsschritte sind im Repository-Readme nochmals besser und detaillierter beschrieben.

== Bot Funktionalitäten

Detaillierte Bilder der Funktionalitäten vom Bot sind im #link("https://github.com/baltermia/study-companion?tab=readme-ov-file#screenshots")[Repository README] dokumentiert.

=== Start

Der Bot bietet eine einfache Navigation über Buttons die jederzeit angezeigt werden:

#figure(
  image("docs/buttons.png", width: 70%),
  caption: [
    Telegram Bot Navigation durch Buttons
  ]
)

Ganz am Anfang fragt der Bot nach dem iCal Link, damit der Stundenplan eingelesen werden kann. Danach wird täglich um (standardmässig) 8 Uhr morgens eine Zusammenfassung der anstehenden Lektionen und Hausaufgaben geschickt.

=== Einstellungen

In den Einstellungen kann der User die Sprache zwischen Deutsch und Englisch wechseln, sowie die Zeitzone anpassen.

=== Hausaufgaben

Hausaufgaben können einfach hinzugefügt, aufgelistet und gelöscht werden. Zudem wird der Nutzer am vorherigen Tag um 12 Uhr an die anstehenden Hausaufgaben erinnert. Erledigte Hausaufgaben werden automatisch aus der Liste gelöscht und bis zum Tagesende noch als erledigt angezeigt.

=== Kalender

Der Kalender, welcher über den iCal Link eingelesen wird, kann jederzeit abgerufen werden. Der Bot zeigt die nächsten 7 Tage an und listet alle Lektionen auf, inklusive Raum und Dozent. Über Buttons kann man im Wochentakt vor- und zurückspringen. Der Bot erinnert an Lektionen (standardmässig) 1 Stunde vor beginn.

=== AI Zusammenfassungen

Neben der automatischen täglichen Zusammenfassung kann der User auch jederzeit eine Zusammenfassung der nächsten Tage anfordern.

= Beantwortung der Fragestellung

*1. Welche Frameworks und Bibliotheken eignen sich für die Entwicklung des Bots?* \
Es eignen sich verschiedene .NET Bibliotheken, wie z.B. _Minimal Telegram Bot_ für die Bot-Entwicklung, _iCal.NET_ für die iCal Verarbeitung, _Recognizers Text_ für die Datumserkennung und das _Entity Framework_ für die Datenbankzugriffe.

*2. Wie kann der Bot den iCal-Kalender effizient einlesen und verarbeiten?* \
Der Bot kann den iCal-Kalender effizient einlesen, indem er die `.ics`-Datei herunterlädt und mit der `Calendar.Load` Methode der iCal.NET Bibliothek verarbeitet. Die Daten können dann in einer Datenbank gespeichert werden, um schnelle Zugriffe zu ermöglichen und wiederholtes Herunterladen zu vermeiden.

*3. Wie kann die Installation auf einem Raspberry Pi möglichst einfach gestaltet werden?* \
Die Installation kann durch die Verwendung von Docker und Docker Compose vereinfacht werden, da der Bot und seine Abhängigkeiten in Containern verpackt werden können. 

= Fazit

Das Projekt hat gezeigt, wie es möglich ist, einen Telegram Bot zu erstellen, der Studierenden durch automatische Erinnerungen und Zusammenfassungen den Alltag erleichtern sollte. Die Verwendung von .NET Bibliotheken und Docker hat die Entwicklung und Installation des Bots vereinfacht. 

Das erstellen des Bots ist mir leicht gefallen, da ich bereits Erfahrung mit .NET und Telegram Bots hatte. Die grösste Herausforderung war definitiv das erstellen der Docker Compose und das Einrichten der CI/CD Pipeline mit GitHub Actions für das erstellen des Images. Schlussendlich hat aber alles gut funktioniert und ich bin sehr zufrieden mit dem Resultat. 

Die Dokumentation hat mir auch Schwierigkeiten bereitet, denn ich wäre gerne noch etwas detaillierter auf die einzelnen Implementationen und Resultate eingegangen. Leider sind wir aber auf eine vordefinierte Seitenanzahl limitiert, welche ich jetzt bereits etwas überschritten habe.

Der Bot liefert noch viel Potenzial für Erweiterung - z.B. sollten verschiedene AI-Anbieter direkt im Compose‑File konfigurierbar sein, statt aktuell nur OpenAI zu unterstützen. Ich hoffe, dass ich in zukünftigen Projekten die Möglichkeit haben werde, weitere Features hinzuzufügen.

#pagebreak()

#bibliography("sources.yaml")
