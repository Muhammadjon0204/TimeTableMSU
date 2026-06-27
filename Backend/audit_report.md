# Архитектурный аудит Domain + Infrastructure

> Дата первичного аудита: 2026-06-25  
> Дата фактической проверки и исправлений: 2026-06-26  
> Область: Domain entities, EF Core configurations, migrations snapshot  
> Статус: все найденные архитектурные замечания проверены; реальные проблемы исправлены.

---

## Сводная таблица

| # | Статус | Файл | Итог |
|---|--------|------|------|
| 1 | ✅ Проверено, исправление не требуется | `AcademicPerformance.cs`, `Discipline.cs` | `DisciplineId` корректно имеет тип `int`, потому что `Discipline.Id` также `int`. Строковый PK `Code` в текущей модели не используется. |
| 2 | ✅ Исправлено ранее, подтверждено | `DisciplineConfiguration.cs` | `Subject -> Discipline` оставлен `Cascade`, `Teacher -> Discipline` и `Group -> Discipline` стоят `Restrict`. Multiple cascade paths здесь нет. |
| 3 | ✅ Исправлено ранее, подтверждено | `Execution.cs`, `ExecutionConfiguration.cs` | `Execution` больше не ссылается напрямую на `Teacher` и `Discipline`; теперь связь идет через `ScheduleId`. |
| 4 | ✅ Проверено, логика оставлена | `ScheduleConfiguration.cs` | `Discipline -> Schedule` оставлен `Cascade`, остальные FK (`Teacher`, `Audience`, `Group`, `Week`) стоят `Restrict`. Для текущей модели это логично: расписание принадлежит дисциплине. |
| 5 | ✅ Уже исправлено | `StudentConfiguration.cs` | `FatherName` nullable в entity и не имеет `.IsRequired()` в конфигурации. |
| 6 | ✅ Уже исправлено | `Student.cs` | `GroupId` nullable и `Group?` nullable, контракт согласован. |
| 7 | ✅ Исправлено | `Group.cs`, `GroupConfiguration.cs` | `Course` сделан обязательным `short`, в конфигурации добавлен `.HasColumnType("smallint")`. |
| 8 | ✅ Уже исправлено | `SpecialityConfiguration.cs`, `FacultyConfiguration.cs` | Дублирующая настройка `Faculty -> Speciality` удалена из `SpecialityConfiguration`; связь задается в одном месте. |
| 9 | ✅ Проверено, логика оставлена | `SpecialityConfiguration.cs` | `Speciality -> Group` задается в `SpecialityConfiguration` через `Restrict`. Дублировать эту связь в `GroupConfiguration` не нужно. |
| 10 | ✅ Уже исправлено | `AcademicPerformanceConfiguration.cs` | `Tur` и `Mark` nullable и не имеют `.IsRequired()`, тип `smallint` задан явно. |
| 11 | ✅ Проверено, исправление не требуется | `Discipline.cs` | Текущий дизайн использует `int Id`, все FK согласованы. Переход на строковый `Code` был бы отдельным рефакторингом, не багом текущей модели. |
| 12 | ✅ Исправлено | `Weeks.cs`, `WeekConfiguration.cs` | `Name`, `StartDate`, `EndDate` сделаны обязательными в entity, что соответствует сервисной валидации и `.IsRequired()` в конфигурации. |
| 13 | ✅ Исправлено | `Subject.cs`, `SubjectConfiguration.cs` | `Semester` сделан обязательным `short`, добавлен явный `smallint`. |
| 14 | ✅ Исправлено | `Subject.cs`, `SubjectConfiguration.cs` | `HourCount` сделан обязательным `int`, что соответствует валидации `SubjectService`. |
| 15 | ✅ Проверено, исправление не требуется | `AudienceConfiguration.cs` | `AudienceType` помещается в `HasMaxLength(20)`. Запас небольшой, но текущим enum-значениям хватает. |
| 16 | ✅ Проверено, исправление не требуется | `TeacherConfiguration.cs` | Enum-поля преподавателя имеют `HasConversion<string>()` и `HasMaxLength(50)`, текущая структура корректна. |
| 17 | ✅ Уже исправлено | `ScheduleConfiguration.cs` | `Den` и `Para` имеют `.HasColumnType("smallint")`. |
| 18 | ✅ Исправлено | `AttendanceConfiguration.cs` | Для nullable `Attendance.Mark` добавлено явное `.IsRequired(false)`. |
| 19 | ✅ Проверено, исправление не требуется | `GroupConfiguration.cs` | Связи `Discipline -> Group` и `Schedule -> Group` корректно задаются на стороне зависимых сущностей. Дублирование в `GroupConfiguration` не нужно. |
| 20 | ✅ Уже исправлено | `Attendance.cs`, `AttendanceConfiguration.cs` | Навигация называется `Week`, конфигурация использует `builder.HasOne(a => a.Week)`. |

---

## Что изменено в коде

### ✅ `Domain/Entities/Group.cs`

`Course` был nullable:

```csharp
public short? Course { get; set; }
```

Сервис `GroupService` уже требовал курс как обязательное значение, поэтому контракт домена приведен к бизнес-логике:

```csharp
public short Course { get; set; }
```

### ✅ `Infrastructure/Persistence/Configurations/GroupConfiguration.cs`

Для `Course` добавлен явный PostgreSQL-тип:

```csharp
builder.Property(g => g.Course)
    .IsRequired()
    .HasColumnType("smallint");
```

Также удалено дублирующее конфигурирование связи `Group -> Student`, потому что эта связь уже корректно задана в `StudentConfiguration` как `SetNull`. Это важно: `Student.GroupId` nullable, значит при удалении группы студент не должен каскадно удаляться.

### ✅ `Application/Service/GroupService.cs`

После проверки `dto.Course == null` значение теперь присваивается строго:

```csharp
Course = dto.Course.Value;
group.Course = dto.Course.Value;
```

### ✅ `Domain/Entities/Subject.cs`

`Semester` и `HourCount` сделаны обязательными, потому что `SubjectService` не позволяет создать предмет без этих значений:

```csharp
public short Semester { get; set; }
public int HourCount { get; set; }
```

### ✅ `Infrastructure/Persistence/Configurations/SubjectConfiguration.cs`

