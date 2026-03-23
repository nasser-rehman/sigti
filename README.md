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

```bash
npm install
docker-compose up -d
npx prisma migrate dev
npm run dev

📌 Endpoints principais
POST /api/auth/login
POST /api/users
POST /api/tickets
GET /api/tickets
PATCH /api/tickets/:id/assign
PATCH /api/tickets/:id/status
