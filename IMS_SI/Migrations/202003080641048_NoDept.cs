namespace IMS_SI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoDept : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Nurses", "DepartmentId", "dbo.Departments");
            DropIndex("dbo.Nurses", new[] { "DepartmentId" });
            DropColumn("dbo.Nurses", "DepartmentId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Nurses", "DepartmentId", c => c.Int(nullable: false));
            CreateIndex("dbo.Nurses", "DepartmentId");
            AddForeignKey("dbo.Nurses", "DepartmentId", "dbo.Departments", "Id", cascadeDelete: true);
        }
    }
}
