# CLAUDE.md

To opis projektu  common.validation, który jest próbą konstrukcji reużywalnego modułu dostarczającego kompleksowe rozwiązanie szkieletowe pozwalające korzystać z właściwości obiektu lub obiektu, wraz z definiowalnymi wzoracami akceptacji.

## Przegląd projektu

Projekt składa się z kilku komplementarnych części:
1. demo - prosta próba implementacji proponowanego rozwiązania,
2. src/Common.Validation - solucja zawierająca projekty.

## Architektura

1. Interoperacyjne, dynamiczne rozwiązanie pozwalające na zastosowanie walidacji wraz ze szkieletem w warstwie prezentacji obiektu i/lub właściwości obiektu
	a) walidacja na podstawie definicji zawartej w pliku JSON, wraz z wstępną implemetacją po stronie klienta jak i serwera. 
	Strona serwera powinna zakładać możliwość wdrożenia w wielu warstwach rowiązania takich jak: Model API (REST), DTO, Encje bazodanowe (nie musi to być pełen zakres możliwych zastosowań)
	b) prezentacja to szkielet lub komponent, który może być umieszczony w rozwiązaniu FrontEnd, np w ramach Blazor lub TypeScript
	c) na różnych warstwach implementacji wymagalność oraz poziom istotności może być zmienny dla tej samej walidacji, np. od strony klienta mając dwa pola tekstowe (A i B), jedno z nich musi być zawsze wypełnione, w relacyjnej bazie danych (MSSQL) taka konstrukcja jest niemal niemożliwa.

## Założenia wstępne

1. Do stworzenia aplikacji użyty zostanie .NET 10/C#
2. Rozwiązanie pozwala na tworzenie rozszerzeń, ale rdzeń nie powinien być modyfikowalny
3. Ostateczne rozwiązanie będzie dystrybuowane jako paczka nuget

## Priorytety

1. Interoperacyjność
2. Reguły SOLID
3. Możliwość wstrzykiwania reguł w ramach IoC


