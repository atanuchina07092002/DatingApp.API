using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext(DbContextOptions options):Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<AppUser,AppRoles,int,
        IdentityUserClaim<int>,AppUsersRole,IdentityUserLogin<int>,IdentityRoleClaim<int>,IdentityUserToken<int>>(options)
    {
       
        public DbSet<UserLike> Likes {  get; set; }
        public DbSet<Message> Messages { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        { 
            base.OnModelCreating(builder);//Must Be declared at top for base configuration

            builder.Entity<AppUser>()
                .HasMany(u => u.UsersRoles)
                .WithOne(u => u.User)
                .HasForeignKey(u => u.UserId)
                .IsRequired().OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AppRoles>()
                .HasMany(u=>u.UsersRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            ////For User Like
           
            builder.Entity<UserLike>()
                .HasKey(k => new { k.SourceUserId, k.TargetUserId });

            builder.Entity<UserLike>()//1 sourceuser can like many other users
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserLike>()//1 targetuser can like many other users
                .HasOne(s => s.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.NoAction);

            //For Messagees
            builder.Entity<Message>()// 1 sender send many messages to other users
                .HasOne(s => s.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()// similarly 1 receiver received many massages from other users
                .HasOne(r=>r.Recipient)
                .WithMany(m=>m.MessagesReceived)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}


//This method is part of your DbContext class.
//It lets you configure the model schema, relationships, and behaviors beyond what you can do with data annotations.

//You’re defining a composite key using both SourceUserId and TargetUserId.
//This ensures that a user can only like another user once — i.e., no duplicate likes.

//Each UserLike has one SourceUser (who likes someone).
//Each AppUser can like many users (LikedUsers is a collection navigation property).
//SourceUserId is the foreign key.

//Each UserLike has one TargetUser (the person being liked).
//Each AppUser can be liked by many users (LikedByUsers is a collection).
//TargetUserId is the foreign key.

