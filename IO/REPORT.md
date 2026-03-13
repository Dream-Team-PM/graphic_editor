# Отчёт об изменениях — IO модуль (ветка `dev_IO`)

**Дата:** 2026-03-13
**Ветка:** `dev_IO`
**Базируется на:** `dev_ViewModel` (смёржено)

---

## Что изменилось

### 1. Новые файлы

| Файл | Назначение |
|------|-----------|
| `IO/ProjectFormat/SvgProjectFormat.cs` | Сохранение и загрузка SVG |
| `IO/Export/PngExporter.cs` | Экспорт холста в PNG |

### 2. Изменённые файлы

| Файл | Что изменено |
|------|-------------|
| `IO/Services/ProjectService.cs` | Реестр форматов, выбор по расширению |
| `IO/ProjectFormat/IProjectFormat.cs` | Добавлен `using graphic_editor.ViewModels` |
| `IO/IProjectService.cs` | Добавлен `using graphic_editor.ViewModels` |
| `IO/ProjectFormat/JsonProjectFormat.cs` | Добавлен `using graphic_editor.ViewModels` |
| `IO/Mappers/FigureDtoMapper.cs` | Исправлен порядок: `CircleViewModel` перед `EllipseViewModel` |
| `ViewModels/LayerViewModel.cs` | Добавлен конструктор `(Guid id, string name)` |
| `Views/MainWindow.axaml` | Click-обработчики на меню и кнопку Экспорт |
| `Views/MainWindow.axaml.cs` | Файловые диалоги, подключение `ProjectService` |

---

## Обновлённая архитектура IO

```
IO/
├── IProjectService.cs               ← фасад (Save / Load по пути)
├── Dto/                             ← DTO для JSON
│   ├── FigureDto.cs
│   ├── LayerDto.cs
│   └── ProjectDto.cs
├── Mappers/
│   ├── FigureDtoMapper.cs           ← VM ↔ DTO (switch-expression)
│   └── ProjectDtoMapper.cs
├── ProjectFormat/                   ← стратегия форматов
│   ├── IProjectFormat.cs
│   ├── JsonProjectFormat.cs         ← .vec / .json
│   └── SvgProjectFormat.cs          ← .svg  [НОВЫЙ]
├── Export/
│   └── PngExporter.cs               ← .png  [НОВЫЙ]
└── Services/
    └── ProjectService.cs            ← реестр форматов [обновлён]
```

### Реестр форматов в `ProjectService`

```
.vec  →  JsonProjectFormat
.json →  JsonProjectFormat
.svg  →  SvgProjectFormat
```

PNG не входит в `IProjectFormat` — это экспорт (только запись, рендер visual).

---

## Поддерживаемые форматы

| Формат | Расширение | Сохранение | Открытие | Примечание |
|--------|-----------|:----------:|:--------:|-----------|
| JSON (проект) | `.vec` | ✓ | ✓ | Основной формат, полный round-trip |
| JSON (сырой) | `.json` | ✓ | ✓ | То же, что `.vec` |
| SVG | `.svg` | ✓ | ✓ | Все примитивы + группы + слои |
| PNG | `.png` | ✓ | ✗ | Рендер через `RenderTargetBitmap` |

### SVG: что сериализуется

| Фигура | SVG-элемент |
|--------|------------|
| `RectangleViewModel` | `<rect>` |
| `CircleViewModel` | `<circle>` |
| `EllipseViewModel` | `<ellipse>` |
| `LineViewModel` | `<line>` |
| `PenPointViewModel` | `<circle r="thickness/2">` |
| `GroupViewModel` | `<g>` (рекурсивно) |
| Слой | `<g id="{Guid}" data-name="{name}">` |

Атрибуты: `fill`, `stroke`, `stroke-width`, `opacity`, `transform="rotate(...)"`.

---

## Изменения вне IO модуля

### `ViewModels/LayerViewModel.cs`
Добавлен конструктор с явным `Guid` — нужен при загрузке, чтобы сохранить id слоя из файла:
```csharp
public LayerViewModel(Guid id, string name) { ... }
```

### `IO/Mappers/FigureDtoMapper.cs`
Исправлена ошибка компиляции CS8510 — `CircleViewModel` наследует `EllipseViewModel`, поэтому в `switch` он должен идти **раньше**:
```csharp
// БЫЛО (недостижимый паттерн):
EllipseViewModel e => ...
CircleViewModel c => ...   // ← никогда не вызывался

// СТАЛО:
CircleViewModel c => ...   // ← сначала специфичный тип
EllipseViewModel e => ...
```

### `Views/MainWindow.axaml` + `MainWindow.axaml.cs`
Файловые диалоги подключены напрямую в code-behind (без изменения ViewModel):

- **Открыть** (`Ctrl+O`) — `OpenFilePickerAsync`, фильтры: `.vec`, `.json`, `.svg`
- **Сохранить** (`Ctrl+S`) — сохраняет в `_currentFilePath`, при первом запуске вызывает "Сохранить как"
- **Сохранить как** (`Ctrl+Shift+S`) — `SaveFilePickerAsync`, форматы: `.vec`, `.svg`
- **Экспорт** — `SaveFilePickerAsync` → PNG, рендерит `VectorCanvasControl`

Title окна обновляется при открытии/сохранении: `INKognida — имя_файла`.

---

## Используемые библиотеки

| Библиотека | Версия | Где используется |
|-----------|--------|-----------------|
| `Avalonia` | 11.3.12 | UI, рендер PNG (`RenderTargetBitmap`) |
| `Avalonia.Platform.Storage` | 11.3.12 | Файловые диалоги (`StorageProvider`) |
| `System.Text.Json` | .NET 9 | JSON сериализация (`.vec`) |
| `System.Xml.Linq` | .NET 9 | SVG парсинг (`XDocument`) |
| `ReactiveUI` | 22.3.1 | Команды в ViewModel |

`System.Xml.Linq` уже входит в .NET 9 — новых зависимостей в `.csproj` не добавлялось.