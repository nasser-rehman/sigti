# 🛠️ SIGTI - Sistema de Chamados de TI (Em desenvolvimento)

Sistema fullstack para gestão de chamados de suporte técnico.

## 🚀 Tecnologias

- Node.js
- TypeScript
- Express
- Prisma ORM
- PostgreSQL
- JWT (Autenticação)

## 🔐 Funcionalidades

- Autenticação com JWT
- Controle de acesso por perfil (ADMIN, AGENT, USER)
- Criação de chamados
- Atribuição de chamados a técnicos
- Atualização de status
- Listagem com regras por perfil

## ⚙️ Como rodar
Atribuir os valores referentes as variáveis no .env da pasta ./backend
- DATABASE_URL = "your_database_url_here"
- JWT_SECRET=supersecretkeyhahaha
- JWT_EXPIRES_IN=1d

```bash
npm install
docker-compose up -d
npx prisma migrate dev
npx prisma generate
npm run dev

📌 Endpoints principais
POST /api/auth/login
POST /api/users
POST /api/tickets
GET /api/tickets
PATCH /api/tickets/:id/assign
PATCH /api/tickets/:id/status
