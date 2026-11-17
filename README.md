# Lekcja 3.1 — Minutnik i okno kontekstu

**Autor:** Julia Kluga (`estr311a`)  
**Data:** 17.11.2026 r.  
**Narzędzie:** Antigravity IDE (Google DeepMind)  
**Model sztucznej inteligencji:** Gemini 3.8 Flash (High)  

---

## ⏱️ 1. Opis projektu — Minutnik Pomodoro

Aplikacja desktopowa typu Minutnik Pomodoro zbudowana w oparciu o framework **Avalonia UI (.NET 9)**, bez użycia zewnętrznych bibliotek, z całą logiką zawartą w pliku `MainWindow.axaml.cs`.

### Spełnienie 7 zasad lekcji:
1. **Czas pracy (domyślnie 25 min) i przerwy (domyślnie 5 min)** — obie wartości można dowolnie edytować przed startem sesji.
2. **Przyciski Start/Pauza oraz Reset** — pełna kontrola nad stanem odliczania i możliwość zresetowania do wartości początkowych.
3. **Widoczny licznik ukończonych cykli** — informacja o liczbie pełnych cykli (praca + przerwa) na bieżąco prezentowana użytkownikowi.
4. **Automatyczne rozpoczęcie przerwy po pracy** — gdy skończy się czas pracy, minutnik natychmiast automatycznie rozpoczyna odliczanie przerwy. Po zakończeniu przerwy licznik cykli zwiększa się i minutnik przechodzi do kolejnego cyklu.
5. **Format wyświetlania `mm:ss`** — czas prezentowany w czytelnej, dużej typografii w formacie minut i sekund (np. `25:00`, `04:59`).
6. **Polski interfejs, angielskie nazwy w kodzie** — wszystkie etykiety, przyciski i komunikaty UI w języku polskim, natomiast identyfikatory, metody, pola i klasy w kodzie C# wyłącznie w języku angielskim (`_cycles`, `_timer`, `_remainingSeconds`, `_workMinutes`, `_breakMinutes`, `_isWorkSession`, `_isRunning`).
7. **Bez dodatkowych bibliotek** — aplikacja wykorzystuje wyłącznie standardowe komponenty frameworka Avalonia oraz biblioteki bazowe .NET (`System`, `Avalonia.Controls`, `Avalonia.Threading.DispatcherTimer`).

Dodatkowo zaimplementowano **własnoręcznie dodaną historię cykli** (`ListBox Name="HistoryList"`), która po każdym ukończonym cyklu odnotowuje godzinę i numer cyklu w formacie: `HH:mm — cykl X`.

---

## 📊 2. Analiza tokenów i okna kontekstu

### Treść pierwszej wiadomości (Prompt zadania):
- **Wersja polska:**
  > „Poproś agenta o minutnik spełniający 7 zasad:  
  > 1. Praca 25 minut, przerwa 5 minut – obie wartości można zmieniać.  
  > 2. Przyciski start/pauza i reset.  
  > 3. Widoczny licznik ukończonych cykli.  
  > 4. Automatyczne rozpoczęcie przerwy po pracy.  
  > 5. Czas wyświetlany jako mm:ss.  
  > 6. Polski interfejs, angielskie nazwy w kodzie.  
  > 7. Bez dodatkowych bibliotek.  
  > Cała logika ma znajdować się w MainWindow.axaml.cs.”

- **Wersja angielska:**
  > „Ask the agent for a timer meeting 7 rules:  
  > 1. Work 25 minutes, break 5 minutes – both values can be changed.  
  > 2. Start/pause and reset buttons.  
  > 3. Visible completed cycles counter.  
  > 4. Automatic start of break after work.  
  > 5. Time displayed as mm:ss.  
  > 6. Polish interface, English names in code.  
  > 7. Without additional libraries.  
  > All logic must be located in MainWindow.axaml.cs.”

### Liczba tokenów:
| Język promptu | Liczba znaków | Liczba słów | Szacowana liczba tokenów (BPE / Gemini Tokenizer) |
| :--- | :---: | :---: | :---: |
| **Język polski** | 365 | 45 | **~96 tokenów** |
| **Język angielski** | 312 | 44 | **~68 tokenów** |

