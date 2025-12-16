// DocumentNumberApi/AppDbContextFactory.cs (Root 계정 사용으로 복구)
using DocumentNumberApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace DocumentNumberApi
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // 마이그레이션 도구용 Root 연결 (Root 계정이 외부 접속 권한을 가질 경우 성공)
            var connectionString = "Server=127.0.0.1;Port=3307;Database=documentdb;User=root;Password=password;";

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                options => options.SchemaBehavior(MySqlSchemaBehavior.Ignore)
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}