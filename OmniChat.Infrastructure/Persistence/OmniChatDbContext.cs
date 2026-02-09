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

        public DbSet<KeywordTypes> KeywordTypes { get; set; }

        public DbSet<MessageKeywordTypes> MessageKeywordTypes { get; set; }

        public DbSet<InternalConversationFile> InternalConversationFiles { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Claim> Claims { get; set; }

        public DbSet<SupportTask> SupportTasks { get; set; }

        public DbSet<InternalStaffMessage> InternalStaffMessages { get; set; }

        public DbSet<ClaimType> ClaimTypes { get; set; }

        public DbSet<Keyword> Keywords { get; set; }

        public DbSet<CustomerMessage> CustomerMessages { get; set; }

        public DbSet<CustomerProfile> CustomerProfiles { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public DbSet<SupportConversation> SupportConversations { get; set; }

        public DbSet<MessageKeyword> MessageKeywords { get; set; }

        public DbSet<FeedBack> FeedBacks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<SupportStaffMessage> SupportStaffMessages { get; set; }

        public DbSet<ConversationFile> ConversationFiles { get; set; }

        public DbSet<SupportConversationFile> SupportConversationFiles { get; set; }
        
        public DbSet<InternalConversation> InternalConversations { get; set; }

        public DbSet<TaskAssignmentHistory> TaskAssignmentHistories { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductBatch> ProductBatches { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<BillingItem> BillingItems { get; set; }

        public DbSet<ChatTemplate> ChatTemplates { get; set; }

        public DbSet<ZaloOathToken> ZaloOathTokens { get; set; }

        public DbSet<FacebookOathToken> FacebookOathTokens { get; set; }

        public DbSet<InstagramOathToken> InstagramOathTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // convert enum to string
            modelBuilder.Entity<Staff>()
                .Property(s => s.Status)
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
            modelBuilder.Entity<InternalConversation>()
                .Property(ic => ic.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<InternalStaffMessage>()
                .Property(ism => ism.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<SupportTask>()
                .Property(st => st.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<TaskAssignmentHistory>()
                .Property(tah => tah.Action)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // convert enum to string
            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryStatus)
                .HasConversion<string>();

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductPackagingType)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.PayStatus)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.PayMethod)
                .HasConversion<string>();

            modelBuilder.Entity<BillingItem>()
                .Property(bi => bi.BillStatus)
                .HasConversion<string>();

            // default value IsActive = true

            modelBuilder.Entity<Account>()
            .Property(x => x.IsActive)
            .HasDefaultValueSql("true");

            modelBuilder.Entity<Role>()
           .Property(x => x.IsActive)
           .HasDefaultValueSql("true");

            modelBuilder.Entity<Staff>()
            .Property(x => x.IsActive)
            .HasDefaultValueSql("true");

            modelBuilder.Entity<KeywordTypes>()
             .Property(x => x.IsActive)
             .HasDefaultValueSql("true");

            modelBuilder.Entity<ClaimType>()
            .Property(x => x.IsActive)
            .HasDefaultValueSql("true");

            modelBuilder.Entity<InternalConversation>()
                .Property(x => x.IsActive)
                .HasDefaultValueSql("true");

            modelBuilder.Entity<ProductBatch>()
                .Property(x => x.IsActive)
                .HasDefaultValueSql("true");

            modelBuilder.Entity<Product>()
                .Property(x => x.IsActive)
                .HasDefaultValueSql("true");

            modelBuilder.Entity<FacebookOathToken>()
                .Property(x => x.IsActive)
                .HasDefaultValueSql("true");

            modelBuilder.Entity<ZaloOathToken>()
                .Property(x => x.IsActive)
                .HasDefaultValueSql("true");

            modelBuilder.Entity<InstagramOathToken>()
                .Property(x => x.IsActive)
                .HasDefaultValueSql("true");

            // default vaule isDelete = false

            modelBuilder.Entity<Keyword>()
            .Property(x => x.IsDeleted)
            .HasDefaultValueSql("false");

            //default createDate utc now 
            modelBuilder.Entity<RefreshToken>()
            .Property(x => x.CreateDate)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");


            modelBuilder.Entity<Keyword>()
            .Property(x => x.CreateDate)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");


            modelBuilder.Entity<Notification>()
            .Property(x => x.CreatedDate)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

            modelBuilder.Entity<Provider>()
            .Property(x => x.CreateDate)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");


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

            // Unique constraint on Email in staff 
            modelBuilder.Entity<Staff>()
                .HasIndex(a => a.Email) // 1 email only for 1 staff
                .IsUnique();

            // Unique constraint on Phone in staff 
            modelBuilder.Entity<Staff>()
                .HasIndex(a => a.Phone) // 1 Phone only for 1 staff
                .IsUnique();

            // ==== keywordTypes - Claim ( one to Many ) ====
            modelBuilder.Entity<Claim>()
                .HasKey(c => c.Id);

            //Auto gen Guid Id Claim
            modelBuilder.Entity<Claim>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<KeywordTypes>()
                .HasMany(d => d.Claims)
                .WithOne(c => c.KeywordTypes)
                .HasForeignKey(c => c.KeywordTypeId)
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

            // index on KeywordTypeId in Claim
            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.KeywordTypeId); // index scan claim by KeywordType faster

            // index on StaffId in Claim
            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.StaffId); // index scan claim by staff faster

            // ==== KeywordTypes - Keyword ( one to many ) ====
            modelBuilder.Entity<Keyword>()
                .HasKey(k => k.Id);

            modelBuilder.Entity<Keyword>()
                 .Property(k => k.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<KeywordTypes>()
                .HasMany(kt => kt.Keywords)
                .WithOne(k => k.KeyWordType)
                .HasForeignKey(k => k.KeyWordTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KeywordTypes>()
                .HasIndex(kt => kt.TypeName); // index scan keywordtypes by typename faster

            modelBuilder.Entity<Keyword>()
                .HasIndex(k => k.KeyWordTypeId); // index scan keyword by keywordtype faster

            // Unique constraint on Code in Keyword
            modelBuilder.Entity<Keyword>()
            .HasIndex(k => k.Code)
            .IsUnique();


            // ==== MessageKeywordTypes - MessageKeyword ( one to Many ) ====
            modelBuilder.Entity<MessageKeywordTypes>()
                .HasKey(mkt => mkt.Id);

            //Auto gen Guid Id MessageKeywordTypes
            modelBuilder.Entity<MessageKeywordTypes>()
                .Property(mkt => mkt.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<MessageKeywordTypes>()
                .HasMany(mkt => mkt.MessageKeywords)
                .WithOne(mk => mk.MessageKeywordTypes)
                .HasForeignKey(mk => mk.MessageKeywordTypesId)
                .OnDelete(DeleteBehavior.Cascade);

           modelBuilder.Entity<MessageKeyword>()
                .HasIndex(mk => mk.MessageKeywordTypesId); // index scan messagekeyword by messagekeywordtypes faster


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

            modelBuilder.Entity<Keyword>()
                .HasIndex(k => k.KeywordText); // index scan keyword by keywordtext faster

            modelBuilder.Entity<MessageKeyword>()
                .HasIndex(mk => mk.KeywordId); // index scan messagekeyword by keyword faster

            // ==== CustomerMessage - MessageKeywordType ( one to Many ) ====
            modelBuilder.Entity<CustomerMessage>()
                .HasKey(cm => cm.Id);

            //Auto gen Guid Id CustomerMessage
            modelBuilder.Entity<CustomerMessage>()
            .Property(cm => cm.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");
             
            modelBuilder.Entity<CustomerMessage>()
                .HasMany(cm => cm.MessageKeywordTypes)
                .WithOne(mkt => mkt.CustomerMessage)
                .HasForeignKey(mkt => mkt.MessageId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==== KeywordTypes - MessageKeywordType ( one to Many ) ====
            modelBuilder.Entity<KeywordTypes>()
                .HasKey(mkt => mkt.Id);

            modelBuilder.Entity<KeywordTypes>()
                 .Property(kt => kt.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<KeywordTypes>()
                .HasMany(kt => kt.MessageKeywordTypes)
                .WithOne(mkt => mkt.KeywordTypes)
                .HasForeignKey(mkt => mkt.KeywordTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MessageKeywordTypes>()
                .HasIndex(mkt => mkt.KeywordTypeId); // index scan messagekeywordtypes by keywordtype faster

            // ==== CustomerProfile - CustomerMessage ( one to Many ) ====
            modelBuilder.Entity<CustomerProfile>()
                .HasKey(cp => cp.Id);

            //Auto gen Guid Id CustomerProfile
            modelBuilder.Entity<CustomerProfile>()
            .Property(cp => cp.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<CustomerProfile>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<CustomerProfile>()
                .HasIndex(x => x.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<CustomerProfile>()
                .HasMany(cp => cp.CustomerMessages)
                .WithOne(cm => cm.Customer)
                .HasForeignKey(cm => cm.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerMessage>()
                .HasIndex(cm => cm.CustomerId); // index scan customermessage by customer faster

            modelBuilder.Entity<CustomerMessage>()
                .HasIndex(cm => new { cm.ConversationId, cm.Timestamp }); // index scan customermessage by conversation faster


            // ==== Provider - SupportConversation ( one to Many ) ====
            modelBuilder.Entity<Provider>()
                .HasIndex(p => p.Id);
                
            modelBuilder.Entity<Provider>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Provider>()
                .HasMany(p => p.SupportConversations)
                .WithOne(sc => sc.Providers)
                .HasForeignKey(sc => sc.ProvidersId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportConversation>()
                .HasIndex(sc => sc.ProvidersId); // index scan supportconversation by provider faster

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

            modelBuilder.Entity<SupportConversation>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<SupportConversation>()
                .Property(x => x.UpdateDate)
                .HasDefaultValueSql("now()");


            modelBuilder.Entity<ConversationFile>()
                .HasKey(cf => cf.Id);
            //Auto gen Guid Id ConversationFile
            modelBuilder.Entity<ConversationFile>()
            .Property(cf => cf.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<ConversationFile>()
            .Property(x => x.TimeStamp)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

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

            // ==== ConversationFile - InternalConversationFile ( one to Many ) ====

            modelBuilder.Entity<InternalConversationFile>()
                .HasKey(icf => icf.Id);

            modelBuilder.Entity<InternalConversationFile>()
                 .Property(icf => icf.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");



            // ==== Staff - SupportTask ( one to Many ) ====

            modelBuilder.Entity<SupportTask>()
                .HasKey(st => st.Id);

            //Auto gen Guid Id SupportTask
            modelBuilder.Entity<SupportTask>()
                .Property(st => st.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Staff>()
                .HasMany(s => s.SupportTasks)
                .WithOne(st => st.CurrentAssignedStaff)
                .HasForeignKey(st => st.CurrentAssignedStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTask>()
                .HasIndex(st => st.CurrentAssignedStaffId); // scan by CurrentAssignedStaffId faster

            modelBuilder.Entity<SupportTask>()
                .HasIndex(st => st.Status);
            // ==== SupportConversation - SupportTask ( one to Many ) ====

            modelBuilder.Entity<SupportConversation>()
                .HasMany(sc => sc.SupportTasks)
                .WithOne(st => st.SupportConversation)
                .HasForeignKey(st => st.SupportConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportTask>()
                .HasIndex(st => st.SupportConversationId); // scan by SupportConversationId faster

            // ==== KeywordTypes - SupportTask ( one to Many ) ====

            modelBuilder.Entity<KeywordTypes>()
                .HasMany(kt => kt.SupportTasks)
                .WithOne(st => st.KeywordType)
                .HasForeignKey(st => st.KeywordTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTask>()
                .HasIndex(st => st.KeywordTypeId); // scan by KeywordTypeId faster

            // ==== ChatTemplate ====
            modelBuilder.Entity<ChatTemplate>()
                .HasKey(ct => ct.Id);
            //Auto gen Guid Id ChatTemplate
            modelBuilder.Entity<ChatTemplate>()
            .Property(ct => ct.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

            // ==== InternalConversation - InternalConversationFile ( one to Many ) ====

            modelBuilder.Entity<InternalConversation>()
                .HasKey(ic => ic.Id);

            // auto gen Guid Id InternalConversation

            modelBuilder.Entity<InternalConversation>()
                .Property(ic => ic.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<InternalConversation>()
                .HasMany(ic => ic.InternalConversationFiles)
                .WithOne(icf => icf.InternalConversation)
                .HasForeignKey(icf => icf.InternalConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InternalConversationFile>()
                .HasIndex(icf => icf.InternalConversationId); // scan by InternalConversationId faster

            // ==== Staff - InternalStaffMessages ( one to Many ) ====
            modelBuilder.Entity<Staff>()
                .HasMany(s => s.InternalStaffMessages)
                .WithOne(ism => ism.Staff)
                .HasForeignKey(ism => ism.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InternalStaffMessage>()
                .HasIndex(ism => ism.StaffId); // scan by StaffId faster 

            // ==== InternalConversation - InternalStaffMessage ( one to Many ) ====
            modelBuilder.Entity<InternalStaffMessage>()
                .HasKey(ism => ism.Id);

            modelBuilder.Entity<InternalStaffMessage>()
                .Property(ism => ism.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<InternalConversation>()
                .HasMany(ic => ic.InternalMessages)
                .WithOne(ism => ism.InternalConversation)
                .HasForeignKey(ism => ism.InternalConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InternalStaffMessage>()
                .HasIndex(ism => ism.InternalConversationId); // scan by InternalConversationId faster

            modelBuilder.Entity<ChatTemplate>()
                .HasIndex(ct => ct.Code).IsUnique(); // 1 code only for 1 chat template

            modelBuilder.Entity<ChatTemplate>()
                .Property(ct => ct.Code)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ChatTemplate>()
                .Property(ct => ct.Content)
                .IsRequired(); // Content is required

            // ==== staff - InternalStaffMessage ( one to Many ) ====
            modelBuilder.Entity<Staff>()
                .HasMany(s => s.InternalStaffMessages)
                .WithOne(ism => ism.Staff)
                .HasForeignKey(ism => ism.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InternalStaffMessage>()
                .HasIndex(ism => ism.StaffId); // scan by StaffId faster

            // == TaskAssignmentHistory - SupportTask ( many to one ) ==

            modelBuilder.Entity<TaskAssignmentHistory>()
                .HasKey(tah => tah.Id);

            // Auto gen Guid Id TaskAssignmentHistory
            modelBuilder.Entity<TaskAssignmentHistory>()
                .Property(tah => tah.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<SupportTask>()
                .HasMany(st => st.TaskAssignmentHistories)
                .WithOne(tah => tah.SupportTask)
                .HasForeignKey(tah => tah.SupportTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAssignmentHistory>()
                .HasIndex(tah => tah.SupportTaskId); // scan by SupportTaskId faster


            // ==== CustomerProfile - Order ( one to Many ) ====

            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);

            //Auto gen Guid Id Order
            modelBuilder.Entity<Order>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<CustomerProfile>()
                .HasMany(cp => cp.Orders)
                .WithOne(o => o.CustomerProfile)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
               .HasIndex(o => o.Code)
               .IsUnique();

            modelBuilder.Entity<Order>()
               .Property(o => o.Code)
               .IsRequired();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CustomerId); // index scan order by customer faster

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.DeliveryStatus);

            // ==== Order - OrderItem ( one to Many ) ====

            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id);

            //Auto gen Guid Id OrderItem
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId); // index scan orderitem by order faster

            // ==== ProductBatch - OrderItem ( one to Many ) ====

            modelBuilder.Entity<ProductBatch>()
                .HasKey(pb => pb.Id);

            //Auto gen Guid Id ProductBatch
            modelBuilder.Entity<ProductBatch>()
                .Property(pb => pb.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<ProductBatch>()
                .HasMany(pb => pb.OrderItems)
                .WithOne(oi => oi.ProductBatch)
                .HasForeignKey(oi => oi.ProductBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.ProductBatchId); // index scan orderitem by productbatch faster

            // ==== Product - ProductBatch ( one to Many ) ====

            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            //Auto gen Guid Id Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<Product>()
             .Property(o => o.Code)
             .IsRequired();

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductBatches)
                .WithOne(pb => pb.Product)
                .HasForeignKey(pb => pb.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductBatch>()
                .HasIndex(pb => pb.ProductId); // index scan productbatch by product faster

            // ==== CustomerProfile - Paymment( one - Many)
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<CustomerProfile>()
                .HasMany(cp => cp.Payments)
                .WithOne(p => p.CustomerProfile)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.CustomerId); // index scan payment by customer faster

            // ==== Order - BillingItem (one - Many)

            modelBuilder.Entity<BillingItem>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<BillingItem>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Order>()
                .HasMany(o => o.BillingItems)
                .WithOne(o => o.Order)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BillingItem>()
                .HasIndex(o => o.OrderId);   // index scan bill by order faster

            // ==== Payment - billingItem (one - many)
            modelBuilder.Entity<Payment>()
                .HasMany(p => p.BillingItems)
                .WithOne(bi => bi.Payment)
                .HasForeignKey(p => p.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BillingItem>()
                .HasIndex(o => o.PaymentId); // index scan bill by payment Id faster

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

            // ==== FacebookOathToken ====
            modelBuilder.Entity<FacebookOathToken>()
                .HasKey(fot => fot.Id);

            //Auto gen Guid Id FacebookOathToken
            modelBuilder.Entity<FacebookOathToken>()
                .Property(fot => fot.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<FacebookOathToken>()
                .Property(fot => fot.AccessToken)
                .IsRequired();

            modelBuilder.Entity<FacebookOathToken>()
                .HasIndex(x => x.IsActive); // scan by isActive

            // ==== InstagramOathToken ====

            modelBuilder.Entity<InstagramOathToken>()
                .HasKey(iot => iot.Id);

            //Auto gen Guid Id InstagramOathToken
            modelBuilder.Entity<InstagramOathToken>()
                .Property(iot => iot.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<InstagramOathToken>()
                .Property(iot => iot.AccessToken)
                .IsRequired();

            modelBuilder.Entity<InstagramOathToken>()
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