`Semester` теперь явно хранится как `smallint`, а длина `Name` увеличена до 150, чтобы совпадать с валидацией `SubjectService`:

```csharp
builder.Property(s => s.Name)
    .IsRequired()
    .HasMaxLength(150);

builder.Property(s => s.Semester)
    .IsRequired()
    .HasColumnType("smallint");
```

### ✅ `Domain/Entities/Weeks.cs`

Учебная неделя теперь не может существовать без названия и дат:

```csharp
public string Name { get; set; } = null!;
public DateTime StartDate { get; set; }
public DateTime EndDate { get; set; }
```

Это соответствует `WeekService`, где `Name`, `StartDate`, `EndDate` уже валидируются как обязательные.

### ✅ `Infrastructure/Persistence/Configurations/AttendanceConfiguration.cs`

Для nullable enum `Mark` добавлена явная optional-настройка:

```csharp
builder.Property(a => a.Mark)
    .HasConversion<string>()
    .HasMaxLength(20)
    .IsRequired(false);
```

### ✅ `Infrastructure/Migrations`

Добавлена миграция:

```text
20260626125537_AuditArchitectureFixes
```

Она синхронизирует snapshot с текущей моделью и фиксирует накопленные изменения: `Execution -> Schedule`, `Schedule.Week`, таблицу `users`, а также изменение длины `subjects.Name` до 150.

После проверки через `dotnet ef database update` миграция была дополнительно исправлена для существующих данных: `schedules.WeekId` сначала добавляется как nullable, затем старые записи расписания получают первую доступную неделю. Если в базе есть расписания, но нет ни одной недели, миграция создает служебную запись `Default migration week`. Только после этого `WeekId` становится `NOT NULL` и получает FK на `weeks`.

---

## Что не трогалось специально

### ✅ `Discipline.Id` против `Code`

Отчет подозревал, что `Discipline` должна иметь строковый ключ `Code`. В текущем коде вся модель последовательно использует:

```csharp
public int Id { get; set; }
public int DisciplineId { get; set; }
```

Это согласованная структура. Менять ее на строковый ключ без отдельного требования нельзя: это затронет `Schedule`, `AcademicPerformance`, сервисы, DTO и миграции.

### ✅ `Speciality -> Group`

Связь оставлена в `SpecialityConfiguration`:

```csharp
builder.HasMany(s => s.Groups)
    .WithOne(g => g.Speciality)
    .HasForeignKey(g => g.SpecialityId)
    .OnDelete(DeleteBehavior.Restrict);
```

Дублировать ее в `GroupConfiguration` не нужно.

### ✅ Enum length

Текущие enum-значения помещаются в заданные лимиты. Это рекомендация на будущее, а не ошибка архитектуры.

---

## Проверка

Выполнена сборка:

```powershell
dotnet build Backend/Backend.slnx
```

Результат:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Миграция применена к базе:

```powershell
dotnet ef database update --project Backend/src/Infrastructure --startup-project Backend/src/Api
```

Результат:

```text
Done.
```

---

# Дополнительный отчет по завершенным слоям Application, Infrastructure Security, Repositories и API

> Дата дополнения отчета: 2026-06-26  
> Область: Application DI, typed settings, Infrastructure repositories, security services, API controllers  
> Статус: выполнено, проект успешно собирается.

---

## ✅ Строго типизированные настройки через Options Pattern

Добавлены POCO-классы настроек в `Application/Common/Settings/`:

| Файл | Назначение |
|---|---|
| `JwtSettings.cs` | Настройки JWT: `Secret`, `Issuer`, `Audience`, время жизни access/refresh токенов. |
| `SmtpSettings.cs` | Настройки SMTP: хост, порт, логин, пароль, SSL, адрес отправителя. |
| `GoogleAuthSettings.cs` | Настройки Google OAuth2: `ClientId`, `ClientSecret`. |

Итог: инфраструктурные сервисы больше не читают конфигурацию через сырые строки вида `IConfiguration["..."]`. Настройки внедряются через `IOptions<T>`, что снижает риск опечаток и делает конфигурацию типобезопасной.

---

## ✅ Инфраструктурная безопасность

Реализован контур авторизации и аутентификации:

| Файл | Что сделано |
|---|---|
| `Infrastructure/Security/PasswordHasher.cs` | Добавлено безопасное хэширование и проверка паролей через `BCrypt.Net-Next`. |
| `Infrastructure/Services/SmtpService.cs` | SMTP-сервис переведен на `IOptions<SmtpSettings>`, отправка писем обернута в `Result`. |
| `Infrastructure/Services/AuthService.cs` | Реализованы регистрация, логин, refresh token, forgot/reset password и внешний вход через Google. |

В `AuthService` учтено:

- одинаковая ошибка при неверном email или пароле;
- проверка email и сложности пароля;
- генерация JWT access token с claims пользователя;
- генерация криптографически стойкого refresh token;
- хранение refresh token в виде хэша;
- безопасный forgot password без раскрытия существования email;
- reset password с проверкой кода и срока действия;
- создание пользователя при первом Google external login.

---

## ✅ Реализация Repository Pattern

Полностью закрыт слой `Infrastructure/Persistence/Repositories/`.

Реализованы репозитории:

| # | Репозиторий | Статус |
|---|---|---|
| 1 | `FacultyRepository` | ✅ CRUD + проверка уникальности имени |
| 2 | `SpecialityRepository` | ✅ CRUD + `Include(Faculty)` + уникальность в рамках факультета |
| 3 | `GroupRepository` | ✅ CRUD + `Include(Speciality)` + проверка существования группы |
| 4 | `StudentRepository` | ✅ CRUD + пагинация + поиск + `Include(Group)` |
| 5 | `TeacherRepository` | ✅ CRUD + уникальность по email/ФИО |
| 6 | `SubjectRepository` | ✅ CRUD + уникальность названия в семестре |
| 7 | `DisciplineRepository` | ✅ CRUD + `Include(Subject)`, `Include(Teacher)`, `Include(Group)` |
| 8 | `AttendanceRepository` | ✅ CRUD + пагинация + `Include(Student)`, `Include(Week)` |
| 9 | `WeekRepository` | ✅ CRUD + проверка пересечения дат |
| 10 | `ScheduleRepository` | ✅ CRUD + цепочка include для расписания |
| 11 | `AudienceRepository` | ✅ CRUD + уникальность номера |
| 12 | `ExecutionRepository` | ✅ CRUD + `Include(Schedule)` |
| 13 | `UserRepository` | ✅ методы авторизации: поиск по email/refresh token, add/update |

