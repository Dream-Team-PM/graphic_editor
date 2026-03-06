# **Модуль IO**


### Архитектура

```
├── IProjectService.cs                 ← главный фасад (используется в ViewModel)
├── Dto/                               ← DTO-объекты для сериализации
│   ├── FigureDto.cs
│   ├── LayerDto.cs
│   └── ProjectDto.cs
├── Mappers/                           ← вся логика преобразования
│   ├── FigureDtoMapper.cs
│   └── ProjectDtoMapper.cs
├── ProjectFormat/                     ← стратегия форматов (Open/Closed)
│   ├── IProjectFormat.cs
│   └── JsonProjectFormat.cs
└── Services/
    └── ProjectService.cs              ← реализация IProjectService
```

---

### Ключевые классы

| Класс                    | Что делает                                                                 | Где используется          |
|--------------------------|----------------------------------------------------------------------------|---------------------------|
| `IProjectService`        | Основной интерфейс (Save / Load)                                           | MainWindowViewModel       |
| `IProjectFormat`         | Стратегия формата (можно добавить SVG, PSD и т.д.)                         | DI-контейнер              |
| `JsonProjectFormat`      | Реальная работа с JSON                                                     | сейчас используется       |
| `ProjectDto` / `LayerDto`| Корневые DTO проекта и слоя                                                | сериализация              |
| `FigureDto` + наследники | Полиморфные DTO всех фигур (`$type`)                                       | FigureDtoMapper           |
| `FigureDtoMapper`        | **ЕДИНСТВЕННОЕ** место преобразования VM ↔ DTO (switch-expression)         | JsonProjectFormat         |
| `ProjectService`         | DI-обёртка над форматом + обработка расширений файлов                      | внедряется в ViewModel    |

---

### Минимальные изменения в других файлах

**1. `App.axaml.cs`** (или где `services.Add...`)
```csharp
// в ConfigureServices:
services.AddSingleton<IProjectFormat, JsonProjectFormat>();
services.AddSingleton<IProjectService, ProjectService>();
```

**2. `MainWindowViewModel.cs`**
```csharp
private readonly IProjectService _projectService;

public MainWindowViewModel(IProjectService projectService, CanvasViewModel canvas)
{
    _projectService = projectService;
    Canvas = canvas;

    SaveCommand = ReactiveCommand.CreateFromTask(async () => {
        var path = await ShowSaveFileDialogAsync();
        if (path != null) await _projectService.SaveProjectAsync(path, Canvas);
    });

    LoadCommand = ReactiveCommand.CreateFromTask(async () => {
        var path = await ShowOpenFileDialogAsync();
        if (path != null) await _projectService.LoadProjectAsync(path, Canvas);
    });
}

public ReactiveCommand<Unit, Unit> SaveCommand { get; }
public ReactiveCommand<Unit, Unit> LoadCommand { get; }
```

**3. `MainWindow.axaml.cs`**
```csharp
DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
```


---


### Как использовать (в MainWindowViewModel)

```csharp
public class MainWindowViewModel
{
    private readonly IProjectService _io;

    public MainWindowViewModel(IProjectService io, CanvasViewModel canvas)
    {
        _io = io;
        Canvas = canvas;
    }

    public ReactiveCommand<Unit, Unit> SaveCommand => ReactiveCommand.CreateFromTask(async () =>
    {
        var path = await ShowSaveDialogAsync();
        if (path != null)
            await _io.SaveProjectAsync(path, Canvas);
    });

    public ReactiveCommand<Unit, Unit> LoadCommand => ReactiveCommand.CreateFromTask(async () =>
    {
        var path = await ShowOpenDialogAsync();
        if (path != null)
            await _io.LoadProjectAsync(path, Canvas);
    });
}
```

---

### Пример файла проекта `.vec` (JSON)

```json
{
  "version": "1.0",
  "zoom": 1.25,
  "offsetX": 50,
  "offsetY": 30,
  "layers": [
    {
      "id": "a1b2c3d4-1111-2222-3333-444455556666",
      "name": "Слой 1",
      "isVisible": true,
      "isLocked": false,
      "figures": [
        {
          "$type": "Rectangle",
          "id": "f1111111-aaaa-bbbb-cccc-dddddddddddd",
          "name": "Прямоугольник",
          "opacity": 1.0,
          "rotation": 45,
          "lineColorArgb": -16777216,
          "fillColorArgb": -16711936,
          "thickness": 3.0,
          "isSelected": false,
          "x": 100,
          "y": 150,
          "width": 200,
          "height": 120
        },
        {
          "$type": "Group",
          "id": "g2222222-xxxx-yyyy-zzzz-ffffffffffff",
          "name": "Группа",
          "opacity": 0.9,
          "rotation": 0,
          "children": [
            {
              "$type": "Ellipse",
              "id": "e3333333-...",
              "x": 300,
              "y": 200,
              "width": 80,
              "height": 80
            },
            {
              "$type": "Line",
              "id": "l4444444-...",
              "x1": 400,
              "y1": 250,
              "x2": 500,
              "y2": 350
            }
          ]
        }
      ]
    }
  ]
}
```