using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Coalesce.Domain;

namespace Coalesce.Domain.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20160811224626_MakePersonStatsIdNullable")]
partial class MakePersonStatsIdNullable
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

        modelBuilder.Entity("Coalesce.Domain.Case", b =>
            {
                b.Property<int>("CaseKey")
                    .ValueGeneratedOnAdd();

                b.Property<int?>("AssignedToId");

                b.Property<byte[]>("Attachment");

                b.Property<string>("Description");

                b.Property<int?>("DevTeamAssignedId");

                b.Property<DateTimeOffset>("OpenedAt");

                b.Property<int?>("ReportedById");

                b.Property<string>("Severity");

                b.Property<int>("Status");

                b.Property<string>("Title");

                b.HasKey("CaseKey");

                b.HasIndex("AssignedToId");

                b.HasIndex("ReportedById");

                b.ToTable("Case");
            });

        modelBuilder.Entity("Coalesce.Domain.CaseProduct", b =>
            {
                b.Property<int>("CaseProductId")
                    .ValueGeneratedOnAdd();

                b.Property<int>("CaseId");

                b.Property<int>("ProductId");

                b.HasKey("CaseProductId");

                b.HasIndex("CaseId");

                b.HasIndex("ProductId");

                b.ToTable("CaseProduct");
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

                b.ToTable("Company");
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

                b.Property<int?>("PersonStatsId");

                b.Property<byte[]>("ProfilePic");

                b.Property<int>("Title");

                b.HasKey("PersonId");

                b.HasIndex("CompanyId");

                b.ToTable("Person");
            });

        modelBuilder.Entity("Coalesce.Domain.Product", b =>
            {
                b.Property<int>("ProductId")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Name");

                b.HasKey("ProductId");

                b.ToTable("Product");
            });

        modelBuilder.Entity("Coalesce.Domain.Case", b =>
            {
                b.HasOne("Coalesce.Domain.Person", "AssignedTo")
                    .WithMany("CasesAssigned")
                    .HasForeignKey("AssignedToId");

                b.HasOne("Coalesce.Domain.Person", "ReportedBy")
                    .WithMany("CasesReported")
                    .HasForeignKey("ReportedById");
            });

        modelBuilder.Entity("Coalesce.Domain.CaseProduct", b =>
            {
                b.HasOne("Coalesce.Domain.Case", "Case")
                    .WithMany("CaseProducts")
                    .HasForeignKey("CaseId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Coalesce.Domain.Product", "Product")
                    .WithMany()
                    .HasForeignKey("ProductId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity("Coalesce.Domain.Person", b =>
            {
                b.HasOne("Coalesce.Domain.Company", "Company")
                    .WithMany("Employees")
                    .HasForeignKey("CompanyId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
    }
}
