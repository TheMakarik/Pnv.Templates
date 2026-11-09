---
name: frontend
description: Фронтенд-варианты шаблона: только TypeScript (никогда JavaScript), расположение по вариантам (Pages/, ClientApp/, Frontend/), Vite и прокси для SPA, CORS на API, единый API-клиент. Используй при работе с фронтендом.
---

# Фронтенд

## Железное правило

Вместо JavaScript ВСЕГДА TypeScript. Новые `.js`-файлы не создавать.

## Где живёт фронтенд

| Вариант | Расположение |
|---|---|
| Razor Pages | проект бэкенда, `Pages/` |
| Razor Pages + Vue / React | `ClientApp/` внутри проекта бэкенда |
| Blazor WASM / Server | проект `Frontend/Pnv.Web.Frontend` |
| React / Vue SPA | самостоятельное SPA в `Frontend/` |

- `ClientApp/`: Vue или React на Vite; сборка `npm run build` → `wwwroot/dist`;
  `_Layout.cshtml` подключает `/dist/main.js`.
- React/Vue SPA: Vite, dev-сервер `http://localhost:5173` с прокси `/api` →
  `http://localhost:5080`.

## CORS

Для SPA-вариантов (React, Vue, Blazor WASM) на API включён CORS:
в dev — любые origins. При деплое сузь список разрешённых origins.

## API-клиенты

Клиенты API держи в одном месте:

- Blazor — `Services/GreetingsApiClient.cs`;
- SPA — отдельный api-модуль (`src/api/`).

Не разбрасывай `fetch`/`HttpClient` по компонентам.

## Чек-лист

- [ ] Все новые файлы — TypeScript (`.ts`/`.tsx`), никаких `.js`.
- [ ] Фронтенд лежит в каталоге своего варианта (Pages/, ClientApp/, Frontend/).
- [ ] Vite-сборка и dev-прокси настроены как в шаблоне.
- [ ] Для SPA включён CORS (в dev — любые origins).
- [ ] API-клиент один и переиспользуется компонентами.
