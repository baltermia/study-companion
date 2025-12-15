#import "@preview/bubble:0.2.2": *
#import "@preview/codly:1.3.0": *
#import "@preview/codly-languages:0.1.1": *

#show: bubble.with(
  title: "Projekt Raspberry Pi: Study Companion",
  subtitle: "Computer Science (cds-205) HS25",
  author: "Baltermia Clopath",
  affiliation: "FHGR",
  date: datetime.today().display(),
  //year: "2025",
  class: "AISE25",
  // other: ("Made with Typst", "https://typst.com"),
  main-color: "4DA6FF", //set the main color
  // logo: image("logo.png"), //set the logo
) 

#set heading(numbering: "1.")

#set text(lang: "de")

#set quote(block: true)

#show: codly-init.with()

#codly(languages: codly-languages)

#outline(title: "Inhaltsverzeichnis")
#pagebreak()

= Zusammenfassung

#lorem(200)

= Einführung

== Themenwahl / Relevanz

Während einer Präsentation eines alten Schülers vom Modul in unserer Blockwoche kam mir die Idee, etwas zu machen, was mir das ganze Studium über helfen könnte. Letztendlich habe ich mich dann für einen Telegram Bot entschieden. Einerseits, weil ich auf dieser Platform viel Zeit verbringe, anderseits, weil ich damit auch schon einige Erfahrung habe. 

Der Bot - den ich _Study Companion_ nenne - soll Schülern helfen, den überblick über Lektionen, Prüfungen, Hausaufgaben und allgemeine Notizen zu behalten. Das macht er, indem er einerseits tägliche Erinnerungen schickt, aber auch auf Anfrage jegliche Informationen so schnell wie möglich bereitstellt. Das wichtigste Feature wird das Einlesen und Verarbeiten des von der FHGR bereitgestellten iCal Stundenplan sein, womit der Bot automatisch weiss, wann welche Lektion stattfindet. 

Ich finde das Thema relevant, einerseits, weil ich es andern Schülern so einfach wie möglichen mache möchte, den Bot selbst auf einem Raspberry laufen zu lassen. Anderseits werde ich künstliche Intelligenz verwenden, um schlanke Zusammenfassungen zu generieren. Wenn möglich, möchte ich auch einen Helligkeitssensor installieren, welcher erkennt, wann man am morgen aufsteht und somit automatische die Tägliche Übersicht verschickt wird.

== Eingrenzung des Themas

Der Bot soll im moment wirklich nur das nötigste können. Mein Ziel ist es, auch in späteren Modulen - wenn sich die Möglichkeit bietet - weitere Features hinzuzufügen. Das wichtigste:
- Einlesen von iCal
- Tägliche Zusammenfassung generiert mit AI
- Einfache Installation auf einem Raspberry Pi

== Fragestellung

Die zentrale Fragestellung meines Projekts lautet:\
Wie kann man einen Telegram-Bot erstellen, welcher Schülern mit automatischen Erinnerungen und Zusammenfassungen, durch Bereitstellung eines iCal-Kalenders und manueller Eingabe von Terminen den Alltag erleichtert und zudem leichte installation erlaubt?

Dazu habe ich noch folgende Unterfragen formuliert:\
1. Welche Frameworks und Bibliotheken eignen sich für die Entwicklung des Bots?
2. Wie kann der Bot den iCal-Kalender effizient einlesen und verarbeiten?
3. Wie kann die Installation auf einem Raspberry Pi möglichst einfach gestaltet werden?

== Verwendung von Typst

