﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nezaboodka.MySqlClient.UnitTests.TestUtils;
using Nezaboodka.ToSqlConnector;

namespace Nezaboodka.MySqlClient.UnitTests
{
    [TestClass]
    public class MySqlDatabaseClientUnitTests
    {
        private static string TestServerAddress => $"http://localhost:{SqlAuthData.DefaultPort}";

        [TestMethod]
        public void AlterDatabaseListTest()
        {
            var client = new MySqlDatabaseClient(TestServerAddress, null, null);

            var alterList = RandomDatabaseNamesGenerator.GetRandomDatabaseNamesList(3, 15, "nz_");
            var databaseNames = client.GetDatabaseList();

            HashSet<string> removeExpectedResult = new HashSet<string>(databaseNames);
            var addExpectedResult = new HashSet<string>(removeExpectedResult);
            foreach (string name in alterList)
            {
                addExpectedResult.Add(name);
            }

            try
            {
                var actualResult = client.AlterDatabaseList(alterList, null);
                Assert.IsTrue(addExpectedResult.SetEquals(actualResult));

                actualResult = client.AlterDatabaseList(null, alterList);
                Assert.IsTrue(removeExpectedResult.SetEquals(actualResult));
            }
            finally
            {
                client.CleanupRemovedDatabases();
            }
        }

        [TestMethod]
        public void AlterDatabaseListTest_EmptyLists()
        {
            var client = new MySqlDatabaseClient(TestServerAddress, null, null);

            var alterList = new List<string>(); // empty list
            var databaseNames = client.GetDatabaseList();

            HashSet<string> expectedResult = new HashSet<string>(databaseNames);
            
            var actualResult = client.AlterDatabaseList(alterList, alterList);
            Assert.IsTrue(expectedResult.SetEquals(actualResult));
        }

        [TestMethod]
        public void AlterDatabaseListTest_NullLists()
        {
            var client = new MySqlDatabaseClient(TestServerAddress, null, null);
            var databaseNames = client.GetDatabaseList();

            HashSet<string> expectedResult = new HashSet<string>(databaseNames);

            var actualResult = client.AlterDatabaseList(null, null);
            Assert.IsTrue(expectedResult.SetEquals(actualResult));
        }

        [TestMethod]
        public void GetDatabaseAccessModeTest_NewDatabase()
        {
            DatabaseAccessMode expectedResult = DatabaseAccessMode.ReadWrite;

            var adminClient = new MySqlDatabaseClient(TestServerAddress, null, null);
            var dbName = RandomDatabaseNamesGenerator.GetRandomDatabaseName(15, "nz_");
            var dbList = new List<string>() { dbName };
            adminClient.AlterDatabaseList(dbList, null);

            try
            {
                var client = new MySqlDatabaseClient(TestServerAddress, dbName, null);
                DatabaseAccessMode actualResult = client.GetDatabaseAccessMode();
                Assert.AreEqual(expectedResult, actualResult);
            }
            finally
            {
                adminClient.AlterDatabaseList(null, dbList);
                adminClient.CleanupRemovedDatabases();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NezaboodkaAvailabilityException))]
        public void GetDatabaseAccessModeTest_DatabaseNotExists()
        {
            var dbName = RandomDatabaseNamesGenerator.GetRandomDatabaseName(15, "nz_");
            var client = new MySqlDatabaseClient(TestServerAddress, dbName, null);
            client.GetDatabaseAccessMode();
        }

        [TestMethod]
        public void AlterDatabaseConfigurationTest_SchemaTest_OneType()
        {
            DatabaseConfiguration configuration = new DatabaseConfiguration
            {
                DatabaseSchema = DatabaseConfigTestUtils.GetSingleClassDbSchema()
            };

            CheckNewAlterDatabaseConfiguration(configuration);
        }

        [TestMethod]
        public void AlterDatabaseConfigurationTest_SchemaTest_MultipleTypes_Refs()
        {
            DatabaseConfiguration configuration = new DatabaseConfiguration
            {
                DatabaseSchema = DatabaseConfigTestUtils.GetMultipleClassDbSchema()
            };

            CheckNewAlterDatabaseConfiguration(configuration);
        }