Архитектурные правила соблюдены:

- чтение выполняется через `AsNoTracking()`;
- методы асинхронные и используют EF Core async API;
- изменения сохраняются через `SaveChangesAsync()`;
- связи подгружаются в репозиториях, а не размазываются по Application/API слоям.

---

## ✅ Регистрация зависимостей

Обновлена инфраструктурная регистрация:

| Файл | Что сделано |
|---|---|
| `Infrastructure/DependencyInjection.cs` | Зарегистрированы все репозитории, security-сервисы, `PasswordHasher`, Options для `Jwt`, `Smtp`, `GoogleAuth`. |
| `Application/DependencyInjection.cs` | Добавлена регистрация всех Application-сервисов. |
| `Application/Application.csproj` | Добавлен пакет `Microsoft.Extensions.DependencyInjection.Abstractions`, чтобы Application мог иметь собственный DI extension без зависимости от Web/API. |
| `Api/Program.cs` | Подключены `AddApplicationServices()`, `AddInfrastructureRepositories()`, `AddInfrastructureSecurity()`. |

Итог: API-слой не регистрирует сервисы вручную по одному, а подключает готовые extension methods каждого слоя.

---

## ✅ API Controllers

Добавлен полноценный слой контроллеров в `Api/Controllers/`.

| Контроллер | Маршрут | Назначение |
|---|---|---|
| `AuthController.cs` | `api/auth` | Регистрация, логин, refresh, forgot/reset password, external login. |
| `FacultiesController.cs` | `api/faculties` | CRUD факультетов. |
| `SpecialitiesController.cs` | `api/specialities` | CRUD специальностей. |
| `GroupsController.cs` | `api/groups` | CRUD групп. |
| `StudentsController.cs` | `api/students` | Получение с пагинацией/поиском, CRUD студентов. |
| `TeachersController.cs` | `api/teachers` | CRUD преподавателей. |
| `SubjectsController.cs` | `api/subjects` | CRUD предметов. |
| `DisciplinesController.cs` | `api/disciplines` | CRUD дисциплин. |
| `AudiencesController.cs` | `api/audiences` | CRUD аудиторий. |
| `WeeksController.cs` | `api/weeks` | CRUD учебных недель. |
| `SchedulesController.cs` | `api/schedules` | Получение с пагинацией, CRUD расписания. |
| `AttendancesController.cs` | `api/attendances` | Получение с пагинацией, создание/обновление/удаление посещаемости. |
| `AcademicPerformancesController.cs` | `api/academic-performances` | Получение с пагинацией, создание/обновление/удаление успеваемости. |
| `ExecutionsController.cs` | `api/executions` | CRUD выполнения занятий. |

Также добавлен базовый контроллер:

| Файл | Назначение |
|---|---|
| `ApiControllerBase.cs` | Единая обработка `Result<T>` и `Result`, возврат `Ok`, `Created`, `NoContent`, `BadRequest`, проверка несовпадения route id и `dto.Id`. |

Архитектурные правила контроллеров:

- контроллеры тонкие и не содержат бизнес-логику;
- вся валидация предметной области остается в Application-сервисах;
- все CRUD-контроллеры закрыты через `[Authorize]`;
- `AuthController` открыт через `[AllowAnonymous]`;
- update endpoints проверяют `id` из маршрута против `dto.Id`;
- пагинация вынесена в query parameters.

---

## ✅ Текущий итог по слоям

| Слой | Статус |
|---|---|
| Domain | ✅ Сущности и связи проверены, найденные архитектурные замечания исправлены. |
| Application | ✅ DTO, интерфейсы, сервисы и DI-регистрация готовы. |
| Infrastructure/Persistence | ✅ Все репозитории реализованы через EF Core и зарегистрированы в DI. |
| Infrastructure/Security | ✅ JWT, SMTP, Google OAuth2, BCrypt и refresh tokens реализованы через Options Pattern. |
| Api | ✅ Контроллеры созданы под реальные сервисные контракты. |

---

## ✅ Финальная проверка после добавления контроллеров

Выполнена сборка:

```powershell
dotnet build Backend/Backend.slnx
```

Результат:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Итог: проект находится в рабочем состоянии и готов к следующему этапу — проверке API через Swagger/Postman, настройке ролей/политик доступа и интеграционным тестам основных сценариев.

---

# TimeTableMSU Final Audit Report

## 1. Общий статус

✅ Готово на 85-90%

Backend архитектурно собран и компилируется: Domain, Application, Infrastructure, Repository Pattern, JWT/Refresh Token, SMTP, GoogleAuth, DTO, Result Pattern, DI и API controllers на месте. API фактически запускается, Swagger UI открывается, Swagger Bearer настроен, CORS подключен, role policies применены, защищенные endpoints без токена возвращают `401`, Student получает `403` на Admin CRUD, Admin проходит в Admin CRUD.

Проект все еще нельзя считать полностью production-ready из-за оставшихся крупных задач: нет `StudentPortalController`, нет `TeacherPortalController`, нет integration tests, нет Postman collection, а исторические enum aliases нужно позже очистить или оставить явно задокументированными.

## 2. Таблица проверки

