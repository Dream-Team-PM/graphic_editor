# **Техническое задание (ТЗ)**  
## **Проект: Графический редактор с нейронными сетями (GraphicEditor)**  

---

### **1. Общие сведения**

#### 1.1 Наименование проекта  
**Graphic Editor with Neural Networks** (графический редактор с поддержкой нейросетевых алгоритмов)  

#### 1.2 Цель проекта  
Разработка кроссплатформенного графического редактора на C# с интеграцией нейросетевых алгоритмов для обработки изображений и векторной графики.  

#### 1.3 Целевая платформа  
- **Операционные системы:** Windows, Linux, macOS (благодаря Avalonia)  
- **Технологии:** .NET, Avalonia UI  
- **Инфраструктура:** Docker, GitHub Actions, GitLab CI  

---

### **2. Основные направления разработки**

#### 2.1 GUI (Пользовательский интерфейс)  
**Задачи:**
- Разработка главного окна приложения (меню, панель инструментов, рабочая область)
- Реализация инструментов рисования (кисть, линия, прямоугольник, эллипс)
- Панель слоев и история действий (Undo/Redo)
- Интеграция нейросетевых инструментов (генерация, стилизация, сегментация)
- Настройки горячих клавиш и интерфейса

**Технологии:** Avalonia UI, XAML, ReactiveUI (MVVM)

#### 2.2 ViewModels (Модели представления)  
**Задачи:**
- Реализация паттерна MVVM
- ViewModel главного окна (`MainWindowViewModel`)
- ViewModel для инструментов (`ToolsViewModel`)
- ViewModel для слоев (`LayersViewModel`)
- ViewModel для нейросетевых операций (`NeuralNetworkViewModel`)
- Команды и биндинги для UI

**Технологии:** ReactiveUI, CommunityToolkit.Mvvm, System.Reactive

#### 2.3 IO (Ввод/Вывод)  
**Обязательно:**
- ✅ **Экспорт в SVG** (векторный формат)
- ✅ **Экспорт/импорт в JSON** (свой формат проекта)
- ✅ **Экспорт в растровые форматы** (PNG, JPG, JPEG, BMP) – желательно
- ✅ **Экспорт в другие растровые форматы** (GIF, TIFF, Raw, PSD)
- ✅ **Экспорт в другие векторные форматы** (PDF, Eps, AI, CDR)

**Детали реализации:**
- Сериализация/десериализация документов
- Сохранение истории действий в JSON
- Интеграция с нейросетями для экспорта обработанных изображений

**Технологии:** System.Text.Json, SVG rendering libraries

#### 2.4 QA (Тестирование)  
**Задачи:**
- Модульное тестирование (Unit Tests)
- Интеграционное тестирование
- Тестирование UI (Avalonia UI Testing)
- Тестирование нейросетевых компонентов
- Автоматизация тестов в CI/CD

**Технологии:** xUnit, NUnit, Moq, Avalonia.Headless.NUnit, Playwright

#### 2.5 Geometry (Геометрические фигуры)  
**Задачи:**
- Базовые геометрические примитивы (Point, Size, Rect)
- Фигуры: Line, Rectangle, Ellipse, Path, Bezier curve
- Трансформации (move, scale, rotate)
- Алгоритмы пересечения и булевы операции
- Поддержка слоев и групп фигур

**Технологии:** System.Numerics, Avalonia.Media

---

### **3. Архитектура проекта**

* Ниже приведена примерная архитектура будущего проекта*
```
graphic_editor/
├── src/
│   ├── GraphicEditor/
│   │   ├── GraphicEditor.csproj
│   │   ├── App.axaml
│   │   ├── App.axaml.cs
│   │   ├── MainWindow.axaml
│   │   ├── MainWindow.axaml.cs
│   │   ├── ViewModels/
│   │   ├── Views/
│   │   ├── Models/
│   │   └── Services/
│   ├── GraphicEditor.IO/
│   │   ├── IExportService.cs
│   │   ├── SvgExportService.cs
│   │   ├── JsonProjectService.cs
│   │   └── RasterExportService.cs
│   ├── GraphicEditor.Geometry/
│   │   ├── Primitives/
│   │   ├── Shapes/
│   │   └── Transformations/
│   ├── GraphicEditor.Neural/
│   │   ├── INeuralService.cs
│   │   ├── ONNXRuntimeService.cs
│   │   └── Models/
│   └── GraphicEditor.Utils/
│       ├── Extensions/
│       └── Helpers/
├── tests/
│   ├── GraphicEditor.Tests/
│   ├── GraphicEditor.IO.Tests/
│   ├── GraphicEditor.Geometry.Tests/
│   └── GraphicEditor.Neural.Tests/
├── deploy/
│   ├── Dockerfile
│   └── docker-compose.yml
├── .github/
│   └── workflows/
│       └── deploy.yml
└── README.md
```