        [TestMethod]
        public void AlterDatabaseConfigurationTest_UpdateSchema_AddTypes()
        {
            var adminClient = new MySqlDatabaseClient(TestServerAddress, null, null);

            string dbName = RandomDatabaseNamesGenerator.GetRandomDatabaseName(15, "nz_");
            var dbList = new List<string> { dbName };
            adminClient.AlterDatabaseList(dbList, null);

            DatabaseConfiguration configuration = new DatabaseConfiguration
            {
                DatabaseSchema = DatabaseConfigTestUtils.GetSingleClassDbSchema()
            };

            try
            {
                var client = new MySqlDatabaseClient(TestServerAddress, dbName, null);
                client.AlterDatabaseConfiguration(configuration);

                DatabaseConfigTestUtils.AddClasses(configuration.DatabaseSchema);
                DatabaseConfiguration expectedResult = configuration;

                DatabaseConfiguration actualResult = client.AlterDatabaseConfiguration(configuration);
                bool result = DatabaseConfigTestUtils.AreEqualDbConfigurations(expectedResult, actualResult);
                Assert.IsTrue(result);
            }
            finally
            {
                adminClient.AlterDatabaseList(null, dbList);
                adminClient.CleanupRemovedDatabases();
            }
        }

        [TestMethod]
        public void AlterDatabaseConfigurationTest_UpdateSchema_Complex()
        {
            var adminClient = new MySqlDatabaseClient(TestServerAddress, null, null);

            string dbName = RandomDatabaseNamesGenerator.GetRandomDatabaseName(15, "nz_");
            var dbList = new List<string> { dbName };
            adminClient.AlterDatabaseList(dbList, null);

            DatabaseConfiguration configuration = new DatabaseConfiguration
            {
                DatabaseSchema = DatabaseConfigTestUtils.GetSingleClassDbSchema()
            };

            try
            {
                var client = new MySqlDatabaseClient(TestServerAddress, dbName, null);
                client.AlterDatabaseConfiguration(configuration);

                configuration = new DatabaseConfiguration
                {
                    DatabaseSchema = DatabaseConfigTestUtils.GetMultipleClassDbSchema()
                };

                DatabaseConfiguration expectedResult = configuration;

                DatabaseConfiguration actualResult = client.AlterDatabaseConfiguration(configuration);
                bool result = DatabaseConfigTestUtils.AreEqualDbConfigurations(expectedResult, actualResult);
                Assert.IsTrue(result);

                configuration = new DatabaseConfiguration
                {
                    DatabaseSchema = DatabaseConfigTestUtils.GetSingleClassDbSchema()
                };

                DatabaseConfigTestUtils.AddClasses(configuration.DatabaseSchema);
                expectedResult = configuration;

                actualResult = client.AlterDatabaseConfiguration(configuration);
                result = DatabaseConfigTestUtils.AreEqualDbConfigurations(expectedResult, actualResult);
                Assert.IsTrue(result);
            }
            finally
            {
                adminClient.AlterDatabaseList(null, dbList);
                adminClient.CleanupRemovedDatabases();
            }
        }

        // Internal

        private static void CheckNewAlterDatabaseConfiguration(DatabaseConfiguration expectedResult)
        {
            var adminClient = new MySqlDatabaseClient(TestServerAddress, null, null);

            string dbName = RandomDatabaseNamesGenerator.GetRandomDatabaseName(15, "nz_");
            var dbList = new List<string> { dbName };
            adminClient.AlterDatabaseList(dbList, null);

            try
            {
                var client = new MySqlDatabaseClient(TestServerAddress, dbName, null);

                DatabaseConfiguration actualResult = client.AlterDatabaseConfiguration(expectedResult);
                bool result = DatabaseConfigTestUtils.AreEqualDbConfigurations(expectedResult, actualResult);
                Assert.IsTrue(result);
            }
            finally
            {
                adminClient.AlterDatabaseList(null, dbList);
                adminClient.CleanupRemovedDatabases();
            }
        }
    }
}
