using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvaloniaApp.Models;
using AvaloniaApp.ServiceAbstractions;
using AvaloniaApp.Utils;

namespace AvaloniaApp.Services.DataServices
{
    public class UserService : IUserService
    {
        private readonly string _filePath;
        private const char Separator = ';';

        public UserService()
        {
            _filePath = "Users.csv";

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(
                    _filePath,
                    $"Id{Separator}Name{Separator}Surname{Separator}Email{Separator}DateAdding{Separator}DateEdit\n",
                    Encoding.UTF8
                );
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            var result = await File.ReadAllLinesAsync(_filePath, Encoding.UTF8);

            return result
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(ParseLine)
                .ToList();
        }

        public async Task<User?> GetUserById(int id)
        {
            var result = await GetAllUsers();

            return result.FirstOrDefault(u => u.Id == id);
        }

        public async Task<bool> CreateUser(User user)
        {
            if (!UserValidator.ValidateUser(user).Item1)
                throw new ArgumentException("Неверный формат user");

            List<User> users = await GetAllUsers();

            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            user.DateAdding = DateTime.Now;

            string line =
                $"{user.Id}{Separator}{user.Name}{Separator}{user.Surname}{Separator}{user.Email}{Separator}{user.DateAdding}{Separator}{user.DateEdit}";

            await File.AppendAllLinesAsync(_filePath, new[] { line }, Encoding.UTF8);

            return true;
        }

        public async Task<bool> UpdateUser(User user)
        {
            if (!UserValidator.ValidateUser(user).Item1)
                return false;

            List<User> users = await GetAllUsers();

            int index = users.FindIndex(u => u.Id == user.Id);

            if (index == -1)
                return false;

            user.DateEdit = DateTime.Now;
            user.DateAdding = users[index].DateAdding;

            users[index] = user;

            return await WriteAll(users);
        }

        public async Task<bool> DeleteUser(int id)
        {
            List<User> users = await GetAllUsers();

            User? userToRemove = users.FirstOrDefault(u => u.Id == id);

            if (userToRemove == null)
                return false;

            users.Remove(userToRemove);

            return await WriteAll(users);
        }

        private User ParseLine(string line)
        {
            string[] parts = line.Split(Separator);

            (DateTime, bool) edit;
            (DateTime, bool) create;

            edit.Item2 = DateTime.TryParse(parts[5], out edit.Item1);
            create.Item2 = DateTime.TryParse(parts[4], out create.Item1);

            return new User
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
                Surname = parts[2],
                Email = parts[3],
                DateAdding = create.Item2 ? create.Item1 : edit.Item1,
                DateEdit = edit.Item2 ? edit.Item1 : null,
            };
        }

        private async Task<bool> WriteAll(List<User> users)
        {
            var lines = new List<string>
            {
                $"Id{Separator}Name{Separator}Surname{Separator}Email{Separator}DateAdding{Separator}DateEdit",
            };

            lines.AddRange(
                users.Select(u =>
                    $"{u.Id}{Separator}{u.Name}{Separator}{u.Surname}{Separator}{u.Email}{Separator}{u.DateAdding}{Separator}{u.DateEdit}"
                )
            );

            await File.WriteAllLinesAsync(_filePath, lines, Encoding.UTF8);

            return true;
        }
    }
}
