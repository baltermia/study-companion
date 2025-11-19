#import "@preview/bubble:0.2.2": *

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

#outline(title: "Inhaltsverzeichnis")
#pagebreak()

= Einführung
== Ideenfindung
Die Idee für mein Projekt habe ich erstaunlicherweise schnell gefunden. Während einer der Präsentationen eines alten Schülers des Moduls während unserer Blockwoche kam mir ganz zufällig der Gedanke einen Telegram Bot zu programmieren, der mir das Studienleben erleichtern könnte. Das Schüler erzählte, dass er etwas erstellen wollte, was seine Produktivität steigern würde.

Ich habe schon vorher etwas Erfahrung mit Telegram Bots gesammelt und wusste direkt, dass meine Idee im gegebenen Zeitraum umsetzbar ist. Somit war die Idee für mein Projekt geboren. Mein Ziel ist es, dass auch andere Studierende den Bot möglichst einfach selbst installieren und brauchen können.

== Study Companion

Der Study-Companion ist ein AI Telegram Bot, der das Studienleben erleichtern soll. Das tut er vor Allem durch Zeitgesteuerte Erinnerungen, sei es für Vorlesungen, Abgaben, Prüfungen oder Lernzeiten. Diese Zeiten können einerseits durch den von der FHGR zur Verfügung gestellten iCal Kalender automatisch importiert werden, andererseits können manuelle Termine wie Lernzeiten hinzugefügt werden. Es können auch ganz eine TODOs hinzugefügt werden, welche der Bot dann zu den gewünschten Zeiten erinnert.

Die Erinnerungen sollten nicht nur wie von jeder anderen App einfach nur eine Benachrichtigung sein, sondern durch die AI Funktionalität des Bots auch motivieren. So kann der Bot zum Beispiel auf eine anstehende Prüfung hinweisen und gleichzeitig Tipps zum Lernen geben oder motivierende Zitate senden.

Der Bot sollte tägliche und wöchentliche Zusammenfassungen der anstehenden Termine und Aufgaben senden, um den Studierenden einen Überblick über ihre Woche zu geben. Mir kam die Idee, dass die tägliche Nachricht am morgen gleich beim aufstehen passieren soll. Das könnte er z.B. durch einen Helligkeitssensor am Raspberry Pi erkennen.

= Planung

Planung vor dem Start des Projektes ist wichtig. Ich kann damit wichtige Ziele setzen und grosse Zeitverschwendung vermeiden. Ich teile die Ziele in zwei Kategorien ein.

Den Bot werde ich in C\# mit dem .NET Framework entwickeln, für die Datenbank werde ich PostgreSQL verwenden. Für die AI Funktionalität werde ich die OpenAI API verwenden. Dazu werde ich folgende Bibliotheken verwenden:
- #link("https://github.com/ical-org/ical.net")[iCal.NET]: Für die iCal Integration
- #link("https://github.com/k-paul-acct/minimal-telegram-bot")[MinimalTelegramBot]: Für den Telegram Bot
- #link("https://github.com/openai/openai-dotnet")[OpenAI.NET]: Für die OpenAI API Integration

Dazu wurde ich noch einige .NET interne Bibliotheken verwenden wie z.B. EntityFramework Core für die Datenbankanbindung.

== Hauptziele

1. iCal Integration: Der von der FHGR bereitgestellte iCal Kalender soll importiert und verarbeitet werden. Termine daraus sollen automatisch Erinnerungen generieren.
2. Automatische Tägliche Zusammenfassungen: Der Bot soll jeden Morgen eine Zusammenfassung der anstehenden Termine und Aufgaben senden.
3. Eintragen von Prüfungen, Hausaufgaben und TODOs: Es sollte so einfach wie möglich sein Termine und TODOs einzufügen.
4. Wöchentliche Ansicht: Kann jederzeit abgerufen werden und enthält die wichtigsten Daten in einer einzigen Ansicht.


== Nebenziele

1. Dockerized: Die Applikation sollte in einem Docker Container laufen, um die Installation und den Betrieb für andere zu vereinfachen.
2. Wöchentliche Kalender Ansicht: Wie ein gewöhnlicher Kalender, der die Woche übersichtlich darstellt. Dies könnte etwas aufwändiger sein.
3. Lernzeiten: Diese gelten als Spezielle Termine, während der Lernzeit sollten auch Erinnerungen für Pausen gesendet werden.
4. Tägliche Zusammenfassung am Morgen durch Helligkeitssensor erkennen.
5. Einfache Codeerweiterung: Der Code sollte modular aufgebaut sein, um zukünftige Erweiterungen zu erleichtern.

== Aufgaben und Reihenfolgeplan

Meiner Erfahrung nach hat es sich nie gross gelohnt, exakte Zeiten für die einzelnen Aufgaben zu planen. Vielmehr möchte ich definieren welche Aufgaben ich in welcher Reihenfolge erledigen möchte.

1. Repository sowie .NET Projekt initialisieren
2. Start Nachricht des Bots einbauen
3. iCal einlesen
4. Zusammenfassung des Kalenders mit AI generieren
5. Eintragen von Prüfungen, Hausaufgaben und TODOs
6. Automatische Erinnerungen
7. Tägliche Zusammenfassung am Morgen