> **Dlaczego polski prompt ma więcej tokenów?**  
> Tokenizery modeli językowych (np. BPE – Byte Pair Encoding) bazują głównie na korpusach anglojęzycznych. W języku angielskim całe słowa lub popularne rdzenie stanowią zazwyczaj pojedyncze tokeny (~1 token na słowo). Język polski posiada bogatą fleksję oraz znaki diakrytyczne (`ą`, `ę`, `ł`, `ś`, `ć`, `ż`, `ź`, `ń`, `ó`), które w kodowaniu UTF-8 zajmują wiele bajtów. W efekcie polskie słowa są dzielone na mniejsze fragmenty (pod-słowa lub pojedyncze znaki), co zwiększa liczbę tokenów o około 40–50% przy tej samej treści semantycznej.

### Parametry modelu:
- **Model:** Gemini 3.8 Flash (High) w narzędziu Antigravity IDE
- **Wielkość okna kontekstu (Context Window):** **1 000 000 tokenów (1M)**
- **Maksymalna długość pojedynczej odpowiedzi (Max Output Tokens):** **8 192 tokeny**
- **Data zebrania informacji:** 17.11.2026 r.
- **Źródła informacji:** Oficjalna dokumentacja Google DeepMind (`ai.google.dev`, specyfikacja architektury Gemini Flash), metadane środowiska uruchomieniowego Antigravity.

---

## 🧪 3. Wynik testu wpisania `25,5` oraz `25.5` w polu długości

W ramach lekcji przetestowano zachowanie aplikacji po wpisaniu wartości niebędących tradycyjnymi liczbami całkowitymi:

1. **Wpisanie wartości `25,5` (z przecinkiem):**
   - **Zachowanie aplikacji:** Walidacja `int.TryParse` zwraca `false`, ponieważ przecinek nie jest dozwolonym znakiem w formacie liczby całkowitej.
   - **Komunikat:** Na pasku stanu pojawia się ostrzeżenie `Błąd: Nieprawidłowy czas pracy!`.
   - **Efekt:** Minutnik **nie uruchamia się**, chroniąc aplikację przed niespójnym stanem odliczania. Po naciśnięciu przycisku „Reset” pole automatycznie przywraca domyślną, bezpieczną wartość `25`.

2. **Wpisanie wartości `25.5` (z kropką):**
   - **Zachowanie aplikacji:** Walidacja `int.TryParse` zwraca `false`, ponieważ kropka dziesiętna również nie jest cyfrą liczby całkowitej.
   - **Komunikat:** Na pasku stanu pojawia się ostrzeżenie `Błąd: Nieprawidłowy czas pracy!`.
   - **Efekt:** Minutnik **nie uruchamia się**.

3. **Wnioski kulturowe i programistyczne:**
   - W standardzie polskim (`pl-PL`) separatorem dziesiętnym jest przecinek (`,`), podczas gdy w standardzie anglosaskim i technicznym (`InvariantCulture`) separatorem jest kropka (`.`).
   - Gdybyśmy użyli `double.TryParse` bez określenia kultury, na polskim systemie Windows wpisanie `25,5` zadziałałoby (jako 25,5 minuty = 1530 sekund), natomiast `25.5` zgłosiłoby błąd.
   - Wymuszenie walidacji całkowitych minut (`int.TryParse`) całkowicie eliminuje problem niejednoznaczności separatora dziesiętnego i zapobiega błędom parsowania na maszynach o różnych ustawieniach regionalnych.

---

## 📝 4. Podsumowanie pracy

- **Czas pracy:** ok. 45 minut.
- **Najtrudniejsza część zadania:** Zapewnienie wielowątkowego bezpieczeństwa timera (`DispatcherTimer`) w środowisku Avalonia UI, aby odliczanie czasu i modyfikacja elementów interfejsu (tekst minutnika, przycisk pauzy, lista historii) zawsze odbywały się w głównym wątku interfejsu (UI Thread), unikając błędu `Call from invalid thread`.
- **Własnoręczna zmiana (Krok 4):** Samodzielne dodanie kontrolki `<ListBox Name="HistoryList" />` w XAML oraz powiązanie jej z kolekcją `_history` w kodzie C# po każdym zakończonym cyklu.

---

## 🚀 5. Jak uruchomić aplikację?

1. **Szybkie uruchomienie skryptem:**
   Dwukrotnie kliknij plik `run.bat` znajdujący się w katalogu głównym projektu.

2. **Uruchomienie przez terminal (.NET CLI):**
   ```bash
   dotnet run --project minutnik/minutnik.csproj
   ```
