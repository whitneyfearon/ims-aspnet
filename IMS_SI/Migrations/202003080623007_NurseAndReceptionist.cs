namespace IMS_SI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NurseAndReceptionist : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Nurses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApplicationUserId = c.String(maxLength: 128),
                        FullName = c.String(),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        EmailAddress = c.String(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        Address = c.String(),
                        PhoneNo = c.String(),
                        Gender = c.String(nullable: false),
                        BloodGroup = c.String(),
                        DateOfBirth = c.DateTime(),
                        Education = c.String(),
                        Status = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId, cascadeDelete: true)
                .Index(t => t.ApplicationUserId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.NurseSchedules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NurseId = c.Int(nullable: false),
                        AvailableStartDay = c.String(nullable: false),
                        AvailableEndDay = c.String(nullable: false),
                        AvailableStartTime = c.DateTime(nullable: false),
                        AvailableEndTime = c.DateTime(nullable: false),
                        Status = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Nurses", t => t.NurseId, cascadeDelete: true)
                .Index(t => t.NurseId);
            
            CreateTable(
                "dbo.Receptionists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApplicationUserId = c.String(maxLength: 128),
                        FullName = c.String(),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        EmailAddress = c.String(nullable: false),
                        Address = c.String(),
                        PhoneNo = c.String(),
                        Gender = c.String(nullable: false),
                        BloodGroup = c.String(),
                        DateOfBirth = c.DateTime(),
                        Status = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .Index(t => t.ApplicationUserId);
            
            CreateTable(
                "dbo.ReceptionistSchedules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReceptionistId = c.Int(nullable: false),
                        AvailableStartDay = c.String(nullable: false),
                        AvailableEndDay = c.String(nullable: false),
                        AvailableStartTime = c.DateTime(nullable: false),
                        AvailableEndTime = c.DateTime(nullable: false),
                        Status = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Receptionists", t => t.ReceptionistId, cascadeDelete: true)
                .Index(t => t.ReceptionistId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReceptionistSchedules", "ReceptionistId", "dbo.Receptionists");
            DropForeignKey("dbo.Receptionists", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NurseSchedules", "NurseId", "dbo.Nurses");
            DropForeignKey("dbo.Nurses", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Nurses", "ApplicationUserId", "dbo.AspNetUsers");
            DropIndex("dbo.ReceptionistSchedules", new[] { "ReceptionistId" });
            DropIndex("dbo.Receptionists", new[] { "ApplicationUserId" });
            DropIndex("dbo.NurseSchedules", new[] { "NurseId" });
            DropIndex("dbo.Nurses", new[] { "DepartmentId" });
            DropIndex("dbo.Nurses", new[] { "ApplicationUserId" });
            DropTable("dbo.ReceptionistSchedules");
            DropTable("dbo.Receptionists");
            DropTable("dbo.NurseSchedules");
            DropTable("dbo.Nurses");
        }
    }
}