| Блок | Статус | Что найдено | Что отсутствует | Что исправить |
| ---- | ------ | ----------- | --------------- | ------------- |
| Swagger/Postman | ⚠️ Частично | API запускается, Swagger UI доступен на `/swagger/index.html`, swagger json доступен на `/swagger/v1/swagger.json`, Swagger Bearer security scheme добавлен, Authorize Bearer доступен. | Postman collection отсутствует, Auth flows не прогнаны end-to-end через реальную БД/SMTP/OAuth. | Создать Postman collection, прогнать register/login/refresh/forgot/reset/external-login. |
| Roles & Authorization Policies | ✅ Исправлено | Есть `UserRole` enum: `Admin`, `Teacher`, `Student`; JWT содержит `ClaimTypes.Role`; добавлены `AuthPolicies`/`RoleNames`; `AddAuthorization` policies подключены; Admin CRUD закрыт через `AdminOnly`; Attendance/AcademicPerformance закрыты через `AdminOrTeacher`. | Teacher/Student scoped portal endpoints пока не созданы. | Создать `StudentPortalController` и `TeacherPortalController` отдельным этапом. |
| AcademicPerformanceRepository | ✅ Исправлено | Есть interface, repository, DI, service, controller; repository использует `AsNoTracking`, `Include(Student)`, `Include(Discipline).ThenInclude(Subject)`, `Include(Teacher)`; добавлен service/controller `GetById`; duplicate Student+Discipline+Teacher блокируется через `ExistsAsync`; runtime duplicate test прошел. | Нет integration tests на duplicate. | Добавить integration tests позже. |
| Integration Tests | ❌ Не готово | `dotnet test Backend/Backend.slnx` выполняется без ошибок. | В solution нет test project, нет WebApplicationFactory, нет тестовой БД, нет тестов Auth/Schedule/Weeks/Roles/AcademicPerformance/Attendance/Student Portal. | Создать `Backend/tests/Api.IntegrationTests`, подключить к solution, добавить PostgreSQL test DB/Testcontainers или isolated test database. |
| Frontend Readiness | ⚠️ Частично | DTO используются в ответах, entity напрямую из controllers не возвращаются, есть `PagedResult<T>`, access/refresh token возвращаются из Auth DTO, CORS настроен для `http://localhost:5173` и `http://localhost:3000`. | Нет Student Portal endpoints, нет единого envelope для success responses, ошибки сейчас чаще `{ error = ... }`, нет refresh-token frontend strategy через cookie/header. | Добавить frontend-specific read endpoints и унифицировать response/error contract позже. |
| Student Portal API | ❌ Не готово | Есть общие read endpoints для schedules, attendance, academic-performances. | Нет `/api/student/me/schedule`, `/api/student/me/marks`, `/api/student/me/attendances`, нет фильтрации по текущему пользователю/student claim. | Создать StudentPortalController и методы, которые читают user id из claims и возвращают только свои данные. |
| Admin Dashboard API | ✅ Готово на базовом уровне | Есть CRUD endpoints для справочников, групп, студентов, преподавателей, дисциплин, недель, аудиторий, расписания; Admin CRUD закрыт policy `AdminOnly`. | Нет dashboard summary/statistics endpoints, нет bulk/import endpoints. | Добавить dashboard endpoints при необходимости. |
| CORS | ✅ Исправлено | Добавлены `AddCors` и `UseCors("Frontend")`; разрешены origins `http://localhost:5173` и `http://localhost:3000`. | Нет вынесения origins в appsettings. | При production-настройке вынести origins в конфигурацию. |
| Result Pattern | ⚠️ Частично | Application services возвращают `Result`/`Result<T>`, controllers централизованно мапят через `ApiControllerBase`. | Нет различения NotFound/Validation/Conflict/Unauthorized в Result; большинство failures превращаются в `400`, `404` почти не используется. | Расширить Result ошибками с типом/status code или добавить error mapper. |
| JWT/Refresh Token | ✅ Готово на базовом уровне | JWT Bearer настроен, token validation есть, refresh token генерируется и хранится хэшем, role claim присутствует. | Нет refresh token rotation tests, нет revoke/logout endpoint, нет хранения refresh token в secure cookie, нет rate limiting на auth. | Добавить logout/revoke, rate limiting, integration tests, решить cookie vs body strategy. |
| DI Registration | ✅ Готово | `AddApplicationServices`, `AddInfrastructureRepositories`, `AddInfrastructureSecurity`, DbContext, JWT, Options, CORS и authorization policies зарегистрированы. | Нет exception handling/problem details. | Добавить global exception handling позже. |
| ControlForm / Enum Compatibility | ✅ Исправлено | `ControlForm` приведен к бизнес-модели `Credit/Exam`; JSON enum строки включены; `GET /api/subjects` и `GET /api/disciplines` возвращают `200`; добавлены совместимые enum aliases для исторических данных. | Исторические aliases пока остаются в enum для совместимости. | Позже очистить данные или оставить aliases задокументированными. |

## 3. Проверка файлов

Найдены и проверены ключевые файлы:

- `Backend/src/Api/Program.cs`
- `Backend/src/Api/Controllers/ApiControllerBase.cs`
- `Backend/src/Api/Controllers/AuthController.cs`
- `Backend/src/Api/Controllers/AcademicPerformancesController.cs`
- `Backend/src/Api/Controllers/FacultiesController.cs`
- `Backend/src/Api/Controllers/SpecialitiesController.cs`
- `Backend/src/Api/Controllers/GroupsController.cs`
- `Backend/src/Api/Controllers/StudentsController.cs`
- `Backend/src/Api/Controllers/TeachersController.cs`
- `Backend/src/Api/Controllers/SubjectsController.cs`
- `Backend/src/Api/Controllers/DisciplinesController.cs`
- `Backend/src/Api/Controllers/AudiencesController.cs`
- `Backend/src/Api/Controllers/WeeksController.cs`
- `Backend/src/Api/Controllers/SchedulesController.cs`
- `Backend/src/Api/Controllers/AttendancesController.cs`
- `Backend/src/Api/Controllers/ExecutionsController.cs`
- `Backend/src/Application/DependencyInjection.cs`
- `Backend/src/Application/Common/Result.cs`
- `Backend/src/Application/Common/PagedResult.cs`
- `Backend/src/Application/Common/Settings/JwtSettings.cs`
- `Backend/src/Application/Common/Settings/SmtpSettings.cs`
- `Backend/src/Application/Common/Settings/GoogleAuthSettings.cs`
- `Backend/src/Application/Interfaces/Repository/IAcademicPerformanceRepository.cs`
- `Backend/src/Application/Interfaces/Interface/IAcademicPerformanceService.cs`
- `Backend/src/Application/Service/AcademicPerformanceService.cs`
- `Backend/src/Infrastructure/DependencyInjection.cs`
- `Backend/src/Infrastructure/Persistence/Repositories/AcademicPerformanceRepository.cs`
- `Backend/src/Infrastructure/Persistence/Repositories/UserRepository.cs`
- `Backend/src/Infrastructure/Services/AuthService.cs`
- `Backend/src/Infrastructure/Services/SmtpService.cs`
- `Backend/src/Infrastructure/Security/PasswordHasher.cs`
- `Backend/src/Domain/Enums/UserRole.cs`
- `Backend/src/Domain/Entities/User.cs`
- `Backend/src/Api/appsettings.json`
- `Backend/src/Api/Properties/launchSettings.json`
- `Backend/Backend.slnx`

