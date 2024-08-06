﻿using ContactManager.Authorization;
using ContactManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManager.Data
{
    // This class defines a SeedData class responsible for seeding the database with initial data.
    public static class SeedData
    {
        // The Initialize method is used to seed the database with test data.
        // It creates and assigns roles to users and then seeds the Contact table with sample contacts.
        public static async Task Initialize(IServiceProvider serviceProvider, string testAdminPw, string testManagerPw)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // For sample purposes, seed both admin and manager users with the same password.
                // The password is set using the following:
                // dotnet user-secrets set SeedUserPW <pw>

                // Ensure that the admin user exists and assign the ContactAdministratorsRole to it.
                var adminID = await EnsureUser(serviceProvider, testAdminPw, "admin@gmail.com");
                await EnsureRole(serviceProvider, adminID, Constants.ContactAdministratorsRole);

                // Ensure that the manager user exists and assign the ContactManagersRole to it.
                var managerID = await EnsureUser(serviceProvider, testManagerPw, "manager@gmail.com");
                await EnsureRole(serviceProvider, managerID, Constants.ContactManagersRole);

                // Seed the Contact table with sample data if it's empty.
                SeedDB(context, adminID);
            }
        }

        // This method ensures that a user with the specified username exists.
        // If the user doesn't exist, it creates a new user with the given password.
        // It returns the user ID.
        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                                    string testUserPw, string UserName)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(UserName);

            // If the user doesn't exist, create a new user with the provided username and password.
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = UserName,
                    EmailConfirmed = true,
                    Email = UserName,
                };
                await userManager.CreateAsync(user, testUserPw);
            }

            // If the user is still null, it means that the password provided may not be strong enough.
            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            // Return the user ID.
            return user.Id;
        }

        // This method ensures that a role with the specified name exists.
        // If the role doesn't exist, it creates a new role with the given name and assigns it to the specified user.
        // It returns the IdentityResult of the role creation.
        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                                      string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            IdentityResult IR;
            if (!await roleManager.RoleExistsAsync(role))
            {
                // Create a new role if it doesn't exist.
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            // Get the user based on the user ID.
            var user = await userManager.FindByIdAsync(uid);

            // If the user is null, it means that the password provided may not be strong enough.
            if (user == null)
            {
                throw new Exception("The testUserPw password was probably not strong enough!");
            }

            // Assign the user to the specified role.
            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }

        // This method seeds the Contact table with sample contact data if it's empty.
        public static void SeedDB(ApplicationDbContext context, string adminID)
        {
            if (context.Contact.Any())
            {
                return;   // DB has been seeded
            }

            // Add sample contact data to the Contact table.
            context.Contact.AddRange(
                new Contact
                {
                    Name = "Debra Garcia",
                    Address = "1234 Main St",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "debra@example.com",
                    Status = ContactStatus.Approved,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Thorsten Weinrich",
                    Address = "5678 1st Ave W",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "thorsten@example.com",
                    Status = ContactStatus.Submitted,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Yuhong Li",
                    Address = "9012 State st",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "yuhong@example.com",
                    Status = ContactStatus.Rejected,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Jon Orton",
                    Address = "3456 Maple St",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "jon@example.com",
                    Status = ContactStatus.Submitted,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Diliana Alexieva-Bosseva",
                    Address = "7890 2nd Ave E",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "diliana@example.com",
                    OwnerID = adminID
                }
            );

            // Save changes to the database.
            context.SaveChanges();
        }
    }
}
