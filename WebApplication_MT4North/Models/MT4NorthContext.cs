﻿using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

//#nullable disable

namespace WebApplication_MT4North.Models
{
    public partial class MT4NorthContext : IdentityDbContext<ApplicationUser>
    {
        public MT4NorthContext()
        {
        }

        public MT4NorthContext(DbContextOptions<MT4NorthContext> options)
            : base(options)
        {
        }


        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<BaseActivityInfo> BaseActivityInfos { get; set; }
        public virtual DbSet<CustomActivityInfo> CustomActivityInfos { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<UserProject> UserProjects { get; set; }
        public virtual DbSet<Theme> Themes { get; set; }
        public virtual DbSet<Resource> Resources { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                //FIXME: TODO: NEEDED!?
                //optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\goma02\\Source\\Repos\\WebApplication_MT4North\\WebApplication_MT4North\\Models\\medtechinnovationmodel_v3.mdf;Integrated Security=True;Trusted_Connection=False;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
