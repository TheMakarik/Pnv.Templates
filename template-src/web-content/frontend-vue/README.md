# Pnv.Web Frontend (Vue)

Самостоятельное SPA-приложение на Vue + TypeScript + Vite. Вариант шаблона: `Frontend == Vue`.

## Запуск в режиме разработки

```bash
npm install
npm run dev
```

Dev-сервер доступен на http://localhost:5173. Запросы `/api` проксируются на http://localhost:5080; CORS на API уже включён.

## Сборка

```bash
npm run build
```

Результат сборки (`npm run build`) помещается в `dist/`.