Отсутствуют важные файлы/проекты:

- `Backend/tests/Api.IntegrationTests/Api.IntegrationTests.csproj`
- `Backend/tests/Api.IntegrationTests/CustomWebApplicationFactory.cs`
- `Backend/tests/Api.IntegrationTests/Auth/AuthEndpointsTests.cs`
- `Backend/tests/Api.IntegrationTests/Schedules/ScheduleConflictTests.cs`
- `Backend/tests/Api.IntegrationTests/Weeks/WeekOverlapTests.cs`
- `Backend/tests/Api.IntegrationTests/AcademicPerformances/AcademicPerformanceEndpointsTests.cs`
- `Backend/tests/Api.IntegrationTests/Attendances/AttendanceEndpointsTests.cs`
- `Backend/tests/Api.IntegrationTests/Authorization/RolePolicyTests.cs`
- `Backend/tests/Api.IntegrationTests/StudentPortal/StudentPortalReadOnlyTests.cs`
- `Backend/src/Api/Controllers/StudentPortalController.cs`
- `Backend/src/Api/Controllers/TeacherPortalController.cs`
- Postman collection file, например `Backend/postman/TimeTableMSU.postman_collection.json`

## 4. Команды проверки

Команды, которые нужно выполнять для текущего проекта:

```powershell
dotnet build Backend/Backend.slnx
dotnet test Backend/Backend.slnx
dotnet ef database update --project Backend/src/Infrastructure --startup-project Backend/src/Api
dotnet run --project Backend/src/Api/Api.csproj --urls http://localhost:5010
```

Фактически выполнено во время аудита:

```powershell
dotnet build Backend/Backend.slnx
```

Результат:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

```powershell
dotnet test Backend/Backend.slnx
```

Результат: команда завершается успешно, но тестовых проектов в solution нет.

```powershell
dotnet run --project Backend/src/Api/Api.csproj --urls http://localhost:5010
```

Фактическая runtime-проверка:

```text
GET /swagger/index.html -> 200
GET /swagger/v1/swagger.json -> 200
GET /api/faculties без JWT -> 401
GET /api/faculties с Student JWT -> 403
GET /api/faculties с Admin JWT -> 200
Swagger Bearer security scheme -> найден
GET /api/subjects -> 200
GET /api/disciplines -> 200
POST /api/academic-performances первый раз -> 201
POST /api/academic-performances повторно -> 400
```

## 5. Что нужно доделать

### Critical

- Создать integration test project и покрыть хотя бы Auth, Authorization, Schedule conflicts, Weeks overlap, AcademicPerformance.
- Создать `StudentPortalController`.
- Создать `TeacherPortalController`.
- Создать Postman collection.
- Ввести корректное различение `400`, `401`, `403`, `404`, `409`, `500`.
- Позже очистить исторические enum aliases или оставить их явно задокументированными.

### Important

- Создать Student Portal API:
  - получить мое расписание;
  - получить мои оценки;
  - получить мои пропуски;
  - получить расписание по группе и неделе.
- Создать Teacher Portal API:
  - мои дисциплины;
  - мое расписание;
  - управление посещаемостью по своим занятиям;
  - управление успеваемостью по своим дисциплинам;
  - execution по своим занятиям.
- Создать Postman collection для всех auth и CRUD endpoints.
- Добавить logout/revoke refresh token endpoint.
- Добавить tests для Attendance и Student Portal read-only scenarios.
- Уточнить модель оценки: только `2-5` или также `Passed/Failed`.

### Optional

- Добавить health checks.
- Добавить seed первого Admin пользователя.
- Добавить rate limiting для Auth endpoints.
- Добавить global exception handler и ProblemDetails.
- Добавить API versioning.
- Добавить dashboard summary endpoints.
- Добавить structured logging.

## 6. Готовый план исправлений

1. Создать portal controllers:
   - `StudentPortalController.cs`;
   - `TeacherPortalController.cs`.

2. Создать test project:
   - `Backend/tests/Api.IntegrationTests/Api.IntegrationTests.csproj`;
   - `CustomWebApplicationFactory.cs`;
   - `TestAuthHelper.cs`;
   - тестовые классы для Auth, Roles, Schedule, Weeks, Attendance, AcademicPerformance, Student Portal.

3. Создать Postman collection:
   - auth flow;
   - CRUD directories;
   - schedule flow;
   - attendance/performance flow;
   - token refresh flow.

4. Написать обязательные integration tests:
   - register/login/refresh;
   - protected endpoint returns `401` without token;
   - Student cannot call Admin CRUD;
   - Admin can call CRUD;
   - Schedule conflict is blocked;
   - Week overlap is blocked;
   - AcademicPerformance duplicate is blocked;
   - Attendance duplicate is blocked;
   - Student Portal returns only current student's data.

5. Провести cleanup enum aliases:
   - решить, мигрировать ли исторические значения в БД;
   - либо оставить aliases как backward-compatible контракт.

6. Улучшить error mapping:
   - различать validation/not found/conflict/server errors;
   - привести `Result` responses к единому контракту.

7. Добавить dashboard summary endpoints при необходимости.

## 7. Итог

- Backend готов примерно на 85-90%.
- Frontend можно подключать: Частично.
- Главный риск проекта сейчас: отсутствие integration tests и отдельных portal endpoints для Student/Teacher.
- Следующий лучший шаг: создать `StudentPortalController` и `TeacherPortalController`, затем закрепить Auth/Roles/AcademicPerformance/Schedule/Weeks сценарии integration tests.

---

# Critical Fixes Applied

> Дата применения: 2026-06-27  
> Область: Swagger Bearer, CORS, authorization policies, controller policies, AcademicPerformance duplicate/GetById  
> Статус: critical backend fixes применены; сборка проходит.

## Измененные файлы

