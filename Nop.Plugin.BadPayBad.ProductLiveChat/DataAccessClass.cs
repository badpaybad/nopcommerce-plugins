﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.BadPayBad.ProductLiveChat
{
    public class ProductComment : BaseEntity, ILocalizedEntity
    {
        public int ProductId { get; set; }
        public int ParentId { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastComment { get; set; }
        public int Status { get; set; }
        public string RatesByCommas { get; set; }
        public string RatesCalculated { get; set; }
    }



    public class ProductCommentMap : NopEntityTypeConfiguration<ProductComment>
    {
        public ProductCommentMap()
        {
            ToTable("BadPayBadProductLiveComment");
            HasKey(i => i.Id);
            Property(i => i.ProductId);
            Property(i => i.ParentId);
            Property(i => i.Username);
            Property(i => i.Comment);
            Property(i => i.CreatedDate);
            Property(i => i.LastComment);
            Property(i => i.Status);
            Property(i => i.RatesByCommas);
            Property(i => i.RatesCalculated);
        }
    }

    public class ProductCommentLiveChatDbContext : DbContext, IDbContext
    {
        public ProductCommentLiveChatDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {

            return base.Set<TEntity>();
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            return objectContext.ExecuteStoreQuery<TEntity>(commandText, parameters).ToList();
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return base.Database.SqlQuery<TElement>(sql, parameters);
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.CommandTimeout = timeout;
            var notEnsureTransaction = doNotEnsureTransaction ? TransactionalBehavior.DoNotEnsureTransaction : TransactionalBehavior.EnsureTransaction;
            return objectContext.ExecuteStoreCommand(notEnsureTransaction, sql, parameters);
            //return base.Database.ExecuteSqlCommand(sql, parameters);
        }

        public void Detach(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            ((IObjectContextAdapter)this).ObjectContext.Detach(entity);
        }

        public bool ProxyCreationEnabled
        {
            get
            {
                return base.Configuration.ProxyCreationEnabled;
            }
            set
            {
                base.Configuration.ProxyCreationEnabled = value;
            }
        }
        public bool AutoDetectChangesEnabled
        {
            get
            {
                return base.Configuration.AutoDetectChangesEnabled;
            }
            set
            {
                base.Configuration.AutoDetectChangesEnabled = value;
            }
        }

        public void Install()
        {
            //create the table
            var dbScript = CreateDatabaseScript();
            Database.ExecuteSqlCommand(dbScript);
            SaveChanges();
        }

        /// <summary>
        /// Uninstall
        /// </summary>
        public void Uninstall()
        {
            //drop the table
            var tableName = this.GetTableName<ProductComment>();
            //var tableName = "TaxRate";
            this.DropPluginTable(tableName);
        }

        public string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var maped = Activator.CreateInstance<ProductCommentMap>();
            modelBuilder.Configurations.Add(maped);
            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }

    }

    public class DependencyRegistrer : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            var contextName = typeof(ProductCommentLiveChatDbContext).ToString();
            //data context
            this.RegisterPluginDataContext<ProductCommentLiveChatDbContext>(builder, contextName);

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ProductComment>>()
                .As<IRepository<ProductComment>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(contextName))
                .InstancePerLifetimeScope();
        }

        public int Order { get { return 1; } }
    }
}
