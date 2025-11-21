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
3. Kann der Bot für flexibleres Hosting mehrere AI-Modelle mühelos einbinden?
4. Wie kann die Installation auf einem Raspberry Pi möglichst einfach gestaltet werden?

== Verwendung von Typst

Ich möchte noch gerne einen kurzen Teil über die Verwendung von #link("https://typst.app/")[Typst] in meinem Projekt schreiben, da es meiner Meinung nach auch direkt mit der Projektarbeit zu tun hat. Typst ist eine Sprache welche ähnliche Ziele verfolgt wie LaTeX, aber meiner Meinung nach viel einfacher zu verwenden ist. Der Quelltext erinnert viel mehr an Markdown, trotzdem fehlt es der Sprache aber nicht an erweiterter Funktionalität, so wie es LaTeX bietet. Durch die Verwendung von Typst kann ich viel schneller und einfacher Dokumente erstellen, welche zudem auch noch viel schöner aussehen. Somit kann ich mich bei Projekten viel mehr auf den Inhalt konzentrieren, anstatt mich mit der Formatierung herumzuschlagen. @typst-for-latex-users \
Ich empfehle, den Quellcode dieses Dokumentes anzuschauen, um einen Eindruck von der Sprache zu bekommen. (Verfügbar auf Github unter #link("https://github.com/baltermia/study-companion/blob/main/doku.typ")[baltermia/study-companion/doku.typ]).

= Durchführung

== Raspberry Pi Installation

Die Installation des Raspberry Pis werde ich kurz halten. Da der _Study Companion_ den Raspberry Pi lediglich als Host verwendet, ist die Installation eines Betriebssystem mit einer Grafischen Benutzeroberfläche nicht nötig. Daher habe ich mich entschieden, Debian zu installieren.

Nach der Installation ist es noch wichtig, Docker zu installieren. Im späteren @docker wird dies verwendet.

```sh
apt install docker
```

== Gewähltes Framework für den Telegram Bot

Durch meine mehrjährige Erfahrung mit dem .NET Framework habe ich mich auch für dieses für den Bot entschieden. Telegram stellt bereits eine eigene Bibliothek zur Verfügung #link("https://github.com/TelegramBots/Telegram.Bot")[TelegramBots/Telegram.Bot]. Ich habe mich für eine erweiterte Bibliothek - #link("https://github.com/k-paul-acct/minimal-telegram-bot")[k-paul-acct/minimal-telegram-bot] - entschieden, da diese viel Boilerplate Code abnimmt und die Entwicklung an die Workflows von .NET anpasst. @minimaltelegrambot-intentions Im hintergrund verwendet diese Bibliothek aber immer noch die offizielle Telegram Bibliothek.

== iCal Verarbeitung

Auch für die iCal Verarbeitung gibt es eine hervorragende .NET Bibliotheke, nämlich #link("https://github.com/ical-org/ical.net")[ical.net].

== AI Integration

Der Bot muss keine komplexen Probleme mit AI Lösen, sondern lediglich Zusammenfassungen generieren. Dazu gibt es ja heutzutage schon eine vielzahl an vortrainierten Modelle zur Verfügung, welche dies ermöglichen. @summarization-benchmark Ich habe mich für die Verwendung der OpenAI API entschieden, da diese einfach zu verwenden ist und gute Resultate liefert. Zudem stellt Microsoft die Pro Version für Studenten mittels Github-Copilot Gratis zu Verfügung. @copilot-students

Um es möglichen Nutzern des Bots so einfach wie möglich zu machen, den Bot selbst zu hosten und ihre eigenen API Keys zu verwenden, verwende ich die von Microsoft entwickelte #link("https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.AI/README.md")[Microsoft.Extensions.AI] Bibliothek. Diese abstrahiert die verschiedenen AI Anbieter und ermöglicht es, den Anbieter einfach zu wechseln, ohne den Code anpassen zu müssen. @microsoft-extensions-ai

== Dockerization <docker>

= Resultate

#pagebreak()

#bibliography("sources.yaml")