- `Backend/src/Api/Program.cs`
- `Backend/src/Api/Authorization/AuthPolicies.cs`
- `Backend/src/Api/Authorization/RoleNames.cs`
- `Backend/src/Api/Controllers/FacultiesController.cs`
- `Backend/src/Api/Controllers/SpecialitiesController.cs`
- `Backend/src/Api/Controllers/GroupsController.cs`
- `Backend/src/Api/Controllers/StudentsController.cs`
- `Backend/src/Api/Controllers/TeachersController.cs`
- `Backend/src/Api/Controllers/SubjectsController.cs`
- `Backend/src/Api/Controllers/DisciplinesController.cs`
- `Backend/src/Api/Controllers/AudiencesController.cs`
- `Backend/src/Api/Controllers/WeeksController.cs`
- `Backend/src/Api/Controllers/SchedulesController.cs`
- `Backend/src/Api/Controllers/ExecutionsController.cs`
- `Backend/src/Api/Controllers/AttendancesController.cs`
- `Backend/src/Api/Controllers/AcademicPerformancesController.cs`
- `Backend/src/Application/Interfaces/Interface/IAcademicPerformanceService.cs`
- `Backend/src/Application/Service/AcademicPerformanceService.cs`
- `Backend/audit_report.md`

## Что исправлено

- В `Program.cs` добавлен Swagger Bearer authorization:
  - `AddSecurityDefinition("Bearer", ...)`;
  - `AddSecurityRequirement(...)`.
- В `Program.cs` добавлен CORS:
  - policy `Frontend`;
  - allowed origins: `http://localhost:5173`, `http://localhost:3000`;
  - `app.UseCors("Frontend")` подключен до `UseAuthentication()` и `UseAuthorization()`.
- Добавлены authorization policies:
  - `AdminOnly`;
  - `TeacherOnly`;
  - `StudentOnly`;
  - `AdminOrTeacher`;
  - `AdminOrTeacherOrStudent`.
- Добавлены constants:
  - `AuthPolicies`;
  - `RoleNames`.
- Admin CRUD controllers закрыты через `AdminOnly`.
- `AttendancesController` закрыт через `AdminOrTeacher`.
- `AcademicPerformancesController` закрыт через `AdminOrTeacher`.
- В `AcademicPerformance` добавлен `GetById` на уровнях:
  - `IAcademicPerformanceService`;
  - `AcademicPerformanceService`;
  - `AcademicPerformancesController`.
- В `AcademicPerformanceService.CreateAsync` добавлена проверка дубля через:

```csharp
_academicPerformanceRepository.ExistsAsync(dto.StudentId, dto.DisciplineId, dto.TeacherId);
```

- В `AcademicPerformanceService.UpdateAsync` добавлена проверка дубля с исключением текущей записи:

```csharp
_academicPerformanceRepository.ExistsAsync(dto.StudentId, dto.DisciplineId, dto.TeacherId, dto.Id);
```

## Какие проверки прошли

Выполнена остановка старых build-server процессов:

```powershell
dotnet build-server shutdown
```

Выполнена сборка:

```powershell
dotnet build Backend/Backend.slnx
```

Результат:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Выполнен runtime-запуск:

```powershell
dotnet run --project Backend/src/Api/Api.csproj --urls http://localhost:5010
```

Фактические runtime-проверки:

```text
GET /swagger/index.html -> 200
GET /swagger/v1/swagger.json -> 200
Swagger Bearer security scheme -> найден
GET /api/faculties без JWT -> 401
GET /api/faculties с Student JWT -> 403
GET /api/faculties с Admin JWT -> 200
```

Итог по security checks:

- Swagger UI открывается.
- Swagger Authorize Bearer схема появилась в swagger json.
- Неавторизованный пользователь получает `401`.
- Student token не проходит в Admin CRUD и получает `403`.
- Admin token проходит в Admin CRUD.

## Проверка duplicate AcademicPerformance

Кодовый фикс применен:

- `CreateAsync` блокирует дубль по `StudentId + DisciplineId + TeacherId`;
- `UpdateAsync` блокирует дубль по `StudentId + DisciplineId + TeacherId`, исключая текущий `Id`;
- при найденном дубле возвращается `Result.Failure("Оценка для указанного студента по этой дисциплине уже существует")`.

На промежуточном этапе runtime duplicate-проверку через API блокировала отдельная проблема данных/enum mapping:

```text
GET /api/disciplines -> 500
Cannot convert string value 'Credit' from the database to any value in the mapped 'ControlForm' enum.
```

Причина: в базе есть значение `subjects.ControlForm = 'Credit'`, а текущий enum `ControlForm` содержит только:

```csharp
Test,
Exam
```

Этот blocker был исправлен позже в разделе `ControlForm Blocker Fixed`.

## Что осталось после этих fixes

- ✅ Mismatch данных и enum `ControlForm` исправлен позже: бизнес-модель теперь `Credit/Exam`.
- ✅ Runtime duplicate test для `AcademicPerformance` повторен позже и прошел: первый `POST` вернул `201`, повторный `POST` вернул `400`.
- Student Portal endpoints пока не создавались намеренно.
- Teacher scoped endpoints пока не создавались намеренно.
- Integration tests пока не создавались намеренно.
- Postman collection пока не создавалась.
- Рекомендуется следующим шагом добавить tests на:
  - Swagger/JWT authorization;
  - Admin vs Student access;
  - duplicate AcademicPerformance;
  - enum/data consistency для `ControlForm`.

---

# ControlForm Blocker Fixed

> Дата исправления: 2026-06-27  
> Область: enum compatibility, JSON enum contracts, Subject/Discipline runtime, AcademicPerformance duplicate runtime check  
> Статус: blocker исправлен; runtime-проверки прошли.

## Какие файлы изменены

- `Backend/src/Domain/Enums/ControlForm.cs`
- `Backend/src/Application/Service/SubjectService.cs`
- `Backend/src/Api/Program.cs`
- `Backend/src/Domain/Enums/AcademicTitle.cs`
- `Backend/src/Domain/Enums/AcademicDegree.cs`
- `Backend/src/Domain/Enums/Post.cs`
- `Backend/src/Application/Service/AcademicPerformanceService.cs`
- `Backend/audit_report.md`

