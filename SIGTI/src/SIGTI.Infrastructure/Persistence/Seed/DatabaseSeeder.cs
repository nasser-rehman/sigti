using Microsoft.EntityFrameworkCore;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Factories;
using SIGTI.Domain.ValueObjects;
using SIGTI.Infrastructure.Persistence.Context;
using SIGTI.Infrastructure.Services;

namespace SIGTI.Infrastructure.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync(user => user.IsSystem))
            {
                return; // Database already seeded
            }

            var departmentFactory = new DepartmentFactory();
            var supportQueueFactory = new SupportQueueFactory();
            var userFactory = new UserFactory();
            var passwordHasher = new PasswordHasher();
            var defaultPasswordHash = passwordHasher.Hash("Senha@123");

            var department = departmentFactory.Create(
                "Tecnologia da Informação",
                "Departamento responsável por gerenciar a infraestrutura de TI e suporte técnico."
            );

            var queue = supportQueueFactory.Create(
                "Suporte Técnico",
                "Fila de suporte técnico para resolver problemas relacionados a hardware, software e rede."
            );

            var systemUser = userFactory.CreateSystemUser(
                "Sistema SIGTI",
                new Email("sistema@sigti.local"),
                defaultPasswordHash,
                Role.Administrator,
                department
            );

            var techUser = userFactory.Create(
                "Nasser Ruiz Rehman",
                new Email("nasser@sigti.local"),
                defaultPasswordHash,
                Role.Technician,
                department
            );

            queue.AddMember(techUser, 10);

            await context.Departments.AddAsync(department);
            await context.SupportQueues.AddAsync(queue);
            await context.Users.AddAsync(systemUser);
            await context.Users.AddAsync(techUser);
            Console.WriteLine(context.ChangeTracker.DebugView.LongView);
            await context.SaveChangesAsync();
        }
    }
}
