﻿using FluentNHibernate;
using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;
using ZKWeb.Database;
using ZKWeb.Database.UserTypes;
using ZKWeb.Testing;
using ZKWebStandard.Extensions;
using ZKWebStandard.Testing;

namespace ZKWeb.Tests.Database {
	[Tests]
	class DatabaseContextTest {
		public void All() {
			var testManager = Application.Ioc.Resolve<TestManager>();
			using (Application.OverrideIoc()) {
				Application.Ioc.Unregister<IMappingProvider>();
				Application.Ioc.RegisterMany<TestTableMap>();
				using (testManager.UseTemporaryDatabase()) {
					var databaseManager = Application.Ioc.Resolve<DatabaseManager>();
					var saveCallback = new TestTableSaveCallback();
					var deleteCallback = new TestTableDeleteCallback();
					Application.Ioc.RegisterInstance<IDataSaveCallback<TestTable>>(saveCallback);
					Application.Ioc.RegisterInstance<IDataDeleteCallback<TestTable>>(deleteCallback);
					// 添加对象
					using (var context = databaseManager.GetContext()) {
						var obj = new TestTable() { Name = "TestName" };
						var otherObj = new TestTable() { Name = "OtherName" };
						context.Save(ref obj);
						context.Save(ref otherObj);
						Assert.Equals(saveCallback.BeforeSaveCount, 2);
						Assert.Equals(saveCallback.AfterSaveCount, 2);
						context.SaveChanges();
					}
					// 更新对象
					using (var context = databaseManager.GetContext()) {
						context.UpdateWhere<TestTable>(
							t => t.Name == "TestName",
							t => {
								t.Name = "TestNameUpdated";
								t.Extra["TestKey"] = "TestValue";
							});
						Assert.Equals(saveCallback.BeforeSaveCount, 3);
						Assert.Equals(saveCallback.AfterSaveCount, 3);
						context.SaveChanges();
					}
					// 获取更新后的对象
					using (var context = databaseManager.GetContext()) {
						var obj = context.Get<TestTable>(t => t.Name == "TestNameUpdated");
						Assert.IsTrue(obj != null);
						Assert.IsTrue(obj.Extra != null);
						Assert.Equals(obj.Extra.GetOrDefault("TestKey"), "TestValue");
						var objFromQuery = context.Query<TestTable>()
							.FirstOrDefault(t => t.Name == "TestNameUpdated");
						Assert.Equals(obj, objFromQuery);
					}
					// 计算对象数量
					using (var context = databaseManager.GetContext()) {
						Assert.Equals(context.Count<TestTable>(_ => true), 2);
						Assert.Equals(context.Count<TestTable>(t => t.Name == "TestNameUpdated"), 1);
						Assert.Equals(context.Count<TestTable>(t => t.Name == "NotExist"), 0);
					}
					// 删除对象
					using (var context = databaseManager.GetContext()) {
						context.DeleteWhere<TestTable>(t => t.Name == "TestNameUpdated");
						Assert.IsTrue(context.Get<TestTable>(t => t.Name == "TestNameUpdated") == null);
						Assert.Equals(deleteCallback.BeforeDeleteCount, 1);
						Assert.Equals(deleteCallback.AfterDeleteCount, 1);
						context.SaveChanges();
					}
					// 检查删除对象
					using (var context = databaseManager.GetContext()) {
						Assert.IsTrue(context.Get<TestTable>(t => t.Name == "TestNameUpdated") == null);
						Assert.IsTrue(context.Get<TestTable>(t => t.Name == "OtherName") != null);
					}
					// 不使用事务
					using (var context = databaseManager.GetContext(null)) {
						var obj = new TestTable() { Name = "TestName" };
						context.Save(ref obj);
						context.SaveChanges();
					}
					using (var context = databaseManager.GetContext(null)) {
						var obj = context.Get<TestTable>(t => t.Name == "TestName");
						Assert.IsTrue(obj != null);
					}
				}
			}
		}

		class TestTable {
			public virtual long Id { get; set; }
			public virtual string Name { get; set; }
			public virtual Dictionary<string, object> Extra { get; set; }
		}

		class TestTableMap : ClassMap<TestTable> {
			public TestTableMap() {
				Id(t => t.Id);
				Map(t => t.Name);
				Map(t => t.Extra).CustomType<JsonSerializedType<Dictionary<string, object>>>();
			}
		}

		class TestTableSaveCallback : IDataSaveCallback<TestTable> {
			public int BeforeSaveCount { get; set; }
			public int AfterSaveCount { get; set; }

			public void BeforeSave(DatabaseContext context, TestTable data) {
				++BeforeSaveCount;
			}

			public void AfterSave(DatabaseContext context, TestTable data) {
				++AfterSaveCount;
			}
		}

		class TestTableDeleteCallback : IDataDeleteCallback<TestTable> {
			public int BeforeDeleteCount { get; set; }
			public int AfterDeleteCount { get; set; }

			public void BeforeDelete(DatabaseContext context, TestTable data) {
				++BeforeDeleteCount;
			}

			public void AfterDelete(DatabaseContext context, TestTable data) {
				++AfterDeleteCount;
			}
		}
	}
}