## Что исправлено

- В `ControlForm` бизнес-значение `Test` заменено на `Credit`.
- `SubjectService` теперь валидирует только допустимые формы контроля:
  - `Credit`;
  - `Exam`.
- В `Program.cs` добавлен `JsonStringEnumConverter`, чтобы Swagger/Postman могли отправлять enum значения строками:

```json
{
  "controlForm": "Credit"
}
```

и

```json
{
  "controlForm": "Exam"
}
```

- После исправления `ControlForm` endpoint `GET /api/subjects` перестал падать на значении `Credit`.
- Во время проверки `GET /api/disciplines` были найдены дополнительные string enum mismatches в исторических данных БД:
  - `AcademicTitle = "Academician"`;
  - `AcademicTitle = "None"`;
  - `AcademicDegree = "None"`;
  - `Post = "SeniorTeacher"`.
- Для совместимости с уже существующими данными добавлены enum values:
  - `AcademicTitle.Academician`;
  - `AcademicTitle.None`;
  - `AcademicDegree.None`;
  - `Post.SeniorTeacher`.
- Исправлен runtime blocker в `AcademicPerformanceService`: при создании/обновлении теперь сохраняются только FK, а no-tracking navigation objects не передаются EF Core как новый graph. Это устранило падение:

```text
duplicate key value violates unique constraint "PK_groups"
```

## Нужна была миграция или нет

Миграция не требовалась.

Причина: enum-поля уже хранятся в PostgreSQL как строки через:

```csharp
.HasConversion<string>()
```

Структура таблиц и типы колонок не менялись. Исправление потребовалось на уровне C# enum values и JSON enum contract.

## Какие проверки прошли

Выполнена сборка:

```powershell
dotnet build Backend/Backend.slnx
```

Результат:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Выполнен runtime-запуск:

```powershell
dotnet run --project Backend/src/Api/Api.csproj --urls http://localhost:5010
```

Проверки API:

```text
GET /api/subjects -> 200
GET /api/disciplines -> 200
POST /api/subjects с controlForm = "Credit" -> 201
POST /api/subjects с controlForm = "Exam" -> 201
POST /api/academic-performances первый раз -> 201
POST /api/academic-performances повторно с тем же StudentId + DisciplineId + TeacherId -> 400
```

Тело ответа при duplicate AcademicPerformance:

```json
{
  "error": "Оценка для указанного студента по этой дисциплине уже существует"
}
```

## Остались ли runtime blockers

По проверенным сценариям runtime blockers не осталось:

- `GET /api/subjects` больше не падает.
- `GET /api/disciplines` больше не падает.
- `Subject` создается с `Credit`.
- `Subject` создается с `Exam`.
- Duplicate `AcademicPerformance` блокируется через `Result.Failure`.

Отдельно остается технический долг, не входивший в этот fix:

- заменить исторические enum aliases на единую бизнес-терминологию в данных или оставить совместимость явно задокументированной;
- добавить integration tests на enum compatibility и duplicate AcademicPerformance;
- убрать временно созданные runtime test subjects из БД при необходимости.

---

# Portal API Added

> Дата добавления: 2026-06-27  
> Область: Student Portal API, Teacher Portal API, scoped read endpoints, server-side data filtering  
> Статус: portal endpoints добавлены; backend готов к минимальному frontend без клиентской фильтрации чужих данных.

## Какие endpoints добавлены

Student Portal:

- `GET /api/student-portal/me`
- `GET /api/student-portal/schedule`
- `GET /api/student-portal/marks`
- `GET /api/student-portal/attendances`

Все Student Portal endpoints закрыты policy `StudentOnly`.

Teacher Portal:

- `GET /api/teacher-portal/me`
- `GET /api/teacher-portal/schedule`
- `GET /api/teacher-portal/disciplines`
- `GET /api/teacher-portal/attendances`
- `GET /api/teacher-portal/academic-performances`
- `GET /api/teacher-portal/executions`

Все Teacher Portal endpoints закрыты policy `TeacherOnly`.

## Какие файлы изменены

- `Backend/src/Api/Controllers/StudentPortalController.cs`
- `Backend/src/Api/Controllers/TeacherPortalController.cs`
- `Backend/src/Application/DTOs/PortalDTOs/PortalStudentProfileDto.cs`
- `Backend/src/Application/DTOs/PortalDTOs/PortalTeacherProfileDto.cs`
- `Backend/src/Application/DTOs/PortalDTOs/PortalScheduleDto.cs`
- `Backend/src/Application/DTOs/PortalDTOs/PortalDisciplineDto.cs`
- `Backend/src/Application/DTOs/PortalDTOs/PortalAttendanceDto.cs`
- `Backend/src/Application/DTOs/PortalDTOs/PortalAcademicPerformanceDto.cs`
- `Backend/src/Application/DTOs/PortalDTOs/PortalExecutionDto.cs`
- `Backend/src/Application/Interfaces/Interface/IPortalService.cs`
- `Backend/src/Application/Interfaces/Repository/IPortalRepository.cs`
- `Backend/src/Application/Service/PortalService.cs`
- `Backend/src/Infrastructure/Persistence/Repositories/PortalRepository.cs`
- `Backend/src/Application/DependencyInjection.cs`
- `Backend/src/Infrastructure/DependencyInjection.cs`
- `Backend/src/Domain/Enums/AudienceType.cs`
- `Backend/audit_report.md`

## Какие repository/service методы добавлены

`IPortalService`:

- `GetStudentProfileAsync(string email)`
- `GetStudentScheduleAsync(string email)`
- `GetStudentMarksAsync(string email)`
- `GetStudentAttendancesAsync(string email)`
- `GetTeacherProfileAsync(string email)`
- `GetTeacherScheduleAsync(string email)`
- `GetTeacherDisciplinesAsync(string email)`
- `GetTeacherAttendancesAsync(string email)`
- `GetTeacherAcademicPerformancesAsync(string email)`
- `GetTeacherExecutionsAsync(string email)`

`IPortalRepository`:

