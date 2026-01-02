using Microsoft.EntityFrameworkCore;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Persistence
{
    public class OmniChatDbContext : DbContext
    {

        public OmniChatDbContext(DbContextOptions<OmniChatDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Staff> Staffs { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<RefreshToken> RefeshTokens { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Claim> Claims { get; set; }

        public DbSet<ClaimType> ClaimTypes { get; set; }

        public DbSet<Shift> Shifts { get; set; }

        public DbSet<StaffShift> StaffShifts { get; set; }

        public DbSet<Kpi> Kpis { get; set; }

        public DbSet<StaffKpi> StaffKpis { get; set; }

        public DbSet<Keyword> Keywords { get; set; }

        public DbSet<DepartmentKeyword> DepartmentKeywords { get; set; }

        public DbSet<CustomerMessage> CustomerMessages { get; set; }

        public DbSet<CustomerProfile> CustomerProfiles { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public DbSet<SupportConversation> SupportConversations { get; set; }

        public DbSet<MessageKeyword> MessageKeywords { get; set; }

        public DbSet<FeedBack> FeedBacks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<SupportStaffMessage> SupportStaffMessages { get; set; }

        public DbSet<DepartmentConversation> DepartmentConversations { get; set; }

        public DbSet<DepartmentStaffMessage> DepartmentStaffMessages { get; set; }

        public DbSet<DepartmentConversationType> DepartmentConversationTypes { get; set; }

        public DbSet<DepartmentConversationFile> DepartmentConversationFiles { get; set; }

        public DbSet<ConversationFile> ConversationFiles { get; set; }

        public DbSet<SupportConversationFile> SupportConversationFiles { get; set; }

        public DbSet<ChatTemplate> ChatTemplates { get; set; }

        public DbSet<TaskAssignments> TaskAssignments { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<ZaloOathToken> ZaloOathTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // convert enum to string
            modelBuilder.Entity<Staff>()
                .Property(s => s.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<StaffShift>()
                .Property(ss => ss.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<StaffKpi>()
                .Property(sk => sk.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<SupportConversation>()
                .Property(sc => sc.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<SupportStaffMessage>()
                .Property(sm => sm.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<DepartmentStaffMessage>()
                .Property(dsm => dsm.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<TaskAssignments>()
                .Property(ta => ta.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<TaskAssignments>()
                  .Property(ta => ta.AssignedType)
                .HasConversion<string>();

            // ==== Role - Account ( one to Many ) ====

            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);

            //Auto gen Guid Id Role
            modelBuilder.Entity<Role>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Accounts)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== Account - RefreshToken ( one to Many ) ====

            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);

            //Auto gen Guid Id Account
            modelBuilder.Entity<Account>()
            .Property(a => a.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            // Index on RoleId in Account
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.RoleId); // Index scan account by role faster

            // Unique constraint on Email in Account 
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email) // 1 email only for 1 account
                .IsUnique();

            modelBuilder.Entity<Account>()
                .HasMany(a => a.RefreshTokens)
                .WithOne(rt => rt.Account)
                .HasForeignKey(rt => rt.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
             .HasKey(rf => rf.Id);

            //Auto gen Guid Id RefreshToken
            modelBuilder.Entity<RefreshToken>()
            .Property(rf => rf.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            // ==== Department - Staff ( one to Many ) ====

            modelBuilder.Entity<Department>()
                .HasKey(d => d.Id);

            //Auto gen Guid Id Department
            modelBuilder.Entity<Department>()
            .Property(d => d.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Staffs)
                .WithOne(s => s.Department)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== Account - Staff ( one to One ) ====

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Staff)
                .WithOne(s => s.Account)
                .HasForeignKey<Staff>(s => s.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on AccountId in Staff (one Account Id only One staff)
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.AccountId)
                .IsUnique();

            // Index on DepartmentId in Staff
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.DepartmentId); // index scan staff by department faster

            // ==== Department - Claim ( one to Many ) ====
            modelBuilder.Entity<Claim>()
                .HasKey(c => c.Id);

            //Auto gen Guid Id Claim
            modelBuilder.Entity<Claim>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Claims)
                .WithOne(c => c.Department)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== Staff - Claim ( one to Many ) ====

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Id);

            //Auto gen Guid Id Staff
            modelBuilder.Entity<Staff>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Staff>()
                .HasMany(s => s.Claims)
                .WithOne(c => c.Staff)
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== ClaimType - Claim ( one to Many ) ====

            modelBuilder.Entity<ClaimType>()
                .HasKey(ct => ct.Id);

            //Auto gen Guid Id ClaimType
            modelBuilder.Entity<ClaimType>()
            .Property(ct => ct.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<ClaimType>()
               .HasMany(ct => ct.Claims)
               .WithOne(c => c.ClaimType)
               .HasForeignKey(c => c.ClaimTypeId)
               .OnDelete(DeleteBehavior.Restrict);

            // index on CliamTypeId in Claim
            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.ClaimTypeId); // index scan claim by claim type faster

            // index on DepartmentId in Claim
            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.DepartmentId); // index scan claim by department faster

            // index on StaffId in Claim
            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.StaffId); // index scan claim by staff faster

            // ==== Department - Shift ( one to Many ) ====

            modelBuilder.Entity<Shift>()
                .HasKey(s => s.Id);

            //Auto gen Guid Id Shift
            modelBuilder.Entity<Shift>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Shifts)
                .WithOne(s => s.Department)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // index on DepartmentId in Shift
            modelBuilder.Entity<Shift>()
                .HasIndex(s => s.DepartmentId); // index scan shift by department faster

            // ==== Shift - StaffShift ( one to Many ) ====

            modelBuilder.Entity<StaffShift>()
                .HasKey(ss => ss.Id);

            //Auto gen Guid Id StaffShift
            modelBuilder.Entity<StaffShift>()
            .Property(ss => ss.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Shift>()
                .HasMany(s => s.StaffShifts)
                .WithOne(ss => ss.Shift)
                .HasForeignKey(ss => ss.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==== Staff - StaffShift ( one to Many ) ====

            modelBuilder.Entity<Staff>()
                .HasMany(s => s.StaffShifts)
                .WithOne(ss => ss.Staff)
                .HasForeignKey(ss => ss.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            // index on status in StaffShift
            modelBuilder.Entity<StaffShift>()
            .HasIndex(ss => ss.Status); // index scan staffshift by status faster

            // Unique constraint on StaffId and ShiftId in StaffShift
            modelBuilder.Entity<StaffShift>()
            .HasIndex(ss => new { ss.StaffId, ss.ShiftId }) // 1 staff only have 1 record in 1 shift
            .IsUnique();

            // index on ShiftId and Status in StaffShift
            modelBuilder.Entity<StaffShift>()
                .HasIndex(ss => new { ss.ShiftId, ss.Status }); // index scan staffshift by shift and status faster


            // ==== Department - Kpi ( one to Many ) ====

            modelBuilder.Entity<Kpi>()
                .HasKey(k => k.Id);

            //Auto gen Guid Id Kpi
            modelBuilder.Entity<Kpi>()
            .Property(k => k.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Kpis)
                .WithOne(k => k.Department)
                .HasForeignKey(k => k.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // index on DepartmentId in Kpi
            modelBuilder.Entity<Kpi>()
                .HasIndex(k => k.DepartmentId); // index scan kpi by department faster

            // ==== Kpi - StaffKpi ( one to Many ) ====

            modelBuilder.Entity<StaffKpi>()
                .HasKey(sk => sk.Id);

            //Auto gen Guid Id StaffKpi
            modelBuilder.Entity<StaffKpi>()
            .Property(sk => sk.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Kpi>()
                .HasMany(k => k.StaffKpis)
                .WithOne(sk => sk.Kpi)
                .HasForeignKey(sk => sk.KpiId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==== Staff - StaffKpi ( one to Many ) ====

            modelBuilder.Entity<Staff>()
                .HasMany(s => s.StaffKpis)
                .WithOne(sk => sk.Staff)
                .HasForeignKey(sk => sk.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            // index on status in StaffKpi
            modelBuilder.Entity<StaffKpi>()
                .HasIndex(sk => new { sk.KpiId, sk.Status }); // index scan staffkpi by kpi and status faster

            modelBuilder.Entity<StaffKpi>()
                .HasIndex(sk => new { sk.StaffId, sk.Status }); // index scan staffkpi by staff and status faster

            // Unique constraint on StaffId and KpiId in StaffKpi
            modelBuilder.Entity<StaffKpi>()
                .HasIndex(sk => new { sk.StaffId, sk.KpiId }) // 1 staff only have 1 record in 1 kpi
                .IsUnique();

            // ==== Department - DepartmentKeyword ( one to Many ) ====
            modelBuilder.Entity<DepartmentKeyword>()
                .HasKey(dk => dk.Id);

            //Auto gen Guid Id DepartmentKeyword
            modelBuilder.Entity<DepartmentKeyword>()
            .Property(dk => dk.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Department>()
                .HasMany(d => d.DepartmentKeywords)
                .WithOne(dk => dk.Department)
                .HasForeignKey(dk => dk.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DepartmentKeyword>()
                .HasIndex(dk => dk.DepartmentId); // index scan departmentkeyword by department faster

            // ==== Keyword - DepartmentKeyword ( one to one ) ====
            modelBuilder.Entity<Keyword>()
                .HasKey(k => k.Id);

            //Auto gen Guid Id Keyword
            modelBuilder.Entity<Keyword>()
            .Property(k => k.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Keyword>()
                .HasOne(k => k.DepartmentKeyword)
                .WithOne(dk => dk.Keyword)
                .HasForeignKey<DepartmentKeyword>(dk => dk.KeywordId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on KeywordId in DepartmentKeyword
            modelBuilder.Entity<DepartmentKeyword>()
            .HasIndex(dk => dk.KeywordId)
            .IsUnique();

            // Unique constraint on Code in Keyword
            modelBuilder.Entity<Keyword>()
            .HasIndex(k => k.Code)
            .IsUnique();


            // ==== Keyword - MessageKeyword ( one to Many ) ====
            modelBuilder.Entity<MessageKeyword>()
                .HasKey(mk => mk.Id);

            //Auto gen Guid Id MessageKeyword
            modelBuilder.Entity<MessageKeyword>()
            .Property(mk => mk.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<Keyword>()
                .HasMany(k => k.MessageKeywords)
                .WithOne(mk => mk.Keyword)
                .HasForeignKey(mk => mk.KeywordId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==== CustomerMessage - MessageKeyword ( one to Many ) ====
            modelBuilder.Entity<CustomerMessage>()
                .HasKey(cm => cm.Id);

            //Auto gen Guid Id CustomerMessage
            modelBuilder.Entity<CustomerMessage>()
            .Property(cm => cm.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<CustomerMessage>()
                .HasMany(cm => cm.MessageKeywords)
                .WithOne(mk => mk.CustomerMessage)
                .HasForeignKey(mk => mk.CustomerMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on KeywordId and CustomerMessageId in MessageKeyword
            modelBuilder.Entity<MessageKeyword>()
             .HasIndex(mk => new { mk.KeywordId, mk.CustomerMessageId }) // 1 keyword only appear once in 1 customer message
            .IsUnique();

            modelBuilder.Entity<MessageKeyword>()
            .HasIndex(mk => mk.CustomerMessageId); // index scan messagekeyword by customermessage faster

            modelBuilder.Entity<MessageKeyword>()
             .HasIndex(mk => mk.KeywordId); // index scan messagekeyword by keyword faster


            // ==== CustomerProfile - CustomerMessage ( one to Many ) ====
            modelBuilder.Entity<CustomerProfile>()
                .HasKey(cp => cp.Id);

            //Auto gen Guid Id CustomerProfile
            modelBuilder.Entity<CustomerProfile>()
            .Property(cp => cp.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<CustomerProfile>()
                .HasMany(cp => cp.CustomerMessages)
                .WithOne(cm => cm.Customer)
                .HasForeignKey(cm => cm.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerMessage>()
                .HasIndex(cm => cm.CustomerId); // index scan customermessage by customer faster

            modelBuilder.Entity<CustomerProfile>()
                .HasIndex(cp => cp.ProvidersId); // index scan customerprofile by provider faster

            modelBuilder.Entity<CustomerMessage>()
                .HasIndex(cm => new { cm.ConversationId, cm.Timestamp }); // index scan customermessage by conversation faster


            // ==== Provider - CustomerProfile ( one to Many ) ====
            modelBuilder.Entity<Provider>()
                .HasKey(p => p.Id);

            //Auto gen Guid Id Provider
            modelBuilder.Entity<Provider>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Provider>()
                .HasMany(p => p.CustomerProfiles)
                .WithOne(cp => cp.Providers)
                .HasForeignKey(cp => cp.ProvidersId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== Provider - SupportConversation ( one to Many ) ====
            modelBuilder.Entity<Provider>()
                .HasMany(p => p.SupportConversations)
                .WithOne(sc => sc.Providers)
                .HasForeignKey(sc => sc.ProvidersId)
                .OnDelete(DeleteBehavior.Restrict);


            // == CustomerMessage - SupportConversation ( many to one ) ==
            modelBuilder.Entity<SupportConversation>()
                .HasKey(sc => sc.Id);

            //Auto gen Guid Id SupportConversation
            modelBuilder.Entity<SupportConversation>()
            .Property(sc => sc.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<SupportConversation>()
                .HasMany(sc => sc.CustomerMessages)
                .WithOne(cm => cm.Conversation)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<SupportConversation>()
               .HasIndex(sc => new { sc.ActiveStaffId, sc.Status }); // index scan supportconversation by staff and status faster


            modelBuilder.Entity<SupportConversation>()
              .HasIndex(sc => new { sc.ProvidersId, sc.Status }); // index scan supportconversation by provider and status faster

            // ==== Staff - SupportConversation ( one to Many ) ====
            modelBuilder.Entity<Staff>()
                .HasMany(s => s.SupportConversations)
                .WithOne(sc => sc.Staff)
                .HasForeignKey(sc => sc.ActiveStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== CustomerProfile - SupportConversation ( one to Many ) ====

            modelBuilder.Entity<CustomerProfile>()
                .HasMany(cp => cp.SupportConversations)
                .WithOne(sc => sc.CustomerProfile)
                .HasForeignKey(sc => sc.ActiveCustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportConversation>()
                .HasIndex(cp => cp.ActiveCustomerId); // index scan supportconversation by customerprofile faster

            // ==== Staff - FeedBack ( one to Many ) ====
            modelBuilder.Entity<FeedBack>()
                .HasKey(fb => fb.Id);
            //Auto gen Guid Id FeedBack
            modelBuilder.Entity<FeedBack>()
            .Property(fb => fb.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<Staff>()
                .HasMany(s => s.FeedBacks)
                .WithOne(fb => fb.Staff)
                .HasForeignKey(fb => fb.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeedBack>()
                .HasIndex(fb => fb.StaffId); // index scan feedback by staff faster

            modelBuilder.Entity<FeedBack>()
                .HasIndex(fb => new { fb.StaffId, fb.Rating }); // index scan feedback by staff and rating faster

            // ==== SupportConversation - FeedBack ( one to one ) ====
            modelBuilder.Entity<SupportConversation>()
                .HasOne(sc => sc.FeedBack)
                .WithOne(fb => fb.SupportConversation)
                .HasForeignKey<FeedBack>(fb => fb.SupportConversationId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==== Staff - Nofitication ( one to Many ) ====
            modelBuilder.Entity<Notification>()
                .HasKey(nf => nf.Id);
            //Auto gen Guid Id Notification
            modelBuilder.Entity<Notification>()
            .Property(nf => nf.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<Staff>()
                .HasMany(s => s.Notifications)
                .WithOne(nf => nf.Staff)
                .HasForeignKey(nf => nf.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasIndex(nf => nf.StaffId); // index scan nofitication by staff faster

            modelBuilder.Entity<Notification>()
                .HasIndex(nf => new { nf.StaffId, nf.IsRead }); // index scan nofitication by staff and isread fasters


            // ==== SupportConversation - Nofitication ( one to Many ) ====

            modelBuilder.Entity<SupportConversation>()
                .HasMany(sc => sc.Notifications)
                .WithOne(nf => nf.SupportConversation)
                .HasForeignKey(nf => nf.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasIndex(nf => new { nf.ConversationId, nf.IsRead }); // index scan nofitication by conversation and isread fasters


            // ==== Staff - SupportStaffMessage ( one to Many ) ====

            modelBuilder.Entity<SupportStaffMessage>()
                .HasKey(sm => sm.Id);

            //Auto gen Guid Id SupportStaffMessage
            modelBuilder.Entity<SupportStaffMessage>()
            .Property(sm => sm.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Staff>()
                .HasMany(s => s.SupportStaffMessages)
                .WithOne(sm => sm.Staff)
                .HasForeignKey(sm => sm.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportStaffMessage>()
                .HasIndex(sm => new { sm.StaffId, sm.Status }); // index scan SupportStaffMessage by staff and status faster


            modelBuilder.Entity<SupportStaffMessage>()
                .HasIndex(sm => new { sm.SupportConversationId, sm.Status }); // index scan SupportStaffMessage by supportconversation and status faster

            // ==== SupportConversation - SupportStaffMessage ( one to Many ) ====

            modelBuilder.Entity<SupportConversation>()
                .HasMany(sc => sc.SupportStaffMessages)
                .WithOne(sm => sm.SupportConversation)
                .HasForeignKey(sm => sm.SupportConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== DepartmentConversation - DepartmentStaffMessage ( one to Many ) ====

            modelBuilder.Entity<DepartmentStaffMessage>()
                .HasKey(dsm => dsm.Id);


            //Auto gen Guid Id DepartmentStaffMessage
            modelBuilder.Entity<DepartmentStaffMessage>()
            .Property(dsm => dsm.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<DepartmentConversation>()
                .HasMany(dc => dc.DepartmentStaffMessages)
                .WithOne(dsm => dsm.DepartmentConversation)
                .HasForeignKey(dsm => dsm.DepartmentConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DepartmentStaffMessage>()
                .HasIndex(dsm => new { dsm.DepartmentConversationId, dsm.Status }); // index scan DepartmentStaffMessage by departmentconversation and status faster

            modelBuilder.Entity<DepartmentConversation>()
                .HasKey(dc => dc.Id);

            //Auto gen Guid Id DepartmentConversation
            modelBuilder.Entity<DepartmentConversation>()
            .Property(dc => dc.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            // ==== Staff - DepartmentStaffMessage ( one to Many ) ====

            modelBuilder.Entity<Staff>()
                .HasMany(s => s.DepartmentStaffMessages)
                .WithOne(dsm => dsm.Staff)
                .HasForeignKey(dsm => dsm.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== DepartmentConversationType - DepartmentConversation ( one to Many ) ====

            modelBuilder.Entity<DepartmentConversationType>()
                .HasKey(dct => dct.Id);

            //Auto gen Guid Id DepartmentConversationType
            modelBuilder.Entity<DepartmentConversationType>()
            .Property(dct => dct.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<DepartmentConversationType>()
                .HasMany(dct => dct.DepartmentConversations)
                .WithOne(dc => dc.DepartmentConversationType)
                .HasForeignKey(dc => dc.DepartmentConversationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DepartmentConversation>()
                .HasIndex(dc => dc.DepartmentConversationTypeId); // index scan DepartmentConversation by DepartmentConversationType faster


            // ==== Department - DepartmentConversation ( one to Many ) ====

            modelBuilder.Entity<Department>()
                .HasMany(d => d.DepartmentConversations)
                .WithOne(dc => dc.Department)
                .HasForeignKey(dc => dc.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DepartmentConversation>()
                .HasIndex(dc => new { dc.DepartmentId, dc.DepartmentConversationTypeId, dc.Status }); // index scan DepartmentConversation by Department and DepartmentConversationType and Status faster


            // ==== DepartmentConversation - DepartmentConversationFile ( one to Many ) ====
            modelBuilder.Entity<DepartmentConversationFile>()
                .HasKey(dcf => dcf.Id);
            //Auto gen Guid Id DepartmentConversationFile
            modelBuilder.Entity<DepartmentConversationFile>()
            .Property(dcf => dcf.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<DepartmentConversation>()
                .HasMany(dc => dc.DepartmentConversationFiles)
                .WithOne(dcf => dcf.DepartmentConversation)
                .HasForeignKey(dcf => dcf.DepartmentConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DepartmentConversationFile>()
                .HasIndex(dcf => dcf.ConversationFileId);

            // ==== ConversationFile - DepartmentConversationFile ( one to Many ) ====

            modelBuilder.Entity<ConversationFile>()
                .HasKey(cf => cf.Id);
            //Auto gen Guid Id ConversationFile
            modelBuilder.Entity<ConversationFile>()
            .Property(cf => cf.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<ConversationFile>()
                .HasMany(cf => cf.DepartmentConversationFiles)
                .WithOne(dcf => dcf.ConversationFile)
                .HasForeignKey(dcf => dcf.ConversationFileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DepartmentConversationFile>()
                .HasIndex(dcf => new { dcf.DepartmentConversationId, dcf.ConversationFileId }).IsUnique();// scan by departmentconversation and conversationfile faster


            // ==== ConversationFile - SupportConversationFile ( one to Many ) ====
            modelBuilder.Entity<SupportConversationFile>()
                .HasKey(scf => scf.Id);
            //Auto gen Guid Id SupportConversationFile
            modelBuilder.Entity<SupportConversationFile>()
            .Property(scf => scf.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<ConversationFile>()
                .HasMany(cf => cf.SupportConversationFiles)
                .WithOne(scf => scf.ConversationFile)
                .HasForeignKey(scf => scf.ConversationFileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportConversationFile>()
                .HasIndex(scf => scf.ConversationFileId);

            // ==== SupportConversation - SupportConversationFile ( one to Many ) ====
            modelBuilder.Entity<SupportConversation>()
                .HasMany(sc => sc.SupportConversationFiles)
                .WithOne(scf => scf.SupportConversation)
                .HasForeignKey(scf => scf.SupportConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportConversationFile>()
                .HasIndex(scf => new { scf.SupportConversationId, scf.ConversationFileId }).IsUnique();// scan by supportconversation and conversationfile faster


            // ==== ChatTemplate ====
            modelBuilder.Entity<ChatTemplate>()
                .HasKey(ct => ct.Id);
            //Auto gen Guid Id ChatTemplate
            modelBuilder.Entity<ChatTemplate>()
            .Property(ct => ct.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<ChatTemplate>()
                .HasIndex(ct => ct.Code).IsUnique(); // 1 code only for 1 chat template

            modelBuilder.Entity<ChatTemplate>()
                .Property(ct => ct.Code)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ChatTemplate>()
                .Property(ct => ct.Content)
                .IsRequired(); // Content is required

            // ==== Staff - TaskAssignments ( one to Many ) ====
            modelBuilder.Entity<TaskAssignments>()
                .HasKey(ta => ta.Id);
            //Auto gen Guid Id TaskAssignments
            modelBuilder.Entity<TaskAssignments>()
            .Property(ta => ta.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Staff>()
                .HasMany(s => s.TaskAssignments)
                .WithOne(ta => ta.Staff)
                .HasForeignKey(ta => ta.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignments>()
                .HasIndex(ta => new { ta.StaffId, ta.Status }); // index scan taskassignments by staff and status faster

            // ==== Department - TaskAssignments ( one to Many ) ====
            modelBuilder.Entity<Department>()
                .HasMany(d => d.TaskAssignments)
                .WithOne(ta => ta.Department)
                .HasForeignKey(ta => ta.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignments>()
                .HasIndex(ta => new { ta.DepartmentId, ta.Status }); // index scan taskassignments by department and status faster


            // ==== SupportConversation - TaskAssignments ( one to One ) ====

            modelBuilder.Entity<SupportConversation>()
                .HasOne(d => d.TaskAssignments)
                .WithOne(ta => ta.SupportConversation)
                .HasForeignKey<TaskAssignments>(ta => ta.SupportConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignments>()
                .HasIndex(ta => ta.SupportConversationId) // index scan taskassignments by supportconversation faster
                .IsUnique();

            modelBuilder.Entity<TaskAssignments>()
               .HasIndex(ta => new { ta.SupportConversationId, ta.IsActive }) // only 1 active taskassignment in 1 supportconversation
               .IsUnique();

            // ==== AuditLog ====
            modelBuilder.Entity<AuditLog>()
                .HasKey(al => al.Id);
            //Auto gen Guid Id AuditLog
            modelBuilder.Entity<AuditLog>()
            .Property(al => al.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.CreateDate); // index scan auditlog by createdate faster

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => new { al.EntityType, al.EntityId }); // index scan auditlog by entitytype and entityid faster

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.OldData)
                .HasColumnType("jsonb");

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.NewData)
                .HasColumnType("jsonb");

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            // ==== ZaloOathToken ====

            modelBuilder.Entity<ZaloOathToken>()
                .HasKey(zot => zot.Id);
            //Auto gen Guid Id ZaloOathToken
            modelBuilder.Entity<ZaloOathToken>()
            .Property(zot => zot.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<ZaloOathToken>()
                .Property(zot => zot.AccessToken)
                .IsRequired();

            modelBuilder.Entity<ZaloOathToken>()
                .Property(zot => zot.RefreshToken)
                .IsRequired();

            modelBuilder.Entity<ZaloOathToken>()
                .HasIndex(x => x.IsActive); // scan by isActive

            var entitiesWithoutGuidDefault = modelBuilder.Model
    .GetEntityTypes()
    .Where(e =>
    {
        var idProp = e.FindProperty("Id");
        return idProp != null
            && idProp.ClrType == typeof(Guid)
            && idProp.GetDefaultValueSql() == null;
    })
    .Select(e => e.Name)
    .ToList();

            if (entitiesWithoutGuidDefault.Any())
            {
                throw new Exception(
                    "Entities missing gen_random_uuid(): " +
                    string.Join(", ", entitiesWithoutGuidDefault));
            }
        }
    }
}