Ich möchte noch gerne einen kurzen Teil über die Verwendung von #link("https://typst.app/")[Typst] in meinem Projekt schreiben, da es meiner Meinung nach auch direkt mit der Projektarbeit zu tun hat. Typst ist eine Sprache welche ähnliche Ziele verfolgt wie LaTeX, aber meiner Meinung nach viel einfacher zu verwenden ist. Der Quelltext erinnert viel mehr an Markdown, trotzdem fehlt es der Sprache aber nicht an erweiterter Funktionalität, so wie es LaTeX bietet. Durch die Verwendung von Typst kann ich viel schneller und einfacher Dokumente erstellen. Da Dies eine der ersten Projektarbeiten in unserem Studium ist, und auch LaTex empfohlen wurde, wollte ich dies hier kurz erwähnen.
Ich empfehle, den Quellcode dieses Dokumentes anzuschauen, um einen Eindruck von der Sprache zu bekommen. (Verfügbar auf Github unter #link("https://github.com/baltermia/study-companion/blob/main/doku.typ")[baltermia/study-companion/doku.typ]).

= Durchführung

== Raspberry Pi Installation

Die Installation des Raspberry Pis werde ich kurz halten. Da der _Study Companion_ den Raspberry Pi lediglich als Host verwendet, ist die Installation eines Betriebssystem mit einer Grafischen Benutzeroberfläche nicht nötig. Daher habe ich mich entschieden, Debian zu installieren.

Debian stellt eigene Images für den Raspberry Pi zur Verfügung und auch direkt ein Online-Tutorial mit welchem man das Betriebssystem auf eine SD-Karte schreibt. @debian

Da Docker verwendet wird, um den Bot laufen zu lassen, wird auch noch Docker benötigt. Dazu gibt es auch eine offizielle Anleitung für Debian. @docker-engine

== Gewähltes Framework für den Telegram Bot

Durch meine mehrjährige Erfahrung mit dem .NET Framework habe ich mich auch für dieses für den Bot entschieden. Telegram stellt bereits eine eigene Bibliothek zur Verfügung. @telegram-bots Ich habe mich aber für eine erweiterte Bibliothek - _Minimal Telegram Bot_ - entschieden, da diese viel Boilerplate Code abnimmt und, laut den Entwicklern selbst, "die Entwicklung an die Workflows von .NET anpasst". @minimal-telegram-bot Im hintergrund verwendet diese Bibliothek aber immer noch die offizielle Telegram Bibliothek.

Die verwendete Telegram Bibliothek enthält eine `StateMachine`, welche verschiedene Zustände von Konversationen verwalten kann. Dies ist sehr nützlich, da der Bot so verschiedene Konversationen mit verschiedenen Benutzern gleichzeitig führen kann, ohne dass es zu Konflikten kommt. Diese `States` werden im Arbeitsspeicher gespeichert, sie können aber auch in einem Persistenten Speicher - wie einer Datenbank - gespeichert werden. Für solch eine kleine Applikation wäre das zwar etwas übertrieben, aber aus Erfahrung hat sich gezeigt, dass es vor allem beim Testen nützlich sein kann, wenn der Zustand nicht nur im Arbeitsspeicher gehalten wird. Diese wird sonst bei jedem Neustart der Applikation zurückgesetzt, was Testing etwas umständlich macht. Daher habe ich mich für die Verwendung von Redis entschieden. @redis

== iCal Verarbeitung

Auch für die iCal Verarbeitung gibt es eine geeignete .NET Bibliotheke, nämlich _iCal.NET_. @ical 

Erst muss der Kalender heruntergeladen werden, dazu kann direkt die `.ics`-Datei über den .NET-integrierten `HttpClient` heruntergeladen werden. Danach kann die Datei mit der `Calendar.Load` Methode der iCal.NET Bibliothek eingelesen werden. 

Da der User den Kalender jederzeit anschauen kann, macht es nicht Sinn, diesen bei jeder Anfrage neu herunterzuladen. Daher wird diese Datei lokal in einer Datenbank gespeichert. Ich habe mich für eine Postgres Datenbank entschieden, es kann aber dank der Nutzung des .NET Entity Frameworks auch einfach auf eine andere Datenbank gewechselt werden.
Das Entity Framework ist eine Art `Repository` Pattern, welches die Datenbankzugriffe abstrahiert und es so ermöglicht, mit verschiedenen Datenbanken zu arbeiten, ohne den Code anpassen zu müssen. @ef

== AI Integration

Der Bot muss keine komplexen Probleme mit AI Lösen, sondern lediglich Zusammenfassungen generieren. Dazu gibt es ja heutzutage schon eine vielzahl an vortrainierten Modelle zur Verfügung, welche dies ermöglichen. @summarization-benchmark Ich habe mich für die Verwendung der OpenAI API entschieden, da diese aus Erfahrung einfach zu verwenden ist und gute Resultate liefert. 

Ähnlich wie bei der Datenbank stellt Microsoft für .NET auch eine Bibliothek zur Verfügung, welche die verschiedenen AI Anbieter abstrahiert. @microsoft-extensions-ai

== Dockerization <docker>

Um den Bot möglichst einfach auf einem Raspberry Pi installieren zu können, habe ich mich entschieden, den Bot in einem Docker Container laufen zu lassen. Docker ermöglicht es, Anwendungen in Containern zu verpacken, welche alle Abhängigkeiten enthalten und somit auf jedem System mit Docker-Unterstützung laufen können. @what-is-docker

Docker hat ein Erweitertes Tool namens Docker Compose, welches es ermöglicht, mehrere Container zu orchestrieren. Einerseits ist es nützlich, da der Bot mehrere Abhängigkeiten hat (Datenbank, Redis), anderseits ermöglicht es auch eine einfache Konfiguration der Container (welche auch gespeichert werden kann). @docker-compose

Bereits für Testing Zwecke habe ich solch ein Docker-Compose erstellt, diese enthält aber nur die beiden Datenbanken, Postgres & Redis. Für die Produktion habe ich ein weiteres Docker-Compose erstellt, welches auch den Bot Container enthält.

=== Image

Für das laufen lassen von Containern wird entweder ein `Dockerfile` mit dem Source-Code benötigt, oder ein bereits gebautes `Image`. Um den Bot so einfach wie möglich installieren zu können, macht es Sinn, ein Image des Bots zu erstellen und dieses auf einem Container Repository wie Docker Hub zu speichern. @docker-hub

Ich wollte den ganzen Prozess aber automatisieren, daher habe ich mich entschieden, GitHub Actions zu verwenden, um bei jedem Push auf den `main` Branch ein neues Image zu bauen und dieses auf Docker Hub zu pushen. Github stellt dafür eigene CI/CD Workflows sowie auch ein eigenes Container Registry zur Verfügung. @github-actions @github-container-registry

Die erstellen Images können unter https://github.com/baltermia/study-companion/pkgs/container/study-companion gefunden werden.


= Resultate

#pagebreak()

#bibliography("sources.yaml")
