using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OneSTools.BracketsFile;

namespace OneSTools.Config
{
    public class Configuration
    {
        public string Name { get; private set; }
        public string Synonym { get; private set; }
        public string Comment { get; private set; }
        public string Version { get; private set; }
        public string Supplier { get; private set; }
        public DataLockingControlMode DataLockingControlMode { get; private set; }
        public string CompatibilityMode { get; private set; }
        public List<ExchangePlan> ExchangePlans { get; private set; } = new List<ExchangePlan>();
        public List<Constant> Constants { get; private set; } = new List<Constant>();
        public List<Catalog> Catalogs { get; private set; } = new List<Catalog>();
        public List<Document> Documents { get; private set; } = new List<Document>();
        public List<DocumentJournal> DocumentJournals { get; private set; } = new List<DocumentJournal>();
        public List<OneSEnum> Enums { get; private set; } = new List<OneSEnum>();
        public List<ChartOfCharacteristicTypes> ChartsOfCharacteristicTypes { get; private set; } = new List<ChartOfCharacteristicTypes>();
        public List<ChartOfAccounts> ChartsOfAccounts { get; private set; } = new List<ChartOfAccounts>();
        public List<AccumulationRegister> AccumulationRegisters { get; private set; } = new List<AccumulationRegister>();
        public List<AccountingRegister> AccountingRegisters { get; private set; } = new List<AccountingRegister>();
        public List<CalculationRegister> CalculationRegisters { get; private set; } = new List<CalculationRegister>();
        public List<BusinessProcess> BusinessProcesses { get; private set; } = new List<BusinessProcess>();
        public List<OneSTask> Tasks { get; private set; } = new List<OneSTask>();

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        public async static Task<Configuration> ReadFromDatabaseAsync(DbConnection connection, CancellationToken ct)
        {
            var config = new Configuration();

            var root = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, "root", ct));
            var conf = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, (string)root.GetNode(1), ct));

            config.ReadConfigurationProperties(conf);

            await config.ReadExchangePlansAsync(connection, conf, ct);
            await config.ReadConstantsAsync(connection, conf, ct);
            await config.ReadCatalogsAsync(connection, conf, ct);
            await config.ReadDocumentsAsync(connection, conf, ct);
            await config.ReadDocumentJournalsAsync(connection, conf, ct);
            await config.ReadEnumsAsync(connection, conf, ct);
            await config.ReadChartsOfCharacteristicTypesAsync(connection, conf, ct);
            await config.ReadChartsOfAccountsAsync(connection, conf, ct);
            await config.ReadAccumulationRegistersAsync(connection, conf, ct);
            await config.ReadAccountingRegistersAsync(connection, conf, ct);
            await config.ReadCalculationRegistersAsync(connection, conf, ct);
            await config.ReadBusinessProcessesAsync(connection, conf, ct);
            await config.ReadTasksAsync(connection, conf, ct);

            return config;
        }
        /// <summary>
        /// Reads properties of the 1C configuration
        /// </summary>
        /// <param name="config"></param>
        /// <param name="conf"></param>
        private void ReadConfigurationProperties(BracketsFileNode node)
        {
            Name = (string)node.GetNode(3, 1, 1, 1, 1, 2);
            Synonym = GetSynonym(node.GetNode(3, 1, 1, 1, 1, 3));
            Comment = (string)node.GetNode(3, 1, 1, 1, 1, 4);
            Supplier = (string)node.GetNode(3, 1, 1, 14);
            Version = (string)node.GetNode(3, 1, 1, 15);
            DataLockingControlMode = (DataLockingControlMode)(int)node.GetNode(3, 1, 1, 17);
            CompatibilityMode = (string)node.GetNode(3, 1, 1, 26);
        }
        /// <summary>
        /// Reads properties of "Exchange plan" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="config"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadExchangePlansAsync(DbConnection connection, BracketsFileNode node, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(node.GetNode(3, 1, 19));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<ExchangePlan>(objectInfo.GetNode(1, 12), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "1a1b4fea-e093-470d-94ff-1d2f16cda2ab":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "52293f4b-f98c-43ea-a80f-41047ae7ab58":
                            obj.TabularSections = ReadTabularSections(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                ExchangePlans.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Constant" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadConstantsAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(4, 1, 1, 3));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<Constant>(objectInfo.GetNode(1, 1, 1, 1), objectGuid);

                var typesNode = objectInfo.GetNode(1, 1, 1, 2);
                obj.Types = ReadTypesInfo(typesNode);

                Constants.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Catalog" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadCatalogsAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(4, 1, 1, 16));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<Catalog>(objectInfo.GetNode(1, 9, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "cf4abea7-37b2-11d4-940f-008048da11f9":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "932159f9-95b2-4e76-a8dd-8849fe5c5ded":
                            obj.TabularSections = ReadTabularSections(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                Catalogs.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Document" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadDocumentsAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(4, 1, 1, 4));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<Document>(objectInfo.GetNode(1, 9, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch(objectDetailsNodeGuid)
                    {
                        case "45e46cbc-3e24-4165-8b7b-cc98a6f80211":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "21c53e09-8950-4b5e-a6a0-1054f1bbc274":
                            obj.TabularSections = ReadTabularSections(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                Documents.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Document journal" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadDocumentJournalsAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(4, 1, 1, 10));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<DocumentJournal>(objectInfo.GetNode(1, 3, 1), objectGuid);

                var requisitiesNode = objectInfo.GetNode(4);

                obj.Graphs = ReadRequisities(requisitiesNode, true);

                DocumentJournals.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Enum" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadEnumsAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(4, 1, 1, 17));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<OneSEnum>(objectInfo.GetNode(1, 5, 1), objectGuid);

                Enums.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Chart of characteristic types" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadChartsOfCharacteristicTypesAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(4, 1, 1, 12));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<ChartOfCharacteristicTypes>(objectInfo.GetNode(1, 13, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "31182525-9346-4595-81f8-6f91a72ebe06":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "54e36536-7863-42fd-bea3-c5edd3122fdc":
                            obj.TabularSections = ReadTabularSections(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                ChartsOfCharacteristicTypes.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Chart of accounts" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadChartsOfAccountsAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(5, 1, 3));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<ChartOfAccounts>(objectInfo.GetNode(1, 15, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "6e65cbf5-daa8-4d8d-bef8-59723f4e5777":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "4c7fec95-d1bd-4508-8a01-f1db090d9af8":
                            obj.TabularSections = ReadTabularSections(objectDetailsNode);
                            break;
                        case "78bd1243-c4df-46c3-8138-e147465cb9a4":
                            obj.AccountingFlags = ReadRequisities(objectDetailsNode);
                            break;
                        case "c70ca527-5042-4cad-a315-dcb4007e32a3":
                            obj.ExtDimensionAccountingFlags = ReadRequisities(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                ChartsOfAccounts.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Accumulation register" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadAccumulationRegistersAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(4, 1, 1, 13));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<AccumulationRegister>(objectInfo.GetNode(1, 13, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "b64d9a42-1642-11d6-a3c7-0050bae0a776":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "b64d9a43-1642-11d6-a3c7-0050bae0a776":
                            obj.Dimensions = ReadRequisities(objectDetailsNode);
                            break;
                        case "b64d9a41-1642-11d6-a3c7-0050bae0a776":
                            obj.Resources = ReadRequisities(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                AccumulationRegisters.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Accounting register" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadAccountingRegistersAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(5, 1, 4));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<AccountingRegister>(objectInfo.GetNode(1, 15, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "9d28ee33-9c7e-4a1b-8f13-50aa9b36607b":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "35b63b9d-0adf-4625-a047-10ae874c19a3":
                            obj.Dimensions = ReadRequisities(objectDetailsNode);
                            break;
                        case "63405499-7491-4ce3-ac72-43433cbe4112":
                            obj.Resources = ReadRequisities(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                AccountingRegisters.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Calculation register" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadCalculationRegistersAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(6, 1, 4));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<CalculationRegister>(objectInfo.GetNode(1, 15, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "1b304502-2216-440b-960f-60decd04bb5d":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "b12fc850-8210-43c8-ae05-89567e698fbb":
                            obj.Dimensions = ReadRequisities(objectDetailsNode);
                            break;
                        case "702b33ad-843e-41aa-8064-112cd38cc92c":
                            obj.Resources = ReadRequisities(objectDetailsNode);
                            break;
                        case "274bf899-db0e-4df6-8ab5-67bf6371ec0b":
                            obj.Recalculations = await ReadRecalculations(connection, objectDetailsNode, ct);
                            break;
                        default:
                            continue;
                    }
                }

                CalculationRegisters.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Business process" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadBusinessProcessesAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(7, 1, 4));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));

                var obj = ReadMetadataObject<BusinessProcess>(objectInfo.GetNode(1, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "87c988de-ecbf-413b-87b0-b9516df05e28":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "a3fe6537-d787-40f7-8a06-419d2f0c1cfd":
                            obj.TabularSections = ReadTabularSections(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                BusinessProcesses.Add(obj);
            }
        }
        /// <summary>
        /// Reads properties of "Task" metadata objects
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="conf"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadTasksAsync(DbConnection connection, BracketsFileNode conf, CancellationToken ct)
        {
            var objectGuids = GetMetadataObjectGuids(conf.GetNode(7, 1, 3));

            foreach (var objectGuid in objectGuids)
            {
                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, objectGuid, ct));
                
                var obj = ReadMetadataObject<OneSTask>(objectInfo.GetNode(1, 1), objectGuid);

                var objectDetailsNodesCount = (int)objectInfo.GetNode(2);

                for (int i = 3; i < (3 + objectDetailsNodesCount); i++)
                {
                    var objectDetailsNode = objectInfo.GetNode(i);

                    var objectDetailsNodeGuid = (string)objectDetailsNode.GetNode(0);

                    switch (objectDetailsNodeGuid)
                    {
                        case "8ddfb495-c5fc-46b9-bdc5-bcf58341bff0":
                            obj.Requisities = ReadRequisities(objectDetailsNode);
                            break;
                        case "ee865d4b-a458-48a0-b38f-5a26898feeb0":
                            obj.TabularSections = ReadTabularSections(objectDetailsNode);
                            break;
                        case "e97c0570-251c-4566-b0f1-10686820f143":
                            obj.AddressingAttributes = ReadRequisities(objectDetailsNode);
                            break;
                        default:
                            continue;
                    }
                }

                Tasks.Add(obj);
            }
        }
        private List<Requisite> ReadRequisities(BracketsFileNode node, bool shortPath = false)
        {
            var fields = new List<Requisite>();

            var count = (int)node.GetNode(1);

            for (int i = 0; i < count; i++)
            {
                var requisiteNode = node.GetNode(i + 2);

                Requisite requisite;

                if (shortPath)
                {
                    requisite = ReadMetadataObject<Requisite>(requisiteNode.GetNode(0, 1));
                    requisite.Types = ReadTypesInfo(requisiteNode.GetNode(0, 2));
                }
                else
                {
                    requisite = ReadMetadataObject<Requisite>(requisiteNode.GetNode(0, 1, 1, 1));
                    requisite.Types = ReadTypesInfo(requisiteNode.GetNode(0, 1, 1, 2));
                }

                fields.Add(requisite);
            }

            return fields;
        }
        private List<TabularSection> ReadTabularSections(BracketsFileNode node)
        {
            var list = new List<TabularSection>();

            var count = (int)node.GetNode(1);

            for (int i = 0; i < count; i++)
            {
                var tabularSectionNode = node.GetNode(i + 2);

                var obj = ReadMetadataObject<TabularSection>(tabularSectionNode.GetNode(0, 1, 5, 1));

                obj.Requisities = ReadRequisities(tabularSectionNode.GetNode(2));

                list.Add(obj);
            }

            return list;
        }
        private async Task<List<Recalculation>> ReadRecalculations(DbConnection connection, BracketsFileNode node, CancellationToken ct)
        {
            var recalculations = new List<Recalculation>();

            var count = (int)node.GetNode(1);

            for (int i = 0; i < count; i++)
            {
                var recalculationNode = node.GetNode(i + 2);

                var objectInfo = BracketsFileParser.Parse(await GetStringConfigFileDataAsync(connection, recalculationNode.Text, ct));

                var recalculation = ReadMetadataObject<Recalculation>(objectInfo.GetNode(1, 7, 1));

                recalculations.Add(recalculation);
            }

            return recalculations;
        }
        private static List<TypeInfo> ReadTypesInfo(BracketsFileNode node)
        {
            var list = new List<TypeInfo>();

            int count;
            int offset = 1;

            if ((string)node[0] == "Pattern")
                count = node.Nodes.Count - 1;
            else
            {
                count = (int)node.GetNode(1);
                offset = 2;
            }

            for (int x = 0; x < count; x++)
            {
                var typeInfoNode = node.GetNode(x + offset);
                var typeInfo = ReadTypeInfo(typeInfoNode);

                list.Add(typeInfo);
            }

            return list;
        }
        private static TypeInfo ReadTypeInfo(BracketsFileNode node)
        {
            var type = (string)node.GetNode(0);

            switch (type)
            {
                case "#":
                    var uuid = (string)node.GetNode(1);
                    return new ReferenceTypeInfo(uuid);
                case "B":
                    return new TypeInfo(ValueType.Boolean);
                case "S":
                    if (node.Nodes.Count > 1)
                    {
                        var sLength = (int)node.GetNode(1);
                        var fixedLength = (int)node.GetNode(2) == 0;
                        return new StringTypeInfo(sLength, fixedLength);
                    }
                    else
                        return new StringTypeInfo(0, false);
                case "N":
                    var nLength = (int)node.GetNode(1);
                    var precision = (int)node.GetNode(2);
                    var notNegative = (int)node.GetNode(3) == 0;
                    return new NumericTypeInfo(nLength, precision, notNegative);
                case "D":
                    if (node.Nodes.Count > 1)
                    {
                        var kindStr = (string)node.GetNode(1);
                        if (kindStr == "D")
                            return new DateTimeTypeInfo(DateTimeKind.Date);
                        else if (kindStr == "T")
                            return new DateTimeTypeInfo(DateTimeKind.Time);
                        else
                            throw new Exception($@"{kindStr} is unknown marker of the date and time kind");
                    }
                    else
                        return new DateTimeTypeInfo(DateTimeKind.DateTime);
                default:
                    throw new Exception($@"{type} is unknown marker of the requisite type");
            }
        }
        private T ReadMetadataObject<T>(BracketsFileNode node) where T : MetadataObject, new()
        {
            var uuid = (string)node.GetNode(1, 2);

            return ReadMetadataObject<T>(node, uuid);
        }
        private T ReadMetadataObject<T>(BracketsFileNode node, string uuid) where T : MetadataObject, new()
        {
            T obj = new T
            {
                UUID = uuid,
                Name = (string)node.GetNode(2),
                Synonym = GetSynonym(node.GetNode(3))
            };

            return obj;
        }
        private static List<string> GetMetadataObjectGuids(BracketsFileNode node)
        {
            var data = new List<string>();

            var objectsCount = (int)node.GetNode(1);

            if (objectsCount == 0)
                return data;

            for (int i = 0; i < objectsCount; i++)
                data.Add((string)node.GetNode(i + 2));

            return data;
        }
        /// <summary>
        /// Returns a text that contains in the file from the "Config" table
        /// </summary>
        /// <param name="connection">Sql connectiob to the database</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        private static async Task<string> GetStringConfigFileDataAsync(DbConnection connection, string fileName, CancellationToken ct)
        {
            var bytes = await GetConfigFileDataAsync(connection, fileName, ct);
            var data = await GetStringFileDataAsync(bytes, ct);

            return data;
        }
        /// <summary>
        /// Returns a binary data of the file from the "Config" table
        /// </summary>
        /// <param name="connection">Sql connection to the database</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        private static async Task<byte[]> GetConfigFileDataAsync(DbConnection connection, string fileName, CancellationToken ct)
        {
            byte[] data;

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT [BinaryData] FROM [dbo].[Config] WITH(NOLOCK) WHERE [FileName] = '{fileName}'";

            data = (byte[])await cmd.ExecuteScalarAsync(ct);

            return data;
        }
        /// <summary>
        /// Returns the text of the "Params" table item
        /// </summary>
        /// <param name="connection">Sql connection to the database</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        public static async Task<string> GetStringParamsFileDataAsync(DbConnection connection, string fileName, CancellationToken ct)
        {
            var bytes = await GetParamsFileDataAsync(connection, fileName, ct);
            var data = await GetStringFileDataAsync(bytes, ct);

            return data;
        }
        /// <summary>
        /// Returns binary data of the "Params" table item
        /// </summary>
        /// <param name="connection">Sql connection to the database</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        private static async Task<byte[]> GetParamsFileDataAsync(DbConnection connection, string fileName, CancellationToken ct)
        {
            byte[] data;

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT [BinaryData] FROM [dbo].[Params] WITH(NOLOCK) WHERE [FileName] = '{fileName}'";

            data = (byte[])await cmd.ExecuteScalarAsync(ct);

            return data;
        }
        /// <summary>
        /// Returns a text that contains in the file from the "SchemaStorage" table
        /// </summary>
        /// <param name="connection">Sql connectiob to the database</param>
        /// <returns></returns>
        public static async Task<string> GetStringSchemaStorageFileDataAsync(DbConnection connection, CancellationToken ct)
        {
            var bytes = await GetSchemaStorageFileDataAsync(connection, ct);
            var data = await GetStringFileDataAsync(bytes, ct);

            return data;
        }
        /// <summary>
        /// Returns a binary data of the file from the "SchemaStorage" table
        /// </summary>
        /// <param name="connection">Sql connection to the database</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        private static async Task<byte[]> GetSchemaStorageFileDataAsync(DbConnection connection, CancellationToken ct)
        {
            byte[] data;

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT [CurrentSchema] FROM [dbo].[SchemaStorage] WITH(NOLOCK)";

            data = (byte[])await cmd.ExecuteScalarAsync(ct);

            return data;
        }
        /// <summary>
        /// Returns a text from the bytes array
        /// </summary>
        /// <param name="data">Source bytes array</param>
        /// <returns></returns>
        private static async Task<string> GetStringFileDataAsync(byte[] data, CancellationToken ct)
        {
            string stringData;

            try
            {
                stringData = await DecompressDataAsync(data, ct);
            }
            catch
            {
                stringData = Encoding.UTF8.GetString(data);
            }

            return stringData;
        }
        /// <summary>
        /// Unpacks and returns bytes array that was packed with the help of "Deflate" algorithm
        /// </summary>
        /// <param name="data">Source byte array</param>
        /// <returns></returns>
        private static async Task<string> DecompressDataAsync(byte[] data, CancellationToken ct)
        {
            string decompressedData;

            using (var source = new MemoryStream(data))
            using (var destination = new MemoryStream())
            using (var deflateStream = new DeflateStream(source, CompressionMode.Decompress))
            {
                var buffer = new byte[data.Length];
                await deflateStream.CopyToAsync(destination, buffer.Length, ct);
                decompressedData = Encoding.UTF8.GetString(destination.ToArray());
            }

            return decompressedData;
        }
        private static string GetSynonym(BracketsFileNode node)
        {
            var hasSynonym = (int)node.GetNode(0) != 0;

            if (hasSynonym)
                return (string)node.GetNode(2);

            return null;
        }
    }
}