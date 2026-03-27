# Messenger REST API


- Діаграма бази даних: [[В Фігмі]](https://www.figma.com/board/5H0Co2OnhrzPGsPO9I0IJC/Messenger?node-id=0-1&p=f&t=PxBKwuAFF1B7MsQ4-0) 🔗
- Технологій: C#, ASP.NET Core 9.0

### 1. Додано NuGet пакети  
   - Pomelo.EntityFrameworkCore.MySql 9.0.0  
   - Microsoft.EntityFrameworkCore.Design 9.0.13  
   - Microsoft.EntityFrameworkCore.Tools  9.0.13

#### Authentication
   - System.IdentityModel.Tokens.Jwt v8.3.1
   - Microsoft.AspNetCore.Authentication.JwtBearer v9.0.13
   - BCrypt.Net-Core v1.6.0


## База даних
- `Users` — дані користувачів.
- `Chats` — інформація про чати.
- `Messages` — історія повідомлень.
- `ChatUsers` — таблиця звязок між юзерами та чатами.

> SQL-скрипт лежить у файлі: [chat_app.sql](./chat_app.sql)
