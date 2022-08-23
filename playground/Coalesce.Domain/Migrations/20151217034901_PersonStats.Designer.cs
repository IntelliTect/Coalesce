using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Coalesce.Domain;

namespace Coalesce.Domain.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20151217034901_PersonStats")]
    partial class PersonStats
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Coalesce.Domain.Case", b =>
                {
                    b.Property<int>("CaseKey")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AssignedToId");

                    b.Property<byte[]>("Attachment");

                    b.Property<string>("Description");

                    b.Property<DateTimeOffset>("OpenedAt");

                    b.Property<int?>("ReportedById");

                    b.Property<string>("Severity");

                    b.Property<int>("Status");

                    b.Property<string>("Title");

                    b.HasKey("CaseKey");
                });

            modelBuilder.Entity("Coalesce.Domain.CaseProduct", b =>
                {
                    b.Property<int>("CaseProductId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaseId");

                    b.Property<int>("ProductId");

                    b.HasKey("CaseProductId");
                });

            modelBuilder.Entity("Coalesce.Domain.Company", b =>
                {
                    b.Property<int>("CompanyId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address1");

                    b.Property<string>("Address2");

                    b.Property<string>("City");

                    b.Property<string>("Name");

                    b.Property<string>("State");

                    b.Property<string>("ZipCode");

                    b.HasKey("CompanyId");
                });

            modelBuilder.Entity("Coalesce.Domain.Person", b =>
                {
                    b.Property<int>("PersonId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("BirthDate");

                    b.Property<int>("CompanyId");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName")
                        .HasAnnotation("MaxLength", 75);

                    b.Property<int>("Gender");

                    b.Property<DateTime?>("LastBath");

                    b.Property<string>("LastName")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<DateTimeOffset?>("NextUpgrade");

                    b.Property<int>("PersonStatsId");

                    b.Property<byte[]>("ProfilePic");

                    b.Property<int>("Title");

                    b.HasKey("PersonId");
                });

            modelBuilder.Entity("Coalesce.Domain.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ProductId");
                });

            modelBuilder.Entity("Coalesce.Domain.Case", b =>
                {
                    b.HasOne("Coalesce.Domain.Person")
                        .WithMany()
                        .HasForeignKey("AssignedToId");

                    b.HasOne("Coalesce.Domain.Person")
                        .WithMany()
                        .HasForeignKey("ReportedById");
                });

            modelBuilder.Entity("Coalesce.Domain.CaseProduct", b =>
                {
                    b.HasOne("Coalesce.Domain.Case")
                        .WithMany()
                        .HasForeignKey("CaseId");

                    b.HasOne("Coalesce.Domain.Product")
                        .WithMany()
                        .HasForeignKey("ProductId");
                });

            modelBuilder.Entity("Coalesce.Domain.Person", b =>
                {
                    b.HasOne("Coalesce.Domain.Company")
                        .WithMany()
                        .HasForeignKey("CompanyId");
                });
        }
    }
}