- `GetStudentByEmailAsync(string email)`
- `GetTeacherByEmailAsync(string email)`
- `GetSchedulesByGroupAsync(int groupId)`
- `GetAcademicPerformancesByStudentAsync(int studentId)`
- `GetAttendancesByStudentAsync(int studentId)`
- `GetDisciplinesByTeacherAsync(int teacherId)`
- `GetSchedulesByTeacherAsync(int teacherId)`
- `GetAttendancesByTeacherAsync(int teacherId)`
- `GetAcademicPerformancesByTeacherAsync(int teacherId)`
- `GetExecutionsByTeacherAsync(int teacherId)`

Все read-запросы в `PortalRepository` используют `AsNoTracking()`. Mapping выполняется вручную в `PortalService`; EF entities наружу не возвращаются.

## Логика data scope

- Текущий пользователь определяется по JWT email claim: `ClaimTypes.Email` или `JwtRegisteredClaimNames.Email`.
- Student находится через временную MVP-связь `User.Email == Student.Email`.
- Teacher находится через временную MVP-связь `User.Email == Teacher.Email`.
- Если профиль Student/Teacher не найден для текущего email, portal endpoint возвращает `404`.
- Student получает только свои `marks`, `attendances` и расписание своей `GroupId`.
- Teacher получает только свои `disciplines`, `schedule`, `academic-performances`, `executions`.
- Teacher attendances фильтруются через занятия преподавателя: совпадают `TeacherId`, `WeekId`, `Day/Den`, `Para` и группа студента.

## Дополнительный compatibility fix

Во время runtime проверки `GET /api/teacher-portal/schedule` был найден исторический enum mismatch:

```text
Cannot convert string value 'LectureRoom' from the database to any value in the mapped 'AudienceType' enum.
```

Для совместимости с существующими данными в `AudienceType` добавлено значение `LectureRoom`. Миграция не требовалась, потому что enum хранится как строка через EF conversion.

## Какие runtime checks прошли

Сборка:

```powershell
dotnet build Backend/Backend.slnx
```

Результат:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Runtime запуск:

```powershell
dotnet run --project Backend/src/Api/Api.csproj --urls http://localhost:5010
```

Swagger:

```text
/api/student-portal/me найден в swagger json -> true
/api/teacher-portal/me найден в swagger json -> true
```

Security checks:

```text
Student token GET /api/student-portal/me -> 200
Student token GET /api/student-portal/schedule -> 200
Student token GET /api/student-portal/marks -> 200
Student token GET /api/student-portal/attendances -> 200
Student token GET /api/teacher-portal/me -> 403

Teacher token GET /api/teacher-portal/me -> 200
Teacher token GET /api/teacher-portal/schedule -> 200
Teacher token GET /api/teacher-portal/disciplines -> 200
Teacher token GET /api/teacher-portal/attendances -> 200
Teacher token GET /api/teacher-portal/academic-performances -> 200
Teacher token GET /api/teacher-portal/executions -> 200
Teacher token GET /api/student-portal/me -> 403

Admin token GET /api/students?pageNumber=1&pageSize=1 -> 200
```

Data scope smoke checks:

```text
Student schedule count -> 1
Student marks count -> 2
Student attendances count -> 1
Teacher disciplines count -> 1
Teacher schedule count -> 4
Teacher attendances count -> 2
Teacher academic performances count -> 3
Teacher executions count -> 2
```

Проверочные токены были локально подписаны тем же JWT secret/issuer/audience из `appsettings.json`, чтобы проверить role policies без изменения данных в БД.

## Какие ограничения остались

- Связь пользователя с Student/Teacher пока временная: `User.Email == Student.Email` и `User.Email == Teacher.Email`. Для production лучше добавить явную связь `UserId`, но для MVP миграция сейчас не нужна.
- Integration tests для portal endpoints пока не добавлены.
- Teacher attendances восстанавливаются через расписание по `TeacherId + WeekId + Day/Para + Student.GroupId`; отдельной FK-связи `Attendance -> Schedule` в текущей модели нет.
- Если в БД нет Student/Teacher с email текущего JWT user, portal endpoints корректно вернут `404`, а не mock данные.

## Готовность к frontend

Backend готов к минимальному React frontend для Student/Teacher portal:

- frontend может брать данные текущего пользователя из portal endpoints;
- фильтрация чужих данных выполняется на backend;
- CRUD endpoints не переписывались;
- существующие authorization policies сохранены;
- `ControlForm` не возвращался к `Test`, compatibility enum values не удалялись.
# Admin Attendance Analytics Added

- Backend files added/changed: `Backend/src/Api/Controllers/AdminDashboardController.cs`, `Backend/src/Application/DTOs/DashboardDTOs/*`, `IAdminDashboardService`, `IAdminDashboardRepository`, `AdminDashboardService`, `AdminDashboardRepository`, `Application/DependencyInjection.cs`, and `Infrastructure/DependencyInjection.cs`.
- Frontend files changed: `Frontend/src/pages/admin/AdminDashboard.tsx`, `Frontend/src/styles/cards.css`, `Frontend/package.json`, and `Frontend/package-lock.json`.
- Added endpoints: `GET /api/admin-dashboard/attendance-weekly?weekId={weekId}&groupId={groupId}` and `GET /api/admin-dashboard/group-lookups`.
- Both endpoints are protected with `Authorize(Policy = AuthPolicies.AdminOnly)`.
- Expected is calculated from schedule and students: for every schedule item in the selected week/day, add the number of students in that schedule group.
- Present is calculated as `Math.Max(expected - absent - late, 0)` because normal present attendance records are not guaranteed to exist for every student/lesson.
- Late is calculated from `AttendanceMark.Late`; the current `Attendance` entity has no `LateMinutes` field.
- Absent is calculated from `AttendanceMark.Absent`, `AttendanceMark.ValidReason`, and `AttendanceMark.None`. `ValidReason` is included in "Не пришли" for the current chart, with a TODO to split it into a separate line later.
- Duplicate attendance records are grouped by `StudentId + WeekId + Day + Para`; absent has priority over late.
- Checks passed: `dotnet build Backend/src/Api/Api.csproj -o Backend/artifacts/api-build-check` and frontend `npm run build`.
- `dotnet build Backend/Backend.slnx` was blocked by an existing `Api (31388)` process locking `Backend/src/Api/bin/Debug/net10.0/Application.dll`; build verification succeeded with an alternate output directory.
- Month/Semester analytics remain TODO; only weekly analytics are implemented.