---

### **4. Технологический стек**

| Компонент | Технология | Обоснование |
|-----------|------------|-------------|
| **GUI Framework** | Avalonia UI | Кроссплатформенность, XAML, MVVM |
| **MVVM Toolkit** | ReactiveUI / CommunityToolkit.Mvvm | Реактивность, тестируемость |
| **DI Container** | Microsoft.Extensions.DependencyInjection | Встроенный DI |
| **Сериализация** | System.Text.Json | Производительность, встроенная |
| **SVG Export** | Custom / Svg.Skia | Векторный экспорт |
| **Тестирование** | xUnit, Moq, Playwright | Полнота покрытия |
| **Нейросети** | ONNX Runtime | Интеграция ML моделей |
| **CI/CD** | GitHub Actions | Автоматизация сборки |
| **Контейнеризация** | Docker, docker-compose | Изоляция, тестирование |

---

### **5. Инструменты разработки**

- **IDE:** Visual Studio 2022 / JetBrains Rider / VS Code
- **Система контроля версий:** Git (GitLab)
- **CI/CD:** GitHub Actions / GitLab CI
- **Контейнеризация:** Docker
- **Управление зависимостями:** NuGet
- **Линтеры:** .NET analyzers, StyleCop
- **Форматирование:** .editorconfig

---

### **6. Сборка и развертывание**

#### 6.1 Локальная сборка
```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/GraphicEditor
```

#### 6.2 Docker-сборка
```dockerfile
# deploy/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "GraphicEditor.dll"]
```

#### 6.3 Docker Compose
```yaml
# deploy/docker-compose.yml
version: '3.8'
services:
  graphic-editor:
    build:
      context: ..
      dockerfile: deploy/Dockerfile
    environment:
      - DOTNET_ENVIRONMENT=Production
    volumes:
      - ./data:/app/data
```

---

### **7. CI/CD (GitHub Actions)**

```yaml
# .github/workflows/deploy.yml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 10.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish -c Release -o publish
    
    - name: Upload artifacts
      uses: actions/upload-artifact@v4
      with:
        name: graphic-editor
        path: publish/
```

---

### **8. Начало работы (Git)**

```bash
# Инициализация репозитория
cd graphic_editor
git init

# Добавление удаленного репозитория (GitHub)
git remote add origin https://github.com/Dream-Team-PM/graphic_editor.git

# Создание основной ветки
git branch -M main

# Первый коммит
git add .
git commit -m "Initial commit: Graphic Editor with Neural Networks"

# Отправка в репозиторий
git push -uf origin main
```

---

### **9. Требования к реализации**

#### 9.1 GUI
- Адаптивный интерфейс под разные разрешения
- Поддержка светлой и темной темы
- Локализация (русский/английский)

#### 9.2 ViewModels
- Полное разделение View и ViewModel
- Поддержка команд и биндингов
- Тестируемость ViewModels

#### 9.3 IO
- **Обязательно:** экспорт в SVG
- **Обязательно:** сохранение/загрузка JSON
- **Желательно:** экспорт в PNG/JPEG
- Валидация данных при импорте

#### 9.4 QA
- Покрытие тестами не менее 70%
- Автоматизация в CI/CD
- Тестирование критических сценариев

#### 9.5 Geometry
- Точные геометрические вычисления
- Поддержка трансформаций
- Оптимизация для работы с большими количествами фигур

---

### **10. Этапы разработки**

| Этап | Срок | Результат |
|------|------|-----------|
| **1. Настройка инфраструктуры** | 1 нед | Репозиторий, CI/CD, Docker |
| **2. Базовый GUI** | 2 нед | Главное окно, панели инструментов |
| **3. Геометрические примитивы** | 2 нед | Рисование и редактирование фигур |
| **4. IO (SVG, JSON)** | 2 нед | Экспорт/импорт документов |
| **5. Нейросетевая интеграция** | 3 нед | ONNX Runtime, предиктивные инструменты |
| **6. Тестирование и оптимизация** | 2 нед | Покрытие тестами, документация |
| **7. Финальная сборка** | 1 нед | Релиз, Docker-образ |

---

### **11. Критерии приемки**

- [x] Приложение собирается и запускается на всех целевых ОС
- [x] Реализован экспорт в SVG и JSON
- [x] Работают базовые инструменты рисования
- [x] Интегрирована хотя бы одна нейросетевая модель
- [x] Проходят все модульные тесты
- [x] CI/CD пайплайн успешно выполняется
- [x] Документация (README, комментарии в коде)

---

**Составил:** F.A.S.T. Development aka Vladimir  
**Дата:** 17.02.2026  
**Версия:** 1.0