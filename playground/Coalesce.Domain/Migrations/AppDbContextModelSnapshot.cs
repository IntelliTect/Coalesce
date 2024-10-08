﻿// <auto-generated />
using System;
using Coalesce.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Coalesce.Domain.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Coalesce.Domain.AuditLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("ClientIp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Endpoint")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KeyValue")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Referrer")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte>("State")
                        .HasColumnType("tinyint");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("State");

                    b.HasIndex("Type");

                    b.HasIndex("UserId");

                    b.HasIndex("Type", "KeyValue");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("Coalesce.Domain.Case", b =>
                {
                    b.Property<int>("CaseKey")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CaseKey"));

                    b.Property<int?>("AssignedToId")
                        .HasColumnType("int");

                    b.Property<byte[]>("AttachmentHash")
                        .HasMaxLength(32)
                        .HasColumnType("varbinary(32)");

                    b.Property<string>("AttachmentName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("AttachmentSize")
                        .HasColumnType("bigint");

                    b.Property<string>("AttachmentType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DevTeamAssignedId")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time");

                    b.Property<string>("Numbers")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("OpenedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("ReportedById")
                        .HasColumnType("int");

                    b.Property<string>("Severity")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("States")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Strings")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CaseKey");

                    b.HasIndex("AssignedToId");

                    b.HasIndex("ReportedById");

                    b.ToTable("Case", (string)null);
                });

            modelBuilder.Entity("Coalesce.Domain.Case+CaseAttachmentContent", b =>
                {
                    b.Property<int>("CaseKey")
                        .HasColumnType("int");

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("CaseKey");

                    b.ToTable("Case", (string)null);
                });

            modelBuilder.Entity("Coalesce.Domain.CaseProduct", b =>
                {
                    b.Property<int>("CaseProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CaseProductId"));

                    b.Property<int>("CaseId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("CaseProductId");

                    b.HasIndex("CaseId");

                    b.HasIndex("ProductId");

                    b.ToTable("CaseProduct");
                });

            modelBuilder.Entity("Coalesce.Domain.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("CompanyId");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address1")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Address2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("State")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebsiteUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ZipCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Company");
                });

            modelBuilder.Entity("Coalesce.Domain.Log", b =>
                {
                    b.Property<int>("LogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LogId"));

                    b.Property<string>("Level")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("LogId");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Coalesce.Domain.Person", b =>
                {
                    b.Property<int>("PersonId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PersonId"));

                    b.Property<string>("ArbitraryCollectionOfStrings")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(75)
                        .HasColumnType("nvarchar(75)");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastBath")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastName")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTimeOffset?>("NextUpgrade")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("ProfilePic")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("Title")
                        .HasColumnType("int");

                    b.HasKey("PersonId");

                    b.HasIndex("CompanyId");

                    b.ToTable("Person");
                });

            modelBuilder.Entity("Coalesce.Domain.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductId"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UniqueId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ProductUniqueId");

                    b.HasKey("ProductId");

                    b.HasIndex("UniqueId")
                        .IsUnique();

                    b.ToTable("Product");
                });

            modelBuilder.Entity("Coalesce.Domain.ZipCode", b =>
                {
                    b.Property<string>("Zip")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Zip");

                    b.ToTable("ZipCodes");
                });

            modelBuilder.Entity("IntelliTect.Coalesce.AuditLogging.AuditLogProperty", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("NewValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NewValueDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValueDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("ParentId")
                        .HasColumnType("bigint");

                    b.Property<string>("PropertyName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("AuditLogProperties");
                });

            modelBuilder.Entity("Coalesce.Domain.AuditLog", b =>
                {
                    b.HasOne("Coalesce.Domain.Person", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("User");
                });

            modelBuilder.Entity("Coalesce.Domain.Case", b =>
                {
                    b.HasOne("Coalesce.Domain.Person", "AssignedTo")
                        .WithMany("CasesAssigned")
                        .HasForeignKey("AssignedToId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Coalesce.Domain.Person", "ReportedBy")
                        .WithMany("CasesReported")
                        .HasForeignKey("ReportedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("AssignedTo");

                    b.Navigation("ReportedBy");
                });

            modelBuilder.Entity("Coalesce.Domain.Case+CaseAttachmentContent", b =>
                {
                    b.HasOne("Coalesce.Domain.Case", null)
                        .WithOne("AttachmentContent")
                        .HasForeignKey("Coalesce.Domain.Case+CaseAttachmentContent", "CaseKey")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Coalesce.Domain.CaseProduct", b =>
                {
                    b.HasOne("Coalesce.Domain.Case", "Case")
                        .WithMany("CaseProducts")
                        .HasForeignKey("CaseId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Coalesce.Domain.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Case");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Coalesce.Domain.Person", b =>
                {
                    b.HasOne("Coalesce.Domain.Company", "Company")
                        .WithMany("Employees")
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("Coalesce.Domain.Product", b =>
                {
                    b.OwnsOne("Coalesce.Domain.ProductDetails", "Details", b1 =>
                        {
                            b1.Property<int>("ProductId")
                                .HasColumnType("int");

                            b1.HasKey("ProductId");

                            b1.ToTable("Product");

                            b1.WithOwner()
                                .HasForeignKey("ProductId");

                            b1.OwnsOne("Coalesce.Domain.StreetAddress", "CompanyHqAddress", b2 =>
                                {
                                    b2.Property<int>("ProductDetailsProductId")
                                        .HasColumnType("int");

                                    b2.Property<string>("Address")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("City")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("PostalCode")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("State")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("ProductDetailsProductId");

                                    b2.ToTable("Product");

                                    b2.WithOwner()
                                        .HasForeignKey("ProductDetailsProductId");
                                });

                            b1.OwnsOne("Coalesce.Domain.StreetAddress", "ManufacturingAddress", b2 =>
                                {
                                    b2.Property<int>("ProductDetailsProductId")
                                        .HasColumnType("int");

                                    b2.Property<string>("Address")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("City")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("PostalCode")
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("State")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("ProductDetailsProductId");

                                    b2.ToTable("Product");

                                    b2.WithOwner()
                                        .HasForeignKey("ProductDetailsProductId");
                                });

                            b1.Navigation("CompanyHqAddress");

                            b1.Navigation("ManufacturingAddress");
                        });

                    b.Navigation("Details")
                        .IsRequired();
                });

            modelBuilder.Entity("IntelliTect.Coalesce.AuditLogging.AuditLogProperty", b =>
                {
                    b.HasOne("Coalesce.Domain.AuditLog", null)
                        .WithMany("Properties")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Coalesce.Domain.AuditLog", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("Coalesce.Domain.Case", b =>
                {
                    b.Navigation("AttachmentContent");

                    b.Navigation("CaseProducts");
                });

            modelBuilder.Entity("Coalesce.Domain.Company", b =>
                {
                    b.Navigation("Employees");
                });

            modelBuilder.Entity("Coalesce.Domain.Person", b =>
                {
                    b.Navigation("CasesAssigned");

                    b.Navigation("CasesReported");
                });
#pragma warning restore 612, 618
        }
    }
}
